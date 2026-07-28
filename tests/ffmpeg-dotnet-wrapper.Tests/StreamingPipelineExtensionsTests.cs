using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;
using FFmpegDotnetWrapper.Configuration;
using FFmpegDotnetWrapper.Services;
using FFmpegDotnetWrapper.Monitoring;

namespace FFmpegDotnetWrapper.Tests.Configuration;

public class StreamingPipelineExtensionsTests
{
    [Fact]
    public void AddAdaptiveBitrateStreaming_Default_RegistersServices()
    {
        var services = new ServiceCollection();

        services.AddAdaptiveBitrateStreaming();

        var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetRequiredService<StreamingPipelineMetrics>());
        Assert.NotNull(provider.GetRequiredService<IStreamingProgressService>());
        Assert.NotNull(provider.GetRequiredService<IAdaptiveBitrateService>());
        Assert.NotNull(provider.GetRequiredService<IOptions<StreamingPipelineOptions>>().Value);
    }

    [Fact]
    public void AddAdaptiveBitrateStreaming_WithConfigure_OverridesOptions()
    {
        var services = new ServiceCollection();

        services.AddAdaptiveBitrateStreaming(opts =>
        {
            opts.MaxConcurrentPipelines = 10;
            opts.DowngradeSpeedThreshold = 0.75;
        });

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<StreamingPipelineOptions>>().Value;

        Assert.Equal(10, options.MaxConcurrentPipelines);
        Assert.Equal(0.75, options.DowngradeSpeedThreshold);
    }

    [Fact]
    public void AddAdaptiveBitrateStreaming_WithConfiguration_BindsOptions()
    {
        var inMemorySettings = new Dictionary<string, string>
        {
            {"FFmpeg:Streaming:MaxConcurrentPipelines", "7"},
            {"FFmpeg:Streaming:DowngradeSpeedThreshold", "0.85"}
        };
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var services = new ServiceCollection();

        services.AddAdaptiveBitrateStreaming(configuration);

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<StreamingPipelineOptions>>().Value;

        Assert.Equal(7, options.MaxConcurrentPipelines);
        Assert.Equal(0.85, options.DowngradeSpeedThreshold);
    }

    [Fact]
    public void AddAdaptiveBitrateStreaming_WithConfigurationAndConfigure_BindsAndOverrides()
    {
        var inMemorySettings = new Dictionary<string, string>
        {
            {"FFmpeg:Streaming:MaxConcurrentPipelines", "5"},
            {"FFmpeg:Streaming:DowngradeSpeedThreshold", "0.80"}
        };
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var services = new ServiceCollection();

        services.AddAdaptiveBitrateStreaming(configuration, opts =>
        {
            opts.DowngradeSpeedThreshold = 0.95; // override
        });

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<StreamingPipelineOptions>>().Value;

        Assert.Equal(5, options.MaxConcurrentPipelines);
        Assert.Equal(0.95, options.DowngradeSpeedThreshold);
    }

    [Fact]
    public void AddAdaptiveBitrateStreaming_NullServices_Throws()
    {
        IServiceCollection services = null!;
        Assert.Throws<ArgumentNullException>(() => services!.AddAdaptiveBitrateStreaming());
    }

    [Fact]
    public void AddAdaptiveBitrateStreaming_NullConfigure_Throws()
    {
        var services = new ServiceCollection();
        Assert.Throws<ArgumentNullException>(() => services.AddAdaptiveBitrateStreaming((Action<StreamingPipelineOptions>)null!));
    }

    [Fact]
    public void AddAdaptiveBitrateStreaming_NullConfiguration_Throws()
    {
        var services = new ServiceCollection();
        Assert.Throws<ArgumentNullException>(() => services.AddAdaptiveBitrateStreaming((IConfiguration)null!));
    }

    [Fact]
    public void AddAdaptiveBitrateStreaming_NullConfigurationAndConfigure_Throws()
    {
        var services = new ServiceCollection();
        Assert.Throws<ArgumentNullException>(() => services.AddAdaptiveBitrateStreaming((IConfiguration)null!, opts => { }));
    }
}
