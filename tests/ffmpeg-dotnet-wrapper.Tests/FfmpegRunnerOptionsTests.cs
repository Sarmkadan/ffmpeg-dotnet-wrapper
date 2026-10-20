// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Configuration;
using FFmpegDotnetWrapper.Exceptions;
using FFmpegDotnetWrapper.Models;

using FluentAssertions;

using Xunit;

namespace FFmpegDotnetWrapper.Tests;

public class FfmpegRunnerOptionsTests : IDisposable
{
    private readonly FakeFfmpegBinary _binaries = new();

    public void Dispose() => _binaries.Dispose();

    private FfmpegRunnerOptions ValidOptions(string versionToken = "6.1.1-1ubuntu1")
    {
        return new FfmpegRunnerOptions
        {
            FfmpegPath = _binaries.CreateHealthyFfmpeg(versionToken),
            FfprobePath = _binaries.CreateHealthyFfprobe(),
        };
    }

    [Fact]
    public void EnsureValid_ShouldReturnParsedVersion_WhenBinariesAreHealthy()
    {
        var options = ValidOptions("6.1.1-1ubuntu1");

        var version = options.EnsureValid();

        version.Raw.Should().Be("6.1.1-1ubuntu1");
        version.Number.Should().Be(new Version(6, 1, 1));
    }

    [Fact]
    public void EnsureValid_ShouldCacheDetectedVersion_OnTheOptionsInstance()
    {
        var options = ValidOptions("7.0");

        options.DetectedVersion.Should().BeNull();

        var version = options.EnsureValid();

        options.DetectedVersion.Should().BeSameAs(version);
    }

    [Fact]
    public void EnsureValid_ShouldLaunchFfmpegOnlyOnce_WhenBinaryIsUnchanged()
    {
        var counterFile = Path.Combine(_binaries.Directory, "launch-count");
        var ffmpeg = _binaries.CreateScript(
            "ffmpeg",
            "ffmpeg version 5.1.4 Copyright (c) 2000-2022 the FFmpeg developers",
            extraShellLines: $"echo launched >> '{counterFile}'");
        var ffprobe = _binaries.CreateHealthyFfprobe();

        new FfmpegRunnerOptions { FfmpegPath = ffmpeg, FfprobePath = ffprobe }.EnsureValid();
        new FfmpegRunnerOptions { FfmpegPath = ffmpeg, FfprobePath = ffprobe }.EnsureValid();

        File.ReadAllLines(counterFile).Should().HaveCount(1);
    }

    [Fact]
    public void EnsureValid_ShouldThrowFfmpegNotFoundException_WhenFfmpegPathDoesNotExist()
    {
        var missing = Path.Combine(_binaries.Directory, "no-such-ffmpeg");
        var options = new FfmpegRunnerOptions { FfmpegPath = missing, FfprobePath = _binaries.CreateHealthyFfprobe() };

        Action act = () => options.EnsureValid();

        act.Should().Throw<FfmpegNotFoundException>()
            .WithMessage($"FfmpegPath '{missing}' does not exist or is not a file.");
    }

    [Fact]
    public void EnsureValid_ShouldThrowFfmpegNotFoundException_WhenFfprobePathDoesNotExist()
    {
        var missing = Path.Combine(_binaries.Directory, "no-such-ffprobe");
        var options = new FfmpegRunnerOptions { FfmpegPath = _binaries.CreateHealthyFfmpeg(), FfprobePath = missing };

        Action act = () => options.EnsureValid();

        act.Should().Throw<FfmpegNotFoundException>()
            .WithMessage($"FfprobePath '{missing}' does not exist or is not a file.");
    }

