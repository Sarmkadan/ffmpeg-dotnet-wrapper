// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FFmpegDotnetWrapper.Configuration;

/// <summary>
/// Extension methods for registering streaming progress services in the dependency injection container.
/// </summary>
public static class StreamingProgressExtensions
{
    /// <summary>
    /// Adds <see cref="IStreamingProgressService"/> to the DI container as a singleton.
    /// The service holds no per-operation state; all processing state is stack-local within each
    /// <see cref="IStreamingProgressService.StreamProgressAsync"/> call, making it safe for concurrent use.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <returns>The configured <see cref="IServiceCollection"/> for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddStreamingProgress(this IServiceCollection services)
        => services.AddSingleton<IStreamingProgressService, StreamingProgressService>();

    /// <summary>
    /// Adds FFmpeg wrapper services and <see cref="IStreamingProgressService"/> in a single call.
    /// Convenience method for applications that require both FFmpeg execution and progress tracking.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <param name="configureOptions">Optional delegate to configure <see cref="FFmpegWrapperOptions"/>.</param>
    /// <returns>The configured <see cref="IServiceCollection"/> for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddFFmpegWrapperWithStreaming(
        this IServiceCollection services,
        Action<FFmpegWrapperOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services
            .AddFFmpegWrapper(configureOptions)
            .AddStreamingProgress();
    }
}
