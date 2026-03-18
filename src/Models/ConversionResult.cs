// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace FFmpegDotnetWrapper.Models;

/// <summary>
/// Represents the result of a conversion operation.
/// </summary>
public class ConversionResult
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public bool IsSuccess { get; set; }
    public string? OutputFilePath { get; set; }
    public MediaFile? OutputMedia { get; set; }
    public TimeSpan Duration { get; set; }
    public string? ErrorMessage { get; set; }
    public string? WarningMessage { get; set; }
    public Dictionary<string, object> Metrics { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime CompletedAt { get; set; }
    public string? FFmpegOutput { get; set; }

    /// <summary>
    /// Gets the elapsed time for the operation.
    /// </summary>
    public TimeSpan GetElapsedTime() => Duration;

    /// <summary>
    /// Gets the size reduction percentage if output file exists.
    /// </summary>
    public double? GetSizeReductionPercentage(long originalSize)
    {
        if (!IsSuccess || OutputMedia == null)
            return null;

        if (originalSize <= 0)
            return null;

        return ((originalSize - OutputMedia.FileSize) / (double)originalSize) * 100;
    }

    /// <summary>
    /// Sets a metric for the operation result.
    /// </summary>
    public void SetMetric(string key, object value) => Metrics[key] = value;

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
    public void MarkAsSuccess(string outputPath)
    {
        IsSuccess = true;
        OutputFilePath = outputPath;
        CompletedAt = DateTime.UtcNow;
        ErrorMessage = null;
    }

    /// <summary>
    /// Marks the result as failed.
    /// </summary>
    public void MarkAsFailed(string errorMessage)
    {
        IsSuccess = false;
        ErrorMessage = errorMessage;
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

        if (!string.IsNullOrEmpty(OutputFilePath))
            summary.AppendLine($"Output: {OutputFilePath}");

        if (OutputMedia != null)
            summary.AppendLine($"Output Size: {OutputMedia.GetFileSizeInMegabytes()} MB");

        if (Metrics.Any())
        {
            summary.AppendLine("\nMetrics:");
            foreach (var metric in Metrics)
                summary.AppendLine($"  {metric.Key}: {metric.Value}");
        }

        if (!string.IsNullOrEmpty(ErrorMessage))
            summary.AppendLine($"\nError: {ErrorMessage}");

        if (!string.IsNullOrEmpty(WarningMessage))
            summary.AppendLine($"Warning: {WarningMessage}");

        return summary.ToString();
    }
}
