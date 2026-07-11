// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;

namespace FFmpegDotnetWrapper.Models;

/// <summary>
/// Provides validation helpers for <see cref="SubtitleSettings"/> instances.
/// </summary>
public static class SubtitleSettingsValidation
{
    /// <summary>
    /// Validates the specified subtitle settings and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The subtitle settings to validate. Must not be null.</param>
    /// <returns>An empty list if the settings are valid; otherwise, a list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this SubtitleSettings value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate FontSize range (6-120 as per SubtitleSettings.FontSize property)
        if (value.FontSize < 6 || value.FontSize > 120)
        {
            errors.Add($"FontSize must be between 6 and 120, but was {value.FontSize}.");
        }

        // Validate SubtitleStreamIndex (must be non-negative as per SubtitleSettings.SubtitleStreamIndex property)
        if (value.SubtitleStreamIndex < 0)
        {
            errors.Add($"SubtitleStreamIndex must be non-negative, but was {value.SubtitleStreamIndex}.");
        }

        // Validate FontName (if set, should not be whitespace-only)
        if (value.FontName is not null && string.IsNullOrWhiteSpace(value.FontName))
        {
            errors.Add("FontName cannot be whitespace.");
        }

        // Validate Language (if set, should be a valid language code format)
        if (value.Language is not null)
        {
            if (string.IsNullOrWhiteSpace(value.Language))
            {
                errors.Add("Language cannot be whitespace.");
            }
            else if (value.Language.Length > 10) // Reasonable limit for language codes
            {
                errors.Add("Language code is too long.");
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified subtitle settings are valid.
    /// </summary>
    /// <param name="value">The subtitle settings to check. Must not be null.</param>
    /// <returns><c>true</c> if the settings are valid; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this SubtitleSettings value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Validates the specified subtitle settings and throws an <see cref="ArgumentException"/>
    /// if any validation errors are found.
    /// </summary>
    /// <param name="value">The subtitle settings to validate. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing a list of all validation errors.</exception>
    public static void EnsureValid(this SubtitleSettings value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"SubtitleSettings validation failed with {errors.Count} error(s):{Environment.NewLine}" +
            string.Join(Environment.NewLine, errors.Select((error, index) => $"  {index + 1}. {error}")));
    }
}