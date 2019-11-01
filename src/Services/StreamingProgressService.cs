// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using FFmpegDotnetWrapper.Models;
using Microsoft.Extensions.Logging;

namespace FFmpegDotnetWrapper.Services;

/// <summary>
/// Streams real-time <see cref="FFmpegProgressUpdate"/> snapshots from a running FFmpeg process.
/// Reads FFmpeg's stderr line by line, parses progress tokens, and calculates percentage and ETA.
/// </summary>
public interface IStreamingProgressService
{
    /// <summary>
    /// Asynchronously streams progress updates from the provided FFmpeg process until it exits
    /// or the <paramref name="cancellationToken"/> is signalled.
    /// </summary>
    /// <param name="operationId">Identifier propagated to every emitted <see cref="FFmpegProgressUpdate"/>.</param>
    /// <param name="ffmpegProcess">
    /// A started <see cref="Process"/> with <see cref="ProcessStartInfo.RedirectStandardError"/> set to <c>true</c>.
    /// </param>
    /// <param name="totalDuration">
    /// Total media duration of the source file, used to compute percentage completion and ETA.
    /// Pass <see cref="TimeSpan.Zero"/> when the duration is unknown; percentage will be reported as 0.
    /// </param>
    /// <param name="cancellationToken">Token to cancel streaming before the process finishes.</param>
    /// <returns>An async stream of <see cref="FFmpegProgressUpdate"/> snapshots, one per FFmpeg progress line.</returns>
    IAsyncEnumerable<FFmpegProgressUpdate> StreamProgressAsync(
        string operationId,
        Process ffmpegProcess,
        TimeSpan totalDuration,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Default implementation of <see cref="IStreamingProgressService"/>.
/// Parses the structured progress lines FFmpeg writes to stderr and yields typed update objects.
/// All per-call state is stack-local, making this implementation safe for concurrent operations.
/// </summary>
public sealed class StreamingProgressService : IStreamingProgressService
{
    // FFmpeg stderr progress line example:
    // frame=  150 fps= 30 q=28.0 size=    1024kB time=00:00:05.00 bitrate=1677.7kbits/s speed=2.00x
    private static readonly Regex FrameRegex   = new(@"frame=\s*(\d+)",              RegexOptions.Compiled);
    private static readonly Regex FpsRegex     = new(@"fps=\s*([\d.]+)",             RegexOptions.Compiled);
    private static readonly Regex SizeRegex    = new(@"size=\s*(\d+)kB",            RegexOptions.Compiled);
    private static readonly Regex TimeRegex    = new(@"time=(\d{2}:\d{2}:\d{2}\.?\d*)", RegexOptions.Compiled);
    private static readonly Regex BitrateRegex = new(@"bitrate=\s*([\d.]+)kbits/s", RegexOptions.Compiled);
    private static readonly Regex SpeedRegex   = new(@"speed=\s*([\d.]+)x",         RegexOptions.Compiled);

    private readonly ILogger<StreamingProgressService> _logger;

    /// <summary>
    /// Initialises a new <see cref="StreamingProgressService"/> with the required logger.
    /// </summary>
    /// <param name="logger">Logger for debug and diagnostic output.</param>
    public StreamingProgressService(ILogger<StreamingProgressService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<FFmpegProgressUpdate> StreamProgressAsync(
        string operationId,
        Process ffmpegProcess,
        TimeSpan totalDuration,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (ffmpegProcess == null)
            throw new ArgumentNullException(nameof(ffmpegProcess));
        if (string.IsNullOrWhiteSpace(operationId))
            throw new ArgumentException("Operation ID must not be empty.", nameof(operationId));

        var stopwatch = Stopwatch.StartNew();

        _logger.LogDebug(
            "Starting progress stream for operation {OperationId}, total duration {Duration}",
            operationId, totalDuration);

        while (!cancellationToken.IsCancellationRequested)
        {
            string? line;
            try
            {
                line = await ffmpegProcess.StandardError.ReadLineAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            if (line == null)
                break;

            var update = TryParseProgressLine(line, operationId, totalDuration, stopwatch.Elapsed);
            if (update == null)
                continue;

            _logger.LogDebug(
                "Progress update for {OperationId}: {Percentage:F1}%",
                operationId, update.ProgressPercentage);

            yield return update;
        }

        stopwatch.Stop();
        _logger.LogDebug("Progress stream ended for operation {OperationId}", operationId);
    }

    private static FFmpegProgressUpdate? TryParseProgressLine(
        string line,
        string operationId,
        TimeSpan totalDuration,
        TimeSpan elapsed)
    {
        // Progress lines always contain "time="; skip informational/error output.
        if (!line.Contains("time=", StringComparison.Ordinal))
            return null;

        var processedDuration = ParseTimecode(line);
        if (processedDuration == TimeSpan.Zero)
            return null;

        var progressPercent = totalDuration.TotalSeconds > 0
            ? Math.Clamp(processedDuration.TotalSeconds / totalDuration.TotalSeconds * 100.0, 0.0, 100.0)
            : 0.0;

        return new FFmpegProgressUpdate
        {
            OperationId           = operationId,
            ProgressPercentage    = progressPercent,
            ProcessedDuration     = processedDuration,
            TotalDuration         = totalDuration,
            EstimatedTimeRemaining = CalculateEta(processedDuration, totalDuration, elapsed),
            ElapsedWallTime       = elapsed,
            FramesProcessed       = ParseInt(FrameRegex, line),
            FramesPerSecond       = ParseDouble(FpsRegex, line),
            OutputSizeBytes       = ParseLong(SizeRegex, line) * 1024L,
            BitrateKbps           = ParseDouble(BitrateRegex, line),
            EncodingSpeed         = ParseDouble(SpeedRegex, line),
            Timestamp             = DateTime.UtcNow,
            RawOutput             = line
        };
    }

    /// <summary>
    /// Estimates remaining wall-clock time using the observed encoding throughput ratio.
    /// </summary>
    private static TimeSpan CalculateEta(TimeSpan processed, TimeSpan total, TimeSpan elapsed)
    {
        if (processed.TotalSeconds <= 0 || total.TotalSeconds <= 0 || elapsed.TotalSeconds <= 0)
            return TimeSpan.Zero;

        var remaining = total - processed;
        if (remaining <= TimeSpan.Zero)
            return TimeSpan.Zero;

        // throughput = media-seconds encoded per wall-clock second
        var throughput = processed.TotalSeconds / elapsed.TotalSeconds;
        return TimeSpan.FromSeconds(remaining.TotalSeconds / throughput);
    }

    private static TimeSpan ParseTimecode(string line)
    {
        var match = TimeRegex.Match(line);
        if (!match.Success)
            return TimeSpan.Zero;

        return TimeSpan.TryParse(match.Groups[1].Value, CultureInfo.InvariantCulture, out var result)
            ? result
            : TimeSpan.Zero;
    }

    private static int ParseInt(Regex regex, string line)
    {
        var match = regex.Match(line);
        return match.Success && int.TryParse(match.Groups[1].Value, out var value) ? value : 0;
    }

    private static long ParseLong(Regex regex, string line)
    {
        var match = regex.Match(line);
        return match.Success && long.TryParse(match.Groups[1].Value, out var value) ? value : 0L;
    }

    private static double ParseDouble(Regex regex, string line)
    {
        var match = regex.Match(line);
        return match.Success
            && double.TryParse(match.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0.0;
    }
}
