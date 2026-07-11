// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using FFmpegDotnetWrapper.Models;

namespace FFmpegDotnetWrapper.Services;

/// <summary>
/// Provides validation helpers for <see cref="IFFmpegService"/> instances.
/// </summary>
public static class FFmpegServiceValidation
{
    /// <summary>
    /// Validates the specified <see cref="IFFmpegService"/> instance.
    /// </summary>
    /// <param name="value">The FFmpegService instance to validate.</param>
    /// <returns>An IReadOnlyList of validation problems, or empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public static IReadOnlyList<string> Validate(this IFFmpegService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate injected dependencies (these would be validated by DI container in real usage)
        // For this static validation class, we focus on the FFmpegService instance itself
        // which doesn't have any state to validate beyond what's injected

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="IFFmpegService"/> instance is valid.
    /// </summary>
    /// <param name="value">The FFmpegService instance to check.</param>
    /// <returns>True if the instance is valid; otherwise, false.</returns>
    public static bool IsValid(this IFFmpegService? value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="IFFmpegService"/> instance is valid.
    /// </summary>
    /// <param name="value">The FFmpegService instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    /// <exception cref="ArgumentException">Thrown when value is not valid, containing the validation problems.</exception>
    public static void EnsureValid(this IFFmpegService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"FFmpegService validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }
}