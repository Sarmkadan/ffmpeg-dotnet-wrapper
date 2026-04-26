// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Channels;
using FFmpegDotnetWrapper.Configuration;
using FFmpegDotnetWrapper.Events;
using FFmpegDotnetWrapper.Models;
using FFmpegDotnetWrapper.Monitoring;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FFmpegDotnetWrapper.Services;

/// <summary>
/// Default implementation of <see cref="IAdaptiveBitrateService"/>.
/// </summary>
/// <remarks>
/// Each call to <see cref="RunPipelineAsync"/> generates an isolated pipeline context
/// tracked in a <see cref="ConcurrentDictionary{TKey,TValue}"/>, launches one FFmpeg
/// process per quality profile, polls for newly written segment files, and applies
/// an adaptive bitrate heuristic based on each segment's actual-to-target bitrate ratio.
/// </remarks>
public sealed class AdaptiveBitrateService : IAdaptiveBitrateService
{
    private sealed record PipelineContext(StreamingPipelineResult Result, CancellationTokenSource Cts);

    private readonly ConcurrentDictionary<string, PipelineContext> _activePipelines = new();
    private readonly ILogger<AdaptiveBitrateService> _logger;
    private readonly IEventPublisher _eventPublisher;
    private readonly IStreamingProgressService _progressService;
    private readonly StreamingPipelineMetrics _metrics;
    private readonly StreamingPipelineOptions _options;
    private readonly string _ffmpegPath;

