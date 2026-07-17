// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics.CodeAnalysis;

namespace FFmpegDotnetWrapper.Services;

/// <summary>
/// Provides validation helpers for <see cref="IFFmpegService"/> instances.
/// Validates that FFmpeg service instances are properly initialized and not null.
/// </summary>
[SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Null checks via ArgumentNullException.ThrowIfNull")]
public static class FFmpegServiceValidation
{
    /// <summary>
    /// Validates the specified <see cref="IFFmpegService"/> instance.
    /// </summary>
    /// <remarks>
    /// This validation method checks that the FFmpeg service instance is not null.
    /// The actual FFmpeg executable availability is validated at runtime when operations are executed,
    /// not during service instantiation, as FFmpeg may be conditionally available in different environments.
    /// </remarks>
    /// <param name="value">The FFmpegService instance to validate.</param>
    /// <returns>An empty <see cref="IReadOnlyList{T}"/> if the instance is valid; otherwise, a list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate([NotNull] this IFFmpegService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the specified <see cref="IFFmpegService"/> instance is valid.
    /// </summary>
    /// <param name="value">The FFmpegService instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid (not null); otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid([NotNullWhen(true)] this IFFmpegService? value)
    {
        return value is not null;
    }

    /// <summary>
    /// Ensures that the specified <see cref="IFFmpegService"/> instance is valid.
    /// </summary>
    /// <param name="value">The FFmpegService instance to validate.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="value"/> is <see langword="null"/>, containing a validation error message.
    /// </exception>
    public static void EnsureValid([NotNull] this IFFmpegService? value)
    {
        ArgumentNullException.ThrowIfNull(value);
    }
}