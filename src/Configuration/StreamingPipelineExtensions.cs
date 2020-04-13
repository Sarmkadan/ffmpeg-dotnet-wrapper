// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Monitoring;
using FFmpegDotnetWrapper.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FFmpegDotnetWrapper.Configuration;

/// <summary>
/// Extension methods for registering the adaptive bitrate streaming pipeline components
/// into a .NET dependency injection container.
/// </summary>
/// <remarks>
/// Call one of the <c>AddAdaptiveBitrateStreaming</c> overloads from your
/// <c>IServiceCollection</c> configuration, typically inside <c>Program.cs</c> or a
/// startup class, after <c>AddFFmpegWrapper()</c>.
/// </remarks>
/// <example>
/// Minimal registration with default options:
/// <code>
/// services.AddFFmpegWrapper()
///   .AddAdaptiveBitrateStreaming();
/// </code>
///
/// Registration bound from <c>appsettings.json</c> with programmatic overrides:
/// <code>
/// services.AddAdaptiveBitrateStreaming(configuration, opts =>
/// {
///   opts.MaxConcurrentPipelines = 5;
///   opts.DowngradeSpeedThreshold = 0.85;
/// });
/// </code>
/// </example>
public static class StreamingPipelineExtensions
{
  /// <summary>
  /// Adds the adaptive bitrate streaming pipeline with default options.
  /// All thresholds and concurrency limits are taken from <see cref="StreamingPipelineOptions"/> defaults.
  /// </summary>
  /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
  /// <returns>The same <paramref name="services"/> instance for fluent chaining.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
  public static IServiceCollection AddAdaptiveBitrateStreaming(
    this IServiceCollection services)
  {
    ArgumentNullException.ThrowIfNull(services);
    services.Configure<StreamingPipelineOptions>(options => StreamingPipelineOptionsValidation.EnsureValid(options));
    RegisterCoreServices(services);
    return services;
  }

  /// <summary>
  /// Adds the adaptive bitrate streaming pipeline with programmatic option overrides.
  /// </summary>
  /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
  /// <param name="configure">
  /// A delegate that receives a <see cref="StreamingPipelineOptions"/> instance for modification.
  /// Invoked after the options object is constructed with its default values.
  /// </param>
  /// <returns>The same <paramref name="services"/> instance for fluent chaining.</returns>
  /// <exception cref="ArgumentNullException">
  /// <paramref name="services"/> is <see langword="null"/>.
  /// <para>-or-</para>
  /// <paramref name="configure"/> is <see langword="null"/>.
  /// </exception>
  public static IServiceCollection AddAdaptiveBitrateStreaming(
    this IServiceCollection services,
    Action<StreamingPipelineOptions> configure)
  {
    ArgumentNullException.ThrowIfNull(services);
    ArgumentNullException.ThrowIfNull(configure);

    services.Configure(configure);
    RegisterCoreServices(services);
    return services;
  }

  /// <summary>
  /// Adds the adaptive bitrate streaming pipeline with options bound from an
  /// <see cref="IConfiguration"/> section.
  /// </summary>
  /// <remarks>
  /// The method automatically looks for the <see cref="StreamingPipelineOptions.Section"/> key
  /// (<c>"FFmpeg:Streaming"</c>) within <paramref name="configuration"/>.
  /// Pass the configuration root rather than a pre-selected section to benefit from
  /// automatic reloading when the underlying configuration source changes.
  /// </remarks>
  /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
  /// <param name="configuration">
  /// The application configuration root or any <see cref="IConfiguration"/> instance.
  /// </param>
  /// <returns>The same <paramref name="services"/> instance for fluent chaining.</returns>
  /// <exception cref="ArgumentNullException">
  /// <paramref name="services"/> is <see langword="null"/>.
  /// <para>-or-</para>
  /// <paramref name="configuration"/> is <see langword="null"/>.
  /// </exception>
  public static IServiceCollection AddAdaptiveBitrateStreaming(
    this IServiceCollection services,
    IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(services);
    ArgumentNullException.ThrowIfNull(configuration);

    services.Configure<StreamingPipelineOptions>(
      configuration.GetSection(StreamingPipelineOptions.Section));

    RegisterCoreServices(services);
    return services;
  }

  /// <summary>
  /// Adds the adaptive bitrate streaming pipeline with options bound from configuration
  /// and additionally overridden via a programmatic delegate.
  /// </summary>
  /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
  /// <param name="configuration">The application configuration source.</param>
  /// <param name="configure">
  /// A delegate applied after configuration binding to apply programmatic overrides.
  /// </param>
  /// <returns>The same <paramref name="services"/> instance for fluent chaining.</returns>
  /// <exception cref="ArgumentNullException">
  /// <paramref name="services"/> is <see langword="null"/>.
  /// <para>-or-</para>
  /// <paramref name="configuration"/> is <see langword="null"/>.
  /// <para>-or-</para>
  /// <paramref name="configure"/> is <see langword="null"/>.
  /// </exception>
  public static IServiceCollection AddAdaptiveBitrateStreaming(
    this IServiceCollection services,
    IConfiguration configuration,
    Action<StreamingPipelineOptions> configure)
  {
    ArgumentNullException.ThrowIfNull(services);
    ArgumentNullException.ThrowIfNull(configuration);
    ArgumentNullException.ThrowIfNull(configure);

    services
      .Configure<StreamingPipelineOptions>(
        configuration.GetSection(StreamingPipelineOptions.Section))
      .Configure(configure);

    RegisterCoreServices(services);
    return services;
  }

  // ── Private helpers ──────────────────────────────────────────────────────

  /// <summary>
  /// Registers the concrete service types that back the streaming pipeline feature.
  /// Calling this method multiple times on the same container is safe — .NET's DI container
  /// deduplicates singleton and scoped descriptors by service type.
  /// </summary>
  /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
  /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
  private static void RegisterCoreServices(IServiceCollection services)
  {
    ArgumentNullException.ThrowIfNull(services);

    // Singleton — aggregates metrics across all pipeline runs for the lifetime of the process.
    services.AddSingleton<StreamingPipelineMetrics>();

    // Transient — all state is stack-local per stream invocation; safe to share lifetime.
    services.AddTransient<IStreamingProgressService, StreamingProgressService>();

    // Scoped — one instance per HTTP request / DI scope.
    // Pipelines themselves are tracked in a ConcurrentDictionary, not the scope lifetime.
    services.AddScoped<IAdaptiveBitrateService, AdaptiveBitrateService>();
  }
}