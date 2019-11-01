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
    /// Adds <see cref="IStreamingProgressService"/> to the DI container.
    /// The service is registered as a singleton because it holds no per-operation state —
    /// all processing state is stack-local within each <c>StreamProgressAsync</c> call.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for fluent chaining.</returns>
    public static IServiceCollection AddStreamingProgress(this IServiceCollection services)
    {
        services.AddSingleton<IStreamingProgressService, StreamingProgressService>();
        return services;
    }

    /// <summary>
    /// Adds FFmpeg wrapper services and <see cref="IStreamingProgressService"/> in a single call.
    /// Convenience overload for applications that use both together.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configureOptions">Optional delegate to configure <see cref="FFmpegWrapperOptions"/>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for fluent chaining.</returns>
    public static IServiceCollection AddFFmpegWrapperWithStreaming(
        this IServiceCollection services,
        Action<FFmpegWrapperOptions>? configureOptions = null)
    {
        return services
            .AddFFmpegWrapper(configureOptions)
            .AddStreamingProgress();
    }
}
