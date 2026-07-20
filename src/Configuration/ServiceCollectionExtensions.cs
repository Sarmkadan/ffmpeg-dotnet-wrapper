// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Repository;
using FFmpegDotnetWrapper.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FFmpegDotnetWrapper.Configuration;

/// <summary>
/// Extension methods for registering FFmpeg wrapper services in dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all FFmpeg wrapper services to the DI container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configureOptions">Optional action to configure FFmpeg wrapper options.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddFFmpegWrapper(
        this IServiceCollection services,
        Action<FFmpegWrapperOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new FFmpegWrapperOptions();
        configureOptions?.Invoke(options);

        // Register repositories
        services.AddSingleton<IMediaRepository, MediaRepository>();
        services.AddSingleton<IOperationRepository, OperationRepository>();

        // Register services with their required dependencies
        services.AddSingleton<TranscodeService>();
        services.AddSingleton<SubtitleService>();
        services.AddSingleton<ThumbnailService>();
            services.AddSingleton<IWatermarkService, WatermarkService>();

        // Register options if they were configured
        if (configureOptions is not null)
        {
            services.AddSingleton(options);
        }

        return services;
    }

    /// <summary>
    /// Adds FFmpeg wrapper with custom configuration options.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="options">The FFmpeg wrapper configuration options.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="services"/> or <paramref name="options"/> is <see langword="null"/>.
    /// </exception>
    public static IServiceCollection AddFFmpegWrapper(
        this IServiceCollection services,
        FFmpegWrapperOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        return services.AddFFmpegWrapper(opts =>
        {
            opts.DefaultTimeout = options.DefaultTimeout;
            opts.FFmpegPath = options.FFmpegPath;
            opts.FFprobePath = options.FFprobePath;
            opts.LogLevel = options.LogLevel;
            opts.EnableOperationCaching = options.EnableOperationCaching;
            opts.MaxCachedOperations = options.MaxCachedOperations;
            opts.EnableDetailedLogging = options.EnableDetailedLogging;
        });
    }
}

/// <summary>
/// Configuration options for FFmpeg wrapper.
/// </summary>
public class FFmpegWrapperOptions
{
    /// <summary>
    /// Gets or sets the default timeout for FFmpeg operations.
    /// </summary>
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(300);

    /// <summary>
    /// Gets or sets the path to the FFmpeg executable.
    /// </summary>
    public string? FFmpegPath { get; set; }

    /// <summary>
    /// Gets or sets the path to the FFprobe executable.
    /// </summary>
    public string? FFprobePath { get; set; }

    /// <summary>
    /// Gets or sets the logging level for FFmpeg operations.
    /// </summary>
    public LogLevel LogLevel { get; set; } = LogLevel.Information;

    /// <summary>
    /// Gets or sets a value indicating whether operation caching is enabled.
    /// </summary>
    public bool EnableOperationCaching { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of cached operations.
    /// </summary>
    public int MaxCachedOperations { get; set; } = 1000;

    /// <summary>
    /// Gets or sets a value indicating whether detailed logging is enabled.
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = false;
}
