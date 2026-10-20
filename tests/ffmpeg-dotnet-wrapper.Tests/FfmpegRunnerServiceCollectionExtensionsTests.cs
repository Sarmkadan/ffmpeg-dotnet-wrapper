// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Configuration;
using FFmpegDotnetWrapper.Exceptions;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Xunit;

using OptionsValidationException = FFmpegDotnetWrapper.Exceptions.OptionsValidationException;

namespace FFmpegDotnetWrapper.Tests;

public class FfmpegRunnerServiceCollectionExtensionsTests : IDisposable
{
    private readonly FakeFfmpegBinary _binaries = new();

    public void Dispose() => _binaries.Dispose();

    [Fact]
    public void AddFfmpeg_ShouldThrowArgumentNullException_WhenServicesIsNull()
    {
        IServiceCollection services = null!;

        Action act = () => services.AddFfmpeg(o => o.FfmpegPath = _binaries.CreateHealthyFfmpeg());

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddFfmpeg_ShouldThrowFfmpegNotFoundException_WhenFfmpegPathIsMissing()
    {
        var services = new ServiceCollection();
        var missing = Path.Combine(_binaries.Directory, "no-such-ffmpeg");

        Action act = () => services.AddFfmpeg(o =>
        {
            o.FfmpegPath = missing;
            o.FfprobePath = _binaries.CreateHealthyFfprobe();
        });

        act.Should().Throw<FfmpegNotFoundException>()
            .WithMessage($"FfmpegPath '{missing}' does not exist or is not a file.");
    }

    [Fact]
    public void AddFfmpeg_ShouldThrowOptionsValidationException_WhenConfigurationIsInvalidAtRegistration()
    {
        var services = new ServiceCollection();

        Action act = () => services.AddFfmpeg(o =>
        {
            o.FfmpegPath = _binaries.CreateHealthyFfmpeg();
            o.FfprobePath = _binaries.CreateHealthyFfprobe();
            o.Timeout = TimeSpan.Zero;
        });

        act.Should().Throw<OptionsValidationException>()
            .Which.Violations.Should().ContainSingle()
            .Which.Should().StartWith("Timeout must be greater than zero");
    }

    [Fact]
    public void AddFfmpeg_ShouldNotRegisterValidator_WhenValidationFails()
    {
        var services = new ServiceCollection();

        Action act = () => services.AddFfmpeg(o =>
        {
            o.FfmpegPath = _binaries.CreateHealthyFfmpeg();
            o.FfprobePath = _binaries.CreateHealthyFfprobe();
            o.LogLevel = "loud";
        });

        act.Should().Throw<OptionsValidationException>();
        services.Should().NotContain(d => d.ServiceType == typeof(IValidateOptions<FfmpegRunnerOptions>));
    }

    [Fact]
    public void AddFfmpeg_ShouldRegisterValidator_WhenValidConfiguration()
    {
        var services = new ServiceCollection();
        services.AddFfmpeg(o =>
        {
            o.FfmpegPath = _binaries.CreateHealthyFfmpeg();
            o.FfprobePath = _binaries.CreateHealthyFfprobe();
        });

        using var provider = services.BuildServiceProvider();
        var validators = provider.GetServices<IValidateOptions<FfmpegRunnerOptions>>().ToList();

        validators.Should().ContainSingle().Which.Should().BeOfType<FfmpegRunnerOptionsValidator>();
    }

    [Fact]
    public void AddFfmpeg_ShouldExposeConfiguredOptions_ThroughIOptions()
    {
        var ffmpeg = _binaries.CreateHealthyFfmpeg();
        var services = new ServiceCollection();
        services.AddFfmpeg(o =>
        {
            o.FfmpegPath = ffmpeg;
            o.FfprobePath = _binaries.CreateHealthyFfprobe();
            o.Timeout = TimeSpan.FromMinutes(2);
            o.LogLevel = "warning";
        });

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<FfmpegRunnerOptions>>().Value;

        options.FfmpegPath.Should().Be(ffmpeg);
        options.Timeout.Should().Be(TimeSpan.FromMinutes(2));
        options.LogLevel.Should().Be("warning");
    }
}
