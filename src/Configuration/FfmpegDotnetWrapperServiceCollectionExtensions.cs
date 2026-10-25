// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace FFmpegDotnetWrapper.Configuration;

/// <summary>
/// Extension methods for registering <see cref="FfmpegDotnetWrapperOptions"/> in dependency injection.
/// </summary>
public static class FfmpegDotnetWrapperServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="FfmpegDotnetWrapperOptions"/> with the options pipeline and adds
    /// <see cref="FfmpegDotnetWrapperOptionsValidator"/>, so invalid values are reported when
    /// <c>IOptions&lt;FfmpegDotnetWrapperOptions&gt;</c> is resolved.
    /// </summary>
    /// <remarks>
    /// The returned builder can bind a configuration section, for example
    /// <c>AddFfmpegDotnetWrapperOptions().Bind(configuration.GetSection(FfmpegDotnetWrapperOptions.SectionName))</c>.
    /// Call <c>ValidateOnStart()</c> on it as well to fail at host startup instead of on first use.
    /// </remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the options to.</param>
    /// <param name="configure">Optional action that sets the options in code. It runs after any configuration binding.</param>
    /// <returns>An <see cref="OptionsBuilder{TOptions}"/> for binding and further options setup.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <see langword="null"/>.</exception>
    public static OptionsBuilder<FfmpegDotnetWrapperOptions> AddFfmpegDotnetWrapperOptions(
        this IServiceCollection services,
        Action<FfmpegDotnetWrapperOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var builder = services.AddOptions<FfmpegDotnetWrapperOptions>();

        if (configure is not null)
        {
            builder.Configure(configure);
        }

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<FfmpegDotnetWrapperOptions>, FfmpegDotnetWrapperOptionsValidator>());

        return builder;
    }
}
