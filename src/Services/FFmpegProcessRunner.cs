// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

#nullable enable

using System.Diagnostics;
using System.Globalization;
using FFmpegDotnetWrapper.Models;

namespace FFmpegDotnetWrapper.Services;

/// <summary>
/// <see cref="IFFmpegProcessRunner"/> implementation that launches the real <c>ffmpeg</c>
/// executable via <see cref="Process"/>. Centralizes stream draining, bounded stderr capture,
/// <c>-progress pipe:1</c> parsing, and graceful shutdown on cancellation.
/// </summary>
public sealed class FFmpegProcessRunner : IFFmpegProcessRunner
{
    /// <summary>
    /// Maximum number of characters of stderr retained for diagnostics on long-running processes.
    /// </summary>
    private const int MaxRetainedStderrChars = 64 * 1024;

    /// <summary>
    /// How long to wait, after sending <c>q</c> to ffmpeg's standard input on cancellation, before
    /// forcibly killing the process tree. Gives ffmpeg time to finalize output (e.g. write the
    /// trailing <c>moov</c> atom of an MP4) instead of leaving a corrupt file behind.
    /// </summary>
    private static readonly TimeSpan GracefulShutdownGracePeriod = TimeSpan.FromSeconds(5);

    /// <inheritdoc/>
    public async Task<FFmpegProcessResult> RunAsync(
        FFmpegProcessRequest request,
        IProgress<FFmpegProgressUpdate>? progress,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var startInfo = new ProcessStartInfo
        {
            FileName = request.FileName,
            Arguments = request.Arguments,
            WorkingDirectory = request.WorkingDirectory ?? Directory.GetCurrentDirectory(),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        var stopwatch = Stopwatch.StartNew();
        var stderrBuffer = new System.Text.StringBuilder(capacity: 4096);
        var stderrLock = new object();

        using var timeoutCts = new CancellationTokenSource();
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
        if (request.Timeout is { } timeout)
            timeoutCts.CancelAfter(timeout);

        process.Start();

        var stdErrTask = DrainStderrBoundedAsync(process, stderrBuffer, stderrLock);
        var stdOutTask = request.ParseProgressFromStdOut
            ? StreamProgressFromStdOutAsync(process, request.OperationId, request.TotalDuration, stopwatch, progress)
            : DrainStdOutAsync(process);

        try
        {
            await process.WaitForExitAsync(linkedCts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            await ShutDownGracefullyAsync(process).ConfigureAwait(false);
        }

        await Task.WhenAll(stdErrTask, stdOutTask).ConfigureAwait(false);
        stopwatch.Stop();

        string stderrTail;
        lock (stderrLock)
            stderrTail = stderrBuffer.ToString();

        var timedOut = timeoutCts.IsCancellationRequested;
        var wasCancelled = !timedOut && cancellationToken.IsCancellationRequested;

        return new FFmpegProcessResult
        {
            ExitCode = process.HasExited ? process.ExitCode : -1,
            StdErrTail = stderrTail,
            ExecutionTime = stopwatch.Elapsed,
            TimedOut = timedOut,
            WasCancelled = wasCancelled
        };
    }

    /// <summary>
    /// Attempts a graceful shutdown by sending <c>q</c> on the process' standard input (the
    /// interactive-quit key ffmpeg listens for), giving it <see cref="GracefulShutdownGracePeriod"/>
    /// to finish finalizing its output, and only then killing the whole process tree.
    /// </summary>
    private static async Task ShutDownGracefullyAsync(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                await process.StandardInput.WriteAsync("q").ConfigureAwait(false);
                await process.StandardInput.FlushAsync().ConfigureAwait(false);
            }
        }
        catch
        {
            // Standard input may already be closed or the process may have exited concurrently.
        }

        try
        {
            using var graceCts = new CancellationTokenSource(GracefulShutdownGracePeriod);
            await process.WaitForExitAsync(graceCts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            try
            {
                if (!process.HasExited)
                    process.Kill(entireProcessTree: true);
            }
            catch
            {
                // Process already exited between the check and the kill call.
            }
        }
    }

    /// <summary>
    /// Reads standard error line by line for the lifetime of the process, retaining only the last
    /// <see cref="MaxRetainedStderrChars"/> characters.
    /// </summary>
    private static async Task DrainStderrBoundedAsync(
        Process process,
        System.Text.StringBuilder buffer,
        object bufferLock)
    {
        while (true)
        {
            string? line;
            try
            {
                line = await process.StandardError.ReadLineAsync().ConfigureAwait(false);
            }
            catch
            {
                break;
            }

            if (line is null)
                break;

            lock (bufferLock)
            {
                buffer.Append(line).Append('\n');
                if (buffer.Length > MaxRetainedStderrChars)
                    buffer.Remove(0, buffer.Length - MaxRetainedStderrChars);
            }
        }
    }

    /// <summary>
    /// Drains standard output without interpreting it, purely to prevent the pipe buffer from
    /// filling up and deadlocking the process when progress parsing is not requested.
    /// </summary>
    private static async Task DrainStdOutAsync(Process process)
    {
        try
        {
            await process.StandardOutput.ReadToEndAsync().ConfigureAwait(false);
        }
        catch
        {
            // Ignore; the process may have already exited or been killed.
        }
    }

    /// <summary>
    /// Reads ffmpeg's <c>-progress pipe:1</c> stdout stream line by line, accumulating one
    /// <c>key=value</c> block at a time and emitting a parsed <see cref="FFmpegProgressUpdate"/>
    /// whenever a <c>progress=continue</c>/<c>progress=end</c> terminator line is seen.
    /// </summary>
    private static async Task StreamProgressFromStdOutAsync(
        Process process,
        string operationId,
        TimeSpan totalDuration,
        Stopwatch stopwatch,
        IProgress<FFmpegProgressUpdate>? progress)
    {
        var fields = new Dictionary<string, string>(StringComparer.Ordinal);

        while (true)
        {
            string? line;
            try
            {
                line = await process.StandardOutput.ReadLineAsync().ConfigureAwait(false);
            }
            catch
            {
                break;
            }

            if (line is null)
                break;

            var separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0)
                continue;

            var key = line[..separatorIndex];
            var value = line[(separatorIndex + 1)..].Trim();
            fields[key] = value;

            if (key != "progress")
                continue;

            progress?.Report(BuildProgressUpdate(fields, operationId, totalDuration, stopwatch.Elapsed));
            fields = new Dictionary<string, string>(StringComparer.Ordinal);

            if (value == "end")
                break;
        }
    }

