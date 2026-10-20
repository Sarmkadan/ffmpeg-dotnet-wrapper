// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace FFmpegDotnetWrapper.Configuration;

/// <summary>
/// Extension methods for registering <see cref="FfmpegRunnerOptions"/> in dependency injection.
/// </summary>
public static class FfmpegRunnerServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="FfmpegRunnerOptions"/> and validates them immediately. The configuration is
    /// applied to a fresh instance and <see cref="FfmpegRunnerOptions.EnsureValid"/> runs before this method
    /// returns, so a missing or broken FFmpeg installation fails at startup rather than on first use.
    /// The registered <see cref="FfmpegRunnerOptionsValidator"/> re-checks the options whenever
    /// <c>IOptions&lt;FfmpegRunnerOptions&gt;</c> is resolved.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the options to.</param>
    /// <param name="configure">Optional action that configures the FFmpeg runner options.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <see langword="null"/>.</exception>
    /// <exception cref="Exceptions.FfmpegNotFoundException">Thrown when FFmpeg or FFprobe cannot be found or launched.</exception>
    /// <exception cref="Exceptions.OptionsValidationException">Thrown when the configured options are invalid.</exception>
    public static IServiceCollection AddFfmpeg(
        this IServiceCollection services,
        Action<FfmpegRunnerOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new FfmpegRunnerOptions();
        configure?.Invoke(options);
        options.EnsureValid();

        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<FfmpegRunnerOptions>, FfmpegRunnerOptionsValidator>());

        return services;
    }
}
