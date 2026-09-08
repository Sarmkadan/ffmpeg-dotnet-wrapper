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
}