    /// <summary>
    /// Builds an <see cref="FFmpegProgressUpdate"/> from a completed block of
    /// <c>-progress pipe:1</c> key/value fields.
    /// </summary>
    private static FFmpegProgressUpdate BuildProgressUpdate(
        IReadOnlyDictionary<string, string> fields,
        string operationId,
        TimeSpan totalDuration,
        TimeSpan elapsed)
    {
        var processedDuration = fields.TryGetValue("out_time_us", out var outTimeUsRaw)
            && long.TryParse(outTimeUsRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var outTimeUs)
            && outTimeUs > 0
                ? TimeSpan.FromMicroseconds(outTimeUs)
                : TimeSpan.Zero;

        var progressPercent = totalDuration.TotalSeconds > 0
            ? Math.Clamp(processedDuration.TotalSeconds / totalDuration.TotalSeconds * 100.0, 0.0, 100.0)
            : 0.0;

        var update = new FFmpegProgressUpdate
        {
            OperationId = operationId,
            ProgressPercentage = progressPercent,
            ProcessedDuration = processedDuration,
            TotalDuration = totalDuration,
            ElapsedWallTime = elapsed,
            FramesProcessed = ParseIntField(fields, "frame"),
            FramesPerSecond = ParseDoubleField(fields, "fps"),
            OutputSizeBytes = ParseLongField(fields, "total_size"),
            BitrateKbps = ParseBitrateField(fields),
            EncodingSpeed = ParseSpeedField(fields),
            Timestamp = DateTime.UtcNow,
            RawOutput = string.Join(' ', fields.Select(kv => $"{kv.Key}={kv.Value}"))
        };

        update.RecalculateEstimatedTimeRemaining();
        return update;
    }

    private static int ParseIntField(IReadOnlyDictionary<string, string> fields, string key) =>
        fields.TryGetValue(key, out var raw) && int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0;

    private static long ParseLongField(IReadOnlyDictionary<string, string> fields, string key) =>
        fields.TryGetValue(key, out var raw) && long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0L;

    private static double ParseDoubleField(IReadOnlyDictionary<string, string> fields, string key) =>
        fields.TryGetValue(key, out var raw) && double.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0.0;

    /// <summary>Parses the <c>bitrate</c> field (e.g. <c>"1234.5kbits/s"</c> or <c>"N/A"</c>) into kbps.</summary>
    private static double ParseBitrateField(IReadOnlyDictionary<string, string> fields)
    {
        if (!fields.TryGetValue("bitrate", out var raw))
            return 0.0;

        var numeric = raw.Replace("kbits/s", string.Empty, StringComparison.OrdinalIgnoreCase).Trim();
        return double.TryParse(numeric, NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : 0.0;
    }

    /// <summary>Parses the <c>speed</c> field (e.g. <c>"2.05x"</c> or <c>"N/A"</c>) into a multiplier.</summary>
    private static double ParseSpeedField(IReadOnlyDictionary<string, string> fields)
    {
        if (!fields.TryGetValue("speed", out var raw))
            return 0.0;

        var numeric = raw.TrimEnd('x', 'X');
        return double.TryParse(numeric, NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : 0.0;
    }
}
