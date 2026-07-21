// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace FFmpegDotnetWrapper.Models;

/// <summary>
/// Represents a real-time progress snapshot emitted during an active FFmpeg operation.
/// Each instance is produced by parsing a single progress line from FFmpeg's stderr output.
/// </summary>
public class FFmpegProgressUpdate
{
    /// <summary>Identifier linking this update to its parent operation.</summary>
    public string OperationId { get; set; } = string.Empty;

    /// <summary>
    /// Completion percentage in the range [0, 100].
    /// Derived from the ratio of <see cref="ProcessedDuration"/> to <see cref="TotalDuration"/>.
    /// </summary>
    public double ProgressPercentage { get; set; }

    /// <summary>Amount of media time encoded so far, parsed from FFmpeg's <c>time=</c> field.</summary>
    public TimeSpan ProcessedDuration { get; set; }

    /// <summary>Total media duration supplied by the caller, used for percentage and ETA calculations.</summary>
    public TimeSpan TotalDuration { get; set; }

    /// <summary>Estimated wall-clock time remaining until the operation completes.</summary>
    public TimeSpan EstimatedTimeRemaining { get; set; }

    /// <summary>Wall-clock time elapsed since the operation started.</summary>
    public TimeSpan ElapsedWallTime { get; set; }

    /// <summary>Number of video frames encoded, parsed from FFmpeg's <c>frame=</c> field.</summary>
    public int FramesProcessed { get; set; }

    /// <summary>Current encoding frame rate, parsed from FFmpeg's <c>fps=</c> field.</summary>
    public double FramesPerSecond { get; set; }

    /// <summary>
    /// Encoding speed relative to real-time playback.
    /// A value of <c>2.0</c> means FFmpeg encodes twice as fast as the video plays back.
    /// Parsed from FFmpeg's <c>speed=</c> field.
    /// </summary>
    public double EncodingSpeed { get; set; }

    /// <summary>Current output file size in bytes, converted from FFmpeg's <c>size=</c> field (kB).</summary>
    public long OutputSizeBytes { get; set; }

    /// <summary>Current output bitrate in kilobits per second, parsed from FFmpeg's <c>bitrate=</c> field.</summary>
    public double BitrateKbps { get; set; }

    /// <summary>UTC timestamp when this snapshot was captured.</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Raw stderr line emitted by FFmpeg for this update.
    /// Retained for debugging or custom downstream parsing.
    /// </summary>
    public string? RawOutput { get; set; }

    /// <summary>
    /// Returns a concise, human‑readable summary of the current progress state.
    /// Example: <c>42.5% | 00:00:42.5 / 00:01:40.0 | 30 fps | 2.0x | ETA 00:01:17</c>
    /// </summary>
    public override string ToString() =>
        $"{ProgressPercentage:F1}% | {ProcessedDuration:hh\\:mm\\:ss\\.f} / {TotalDuration:hh\\:mm\\:ss\\.f} " +
        $"| {FramesPerSecond:F0} fps | {EncodingSpeed:F1}x | ETA {EstimatedTimeRemaining:hh\\:mm\\:ss}";

    /// <summary>
    /// PercentComplete is the clamped version of <see cref="ProgressPercentage"/> (0‑100).
    /// </summary>
    public double PercentComplete => Math.Max(0, Math.Min(100, ProgressPercentage));

    /// <summary>
    /// Recalculates <see cref="EstimatedTimeRemaining"/> based on the processed and total durations
    /// and the elapsed wall‑clock time.
    /// Call this after updating <see cref="ProcessedDuration"/>, <see cref="TotalDuration"/> or <see cref="ElapsedWallTime"/>.
    /// </summary>
    public void RecalculateEstimatedTimeRemaining()
    {
        if (ProcessedDuration.TotalSeconds <= 0 || TotalDuration.TotalSeconds <= 0)
        {
            EstimatedTimeRemaining = TimeSpan.Zero;
            return;
        }

        // Factor = elapsed / processed duration
        var factor = ElapsedWallTime.TotalSeconds / ProcessedDuration.TotalSeconds;
        var remaining = TotalDuration - ProcessedDuration;
        var seconds = Math.Max(0, factor * remaining.TotalSeconds);
        EstimatedTimeRemaining = TimeSpan.FromSeconds(seconds);
    }
}
