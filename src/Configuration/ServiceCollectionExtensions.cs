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
    public static IServiceCollection AddFFmpegWrapper(
        this IServiceCollection services,
        Action<FFmpegWrapperOptions>? configureOptions = null)
    {
        var options = new FFmpegWrapperOptions();
        configureOptions?.Invoke(options);

        // Register repositories
        services.AddSingleton<IMediaRepository, MediaRepository>();
        services.AddSingleton<IOperationRepository, OperationRepository>();

        // Register services
        services.AddSingleton<IFFmpegService, FFmpegService>();
        services.AddSingleton<TranscodeService>();
        services.AddSingleton<BatchOperationService>();

        // Register options
        services.AddSingleton(options);

        return services;
    }

    /// <summary>
    /// Adds FFmpeg wrapper with custom configuration options.
    /// </summary>
    public static IServiceCollection AddFFmpegWrapper(
        this IServiceCollection services,
        FFmpegWrapperOptions options)
    {
        return services.AddFFmpegWrapper(opts =>
        {
            opts.DefaultTimeout = options.DefaultTimeout;
            opts.FFmpegPath = options.FFmpegPath;
            opts.FFprobePath = options.FFprobePath;
            opts.LogLevel = options.LogLevel;
        });
    }
}

/// <summary>
/// Configuration options for FFmpeg wrapper.
/// </summary>
public class FFmpegWrapperOptions
{
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(300);
    public string? FFmpegPath { get; set; }
    public string? FFprobePath { get; set; }
    public LogLevel LogLevel { get; set; } = LogLevel.Information;
    public bool EnableOperationCaching { get; set; } = true;
    public int MaxCachedOperations { get; set; } = 1000;
    public bool EnableDetailedLogging { get; set; } = false;
}
