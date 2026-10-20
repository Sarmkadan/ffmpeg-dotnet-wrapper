// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Exceptions;

using Microsoft.Extensions.Options;

using OptionsValidationException = FFmpegDotnetWrapper.Exceptions.OptionsValidationException;

namespace FFmpegDotnetWrapper.Configuration;

/// <summary>
/// Adapts <see cref="FfmpegRunnerOptions.EnsureValid"/> to the options pipeline, so that resolving
/// <c>IOptions&lt;FfmpegRunnerOptions&gt;</c> reports failures through <see cref="ValidateOptionsResult"/>.
/// </summary>
public sealed class FfmpegRunnerOptionsValidator : IValidateOptions<FfmpegRunnerOptions>
{
    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, FfmpegRunnerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        try
        {
            options.EnsureValid();
            return ValidateOptionsResult.Success;
        }
        catch (OptionsValidationException ex)
        {
            return ValidateOptionsResult.Fail(ex.Violations.ToList());
        }
        catch (FfmpegNotFoundException ex)
        {
            return ValidateOptionsResult.Fail(ex.Message);
        }
    }
}
