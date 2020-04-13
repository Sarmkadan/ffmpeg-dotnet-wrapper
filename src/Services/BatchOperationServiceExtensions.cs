// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Extension methods for BatchOperationService providing additional batch operation utilities
// =====================================================================

using System.Collections.Concurrent;
using FFmpegDotnetWrapper.Models;
using Microsoft.Extensions.Logging;

namespace FFmpegDotnetWrapper.Services;

/// <summary>
/// Service for handling batch operations and concurrent processing.
/// </summary>
public class BatchOperationService
{
    /// <summary>
    /// Initializes a new instance of the BatchOperationService.
    /// </summary>
    public BatchOperationService()
    {
    }
}

/// <summary>
/// Result of batch operation processing.
/// </summary>
public class BatchOperationResult
{
    public string OperationType { get; set; } = string.Empty;
    public int TotalFiles { get; set; }
    public int SuccessfulCount { get; set; }
    public int FailedCount { get; set; }
    public bool IsCancelled { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public List<ConversionResult> Results { get; set; } = new();

    public TimeSpan GetDuration() => CompletedAt.HasValue ? CompletedAt.Value - CreatedAt : TimeSpan.Zero;
    public double GetSuccessRate() => TotalFiles > 0 ? (SuccessfulCount / (double)TotalFiles) * 100 : 0;
}

/// <summary>
/// Result of batch analysis processing.
/// </summary>
public class BatchAnalysisResult
{
    public int TotalFiles { get; set; }
    public bool IsCancelled { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public List<MediaFile> AnalyzedFiles { get; set; } = new();

    public TimeSpan GetDuration() => CompletedAt.HasValue ? CompletedAt.Value - CreatedAt : TimeSpan.Zero;
}

/// <summary>
/// Extension methods for <see cref="BatchOperationService"/> providing additional batch operation utilities.
/// </summary>
/// <remarks>
/// This static class provides utility methods for filtering, analyzing, and reporting on batch operation results.
/// All methods validate their inputs and throw appropriate exceptions for invalid arguments.
/// </remarks>
public static class BatchOperationServiceExtensions
{
    /// <summary>
    /// Filters successful conversions from a batch operation result.
    /// </summary>
    /// <param name="service">The batch operation service instance</param>
    /// <param name="result">The batch operation result to filter</param>
    /// <returns>List of successful conversion results</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/></exception>
    public static List<ConversionResult> GetSuccessfulConversions(this BatchOperationService service, BatchOperationResult result)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(result);

        return result.Results.Where(r => r.IsSuccess).ToList();
    }

    /// <summary>
    /// Filters failed conversions from a batch operation result.
    /// </summary>
    /// <param name="service">The batch operation service instance</param>
    /// <param name="result">The batch operation result to filter</param>
    /// <returns>List of failed conversion results</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/></exception>
    public static List<ConversionResult> GetFailedConversions(this BatchOperationService service, BatchOperationResult result)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(result);

        return result.Results.Where(r => !r.IsSuccess).ToList();
    }

    /// <summary>
    /// Gets the total duration of all successful conversions in the batch.
    /// </summary>
    /// <param name="service">The batch operation service instance</param>
    /// <param name="result">The batch operation result</param>
    /// <returns>Total duration of all successful conversions</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/></exception>
    public static TimeSpan GetTotalDuration(this BatchOperationService service, BatchOperationResult result)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(result);

        if (!result.Results.Any())
        {
            return TimeSpan.Zero;
        }

        var totalTicks = result.Results
            .Where(r => r.IsSuccess)
            .Sum(r => r.Duration.Ticks);

