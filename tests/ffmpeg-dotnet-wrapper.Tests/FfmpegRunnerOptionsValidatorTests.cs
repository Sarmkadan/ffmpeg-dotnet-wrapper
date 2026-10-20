// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Configuration;

using FluentAssertions;

using Xunit;

namespace FFmpegDotnetWrapper.Tests;

public class FfmpegRunnerOptionsValidatorTests : IDisposable
{
    private readonly FakeFfmpegBinary _binaries = new();

    public void Dispose() => _binaries.Dispose();

    [Fact]
    public void Validate_ShouldSucceed_WhenOptionsAreValid()
    {
        var validator = new FfmpegRunnerOptionsValidator();
        var options = new FfmpegRunnerOptions
        {
            FfmpegPath = _binaries.CreateHealthyFfmpeg(),
            FfprobePath = _binaries.CreateHealthyFfprobe(),
        };

        var result = validator.Validate("FFmpeg", options);

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldReturnTimeoutViolation_WhenTimeoutIsNotPositive()
    {
        var validator = new FfmpegRunnerOptionsValidator();
        var options = new FfmpegRunnerOptions
        {
            FfmpegPath = _binaries.CreateHealthyFfmpeg(),
            FfprobePath = _binaries.CreateHealthyFfprobe(),
            Timeout = TimeSpan.FromSeconds(-5),
        };

        var result = validator.Validate(null, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle().Which.Should().StartWith("Timeout must be greater than zero");
    }

    [Fact]
    public void Validate_ShouldReturnEveryViolation_WhenSeveralRulesFail()
    {
        var validator = new FfmpegRunnerOptionsValidator();
        var options = new FfmpegRunnerOptions
        {
            FfmpegPath = _binaries.CreateHealthyFfmpeg(),
            FfprobePath = _binaries.CreateHealthyFfprobe(),
            LogLevel = "loud",
            WorkingDirectory = Path.Combine(_binaries.Directory, "missing"),
        };

        var result = validator.Validate(null, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().HaveCount(2);
    }

    [Fact]
    public void Validate_ShouldReturnFailureMessage_WhenFfprobeIsMissing()
    {
        var validator = new FfmpegRunnerOptionsValidator();
        var missing = Path.Combine(_binaries.Directory, "no-such-ffprobe");
        var options = new FfmpegRunnerOptions
        {
            FfmpegPath = _binaries.CreateHealthyFfmpeg(),
            FfprobePath = missing,
        };

        var result = validator.Validate(null, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle().Which.Should().Be($"FfprobePath '{missing}' does not exist or is not a file.");
    }

    [Fact]
    public void Validate_ShouldThrowArgumentNullException_WhenOptionsIsNull()
    {
        var validator = new FfmpegRunnerOptionsValidator();

        Action act = () => validator.Validate(null, null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
