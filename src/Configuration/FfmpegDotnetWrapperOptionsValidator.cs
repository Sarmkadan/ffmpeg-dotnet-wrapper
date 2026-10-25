// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.Options;

namespace FFmpegDotnetWrapper.Configuration;

/// <summary>
/// Validates <see cref="FfmpegDotnetWrapperOptions"/> whenever <c>IOptions&lt;FfmpegDotnetWrapperOptions&gt;</c> is resolved.
/// Data annotations are checked first, and the log level and working directory rules run on every call,
/// so every failure is reported in one result.
/// </summary>
public sealed class FfmpegDotnetWrapperOptionsValidator : IValidateOptions<FfmpegDotnetWrapperOptions>
{
    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, FfmpegDotnetWrapperOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        var results = new List<ValidationResult>();
        if (!Validator.TryValidateObject(options, new ValidationContext(options), results, validateAllProperties: true))
        {
            failures.AddRange(results.Select(result => result.ErrorMessage ?? "The configuration is invalid."));
        }

        if (!string.IsNullOrEmpty(options.LogLevel) && !FfmpegRunnerOptions.AllowedLogLevels.Contains(options.LogLevel, StringComparer.Ordinal))
        {
            failures.Add(
                $"LogLevel '{options.LogLevel}' is not a valid FFmpeg log level. Allowed values: {string.Join(", ", FfmpegRunnerOptions.AllowedLogLevels)}.");
        }

        if (options.WorkingDirectory is not null && !Directory.Exists(options.WorkingDirectory))
        {
            failures.Add($"WorkingDirectory '{options.WorkingDirectory}' does not exist or is not a directory.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
