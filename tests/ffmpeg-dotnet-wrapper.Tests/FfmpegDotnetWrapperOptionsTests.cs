// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Configuration;
using FFmpegDotnetWrapper.Constants;

using FluentAssertions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Xunit;

namespace FFmpegDotnetWrapper.Tests;

public class FfmpegDotnetWrapperOptionsTests : IDisposable
{
    private readonly string _workingDirectory = Path.Combine(Path.GetTempPath(), $"ffmpeg-wrapper-options-{Guid.NewGuid():N}");

    public FfmpegDotnetWrapperOptionsTests()
    {
        Directory.CreateDirectory(_workingDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_workingDirectory))
        {
            Directory.Delete(_workingDirectory, recursive: true);
        }
    }

    [Fact]
    public void Defaults_ShouldNotContainMachineSpecificPathsOrSecrets()
    {
        var options = new FfmpegDotnetWrapperOptions();

        options.FfmpegPath.Should().BeNull();
        options.FfprobePath.Should().BeNull();
        options.WorkingDirectory.Should().BeNull();
        options.TimeoutSeconds.Should().Be(FFmpegConstants.DefaultTimeoutSeconds);
        options.LogLevel.Should().Be("info");
    }

    [Fact]
    public void Validator_ShouldSucceed_WhenDefaultsAreUsed()
    {
        var result = new FfmpegDotnetWrapperOptionsValidator().Validate(null, new FfmpegDotnetWrapperOptions());

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validator_ShouldFail_WhenTimeoutIsZero()
    {
        var options = new FfmpegDotnetWrapperOptions { TimeoutSeconds = 0 };

        var result = new FfmpegDotnetWrapperOptionsValidator().Validate(null, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle().Which.Should().Contain(nameof(FfmpegDotnetWrapperOptions.TimeoutSeconds));
    }

    [Fact]
    public void Validator_ShouldFail_WhenTimeoutExceedsMaximum()
    {
        var options = new FfmpegDotnetWrapperOptions { TimeoutSeconds = FFmpegConstants.MaxTimeoutSeconds + 1 };

        var result = new FfmpegDotnetWrapperOptionsValidator().Validate(null, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle().Which.Should().Contain(nameof(FfmpegDotnetWrapperOptions.TimeoutSeconds));
    }

    [Theory]
    [InlineData("loud")]
    [InlineData("INFO")]
    [InlineData("")]
    public void Validator_ShouldFail_WhenLogLevelIsNotAllowed(string logLevel)
    {
        var options = new FfmpegDotnetWrapperOptions { LogLevel = logLevel };

        var result = new FfmpegDotnetWrapperOptionsValidator().Validate(null, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(failure => failure.Contains(nameof(FfmpegDotnetWrapperOptions.LogLevel)));
    }

    [Fact]
    public void Validator_ShouldFail_WhenWorkingDirectoryDoesNotExist()
    {
        var options = new FfmpegDotnetWrapperOptions
        {
            WorkingDirectory = Path.Combine(_workingDirectory, "missing")
        };

        var result = new FfmpegDotnetWrapperOptionsValidator().Validate(null, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(failure => failure.Contains(nameof(FfmpegDotnetWrapperOptions.WorkingDirectory)));
    }

    [Fact]
    public void Validator_ShouldReportEveryFailure_WhenSeveralRulesAreBroken()
    {
        var options = new FfmpegDotnetWrapperOptions
        {
            TimeoutSeconds = -5,
            LogLevel = "loud",
            WorkingDirectory = Path.Combine(_workingDirectory, "missing")
        };

        var result = new FfmpegDotnetWrapperOptionsValidator().Validate(null, options);

        result.Failures.Should().HaveCount(3);
    }

    [Fact]
    public void ToRunnerOptions_ShouldCopyEveryValue()
    {
        var options = new FfmpegDotnetWrapperOptions
        {
            FfmpegPath = "/opt/ffmpeg/bin/ffmpeg",
            FfprobePath = "/opt/ffmpeg/bin/ffprobe",
            TimeoutSeconds = 90,
            LogLevel = "warning",
            WorkingDirectory = _workingDirectory
        };

        var runner = options.ToRunnerOptions();

        runner.FfmpegPath.Should().Be("/opt/ffmpeg/bin/ffmpeg");
        runner.FfprobePath.Should().Be("/opt/ffmpeg/bin/ffprobe");
        runner.Timeout.Should().Be(TimeSpan.FromSeconds(90));
        runner.LogLevel.Should().Be("warning");
        runner.WorkingDirectory.Should().Be(_workingDirectory);
    }

    [Fact]
    public void AddFfmpegDotnetWrapperOptions_ShouldApplyConfigureAction_ThroughIOptions()
    {
        var services = new ServiceCollection();
        services.AddFfmpegDotnetWrapperOptions(o => o.TimeoutSeconds = 120);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<FfmpegDotnetWrapperOptions>>().Value;

        options.TimeoutSeconds.Should().Be(120);
    }

    [Fact]
    public void AddFfmpegDotnetWrapperOptions_ShouldRegisterValidator()
    {
        var services = new ServiceCollection();
        services.AddFfmpegDotnetWrapperOptions();

        using var provider = services.BuildServiceProvider();

        provider.GetServices<IValidateOptions<FfmpegDotnetWrapperOptions>>()
            .Should().ContainSingle()
            .Which.Should().BeOfType<FfmpegDotnetWrapperOptionsValidator>();
    }

    [Fact]
    public void AddFfmpegDotnetWrapperOptions_ShouldRejectInvalidValues_WhenResolved()
    {
        var services = new ServiceCollection();
        services.AddFfmpegDotnetWrapperOptions(o => o.TimeoutSeconds = 0);

        using var provider = services.BuildServiceProvider();
        var act = () => provider.GetRequiredService<IOptions<FfmpegDotnetWrapperOptions>>().Value;

        act.Should().Throw<Microsoft.Extensions.Options.OptionsValidationException>()
            .WithMessage($"*{nameof(FfmpegDotnetWrapperOptions.TimeoutSeconds)}*");
    }

    [Fact]
    public void AddFfmpegDotnetWrapperOptions_ShouldBindExampleFile_WithoutValidationErrors()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.example.json", optional: false)
            .Build();

        var services = new ServiceCollection();
        services.AddFfmpegDotnetWrapperOptions()
            .Bind(configuration.GetSection(FfmpegDotnetWrapperOptions.SectionName));

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<FfmpegDotnetWrapperOptions>>().Value;

        options.TimeoutSeconds.Should().Be(FFmpegConstants.DefaultTimeoutSeconds);
        options.LogLevel.Should().Be("info");
        options.FfmpegPath.Should().BeNull();
        options.WorkingDirectory.Should().BeNull();
    }

    [Fact]
    public void AddFfmpegDotnetWrapperOptions_ShouldBindValuesFromConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["FfmpegDotnetWrapper:TimeoutSeconds"] = "45",
                ["FfmpegDotnetWrapper:LogLevel"] = "error",
                ["FfmpegDotnetWrapper:WorkingDirectory"] = _workingDirectory
            })
            .Build();

        var services = new ServiceCollection();
        services.AddFfmpegDotnetWrapperOptions()
            .Bind(configuration.GetSection(FfmpegDotnetWrapperOptions.SectionName));

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<FfmpegDotnetWrapperOptions>>().Value;

        options.TimeoutSeconds.Should().Be(45);
        options.LogLevel.Should().Be("error");
        options.WorkingDirectory.Should().Be(_workingDirectory);
    }
}