        return new TimeSpan(totalTicks);
    }

    /// <summary>
    /// Gets the average duration of successful conversions in the batch.
    /// </summary>
    /// <param name="service">The batch operation service instance</param>
    /// <param name="result">The batch operation result</param>
    /// <returns>Average duration of successful conversions, or TimeSpan.Zero if no successful conversions</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/></exception>
    public static TimeSpan GetAverageDuration(this BatchOperationService service, BatchOperationResult result)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(result);

        var successfulDurations = result.Results
            .Where(r => r.IsSuccess)
            .Select(r => r.Duration)
            .ToList();

        if (successfulDurations.Count == 0)
        {
            return TimeSpan.Zero;
        }

        var totalTicks = successfulDurations.Sum(d => d.Ticks);
        var averageTicks = totalTicks / successfulDurations.Count;
        return new TimeSpan(averageTicks);
    }

    /// <summary>
    /// Creates a summary report of the batch operation results.
    /// </summary>
    /// <param name="service">The batch operation service instance</param>
    /// <param name="result">The batch operation result</param>
    /// <returns>Formatted string summary of the batch operation</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/></exception>
    public static string CreateSummaryReport(this BatchOperationService service, BatchOperationResult result)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(result);

        var duration = result.GetDuration();
        var successRate = result.GetSuccessRate();
        var totalDuration = service.GetTotalDuration(result);
        var averageDuration = service.GetAverageDuration(result);

        return $"Batch Operation Summary Report\n" +
               "================================\n" +
               $"Operation Type: {result.OperationType}\n" +
               $"Total Files: {result.TotalFiles}\n" +
               $"Successful: {result.SuccessfulCount}\n" +
               $"Failed: {result.FailedCount}\n" +
               $"Success Rate: {successRate:F2}%\n" +
               $"Duration: {duration}\n" +
               $"Total Processing Time: {totalDuration}\n" +
               $"Average Processing Time: {averageDuration}\n" +
               $"Cancelled: {result.IsCancelled}\n" +
               $"Created: {result.CreatedAt:yyyy-MM-dd HH:mm:ss}\n" +
               $"Completed: {(result.CompletedAt.HasValue ? result.CompletedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : "N/A")}\n" +
               $"Results Count: {result.Results?.Count ?? 0}\n";
    }

    /// <summary>
    /// Gets the largest file size from successful conversions.
    /// </summary>
    /// <param name="service">The batch operation service instance</param>
    /// <param name="result">The batch operation result</param>
    /// <returns>Size in bytes of the largest file, or 0 if no successful conversions</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/></exception>
    public static long GetLargestFileSize(this BatchOperationService service, BatchOperationResult result)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(result);

        if (!result.Results.Any(r => r.IsSuccess && r.OutputMedia != null))
        {
            return 0;
        }

        return result.Results
            .Where(r => r.IsSuccess && r.OutputMedia != null)
            .Max(r => r.OutputMedia!.FileSize);
    }

    /// <summary>
    /// Gets the smallest file size from successful conversions.
    /// </summary>
    /// <param name="service">The batch operation service instance</param>
    /// <param name="result">The batch operation result</param>
    /// <returns>Size in bytes of the smallest file, or 0 if no successful conversions</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/></exception>
    public static long GetSmallestFileSize(this BatchOperationService service, BatchOperationResult result)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(result);

        if (!result.Results.Any(r => r.IsSuccess && r.OutputMedia != null))
        {
            return 0;
        }

        return result.Results
            .Where(r => r.IsSuccess && r.OutputMedia != null)
            .Min(r => r.OutputMedia!.FileSize);
    }

    /// <summary>
    /// Gets the average file size from successful conversions.
    /// </summary>
    /// <param name="service">The batch operation service instance</param>
    /// <param name="result">The batch operation result</param>
    /// <returns>Average size in bytes of successful conversions, or 0 if no successful conversions</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/></exception>
    public static long GetAverageFileSize(this BatchOperationService service, BatchOperationResult result)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(result);

        var successfulSizes = result.Results
            .Where(r => r.IsSuccess && r.OutputMedia != null)
            .Select(r => r.OutputMedia!.FileSize)
            .ToList();

        return successfulSizes.Count == 0
            ? 0
            : (long)successfulSizes.Average();
    }

    /// <summary>
    /// Gets the batch operation completion percentage.
    /// </summary>
    /// <param name="service">The batch operation service instance</param>
    /// <param name="result">The batch operation result</param>
    /// <returns>Completion percentage (0-100)</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/></exception>
    public static double GetCompletionPercentage(this BatchOperationService service, BatchOperationResult result)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(result);

        return result.TotalFiles == 0
            ? 0
            : (result.SuccessfulCount + result.FailedCount) / (double)result.TotalFiles * 100;
    }

    /// <summary>
    /// Checks if all conversions in the batch were successful.
    /// </summary>
    /// <param name="service">The batch operation service instance</param>
    /// <param name="result">The batch operation result</param>
    /// <returns>True if all conversions succeeded, false otherwise</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/></exception>
    public static bool AllSuccessful(this BatchOperationService service, BatchOperationResult result)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(result);

        return result.TotalFiles > 0 && result.FailedCount == 0;
    }

    /// <summary>
    /// Checks if any conversions in the batch failed.
    /// </summary>
    /// <param name="service">The batch operation service instance</param>
    /// <param name="result">The batch operation result</param>
    /// <returns>True if any conversions failed, false otherwise</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/></exception>
    public static bool AnyFailed(this BatchOperationService service, BatchOperationResult result)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(result);

        return result.FailedCount > 0;
    }
}