    /// <summary>
    /// Initialises the service with its required dependencies.
    /// </summary>
    public AdaptiveBitrateService(
        ILogger<AdaptiveBitrateService> logger,
        IEventPublisher eventPublisher,
        IStreamingProgressService progressService,
        StreamingPipelineMetrics metrics,
        IOptions<StreamingPipelineOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _progressService = progressService ?? throw new ArgumentNullException(nameof(progressService));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _ffmpegPath = ResolveFFmpegPath();
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<string> ActivePipelineIds => [.. _activePipelines.Keys];

    /// <inheritdoc/>
    public async IAsyncEnumerable<StreamingSegment> RunPipelineAsync(
        StreamingPipelineSettings settings,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
            throw new InvalidOperationException("The adaptive bitrate streaming pipeline is disabled via configuration.");

        if (_activePipelines.Count >= _options.MaxConcurrentPipelines)
            throw new InvalidOperationException(
                $"Maximum concurrent pipeline limit of {_options.MaxConcurrentPipelines} has been reached.");

        var pipelineId = Guid.NewGuid().ToString("N")[..12];
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        var result = new StreamingPipelineResult
        {
            PipelineId = pipelineId,
            ActiveProfile = settings.Profiles.MaxBy(p => p.VideoBitrateKbps)
        };

        _activePipelines[pipelineId] = new PipelineContext(result, linkedCts);

        var channel = Channel.CreateUnbounded<StreamingSegment>();
        var pipelineTask = ExecutePipelineAsync(settings, pipelineId, result, linkedCts, channel, cancellationToken);

        await foreach (var segment in channel.Reader.ReadAllAsync(cancellationToken))
            yield return segment;

        await pipelineTask;
    }

    private async Task ExecutePipelineAsync(
        StreamingPipelineSettings settings,
        string pipelineId,
        StreamingPipelineResult result,
        CancellationTokenSource linkedCts,
        Channel<StreamingSegment> channel,
        CancellationToken cancellationToken)
    {
        try
        {
            settings.Validate();
            result.State = PipelineState.Running;

            await _eventPublisher.PublishAsync(new OperationStartedEvent
            {
                Source = nameof(AdaptiveBitrateService),
                CorrelationId = pipelineId,
                InputFile = settings.InputFilePath,
                OutputFile = settings.OutputDirectory,
                OperationType = "StreamingPipeline"
            });

            result.MasterPlaylistPath = await InitialisePipelineAsync(settings, linkedCts.Token);

            _logger.LogInformation(
                "Pipeline {Id} started — {Count} profiles, format {Format}",
                pipelineId, settings.Profiles.Count, settings.Format);

            var speedHistory = new Dictionary<string, Queue<double>>(settings.Profiles.Count);
            var orderedProfiles = settings.Profiles
                .OrderByDescending(p => p.VideoBitrateKbps)
                .ToList();

            if (settings.EncodeProfilesConcurrently)
            {
                var tasks = orderedProfiles
                    .Select(p => DrainRenditionToChannelAsync(settings, p, pipelineId, channel.Writer, linkedCts.Token))
                    .ToList();

                await Task.WhenAll(tasks);
            }
            else
            {
                foreach (var profile in orderedProfiles)
                {
                    await foreach (var segment in EncodeRenditionAsync(settings, profile, linkedCts.Token))
                    {
                        result.AddSegment(segment);
                        EvaluateBitrateSwitch(result, segment, settings, speedHistory);
                        _metrics.RecordSegmentProduced(segment.Profile, segment.FileSizeBytes);
                        await channel.Writer.WriteAsync(segment, linkedCts.Token);
                    }
                }
            }

            result.State = PipelineState.Completed;
            result.EndedAt = DateTimeOffset.UtcNow;
            _metrics.RecordPipelineCompleted(pipelineId, result.Elapsed);

            await _eventPublisher.PublishAsync(new OperationCompletedEvent
            {
                Source = nameof(AdaptiveBitrateService),
                CorrelationId = pipelineId,
                InputFile = settings.InputFilePath,
                OutputFile = result.MasterPlaylistPath ?? settings.OutputDirectory,
                OperationType = "StreamingPipeline",
                Duration = result.Elapsed
            });

            _logger.LogInformation(
                "Pipeline {Id} completed in {Elapsed:c} — {SegmentCount} segments, {SwitchCount} ABR switches",
                pipelineId, result.Elapsed, result.Segments.Count, result.BitrateSwitches.Count);
        }
        catch (OperationCanceledException)
        {
            result.State = PipelineState.Cancelled;
            result.EndedAt = DateTimeOffset.UtcNow;
            _logger.LogInformation("Pipeline {Id} was cancelled after {Elapsed:c}", pipelineId, result.Elapsed);
        }
        catch (Exception ex)
        {
            result.State = PipelineState.Failed;
            result.ErrorMessage = ex.Message;
            result.EndedAt = DateTimeOffset.UtcNow;
            _metrics.RecordPipelineFailed(pipelineId);

            await _eventPublisher.PublishAsync(new OperationFailedEvent
            {
                Source = nameof(AdaptiveBitrateService),
                CorrelationId = pipelineId,
                InputFile = settings.InputFilePath,
                OperationType = "StreamingPipeline",
                ErrorMessage = ex.Message,
                StackTrace = ex.StackTrace
            });

            _logger.LogError(ex, "Pipeline {Id} failed", pipelineId);
            throw;
        }
        finally
        {
            channel.Writer.TryComplete();
            _activePipelines.TryRemove(pipelineId, out _);
        }
    }

    /// <inheritdoc/>
    public Task<string> InitialisePipelineAsync(
        StreamingPipelineSettings settings,
        CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(settings.OutputDirectory);
        foreach (var profile in settings.Profiles)
            Directory.CreateDirectory(Path.Combine(settings.OutputDirectory, profile.Name));

        var masterPath = settings.Format == StreamingFormat.Hls
            ? WriteHlsMasterPlaylist(settings)
            : WriteDashManifestStub(settings);

        _logger.LogDebug("Pipeline manifest written to {Path}", masterPath);
        return Task.FromResult(masterPath);
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<StreamingSegment> EncodeRenditionAsync(
        StreamingPipelineSettings settings,
        StreamingProfile profile,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var renditionDir = Path.Combine(settings.OutputDirectory, profile.Name);
        Directory.CreateDirectory(renditionDir);

        var playlistPath = Path.Combine(renditionDir, "playlist.m3u8");
        var segmentPattern = Path.Combine(renditionDir, "seg_%05d.ts");
        var trackingId = $"{Path.GetFileName(settings.OutputDirectory)}-{profile.Name}";

        var ffmpegArgs = BuildHlsArguments(
            settings.InputFilePath, profile,
            segmentPattern, playlistPath,
            settings.SegmentDurationSeconds,
            settings.EnableHardwareAcceleration);

        _logger.LogDebug("Launching FFmpeg for {Profile}: {Args}", profile.Name, ffmpegArgs);

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _ffmpegPath,
                Arguments = ffmpegArgs,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            }
        };

        process.Start();

        // Drain stderr via the progress service so FFmpeg does not block on a full pipe buffer.
        var stderrTask = Task.Run(async () =>
        {
            await foreach (var _ in _progressService.StreamProgressAsync(
                trackingId, process, TimeSpan.Zero, cancellationToken)) { }
        }, CancellationToken.None);

        var knownFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var seqNum = 0;

        try
        {
            // Poll for new .ts files while the encoder is running, then drain any
            // remaining files produced in the final write cycle after exit.
            while (!process.HasExited
                || Directory.EnumerateFiles(renditionDir, "seg_*.ts").Any(f => !knownFiles.Contains(f)))
            {
                cancellationToken.ThrowIfCancellationRequested();

                foreach (var filePath in Directory
                    .EnumerateFiles(renditionDir, "seg_*.ts")
                    .Where(f => !knownFiles.Contains(f))
                    .OrderBy(f => f))
                {
                    knownFiles.Add(filePath);

                    var fi = new FileInfo(filePath);
                    yield return new StreamingSegment
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        PipelineId = trackingId,
                        Profile = profile,
                        SequenceNumber = seqNum++,
                        FilePath = filePath,
                        DurationSeconds = settings.SegmentDurationSeconds,
                        FileSizeBytes = fi.Exists ? fi.Length : 0
                    };
                }

                if (!process.HasExited)
                    await Task.Delay(250, cancellationToken);
            }
        }
        finally
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);

            await stderrTask.ConfigureAwait(false);
            _logger.LogDebug("{Profile} rendition finished — {Count} segments", profile.Name, knownFiles.Count);
        }
    }

    /// <inheritdoc/>
    public Task<StreamingPipelineResult?> GetPipelineResultAsync(string pipelineId)
    {
        _activePipelines.TryGetValue(pipelineId, out var ctx);
        return Task.FromResult<StreamingPipelineResult?>(ctx?.Result);
    }

    /// <inheritdoc/>
    public Task<bool> CancelPipelineAsync(string pipelineId)
    {
        if (!_activePipelines.TryGetValue(pipelineId, out var ctx))
            return Task.FromResult(false);

        ctx.Cts.Cancel();
        _logger.LogInformation("Cancellation requested for pipeline {Id}", pipelineId);
        return Task.FromResult(true);
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    /// <summary>
    /// Encodes a single rendition and writes every segment into a shared channel so that
    /// concurrent renditions can be merged into a single async sequence in <see cref="RunPipelineAsync"/>.
    /// </summary>
    private async Task DrainRenditionToChannelAsync(
        StreamingPipelineSettings settings,
        StreamingProfile profile,
        string pipelineId,
        ChannelWriter<StreamingSegment> writer,
        CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var segment in EncodeRenditionAsync(settings, profile, cancellationToken))
                await writer.WriteAsync(segment, cancellationToken);
        }
        catch (OperationCanceledException) { /* pipeline cancelled — channel closed by WhenAll continuation */ }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rendition {Profile} failed in pipeline {Id}", profile.Name, pipelineId);
        }
    }

    /// <summary>
    /// Evaluates the most recently produced segment's bitrate against the target and
    /// records an adaptive bitrate switch if the sliding window average crosses a threshold.
    /// </summary>
    private void EvaluateBitrateSwitch(
        StreamingPipelineResult result,
        StreamingSegment segment,
        StreamingPipelineSettings settings,
        Dictionary<string, Queue<double>> speedHistory)
    {
        if (segment.DurationSeconds <= 0 || segment.Profile.VideoBitrateKbps <= 0)
            return;

        var profileKey = segment.Profile.Name;
        if (!speedHistory.TryGetValue(profileKey, out var window))
        {
            window = new Queue<double>(_options.BitrateDecisionWindowSegments + 1);
            speedHistory[profileKey] = window;
        }

        // Use actual/target bitrate ratio as a proxy for encoder load.
        var ratio = segment.ActualBitrateKbps / segment.Profile.VideoBitrateKbps;
        window.Enqueue(ratio);
        while (window.Count > _options.BitrateDecisionWindowSegments)
            window.Dequeue();

        if (window.Count < _options.BitrateDecisionWindowSegments)
            return;

        var avgRatio = window.Average();
        var sortedProfiles = settings.Profiles.OrderBy(p => p.VideoBitrateKbps).ToList();
        var currentIdx = sortedProfiles.FindIndex(p => p.Name == segment.Profile.Name);
        if (currentIdx < 0) return;

        if (avgRatio < _options.DowngradeSpeedThreshold && currentIdx > 0)
        {
            var target = sortedProfiles[currentIdx - 1];
            result.ActiveProfile = target;
            result.RecordSwitch(new BitrateSwitch
            {
                FromProfile = segment.Profile,
                ToProfile = target,
                Reason = $"Avg bitrate ratio {avgRatio:F2} below downgrade threshold {_options.DowngradeSpeedThreshold:F2}"
            });
            _metrics.RecordBitrateSwitch(isUpgrade: false);
            _logger.LogInformation("ABR downgrade: {From} → {To} (ratio {Ratio:F2})",
                segment.Profile.Name, target.Name, avgRatio);
        }
        else if (avgRatio > _options.UpgradeSpeedThreshold && currentIdx < sortedProfiles.Count - 1)
        {
            var target = sortedProfiles[currentIdx + 1];
            result.ActiveProfile = target;
            result.RecordSwitch(new BitrateSwitch
            {
                FromProfile = segment.Profile,
                ToProfile = target,
                Reason = $"Avg bitrate ratio {avgRatio:F2} above upgrade threshold {_options.UpgradeSpeedThreshold:F2}"
            });
            _metrics.RecordBitrateSwitch(isUpgrade: true);
            _logger.LogInformation("ABR upgrade: {From} → {To} (ratio {Ratio:F2})",
                segment.Profile.Name, target.Name, avgRatio);
        }
    }

    /// <summary>Writes the HLS master playlist (<c>master.m3u8</c>) listing all configured renditions.</summary>
    private static string WriteHlsMasterPlaylist(StreamingPipelineSettings settings)
    {
        var masterPath = Path.Combine(settings.OutputDirectory, "master.m3u8");
        var sb = new StringBuilder();
        sb.AppendLine("#EXTM3U");
        sb.AppendLine("#EXT-X-VERSION:3");

        foreach (var p in settings.Profiles.OrderByDescending(p => p.VideoBitrateKbps))
        {
            sb.AppendLine($"#EXT-X-STREAM-INF:BANDWIDTH={p.TotalBitrateKbps * 1000}," +
                          $"RESOLUTION={p.Resolution},NAME=\"{p.Name}\"");
            sb.AppendLine($"{p.Name}/playlist.m3u8");
        }

        File.WriteAllText(masterPath, sb.ToString());
        return masterPath;
    }

    /// <summary>
    /// Writes a placeholder DASH manifest. The full MPD is produced by FFmpeg's <c>dash</c> muxer
    /// when actual DASH segmentation is enabled per profile.
    /// </summary>
    private static string WriteDashManifestStub(StreamingPipelineSettings settings)
    {
        var path = Path.Combine(settings.OutputDirectory, "manifest.mpd");
        File.WriteAllText(path,
            "<!-- MPEG-DASH manifest — generated by ffmpeg-dotnet-wrapper adaptive bitrate pipeline -->");
        return path;
    }

    /// <summary>
    /// Constructs the FFmpeg argument string for HLS segmentation of a single quality rendition.
    /// Produces H.264/AAC segments with VBV buffering appropriate for the target bitrate.
    /// </summary>
    private static string BuildHlsArguments(
        string inputPath,
        StreamingProfile profile,
        string segmentPattern,
        string playlistPath,
        int segmentDuration,
        bool hwAccel)
    {
        var sb = new StringBuilder("-y ");

        if (hwAccel)
            sb.Append("-hwaccel auto ");

        sb.Append($"-i \"{inputPath}\" ");
        sb.Append("-c:v libx264 ");
        sb.Append($"-b:v {profile.VideoBitrateKbps}k ");
        sb.Append($"-maxrate {(int)(profile.VideoBitrateKbps * 1.2)}k ");
        sb.Append($"-bufsize {profile.VideoBitrateKbps * 2}k ");

        if (profile.Width > 0 && profile.Height > 0)
            sb.Append($"-vf \"scale={profile.Width}:{profile.Height}\" ");

        if (profile.FrameRate > 0)
            sb.Append($"-r {profile.FrameRate} ");

        sb.Append("-c:a aac ");
        sb.Append($"-b:a {profile.AudioBitrateKbps}k -ac 2 ");
        sb.Append("-f hls ");
        sb.Append($"-hls_time {segmentDuration} ");
        sb.Append("-hls_playlist_type vod ");
        sb.Append("-hls_flags independent_segments ");
        sb.Append($"-hls_segment_filename \"{segmentPattern}\" ");
        sb.Append($"\"{playlistPath}\"");

        return sb.ToString();
    }

    /// <summary>Resolves the FFmpeg executable path by searching the system PATH.</summary>
    private static string ResolveFFmpegPath()
    {
        var paths = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) ?? [];
        foreach (var dir in paths)
        {
            var unix = Path.Combine(dir, "ffmpeg");
            if (File.Exists(unix)) return unix;
            var win = Path.Combine(dir, "ffmpeg.exe");
            if (File.Exists(win)) return win;
        }
        return "ffmpeg";
    }
}
