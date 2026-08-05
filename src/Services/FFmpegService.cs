// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

#nullable enable

using System.Globalization;
using FFmpegDotnetWrapper.Constants;
using FFmpegDotnetWrapper.Exceptions;
using FFmpegDotnetWrapper.Models;
using FFmpegDotnetWrapper.Policies;
using FFmpegDotnetWrapper.Repository;
using Microsoft.Extensions.Logging;

namespace FFmpegDotnetWrapper.Services;

/// <summary>
/// Main implementation of <see cref="IFFmpegService"/> orchestrating all media operations.
/// </summary>
public class FFmpegService : IFFmpegService
{
    private readonly IMediaRepository _mediaRepository;
    private readonly IOperationRepository _operationRepository;
    private readonly ILogger<FFmpegService> _logger;
    private readonly string _ffmpegPath;
    private readonly string _ffprobePath;
    private readonly TimeSpan _defaultTimeout;
    private readonly IRetryPolicy _retryPolicy;
    private readonly IFFmpegProcessRunner _processRunner;

    /// <summary>
    /// Initializes a new instance of <see cref="FFmpegService"/>, resolving the <c>ffmpeg</c>
    /// and <c>ffprobe</c> executable paths and configuring the default operation timeout.
    /// </summary>
    /// <param name="mediaRepository">Repository used to persist analyzed media file metadata.</param>
    /// <param name="operationRepository">Repository used to persist FFmpeg operation records.</param>
    /// <param name="logger">Logger for operation lifecycle and error reporting.</param>
    /// <param name="retryPolicy">Retry policy applied around failed operations; defaults to no retry.</param>
    /// <param name="processRunner">
    /// Runner used to execute the <c>ffmpeg</c> process; defaults to <see cref="FFmpegProcessRunner"/>.
    /// Substitute <see cref="FakeFFmpegProcessRunner"/> in tests to avoid spawning a real process.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="mediaRepository"/>, <paramref name="operationRepository"/>, or
    /// <paramref name="logger"/> is null.
    /// </exception>
    public FFmpegService(
        IMediaRepository mediaRepository,
        IOperationRepository operationRepository,
        ILogger<FFmpegService> logger,
        IRetryPolicy? retryPolicy = null,
        IFFmpegProcessRunner? processRunner = null)
    {
        _mediaRepository = mediaRepository ?? throw new ArgumentNullException(nameof(mediaRepository));
        _operationRepository = operationRepository ?? throw new ArgumentNullException(nameof(operationRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _retryPolicy = retryPolicy ?? new ExponentialBackoffRetryPolicy(maxAttempts: 1); // No retry by default
        _processRunner = processRunner ?? new FFmpegProcessRunner();

        _ffmpegPath = ResolveExecutablePath(FFmpegConstants.FFmpegExecutableName);
        _ffprobePath = ResolveExecutablePath(FFmpegConstants.FFprobeExecutableName);
        _defaultTimeout = TimeSpan.FromSeconds(FFmpegConstants.DefaultTimeoutSeconds);
    }

    /// <inheritdoc/>
    public async Task<ConversionResult> TranscodeAsync(
        MediaFile inputMedia,
        string outputPath,
        TranscodeSettings settings,
        CancellationToken cancellationToken = default)
    {
        var operation = new FFmpegOperation
        {
            Name = $"Transcode {inputMedia.Name}",
            Type = FFmpegOperationType.Transcode,
            OutputFile = outputPath,
            Timeout = _defaultTimeout
        };

        operation.AddInputFile(inputMedia.FilePath);

        try
        {
            inputMedia.ValidateAsVideo();
            settings.Validate();

            // Build transcoding arguments
            BuildTranscodeArguments(operation, settings);

            _logger.LogInformation("Starting transcode operation for {File}", inputMedia.Name);
            var result = await ExecuteFFmpegAsync(operation, cancellationToken);

            if (result.IsSuccess)
            {
                var outputMedia = await AnalyzeMediaAsync(outputPath, cancellationToken);
                result.OutputMedia = outputMedia;
                result.SetMetric("InputSize", inputMedia.FileSize);
                result.SetMetric("OutputSize", outputMedia.FileSize);
                result.SetMetric("SizeReduction", result.GetSizeReductionPercentage(inputMedia.FileSize));
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transcode operation failed for {File}", inputMedia.Name);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ConversionResult> TranscodeAsync(
        MediaFile inputMedia,
        string outputPath,
        TranscodeSettings settings,
        IProgress<FFmpegProgressUpdate> progress,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputMedia);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(progress);

        var operation = new FFmpegOperation
        {
            Name = $"Transcode {inputMedia.Name}",
            Type = FFmpegOperationType.Transcode,
            OutputFile = outputPath,
            Timeout = _defaultTimeout
        };

        operation.AddInputFile(inputMedia.FilePath);

        try
        {
            inputMedia.ValidateAsVideo();
            settings.Validate();

            BuildTranscodeArguments(operation, settings);

            _logger.LogInformation("Starting transcode operation with progress streaming for {File}", inputMedia.Name);
            var result = await ExecuteFFmpegWithProgressAsync(operation, inputMedia.Duration, progress, cancellationToken);

            if (result.IsSuccess)
            {
                var outputMedia = await AnalyzeMediaAsync(outputPath, cancellationToken);
                result.OutputMedia = outputMedia;
                result.SetMetric("InputSize", inputMedia.FileSize);
                result.SetMetric("OutputSize", outputMedia.FileSize);
                result.SetMetric("SizeReduction", result.GetSizeReductionPercentage(inputMedia.FileSize));
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transcode operation failed for {File}", inputMedia.Name);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ConversionResult> TrimAsync(
        MediaFile inputMedia,
        string outputPath,
        TrimSettings settings,
        CancellationToken cancellationToken = default)
    {
        var operation = new FFmpegOperation
        {
            Name = $"Trim {inputMedia.Name}",
            Type = FFmpegOperationType.Trim,
            OutputFile = outputPath,
            Timeout = _defaultTimeout
        };

        operation.AddInputFile(inputMedia.FilePath);

        try
        {
            settings.Validate(inputMedia);

            // Build trim arguments
            BuildTrimArguments(operation, settings);

            _logger.LogInformation("Starting trim operation for {File}", inputMedia.Name);
            var result = await ExecuteFFmpegAsync(operation, cancellationToken);

            if (result.IsSuccess)
            {
                var outputMedia = await AnalyzeMediaAsync(outputPath, cancellationToken);
                result.OutputMedia = outputMedia;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Trim operation failed for {File}", inputMedia.Name);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ConversionResult> MergeAsync(
        IEnumerable<string> inputFiles,
        string outputPath,
        MergeSettings settings,
        CancellationToken cancellationToken = default)
    {
        var operation = new FFmpegOperation
        {
            Name = "Merge media files",
            Type = FFmpegOperationType.Merge,
            OutputFile = outputPath,
            Timeout = TimeSpan.FromSeconds(FFmpegConstants.DefaultTimeoutSeconds * 2)
        };

        foreach (var file in inputFiles)
            operation.AddInputFile(file);

        try
        {
            settings.InputFiles = operation.InputFiles;
            settings.Validate();

            _logger.LogInformation("Starting merge operation with {Count} files", operation.InputFiles.Count);
            var result = await ExecuteFFmpegAsync(operation, cancellationToken);

            if (result.IsSuccess)
            {
                var outputMedia = await AnalyzeMediaAsync(outputPath, cancellationToken);
                result.OutputMedia = outputMedia;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Merge operation failed");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ConversionResult> AddWatermarkAsync(
        MediaFile inputMedia,
        string outputPath,
        WatermarkSettings settings,
        CancellationToken cancellationToken = default)
    {
        var operation = new FFmpegOperation
        {
            Name = $"Add watermark to {inputMedia.Name}",
            Type = FFmpegOperationType.Watermark,
            OutputFile = outputPath,
            Timeout = _defaultTimeout
        };

        operation.AddInputFile(inputMedia.FilePath);

        try
        {
            settings.Validate(inputMedia);

            // Build watermark arguments
            BuildWatermarkArguments(operation, settings, inputMedia);

            _logger.LogInformation("Starting watermark operation for {File}", inputMedia.Name);
            var result = await ExecuteFFmpegAsync(operation, cancellationToken);

            if (result.IsSuccess)
            {
                var outputMedia = await AnalyzeMediaAsync(outputPath, cancellationToken);
                result.OutputMedia = outputMedia;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Watermark operation failed for {File}", inputMedia.Name);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<MediaFile> AnalyzeMediaAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
            throw new InvalidMediaFileException($"File does not exist: {filePath}", filePath);

        var mediaFile = new MediaFile(filePath);

        try
        {
            _logger.LogInformation("Analyzing media file: {File}", filePath);

            var ffprobeArgs = $"-v quiet -print_format json -show_format -show_streams \"{filePath}\"";

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _ffprobePath,
                    Arguments = ffprobeArgs,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            cancellationToken.ThrowIfCancellationRequested();
            process.Start();

        // Read stdout and stderr concurrently to prevent deadlocks
        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        var output = await outputTask;
        var error = await errorTask;

        if (process.ExitCode != 0)
        {
            throw new FFmpegProcessException($"ffprobe failed with exit code {process.ExitCode}. Error: {error}", process.ExitCode);
        }

            var ffprobeResult = System.Text.Json.JsonDocument.Parse(output);
            var format = ffprobeResult.RootElement.GetProperty("format");

            mediaFile.FormatName = format.GetProperty("format_name").GetString();
            mediaFile.Duration = TimeSpan.FromSeconds(format.GetProperty("duration").GetDouble());
            mediaFile.FileSize = format.GetProperty("size").GetInt64();
            mediaFile.BitRate = format.GetProperty("bit_rate").GetInt64();

            if (ffprobeResult.RootElement.TryGetProperty("streams", out var streamsElement))
            {
                foreach (var streamElement in streamsElement.EnumerateArray())
                {
                    var codecType = streamElement.GetProperty("codec_type").GetString();
                    if (codecType == "video")
                    {
                        mediaFile.Width = streamElement.GetProperty("width").GetInt32();
                        mediaFile.Height = streamElement.GetProperty("height").GetInt32();
                        mediaFile.FrameRate = (int)Math.Round(streamElement.GetProperty("r_frame_rate").GetString().ParseDouble());
                        mediaFile.VideoCodec = streamElement.GetProperty("codec_name").GetString();
                    }
                    else if (codecType == "audio")
                    {
                        mediaFile.AudioCodec = streamElement.GetProperty("codec_name").GetString();
                        mediaFile.SampleRate = streamElement.GetProperty("sample_rate").GetInt32();
                        mediaFile.Channels = streamElement.GetProperty("channels").GetInt32();
                    }
                }
            }

            // Save the media file to the repository
            await _mediaRepository.AddAsync(mediaFile, cancellationToken);
            return mediaFile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Media analysis failed for {File}", filePath);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ConversionResult> ExecuteCustomOperationAsync(
        FFmpegOperation operation,
        CancellationToken cancellationToken = default)
    {
        try
        {
            operation.Validate();
            _logger.LogInformation("Executing custom FFmpeg operation: {Name}", operation.Name);

            var result = await ExecuteFFmpegAsync(operation, cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Custom operation failed: {Name}", operation.Name);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<string> GetFFmpegVersionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _ffmpegPath,
                    Arguments = "-version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
            RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            cancellationToken.ThrowIfCancellationRequested();
        process.Start();

        // Read both stdout and stderr to prevent deadlocks
        var versionOutputTask = process.StandardOutput.ReadLineAsync(cancellationToken);
        var errorOutputTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        var versionOutput = await versionOutputTask;
        await errorOutputTask; // Ensure stderr is drained

        return versionOutput ?? "Unknown";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get FFmpeg version");
            throw;
        }
    }

    /// <inheritdoc/>
    public Task<bool> IsFFmpegAvailableAsync(CancellationToken cancellationToken = default)
    {
        bool available = File.Exists(_ffmpegPath) && File.Exists(_ffprobePath);
        _logger.LogInformation("FFmpeg availability check: {Available}", available);
        return Task.FromResult(available);
    }

    /// <inheritdoc/>
    public async Task<ConversionResult> ExtractAudioAsync(
        MediaFile inputMedia,
        string outputPath,
        AudioCodec audioCodec = AudioCodec.MP3,
        int audioBitrate = 192,
        CancellationToken cancellationToken = default)
    {
        var operation = new FFmpegOperation
        {
            Name = $"Extract audio from {inputMedia.Name}",
            Type = FFmpegOperationType.Demux,
            OutputFile = outputPath,
            Timeout = _defaultTimeout
        };

        operation.AddInputFile(inputMedia.FilePath);

        try
        {
            _logger.LogInformation(
                "Extracting audio from {File} as {Codec} at {Bitrate}kbps",
                inputMedia.Name, audioCodec, audioBitrate);

            var audioCodecName = GetAudioCodecName(audioCodec);
            operation.AddArgument("-vn"); // discard video stream
            operation.AddArgument($"-c:a {audioCodecName}");
            operation.AddArgument($"-b:a {audioBitrate}k");

            var result = await ExecuteFFmpegAsync(operation, cancellationToken);

            if (result.IsSuccess)
            {
                var outputMedia = await AnalyzeMediaAsync(outputPath, cancellationToken);
                result.OutputMedia = outputMedia;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Audio extraction failed for {File}", inputMedia.Name);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<ConversionResult>> BatchTranscodeAsync(
        IEnumerable<MediaFile> inputFiles,
        string outputDirectory,
        TranscodeSettings settings,
        CancellationToken cancellationToken = default)
    {
        if (inputFiles == null)
            throw new ArgumentNullException(nameof(inputFiles));

        Directory.CreateDirectory(outputDirectory);

        var extension = settings.Container switch
        {
            ContainerFormat.MP4 => ".mp4",
            ContainerFormat.WebM => ".webm",
            ContainerFormat.Matroska => ".mkv",
            _ => ".mp4"
        };

        var results = new List<ConversionResult>();

        foreach (var inputMedia in inputFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fileName = Path.GetFileNameWithoutExtension(inputMedia.FilePath) + extension;
            var outputPath = Path.Combine(outputDirectory, fileName);

            var result = await TranscodeAsync(inputMedia, outputPath, settings, cancellationToken);
            results.Add(result);
        }

        return results;
    }

    /// <summary>
    /// Encodes a media file into an HTTP Live Streaming (HLS) playlist with segmented
    /// output, using the codec, bitrate, segmentation, and playlist options in <paramref name="settings"/>.
    /// </summary>
    /// <param name="inputMedia">The source media file with pre-analyzed metadata.</param>
    /// <param name="playlistPath">Destination file path for the generated <c>.m3u8</c> playlist; segment files are written alongside it.</param>
    /// <param name="settings">HLS settings including codecs, bitrates, segment duration, and playlist type.</param>
    /// <param name="cancellationToken">Token to cancel the FFmpeg process.</param>
    /// <returns>A <see cref="ConversionResult"/> describing the outcome of the HLS encode.</returns>
    public async Task<ConversionResult> CreateHlsAsync(
        MediaFile inputMedia,
        string playlistPath,
        HlsSettings settings,
        CancellationToken cancellationToken = default)
    {
        var playlistDir = Path.GetDirectoryName(playlistPath);
        if (!string.IsNullOrEmpty(playlistDir))
            Directory.CreateDirectory(playlistDir);

        // Segment files live alongside the playlist
        var segmentPattern = string.IsNullOrEmpty(playlistDir)
            ? settings.SegmentFilePattern
            : Path.Combine(playlistDir, settings.SegmentFilePattern);

        var operation = new FFmpegOperation
        {
            Name = $"HLS encode {inputMedia.Name}",
            Type = FFmpegOperationType.Transcode,
            OutputFile = playlistPath,
            Timeout = TimeSpan.FromSeconds(FFmpegConstants.DefaultTimeoutSeconds * 2)
        };

        operation.AddInputFile(inputMedia.FilePath);

        try
        {
            settings.Validate();

            var videoCodecName = GetVideoCodecName(settings.VideoCodec);
            var audioCodecName = GetAudioCodecName(settings.AudioCodec);

            operation.AddArgument($"-c:v {videoCodecName}");
            operation.AddArgument($"-b:v {settings.VideoBitrate}k");
            operation.AddArgument($"-c:a {audioCodecName}");
            operation.AddArgument($"-b:a {settings.AudioBitrate}k");

            if (settings.Width.HasValue || settings.Height.HasValue)
            {
                var w = settings.Width ?? -1;
                var h = settings.Height ?? -1;
                operation.AddArgument($"-vf \"scale={w}:{h}\"");
            }

            operation.AddArgument("-f hls");
            operation.AddArgument($"-hls_time {settings.SegmentDuration}");

            var playlistTypeArg = settings.PlaylistType == HlsPlaylistType.Vod ? "vod" : "event";
            operation.AddArgument($"-hls_playlist_type {playlistTypeArg}");

            if (settings.MaxSegments > 0)
                operation.AddArgument($"-hls_list_size {settings.MaxSegments}");

            if (settings.IndependentSegments)
                operation.AddArgument("-hls_flags independent_segments");

            operation.AddArgument($"-hls_segment_filename \"{segmentPattern}\"");

            _logger.LogInformation(
                "Starting HLS encode for {File} -> {Playlist} ({SegDur}s segments, {Type})",
                inputMedia.Name, playlistPath, settings.SegmentDuration, settings.PlaylistType);

            var result = await ExecuteFFmpegAsync(operation, cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HLS encode failed for {File}", inputMedia.Name);
            throw;
        }
    }

    private async Task<ConversionResult> ExecuteFFmpegAsync(
        FFmpegOperation operation,
        CancellationToken cancellationToken)
    {
        var result = new ConversionResult
        {
            Duration = operation.Timeout ?? _defaultTimeout
        };

        var sw = Stopwatch.StartNew();

        try
        {
            await _operationRepository.AddAsync(operation, cancellationToken);

            var arguments = operation.BuildCommandLine();
            _logger.LogDebug("Executing FFmpeg command: {Command}", arguments);

            var timeout = operation.Timeout ?? _defaultTimeout;
            var request = new FFmpegProcessRequest
            {
                FileName = _ffmpegPath,
                Arguments = arguments,
                Timeout = timeout,
                OperationId = operation.Id
            };

            var runResult = await _processRunner.RunAsync(request, progress: null, cancellationToken);

            sw.Stop();
            result.Duration = sw.Elapsed;

            if (runResult.TimedOut)
            {
                throw new FFmpegProcessException(
                    $"FFmpeg process timed out after {timeout.TotalSeconds} seconds",
                    timeout);
            }

            if (runResult.WasCancelled)
                cancellationToken.ThrowIfCancellationRequested();

            if (runResult.Success)
            {
                result.MarkAsSuccess(operation.OutputFile, runResult.ExitCode);
                _logger.LogInformation("FFmpeg operation completed successfully: {Name}", operation.Name);
            }
            else
            {
                // Extract the tail of stderr (last 10 lines) for diagnostic purposes
                var errorOutputTail = ExtractErrorOutputTail(runResult.StdErrTail);
                result.MarkAsFailed($"FFmpeg exited with code {runResult.ExitCode}", runResult.ExitCode, errorOutputTail);
                result.FFmpegOutput = runResult.StdErrTail;
                _logger.LogError("FFmpeg operation failed with exit code {ExitCode}: {Error}", runResult.ExitCode, errorOutputTail);
            }

            operation.ExecutedAt = DateTime.UtcNow;
            await _operationRepository.UpdateAsync(operation, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            result.Duration = sw.Elapsed;
            result.MarkAsFailed(ex.Message);
            _logger.LogError(ex, "FFmpeg execution error");
            throw;
        }
    }



    /// <summary>
    /// Maximum number of characters of ffmpeg stderr retained for diagnostic purposes while
    /// streaming progress. Bounds memory usage on multi-hour transcodes that would otherwise
    /// accumulate unbounded stderr text.
    /// </summary>
    private const int MaxRetainedStderrChars = 64 * 1024;

    /// <summary>
    /// Executes an FFmpeg operation while streaming incremental <see cref="FFmpegProgressUpdate"/>
    /// snapshots parsed from FFmpeg's <c>-progress pipe:1</c> stdout stream. Each stdout line is
    /// parsed and discarded immediately (no accumulation of the full output), and stderr is kept
    /// bounded to the last <see cref="MaxRetainedStderrChars"/> characters for error reporting.
    /// </summary>
    /// <param name="operation">The operation to execute; <c>-progress pipe:1 -nostats</c> is appended to its arguments.</param>
    /// <param name="totalDuration">Total media duration used to compute percentage completion; pass <see cref="TimeSpan.Zero"/> if unknown.</param>
    /// <param name="progress">Receiver of incremental progress snapshots.</param>
    /// <param name="cancellationToken">Token used to cancel the running process.</param>
    /// <returns>A <see cref="ConversionResult"/> describing the outcome of the operation.</returns>
    private async Task<ConversionResult> ExecuteFFmpegWithProgressAsync(
        FFmpegOperation operation,
        TimeSpan totalDuration,
        IProgress<FFmpegProgressUpdate> progress,
        CancellationToken cancellationToken)
    {
        var result = new ConversionResult
        {
            Duration = operation.Timeout ?? _defaultTimeout
        };

        var sw = Stopwatch.StartNew();

        try
        {
            await _operationRepository.AddAsync(operation, cancellationToken);

            // Machine-readable progress on stdout; -nostats stops the human-readable
            // progress banner from also being written to stderr on every frame.
            operation.AddArgument("-progress pipe:1");
            operation.AddArgument("-nostats");

            var timeout = operation.Timeout ?? _defaultTimeout;
            var request = new FFmpegProcessRequest
            {
                FileName = _ffmpegPath,
                Arguments = operation.BuildCommandLine(),
                Timeout = timeout,
                OperationId = operation.Id,
                TotalDuration = totalDuration,
                ParseProgressFromStdOut = true
            };

            var runResult = await _processRunner.RunAsync(request, progress, cancellationToken);

            sw.Stop();
            result.Duration = sw.Elapsed;

            if (runResult.TimedOut)
            {
                throw new FFmpegProcessException(
                    $"FFmpeg process timed out after {timeout.TotalSeconds} seconds",
                    timeout);
            }

            if (runResult.WasCancelled)
                cancellationToken.ThrowIfCancellationRequested();

            if (runResult.Success)
            {
                result.MarkAsSuccess(operation.OutputFile, runResult.ExitCode);
                _logger.LogInformation("FFmpeg operation completed successfully: {Name}", operation.Name);
            }
            else
            {
                var errorOutputTail = ExtractErrorOutputTail(runResult.StdErrTail);
                result.MarkAsFailed($"FFmpeg exited with code {runResult.ExitCode}", runResult.ExitCode, errorOutputTail);
                result.FFmpegOutput = runResult.StdErrTail;
                _logger.LogError("FFmpeg operation failed with exit code {ExitCode}: {Error}", runResult.ExitCode, errorOutputTail);
            }

            operation.ExecutedAt = DateTime.UtcNow;
            await _operationRepository.UpdateAsync(operation, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            result.Duration = sw.Elapsed;
            result.MarkAsFailed(ex.Message);
            _logger.LogError(ex, "FFmpeg execution error");
            throw;
        }
    }

    private void BuildTranscodeArguments(FFmpegOperation operation, TranscodeSettings settings)
    {
        // Hardware acceleration must be specified before codec selection
        if (settings.HardwareAcceleration != HwAccel.None)
        {
            var hwAccelName = GetHwAccelName(settings.HardwareAcceleration);
            operation.Arguments.Insert(0, $"-hwaccel {hwAccelName}");
        }

        var videoCodec = GetVideoCodecName(settings.VideoCodec, settings.HardwareAcceleration);
        var audioCodec = GetAudioCodecName(settings.AudioCodec);

        operation.AddArgument($"-c:v {videoCodec}");
        operation.AddArgument($"-c:a {audioCodec}");
        operation.AddArgument($"-b:v {settings.VideoBitrate}k");
        operation.AddArgument($"-b:a {settings.AudioBitrate}k");
        operation.AddArgument($"-r {settings.FrameRate}");
        operation.AddArgument($"-preset {GetPresetName(settings.Quality)}");

        if (settings.Width.HasValue || settings.Height.HasValue)
        {
            var width = settings.Width ?? -1;
            var height = settings.Height ?? -1;
            operation.AddArgument($"-vf \"scale={width}:{height}\"");
        }

        if (!string.IsNullOrEmpty(settings.CustomFFmpegArgs))
            operation.AddArgument(settings.CustomFFmpegArgs);
    }

    private void BuildTrimArguments(FFmpegOperation operation, TrimSettings settings)
    {
        operation.AddArgument($"-ss {settings.StartTime.TotalSeconds.ToString(CultureInfo.InvariantCulture)}");

        if (settings.Duration.HasValue)
            operation.AddArgument($"-t {settings.Duration.Value.TotalSeconds.ToString(CultureInfo.InvariantCulture)}");
        else if (settings.EndTime.HasValue)
            operation.AddArgument($"-to {settings.EndTime.Value.TotalSeconds.ToString(CultureInfo.InvariantCulture)}");

        operation.AddArgument("-c copy"); // Copy without re-encoding
    }

    private void BuildWatermarkArguments(
        FFmpegOperation operation,
        WatermarkSettings settings,
        MediaFile videoFile)
    {
        var filter = $"overlay=";
        var (x, y) = settings.CalculatePosition(videoFile.Width ?? 1920, videoFile.Height ?? 1080);
        filter += $"{x}:{y}";

        operation.AddArgument($"-i \"{settings.WatermarkPath}\"");
        operation.AddArgument($"-filter_complex \"{filter}\"");
        operation.AddArgument($"-c:a copy");
    }

    private string GetVideoCodecName(VideoCodec codec) =>
        GetVideoCodecName(codec, HwAccel.None);

    private string GetVideoCodecName(VideoCodec codec, HwAccel hwAccel) =>
        hwAccel switch
        {
            HwAccel.NVENC => codec switch
            {
                VideoCodec.H264 => "h264_nvenc",
                VideoCodec.H265 => "hevc_nvenc",
                VideoCodec.AV1  => "av1_nvenc",
                _               => GetVideoCodecName(codec, HwAccel.None)
            },
            HwAccel.VAAPI => codec switch
            {
                VideoCodec.H264 => "h264_vaapi",
                VideoCodec.H265 => "hevc_vaapi",
                VideoCodec.VP9  => "vp9_vaapi",
                VideoCodec.AV1  => "av1_vaapi",
                _               => GetVideoCodecName(codec, HwAccel.None)
            },
            HwAccel.QSV => codec switch
            {
                VideoCodec.H264 => "h264_qsv",
                VideoCodec.H265 => "hevc_qsv",
                VideoCodec.VP9  => "vp9_qsv",
                VideoCodec.AV1  => "av1_qsv",
                _               => GetVideoCodecName(codec, HwAccel.None)
            },
            // Auto and None fall through to software codec names
            _ => codec switch
            {
                VideoCodec.H264 => FFmpegConstants.VideoCodecNames.H264,
                VideoCodec.H265 => FFmpegConstants.VideoCodecNames.H265,
                VideoCodec.VP9  => FFmpegConstants.VideoCodecNames.VP9,
                VideoCodec.AV1  => FFmpegConstants.VideoCodecNames.AV1,
                _               => FFmpegConstants.VideoCodecNames.H264
            }
        };

    private static string GetHwAccelName(HwAccel hwAccel) =>
        hwAccel switch
        {
            HwAccel.NVENC => "cuda",
            HwAccel.VAAPI => "vaapi",
            HwAccel.QSV   => "qsv",
            HwAccel.Auto  => "auto",
            _             => "none"
        };

    private string GetAudioCodecName(AudioCodec codec) =>
        codec switch
        {
            AudioCodec.AAC => FFmpegConstants.AudioCodecNames.AAC,
            AudioCodec.MP3 => FFmpegConstants.AudioCodecNames.MP3,
            AudioCodec.OPUS => FFmpegConstants.AudioCodecNames.OPUS,
            AudioCodec.FLAC => FFmpegConstants.AudioCodecNames.FLAC,
            _ => FFmpegConstants.AudioCodecNames.AAC
        };

    private string GetPresetName(QualityPreset preset) =>
        preset switch
        {
            QualityPreset.Ultrafast => FFmpegConstants.PresetLevels.Ultrafast,
            QualityPreset.Slow => FFmpegConstants.PresetLevels.Slow,
            QualityPreset.Medium => FFmpegConstants.PresetLevels.Medium,
            _ => FFmpegConstants.PresetLevels.Medium
        };

    private string ResolveExecutablePath(string executableName)
    {
        // Check in PATH
        var paths = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) ?? Array.Empty<string>();

        foreach (var path in paths)
        {
            var fullPath = Path.Combine(path, executableName);
            if (File.Exists(fullPath))
                return fullPath;
        }

        // Return the executable name as fallback (assume it's in PATH)
        return executableName;
    }

    /// <inheritdoc/>
    public async Task<ConversionResult> EmbedSubtitlesAsync(
        MediaFile inputMedia,
        string outputPath,
        SubtitleSettings settings,
        CancellationToken cancellationToken = default)
    {
        var operation = new FFmpegOperation
        {
            Name = $"Embed subtitles into {inputMedia.Name}",
            Type = FFmpegOperationType.Filter,
            OutputFile = outputPath,
            Timeout = _defaultTimeout
        };

        operation.AddInputFile(inputMedia.FilePath);

        try
        {
            settings.Validate();

            BuildSubtitleArguments(operation, settings);

            _logger.LogInformation(
                "Embedding subtitles ({Mode}) from {Sub} into {File}",
                settings.HardEmbed ? "hard" : "soft",
                settings.SubtitlePath,
                inputMedia.Name);

            var result = await ExecuteFFmpegAsync(operation, cancellationToken);

            if (result.IsSuccess)
            {
                var outputMedia = await AnalyzeMediaAsync(outputPath, cancellationToken);
                result.OutputMedia = outputMedia;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Subtitle embedding failed for {File}", inputMedia.Name);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ThumbnailResult> ExtractThumbnailsAsync(
        MediaFile inputMedia,
        string outputPattern,
        ThumbnailSettings settings,
        CancellationToken cancellationToken = default)
    {
        var result = new ThumbnailResult();
        var sw = Stopwatch.StartNew();

        try
        {
            settings.Validate(inputMedia);

            var outputDir = Path.GetDirectoryName(outputPattern);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            if (settings.Times.Count > 0)
            {
                // Extract one thumbnail per explicit timestamp
                for (var i = 0; i < settings.Times.Count; i++)
                {
                    var timestamp = settings.Times[i];
                    var singleOutput = settings.Times.Count == 1
                        ? outputPattern
                        : string.Format(outputPattern.Replace("%03d", "{0:D3}"), i + 1);

                    var operation = BuildThumbnailOperation(inputMedia, singleOutput, settings, timestamp);
                    var opResult = await ExecuteFFmpegAsync(operation, cancellationToken);

                    if (opResult.IsSuccess && File.Exists(singleOutput))
                        result.Thumbnails.Add(singleOutput);
                }
            }
            else
            {
                // Extract evenly spaced thumbnails
                var operation = BuildThumbnailOperation(inputMedia, outputPattern, settings, null);
                await ExecuteFFmpegAsync(operation, cancellationToken);

                // Collect all generated files matching the pattern
                var directory = Path.GetDirectoryName(outputPattern) ?? ".";
                var fileNameTemplate = Path.GetFileName(outputPattern);
                var ext = Path.GetExtension(fileNameTemplate);
                var searchPattern = "*" + ext;

                var generatedFiles = Directory.GetFiles(directory, searchPattern)
                    .Where(f => f.StartsWith(
                        Path.Combine(directory, Path.GetFileNameWithoutExtension(fileNameTemplate).Replace("%03d", "").TrimEnd('_')),
                        StringComparison.OrdinalIgnoreCase))
                    .OrderBy(f => f)
                    .ToList();

                result.Thumbnails.AddRange(generatedFiles);
            }

            sw.Stop();
            result.Duration = sw.Elapsed;
            result.IsSuccess = result.Thumbnails.Count > 0;

            _logger.LogInformation(
                "Extracted {Count} thumbnail(s) from {File} in {Elapsed}ms",
                result.Thumbnails.Count, inputMedia.Name, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            result.Duration = sw.Elapsed;
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Thumbnail extraction failed for {File}", inputMedia.Name);
            throw;
        }

        return result;
    }

    private FFmpegOperation BuildThumbnailOperation(
        MediaFile inputMedia,
        string outputPath,
        ThumbnailSettings settings,
        TimeSpan? timestamp)
    {
        var operation = new FFmpegOperation
        {
            Name = $"Extract thumbnail from {inputMedia.Name}",
            Type = FFmpegOperationType.Filter,
            OutputFile = outputPath,
            Timeout = TimeSpan.FromSeconds(60)
        };

        operation.AddInputFile(inputMedia.FilePath);

        if (timestamp.HasValue)
            operation.AddArgument($"-ss {timestamp.Value.TotalSeconds.ToString("F3", CultureInfo.InvariantCulture)}");

        var filters = new List<string>();

        if (settings.Width.HasValue || settings.Height.HasValue)
        {
            var w = settings.Width ?? -1;
            var h = settings.Height ?? -1;
            filters.Add($"scale={w}:{h}");
        }

        if (filters.Count > 0)
            operation.AddArgument($"-vf \"{string.Join(",", filters)}\"");

        operation.AddArgument("-vframes 1");

        if (settings.Format == ThumbnailFormat.Jpeg && settings.JpegQuality.HasValue)
            operation.AddArgument($"-q:v {settings.JpegQuality.Value}");

        return operation;
    }

    private void BuildSubtitleArguments(FFmpegOperation operation, SubtitleSettings settings)
    {
        if (settings.HardEmbed)
        {
            // Burn subtitles into video frames via the subtitles filter
            var escapedPath = settings.SubtitlePath.Replace("\\", "/").Replace(":", "\\:");
            var subtitlesFilter = $"subtitles='{escapedPath}'";

            if (!string.IsNullOrWhiteSpace(settings.FontName) || settings.FontSize != 24)
            {
                var fontStyle = $"force_style='FontName={settings.FontName},FontSize={settings.FontSize}'";
                subtitlesFilter = $"subtitles='{escapedPath}':{fontStyle}";
            }

            operation.AddArgument($"-vf \"{subtitlesFilter}\"");
            operation.AddArgument("-c:a copy");
        }
        else
        {
            // Soft-embed: map original streams + subtitle file
            operation.AddInputFile(settings.SubtitlePath);
            operation.AddArgument("-c:v copy");
            operation.AddArgument("-c:a copy");
            operation.AddArgument("-c:s mov_text");
            operation.AddArgument("-map 0:v");
            operation.AddArgument("-map 0:a");
            operation.AddArgument("-map 1:0");

            if (!string.IsNullOrEmpty(settings.Language))
                operation.AddArgument($"-metadata:s:s:0 language={settings.Language}");
        }
    }

    /// <summary>
    /// Extracts the tail (last 10 lines) of FFmpeg error output for diagnostic purposes.
    /// This prevents overwhelming callers with full FFmpeg output while providing the most
    /// relevant error information.
    /// </summary>
    /// <param name="errorOutput">Full error output from FFmpeg.</param>
    /// <returns>Tail of error output (last 10 lines), or the full output if shorter.</returns>
    private string ExtractErrorOutputTail(string errorOutput)
    {
        if (string.IsNullOrWhiteSpace(errorOutput))
            return errorOutput;

        var lines = errorOutput.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        // Return last 10 lines, or all lines if fewer than 10
        var startIndex = Math.Max(0, lines.Length - 10);
        return string.Join(Environment.NewLine, lines.Skip(startIndex));
    }
}
