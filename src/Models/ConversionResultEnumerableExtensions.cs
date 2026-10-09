using System;
using System.Collections.Generic;

namespace FFmpegDotnetWrapper.Models;

/// <summary>
/// Provides aggregate operations for sequences of <see cref="ConversionResult"/> instances.
/// </summary>
public static class ConversionResultEnumerableExtensions
{
    /// <summary>
    /// Calculates the percentage of conversion results that completed successfully.
    /// </summary>
    /// <param name="results">The conversion results to evaluate.</param>
    /// <returns>
    /// The percentage of results whose <see cref="ConversionResult.IsSuccess"/> property is
    /// <see langword="true"/>, or <c>0</c> when the sequence is empty.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="results"/> is <see langword="null"/>.
    /// </exception>
    public static double GetSuccessRate(this IEnumerable<ConversionResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        var totalCount = 0;
        var successfulCount = 0;

        foreach (var result in results)
        {
            totalCount++;
            if (result.IsSuccess)
            {
                successfulCount++;
            }
        }

        return totalCount == 0 ? 0 : successfulCount * 100.0 / totalCount;
    }

    /// <summary>
    /// Counts the conversion results that did not complete successfully.
    /// </summary>
    /// <param name="results">The conversion results to evaluate.</param>
    /// <returns>
    /// The number of results whose <see cref="ConversionResult.IsSuccess"/> property is
    /// <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="results"/> is <see langword="null"/>.
    /// </exception>
    public static int CountFailed(this IEnumerable<ConversionResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        var failedCount = 0;

        foreach (var result in results)
        {
            if (!result.IsSuccess)
            {
                failedCount++;
            }
        }

        return failedCount;
    }

    /// <summary>
    /// Calculates the combined size, in bytes, of the output files of successful conversions.
    /// </summary>
    /// <param name="results">The conversion results to evaluate.</param>
    /// <returns>
    /// The sum of <see cref="MediaFile.FileSize"/> for each successful result that has
    /// <see cref="ConversionResult.OutputMedia"/> populated, or <c>0</c> when none qualify.
    /// Failed results and results without output metadata are skipped.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="results"/> is <see langword="null"/>.
    /// </exception>
    public static long GetTotalOutputSizeBytes(this IEnumerable<ConversionResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        long totalBytes = 0;

        foreach (var result in results)
        {
            if (result.IsSuccess && result.OutputMedia != null)
            {
                totalBytes += result.OutputMedia.FileSize;
            }
        }

        return totalBytes;
    }
}
