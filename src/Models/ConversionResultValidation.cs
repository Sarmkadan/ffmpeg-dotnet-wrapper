// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Diagnostics.CodeAnalysis;

namespace FFmpegDotnetWrapper.Models;

/// <summary>
/// Provides validation and verification methods for <see cref="ConversionResult"/> instances.
/// </summary>
public static class ConversionResultValidation
{
    /// <summary>
    /// Validates the conversion result and returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="value">The conversion result to validate.</param>
    /// <returns>An enumerable of validation problems; empty if the result is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ConversionResult value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate Id
        if (string.IsNullOrWhiteSpace(value.Id))
        {
            problems.Add("ConversionResult.Id must not be null, empty, or whitespace.");
        }
        else if (!Guid.TryParse(value.Id, out _))
        {
            problems.Add("ConversionResult.Id must be a valid GUID.");
        }

        // Validate IsSuccess
        // No validation needed for boolean - always valid

        // Validate OutputFilePath
        if (value.IsSuccess && string.IsNullOrWhiteSpace(value.OutputFilePath))
        {
            problems.Add("ConversionResult.OutputFilePath must not be null or empty when IsSuccess is true.");
        }
        else if (!string.IsNullOrEmpty(value.OutputFilePath) && !Path.IsPathRooted(value.OutputFilePath))
        {
            problems.Add("ConversionResult.OutputFilePath must be an absolute path when provided.");
        }

        // Validate OutputMedia
        if (value.IsSuccess && value.OutputMedia is null)
        {
            problems.Add("ConversionResult.OutputMedia must not be null when IsSuccess is true.");
        }
        else if (value.OutputMedia is not null)
        {
            // Validate MediaFile properties
            if (string.IsNullOrWhiteSpace(value.OutputMedia.FilePath))
            {
                problems.Add("ConversionResult.OutputMedia.FilePath must not be null or empty.");
            }
            else if (!File.Exists(value.OutputMedia.FilePath))
            {
                problems.Add($"ConversionResult.OutputMedia.FilePath references non-existent file: {value.OutputMedia.FilePath}.");
            }

            if (value.OutputMedia.FileSize <= 0)
            {
                problems.Add("ConversionResult.OutputMedia.FileSize must be greater than zero.");
            }

            if (value.OutputMedia.Duration.HasValue && value.OutputMedia.Duration.Value.TotalSeconds <= 0)
            {
                problems.Add("ConversionResult.OutputMedia.Duration must be greater than zero seconds when provided.");
            }
        }

        // Validate Duration
        if (value.Duration.TotalSeconds < 0)
        {
            problems.Add("ConversionResult.Duration must not be negative.");
        }

        // Validate ErrorMessage
        if (value.IsSuccess && !string.IsNullOrEmpty(value.ErrorMessage))
        {
            problems.Add("ConversionResult.ErrorMessage must be null or empty when IsSuccess is true.");
        }

        // Validate WarningMessage
        // WarningMessage can be null, empty, or contain warnings - no validation needed

        // Validate Metrics
        if (value.Metrics is null)
        {
            problems.Add("ConversionResult.Metrics must not be null.");
        }
        else if (value.Metrics.Count > 1000) // Reasonable upper bound
        {
            problems.Add("ConversionResult.Metrics contains too many entries (maximum 1000 allowed).");
        }

        // Validate CreatedAt
        if (value.CreatedAt == default)
        {
            problems.Add("ConversionResult.CreatedAt must be set to a non-default DateTime.");
        }
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            problems.Add("ConversionResult.CreatedAt cannot be in the future.");
        }

        // Validate CompletedAt
        if (value.IsSuccess && value.CompletedAt == default)
        {
            problems.Add("ConversionResult.CompletedAt must be set when IsSuccess is true.");
        }
        else if (!value.IsSuccess && value.CompletedAt == default)
        {
            problems.Add("ConversionResult.CompletedAt should be set when IsSuccess is false.");
        }
        else if (value.CompletedAt != default && value.CreatedAt != default && value.CompletedAt < value.CreatedAt)
        {
            problems.Add("ConversionResult.CompletedAt cannot be earlier than CreatedAt.");
        }

        // Validate FFmpegOutput
        if (value.IsSuccess && string.IsNullOrWhiteSpace(value.FFmpegOutput))
        {
            problems.Add("ConversionResult.FFmpegOutput must not be null or empty when IsSuccess is true.");
        }

        // Validate consistency between IsSuccess and error state
        if (value.IsSuccess && !string.IsNullOrEmpty(value.ErrorMessage))
        {
            problems.Add("ConversionResult has IsSuccess=true but contains an ErrorMessage.");
        }

        if (!value.IsSuccess && string.IsNullOrEmpty(value.ErrorMessage))
        {
            problems.Add("ConversionResult has IsSuccess=false but ErrorMessage is null or empty.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the conversion result is valid.
    /// </summary>
    /// <param name="value">The conversion result to check.</param>
    /// <returns>True if the result is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ConversionResult value)
        => value.Validate().Count == 0;

    /// <summary>
    /// Ensures that the conversion result is valid, throwing an exception if it is not.
    /// </summary>
    /// <param name="value">The conversion result to validate.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is invalid, containing a list of validation problems.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static void EnsureValid(this ConversionResult value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count != 0)
        {
            throw new ArgumentException(
                $"ConversionResult is invalid. Problems: {string.Join(" ", problems)}",
                nameof(value));
        }
    }
}
