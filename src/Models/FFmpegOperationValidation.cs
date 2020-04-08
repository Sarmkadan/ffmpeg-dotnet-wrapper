// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace FFmpegDotnetWrapper.Models;

/// <summary>
/// Provides validation helpers for <see cref="FFmpegOperation"/> instances.
/// </summary>
public static class FFmpegOperationValidation
{
    /// <summary>
    /// Validates the specified FFmpeg operation and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The operation to validate.</param>
    /// <returns>An empty list if valid; otherwise, a list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this FFmpegOperation? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Id
        if (string.IsNullOrWhiteSpace(value.Id))
        {
            errors.Add("Operation Id cannot be null or whitespace.");
        }
        else if (value.Id == Guid.Empty.ToString())
        {
            errors.Add("Operation Id cannot be an empty GUID.");
        }

        // Validate Name
        if (string.IsNullOrWhiteSpace(value.Name))
        {
            errors.Add("Operation Name cannot be null or whitespace.");
        }
        else if (value.Name.Length > 256)
        {
            errors.Add("Operation Name cannot exceed 256 characters.");
        }

        // Validate Type
        if (!Enum.IsDefined(value.Type))
        {
            errors.Add("Operation Type must be a valid FFmpegOperationType value.");
        }

        // Validate InputFiles
        if (value.InputFiles is null)
        {
            errors.Add("Operation InputFiles collection cannot be null.");
        }
        else
        {
            if (value.InputFiles.Count == 0)
            {
                errors.Add("At least one input file is required.");
            }

            foreach (var inputFile in value.InputFiles)
            {
                if (string.IsNullOrWhiteSpace(inputFile))
                {
                    errors.Add("Input file path cannot be null or whitespace.");
                    break;
                }

                if (inputFile.Length > 4096)
                {
                    errors.Add("Input file path cannot exceed 4096 characters.");
                    break;
                }
            }
        }

        // Validate OutputFile
        if (string.IsNullOrWhiteSpace(value.OutputFile))
        {
            errors.Add("Output file path cannot be null or whitespace.");
        }
        else if (value.OutputFile.Length > 4096)
        {
            errors.Add("Output file path cannot exceed 4096 characters.");
        }
        else
        {
            // Basic path validation
            try
            {
                var outputDir = Path.GetDirectoryName(value.OutputFile);
                if (!string.IsNullOrEmpty(outputDir) && outputDir.Length > 4096)
                {
                    errors.Add("Output file directory path cannot exceed 4096 characters.");
                }
            }
            catch (Exception ex) when (ex is ArgumentException or PathTooLongException)
            {
                errors.Add("Output file path contains invalid characters or is malformed.");
            }
        }

        // Validate Arguments
        if (value.Arguments is null)
        {
            errors.Add("Operation Arguments collection cannot be null.");
        }
        else
        {
            foreach (var argument in value.Arguments)
            {
                if (string.IsNullOrWhiteSpace(argument))
                {
                    errors.Add("Arguments cannot contain null or whitespace entries.");
                    break;
                }

                if (argument.Length > 1024)
                {
                    errors.Add("Individual argument cannot exceed 1024 characters.");
                    break;
                }
            }
        }

        // Validate Timeout
        if (value.Timeout.HasValue)
        {
            if (value.Timeout.Value.TotalMilliseconds <= 0)
            {
                errors.Add("Timeout must be a positive time span.");
            }
            else if (value.Timeout.Value.TotalMilliseconds > TimeSpan.FromHours(24).TotalMilliseconds)
            {
                errors.Add("Timeout cannot exceed 24 hours.");
            }
        }

        // Validate Priority
        if (value.Priority.HasValue)
        {
            if (value.Priority.Value < 0)
            {
                errors.Add("Priority cannot be negative.");
            }
            else if (value.Priority.Value > 100)
            {
                errors.Add("Priority cannot exceed 100.");
            }
        }

        // Validate IsParallel
        // No validation needed for boolean

        // Validate CustomProperties
        if (value.CustomProperties is null)
        {
            errors.Add("Operation CustomProperties dictionary cannot be null.");
        }
        else
        {
            foreach (var kvp in value.CustomProperties)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key))
                {
                    errors.Add("Custom property key cannot be null or whitespace.");
                    break;
                }

                if (kvp.Key.Length > 256)
                {
                    errors.Add("Custom property key cannot exceed 256 characters.");
                    break;
                }

                if (kvp.Value is not null && kvp.Value.Length > 1024)
                {
                    errors.Add("Custom property value cannot exceed 1024 characters.");
                    break;
                }
            }
        }

        // Validate CreatedAt
        if (value.CreatedAt == default)
        {
            errors.Add("CreatedAt must be set to a valid DateTime value.");
        }
        else if (value.CreatedAt.Kind != DateTimeKind.Utc)
        {
            errors.Add("CreatedAt must be in UTC timezone.");
        }
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("CreatedAt cannot be in the future.");
        }
        else if (value.CreatedAt < DateTime.UtcNow.AddYears(-1))
        {
            errors.Add("CreatedAt cannot be more than one year in the past.");
        }

        // Validate ExecutedAt
        if (value.ExecutedAt.HasValue)
        {
            if (value.ExecutedAt.Value == default)
            {
                errors.Add("ExecutedAt must be set to a valid DateTime value if specified.");
            }
            else if (value.ExecutedAt.Value.Kind != DateTimeKind.Utc)
            {
                errors.Add("ExecutedAt must be in UTC timezone.");
            }
            else if (value.ExecutedAt.Value > DateTime.UtcNow.AddMinutes(5))
            {
                errors.Add("ExecutedAt cannot be in the future.");
            }
            else if (value.CreatedAt > value.ExecutedAt.Value)
            {
                errors.Add("ExecutedAt cannot be earlier than CreatedAt.");
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified FFmpeg operation is valid.
    /// </summary>
    /// <param name="value">The operation to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this FFmpegOperation? value)
    {
        return value is not null && Validate(value).Count == 0;
    }

    /// <summary>
    /// Validates the specified FFmpeg operation and throws an exception if invalid.
    /// </summary>
    /// <param name="value">The operation to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the operation is invalid, containing a list of validation errors.</exception>
    public static void EnsureValid(this FFmpegOperation? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"FFmpegOperation is invalid. Validation failed with {errors.Count} error(s):{Environment.NewLine}- ",
                nameof(value),
                new AggregateException(errors.Select(e => new InvalidOperationException(e))));
        }
    }
}