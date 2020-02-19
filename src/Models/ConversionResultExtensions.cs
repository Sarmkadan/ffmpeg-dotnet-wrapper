// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Models;

namespace FFmpegDotnetWrapper.Models;

/// <summary>
/// Provides useful extension methods for the ConversionResult class.
/// </summary>
public static class ConversionResultExtensions
{
    /// <summary>
    /// Calculates the processing speed in frames per second.
    /// </summary>
    /// <param name="result">The conversion result to calculate speed for</param>
    /// <returns>Frames per second if frame rate and duration are available, otherwise null</returns>
    public static double? GetProcessingSpeedFps(this ConversionResult result)
    {
        if (result.Duration.TotalSeconds <= 0)
            return null;

        if (result.OutputMedia?.FrameRate == null || result.OutputMedia.FrameRate <= 0)
            return null;

        return result.OutputMedia.FrameRate;
    }

    /// <summary>
    /// Gets the output file size in megabytes.
    /// </summary>
    /// <param name="result">The conversion result</param>
    /// <returns>File size in MB if output file exists, otherwise null</returns>
    public static double? GetOutputFileSizeMb(this ConversionResult result)
    {
        if (result.OutputMedia == null)
            return null;

        return result.OutputMedia.GetFileSizeInMegabytes();
    }

    /// <summary>
    /// Determines if the conversion had any warnings.
    /// </summary>
    /// <param name="result">The conversion result</param>
    /// <returns>True if warning message is not null or empty, otherwise false</returns>
    public static bool HasWarnings(this ConversionResult result)
    {
        return !string.IsNullOrEmpty(result.WarningMessage);
    }

    /// <summary>
    /// Gets the duration formatted as a human-readable string.
    /// </summary>
    /// <param name="result">The conversion result</param>
    /// <returns>Formatted duration string (e.g., "2m 30s")</returns>
    public static string GetFormattedDuration(this ConversionResult result)
    {
        var totalSeconds = (int)result.Duration.TotalSeconds;
        var minutes = totalSeconds / 60;
        var seconds = totalSeconds % 60;

        if (minutes > 0)
            return $"{minutes}m {seconds}s";

        return $"{seconds}s";
    }

    /// <summary>
    /// Adds a performance metric to the result.
    /// </summary>
    /// <param name="result">The conversion result</param>
    /// <param name="cpuUsage">CPU usage percentage</param>
    /// <param name="memoryUsage">Memory usage in MB</param>
    public static void AddPerformanceMetrics(this ConversionResult result, double cpuUsage, double memoryUsage)
    {
        result.SetMetric("CPU_Usage_Percent", Math.Round(cpuUsage, 2));
        result.SetMetric("Memory_Usage_MB", Math.Round(memoryUsage, 2));
    }

    /// <summary>
    /// Gets the CPU usage percentage from metrics.
    /// </summary>
    /// <param name="result">The conversion result</param>
    /// <returns>CPU usage percentage if available, otherwise null</returns>
    public static double? GetCpuUsage(this ConversionResult result)
    {
        return result.GetMetric<double>("CPU_Usage_Percent");
    }

    /// <summary>
    /// Gets the memory usage in MB from metrics.
    /// </summary>
    /// <param name="result">The conversion result</param>
    /// <returns>Memory usage in MB if available, otherwise null</returns>
    public static double? GetMemoryUsageMb(this ConversionResult result)
    {
        return result.GetMetric<double>("Memory_Usage_MB");
    }

    /// <summary>
    /// Checks if the conversion was completed within a specified time threshold.
    /// </summary>
    /// <param name="result">The conversion result</param>
    /// <param name="maxDuration">Maximum allowed duration</param>
    /// <returns>True if conversion completed within threshold, otherwise false</returns>
    public static bool CompletedWithinThreshold(this ConversionResult result, TimeSpan maxDuration)
    {
        return result.Duration <= maxDuration;
    }

    /// <summary>
    /// Gets a formatted summary of the conversion metrics.
    /// </summary>
    /// <param name="result">The conversion result</param>
    /// <returns>Formatted metrics summary string</returns>
    public static string GetMetricsSummary(this ConversionResult result)
    {
        var metricsSummary = new System.Text.StringBuilder();

        if (result.Metrics.Any())
        {
            metricsSummary.AppendLine("Conversion Metrics:");
            foreach (var metric in result.Metrics)
            {
                metricsSummary.AppendLine($"  {metric.Key}: {metric.Value}");
            }
        }
        else
        {
            metricsSummary.AppendLine("No metrics available");
        }

        return metricsSummary.ToString();
    }
}