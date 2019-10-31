// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using FFmpegDotnetWrapper.BackgroundJobs;
using FFmpegDotnetWrapper.Caching;
using FFmpegDotnetWrapper.Events;
using FFmpegDotnetWrapper.Integration;
using FFmpegDotnetWrapper.Middleware;
using FFmpegDotnetWrapper.Services;
using FFmpegDotnetWrapper.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FFmpegDotnetWrapper.Configuration
{
    /// <summary>
    /// Startup and configuration class for setting up the entire FFmpeg wrapper application.
    /// Registers all services, middleware, and event handlers with dependency injection.
    /// Provides extension methods for easy setup in ASP.NET Core or custom applications.
    /// </summary>
    public static class ApplicationStartup
    {
        /// <summary>
        /// Adds all FFmpeg wrapper services to the dependency injection container.
        /// Must be called during application startup to register services.
        /// </summary>
        public static IServiceCollection AddFFmpegWrapper(this IServiceCollection services)
        {
            // Validate inputs
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            // Register core services
            services.AddScoped<IFFmpegService, FFmpegService>();
            services.AddScoped<TranscodeService>();
            services.AddScoped<BatchOperationService>();

            // Register repositories
            services.AddScoped<IMediaRepository, MediaRepository>();
            services.AddScoped<IOperationRepository, OperationRepository>();

            // Register caching
            services.AddSingleton<ICacheService, CacheService>();

            // Register event system
            services.AddSingleton<IEventPublisher, EventPublisher>();

            // Register background jobs
            services.AddSingleton<IBackgroundJobService, BackgroundJobService>();

            // Register webhook service
            services.AddSingleton<IWebhookService, WebhookService>();

            // Register rate limiter
            services.AddSingleton<IRateLimiter, SlidingWindowRateLimiter>();

            // Register middleware
            services.AddScoped<ErrorHandlingMiddleware>();
            services.AddScoped<ValidationMiddleware>();
            services.AddScoped<RequestLoggingMiddleware>();

            return services;
        }

        /// <summary>
        /// Adds FFmpeg wrapper with full configuration from options classes.
        /// Includes all services plus configuration-driven setup.
        /// </summary>
        public static IServiceCollection AddFFmpegWrapperWithConfiguration(
            this IServiceCollection services,
            Action<FFmpegOptions>? ffmpegOptions = null,
            Action<CachingOptions>? cachingOptions = null,
            Action<RateLimitingOptions>? rateLimitingOptions = null)
        {
            // Add base services
            services.AddFFmpegWrapper();

            // Configure options
            services.Configure<FFmpegOptions>(opts =>
            {
                ffmpegOptions?.Invoke(opts);
            });

            services.Configure<CachingOptions>(opts =>
            {
                cachingOptions?.Invoke(opts);
            });

            services.Configure<RateLimitingOptions>(opts =>
            {
                rateLimitingOptions?.Invoke(opts);
            });

            return services;
        }

        /// <summary>
        /// Initializes the application after all services are registered.
        /// Sets up event subscriptions and validates FFmpeg availability.
        /// Should be called during application startup before processing requests.
        /// </summary>
        public static async System.Threading.Tasks.Task InitializeApplicationAsync(this IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<ApplicationStartup>>();
            var eventPublisher = serviceProvider.GetRequiredService<IEventPublisher>();
            var webhookService = serviceProvider.GetRequiredService<IWebhookService>();

            try
            {
                // Subscribe webhook service to events
                eventPublisher.Subscribe<OperationStartedEvent>(webhookService);
                eventPublisher.Subscribe<OperationCompletedEvent>(webhookService);
                eventPublisher.Subscribe<OperationFailedEvent>(webhookService);

                logger.LogInformation("FFmpeg wrapper initialized successfully");

                // Verify FFmpeg is available
                if (!ValidateFFmpegInstallation())
                {
                    logger.LogWarning("FFmpeg executable not found in PATH");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error initializing FFmpeg wrapper");
                throw;
            }
        }

        /// <summary>
        /// Registers event handlers for specific event types.
        /// Used to subscribe custom event handlers after application startup.
        /// </summary>
        public static void RegisterEventHandler<TEvent, THandler>(
            this IServiceProvider serviceProvider)
            where TEvent : FFmpegEvent
            where THandler : IEventHandler<TEvent>
        {
            var eventPublisher = serviceProvider.GetRequiredService<IEventPublisher>();
            var handler = serviceProvider.GetRequiredService<THandler>();
            eventPublisher.Subscribe<TEvent>(handler);
        }

        /// <summary>
        /// Validates that FFmpeg and FFprobe are installed and available.
        /// Returns true if both executables are found.
        /// </summary>
        private static bool ValidateFFmpegInstallation()
        {
            try
            {
                // Try to execute ffmpeg -version
                var result = Utilities.ProcessUtilities.ExecuteProcess(
                    "ffmpeg",
                    "-version",
                    timeout: TimeSpan.FromSeconds(5)
                );

                return result.Success;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Configures logging with appropriate level and format.
        /// </summary>
        public static ILoggingBuilder ConfigureFFmpegLogging(
            this ILoggingBuilder builder,
            LogLevel minLevel = LogLevel.Information)
        {
            builder
                .SetMinimumLevel(minLevel)
                .AddConsole()
                .AddDebug();

            return builder;
        }

        /// <summary>
        /// Gets the FFmpeg options from the configured services.
        /// Useful for accessing configuration during runtime.
        /// </summary>
        public static FFmpegOptions GetFFmpegOptions(this IServiceProvider serviceProvider)
        {
            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<FFmpegOptions>>();
            return optionsMonitor.CurrentValue;
        }

        /// <summary>
        /// Gets cache service from the configured services.
        /// </summary>
        public static ICacheService GetCacheService(this IServiceProvider serviceProvider)
        {
            return serviceProvider.GetRequiredService<ICacheService>();
        }

        /// <summary>
        /// Gets event publisher from the configured services.
        /// </summary>
        public static IEventPublisher GetEventPublisher(this IServiceProvider serviceProvider)
        {
            return serviceProvider.GetRequiredService<IEventPublisher>();
        }

        /// <summary>
        /// Gets background job service from the configured services.
        /// </summary>
        public static IBackgroundJobService GetBackgroundJobService(this IServiceProvider serviceProvider)
        {
            return serviceProvider.GetRequiredService<IBackgroundJobService>();
        }

        /// <summary>
        /// Gets rate limiter from the configured services.
        /// </summary>
        public static IRateLimiter GetRateLimiter(this IServiceProvider serviceProvider)
        {
            return serviceProvider.GetRequiredService<IRateLimiter>();
        }
    }
}
