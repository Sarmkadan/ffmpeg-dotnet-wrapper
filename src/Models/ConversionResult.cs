// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace FFmpegDotnetWrapper.Models;

/// <summary>
/// Represents the result of a conversion operation.
///
/// <para>This class follows a failure-semantics contract where:</para>
/// <list type="bullet">
/// <item><description>Successful operations set <see cref="IsSuccess"/> to true and populate <see cref="OutputFilePath"/> and <see cref="OutputMedia"/></description></item>
/// <item><description>Failed operations set <see cref="IsSuccess"/> to false and populate <see cref="ErrorMessage"/>, <see cref="ErrorOutput"/>, and <see cref="ExitCode"/></description></item>
/// </list>
///
/// <para>Throw exceptions only for programming/configuration errors (invalid arguments,
/// file not found, etc.). For expected FFmpeg failures (unsupported codec, invalid format,
/// etc.), return a failed <see cref="ConversionResult"/> with appropriate error details.</para>
///
/// <para>Expected FFmpeg errors include:</para>
/// <list type="bullet">
/// <item><description>Unsupported codec combinations</description></item>
/// <item><description>Invalid container formats</description></item>
/// <item><description>Missing input files</description></item>
/// <item><description>Permission issues on output paths</description></item>
/// <item><description>Resource limitations (out of memory, disk space, etc.)</description></item>
/// </list>
/// </summary>
public class ConversionResult
{
    /// <summary>
    /// Unique identifier for this conversion operation.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Indicates whether the conversion operation succeeded.
    /// <para><c>true</c> if the operation completed successfully and produced valid output.</para>
    /// <para><c>false</c> if the operation failed, with details in <see cref="ErrorMessage"/>,
    /// <see cref="ErrorOutput"/> , and <see cref="ExitCode"/>.</para>
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Path to the output file, if the operation produced one.
    /// </summary>
    public string? OutputFilePath { get; set; }

    /// <summary>
    /// Media file metadata for the output, if available.
    /// </summary>
    public MediaFile? OutputMedia { get; set; }

    /// <summary>
    /// Duration of the conversion operation.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Human-readable error message describing the failure reason.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// FFmpeg exit code from the process execution.
    /// <para>0 indicates success.</para>
    /// <para>Non-zero values indicate specific failure modes (consult FFmpeg documentation).</para>
    /// </summary>
    public int ExitCode { get; set; } = 0;

    /// <summary>
    /// Tail of the FFmpeg stderr output (last 10 lines) for diagnostic purposes.
    /// Contains the most relevant error messages without overwhelming the caller with full output.
    /// </summary>
    public string? ErrorOutput { get; set; }

    /// <summary>
    /// Warning messages from the conversion process.
    /// </summary>
    public string? WarningMessage { get; set; }

    /// <summary>
    /// Full FFmpeg output including both stdout and stderr.
    /// </summary>
    public string? FFmpegOutput { get; set; }

    /// <summary>
    /// Metrics collected during the conversion operation.
    /// </summary>
    public Dictionary<string, object> Metrics { get; set; } = new();

    /// <summary>
    /// Timestamp when the conversion was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the conversion completed.
    /// </summary>
    public DateTime CompletedAt { get; set; }

    /// <summary>
    /// Gets the elapsed time for the operation.
    /// </summary>
    public TimeSpan GetElapsedTime() => Duration;

    /// <summary>
    /// Gets the size reduction percentage if output file exists and operation was successful.
    /// </summary>
    /// <param name="originalSize">Size of the original file in bytes.</param>
    /// <returns>Percentage reduction, or null if not applicable.</returns>
    public double? GetSizeReductionPercentage(long originalSize)
    {
        if (!IsSuccess || ExitCode != 0 || OutputMedia == null)
            return null;

        if (originalSize <= 0)
            return null;

        return ((originalSize - OutputMedia.FileSize) / (double)originalSize) * 100;
    }

    /// <summary>
    /// Sets a metric for the operation result.
    /// </summary>
    public void SetMetric(string key, object value)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(value);
        Metrics[key] = value;
    }

    /// <summary>
    /// Gets a metric value if it exists.
    /// </summary>
    public T? GetMetric<T>(string key)
    {
        if (Metrics.TryGetValue(key, out var value))
        {
            if (value is T typed)
                return typed;
        }

        return default;
    }

    /// <summary>
    /// Marks the result as successful.
    /// </summary>
    /// <param name="outputPath">Path to the output file.</param>
    /// <param name="exitCode">FFmpeg exit code (0 for success).</param>
    public void MarkAsSuccess(string outputPath, int exitCode = 0)
    {
        IsSuccess = true;
        ExitCode = exitCode;
        OutputFilePath = outputPath;
        CompletedAt = DateTime.UtcNow;
        ErrorMessage = null;
        ErrorOutput = null;
    }

    /// <summary>
    /// Marks the result as failed.
    /// </summary>
    /// <param name="errorMessage">Human-readable error message.</param>
    /// <param name="exitCode">FFmpeg exit code from the failed process.</param>
    /// <param name="errorOutput">Tail of stderr output for diagnostic purposes.</param>
    public void MarkAsFailed(string errorMessage, int exitCode = 1, string? errorOutput = null)
    {
        IsSuccess = false;
        ErrorMessage = errorMessage;
        ExitCode = exitCode;
        ErrorOutput = errorOutput;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Generates a summary report of the conversion.
    /// </summary>
    public string GenerateSummary()
    {
        var summary = new System.Text.StringBuilder();
        summary.AppendLine($"Conversion ID: {Id}");
        summary.AppendLine($"Status: {(IsSuccess ? "Success" : "Failed")}");
        summary.AppendLine($"Duration: {Duration.TotalSeconds:F2} seconds");
        summary.AppendLine($"Exit Code: {ExitCode}");

        if (!string.IsNullOrEmpty(OutputFilePath))
            summary.AppendLine($"Output: {OutputFilePath}");

        if (OutputMedia != null)
            summary.AppendLine($"Output Size: {OutputMedia.GetFileSizeInMegabytes()} MB");

        if (Metrics.Any())
        {
            summary.AppendLine("\nMetrics:");
            foreach (var metric in Metrics)
                summary.AppendLine($" {metric.Key}: {metric.Value}");
        }

        if (!string.IsNullOrEmpty(ErrorMessage))
            summary.AppendLine($"\nError: {ErrorMessage}");

        if (!string.IsNullOrEmpty(ErrorOutput))
            summary.AppendLine($"Error Output: {ErrorOutput}");

        if (!string.IsNullOrEmpty(WarningMessage))
            summary.AppendLine($"Warning: {WarningMessage}");

        return summary.ToString();
    }
}