    [Fact]
    public void EnsureValid_ShouldThrowOptionsValidationException_WhenBinaryIsNotExecutable()
    {
        var ffmpeg = _binaries.CreateHealthyFfmpeg();
        File.SetUnixFileMode(ffmpeg, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        var options = new FfmpegRunnerOptions { FfmpegPath = ffmpeg, FfprobePath = _binaries.CreateHealthyFfprobe() };

        Action act = () => options.EnsureValid();

        act.Should().Throw<OptionsValidationException>()
            .Which.Violations.Should().ContainSingle()
            .Which.Should().Be($"FfmpegPath '{ffmpeg}' is not executable.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void EnsureValid_ShouldRejectTimeout_WhenNotPositive(int seconds)
    {
        var options = ValidOptions();
        options.Timeout = TimeSpan.FromSeconds(seconds);

        Action act = () => options.EnsureValid();

        act.Should().Throw<OptionsValidationException>()
            .Which.Violations.Should().ContainSingle()
            .Which.Should().Be($"Timeout must be greater than zero, but was {TimeSpan.FromSeconds(seconds)}.");
    }

    [Theory]
    [InlineData("loud")]
    [InlineData("INFO")]
    [InlineData("")]
    public void EnsureValid_ShouldRejectLogLevel_WhenNotAFfmpegLogLevel(string logLevel)
    {
        var options = ValidOptions();
        options.LogLevel = logLevel;

        Action act = () => options.EnsureValid();

        act.Should().Throw<OptionsValidationException>()
            .Which.Violations.Should().ContainSingle()
            .Which.Should().StartWith($"LogLevel '{logLevel}' is not a valid FFmpeg log level.");
    }

    [Theory]
    [InlineData("quiet")]
    [InlineData("error")]
    [InlineData("info")]
    [InlineData("trace")]
    public void EnsureValid_ShouldAcceptLogLevel_WhenFfmpegSupportsIt(string logLevel)
    {
        var options = ValidOptions();
        options.LogLevel = logLevel;

        Action act = () => options.EnsureValid();

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureValid_ShouldThrowOptionsValidationException_WhenWorkingDirectoryDoesNotExist()
    {
        var missing = Path.Combine(_binaries.Directory, "missing-dir");
        var options = ValidOptions();
        options.WorkingDirectory = missing;

        Action act = () => options.EnsureValid();

        act.Should().Throw<OptionsValidationException>()
            .Which.Violations.Should().ContainSingle()
            .Which.Should().Be($"WorkingDirectory '{missing}' does not exist or is not a directory.");
    }

    [Fact]
    public void EnsureValid_ShouldAcceptWorkingDirectory_WhenItExists()
    {
        var options = ValidOptions();
        options.WorkingDirectory = _binaries.Directory;

        Action act = () => options.EnsureValid();

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureValid_ShouldReportEveryViolation_InASingleException()
    {
        var options = ValidOptions();
        options.Timeout = TimeSpan.Zero;
        options.LogLevel = "loud";
        options.WorkingDirectory = Path.Combine(_binaries.Directory, "missing-dir");

        Action act = () => options.EnsureValid();

        act.Should().Throw<OptionsValidationException>()
            .Which.Violations.Should().HaveCount(3);
    }

    [Fact]
    public void EnsureValid_ShouldThrowFfmpegNotFoundException_WhenOutputIsNotFfmpegBanner()
    {
        var options = new FfmpegRunnerOptions
        {
            FfmpegPath = _binaries.CreateScript("ffmpeg", "hello world"),
            FfprobePath = _binaries.CreateHealthyFfprobe(),
        };

        Action act = () => options.EnsureValid();

        act.Should().Throw<FfmpegNotFoundException>()
            .WithMessage("*did not print an 'ffmpeg version' banner*hello world*");
    }

    [Fact]
    public void EnsureValid_ShouldThrowFfmpegNotFoundException_WhenVersionProbeExitsNonZero()
    {
        var options = new FfmpegRunnerOptions
        {
            FfmpegPath = _binaries.CreateScript("ffmpeg", "ffmpeg version 6.0 broken", exitCode: 3),
            FfprobePath = _binaries.CreateHealthyFfprobe(),
        };

        Action act = () => options.EnsureValid();

        act.Should().Throw<FfmpegNotFoundException>()
            .WithMessage("*exited with code 3 for '-version'*");
    }

    [Fact]
    public void EnsureValid_ShouldThrowFfmpegNotFoundException_WhenVersionProbeTimesOut()
    {
        var options = new FfmpegRunnerOptions
        {
            FfmpegPath = _binaries.CreateScript("ffmpeg", "ffmpeg version 6.0", extraShellLines: "sleep 10"),
            FfprobePath = _binaries.CreateHealthyFfprobe(),
            Timeout = TimeSpan.FromMilliseconds(300),
        };

        Action act = () => options.EnsureValid();

        act.Should().Throw<FfmpegNotFoundException>()
            .WithMessage("*did not finish '-version' within 00:00:00.3000000*");
    }

    [Fact]
    public void EnsureValid_ShouldNotCacheFailedProbe_WhenBinaryLaunchFails()
    {
        var ffmpeg = _binaries.CreateScript("ffmpeg", "ffmpeg version 6.0", exitCode: 1);
        var options = new FfmpegRunnerOptions { FfmpegPath = ffmpeg, FfprobePath = _binaries.CreateHealthyFfprobe() };

        Action first = () => options.EnsureValid();
        first.Should().Throw<FfmpegNotFoundException>();

        // Fixing the binary changes its write time, so the next probe must run and succeed.
        File.WriteAllText(ffmpeg, "#!/bin/sh\necho 'ffmpeg version 6.0'\nexit 0\n");
        File.SetUnixFileMode(ffmpeg, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);

        Action second = () => options.EnsureValid();
        second.Should().NotThrow();
    }

    [Fact]
    public void FfmpegVersionTryParse_ShouldExtractToken_WhenBannerIsRelease()
    {
        var version = FfmpegVersion.TryParse("ffmpeg version 6.1.1-1ubuntu1 Copyright (c) 2000-2023");

        version.Should().NotBeNull();
        version!.Raw.Should().Be("6.1.1-1ubuntu1");
        version.Number.Should().Be(new Version(6, 1, 1));
    }

    [Fact]
    public void FfmpegVersionTryParse_ShouldLeaveNumberNull_WhenBuildIsGitSnapshot()
    {
        var version = FfmpegVersion.TryParse("ffmpeg version N-112345-g1a2b3c4d5e Copyright");

        version.Should().NotBeNull();
        version!.Raw.Should().Be("N-112345-g1a2b3c4d5e");
        version.Number.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("usage: ffprobe")]
    public void FfmpegVersionTryParse_ShouldReturnNull_WhenNoBanner(string? output)
    {
        FfmpegVersion.TryParse(output).Should().BeNull();
    }
}
