namespace FFmpegDotnetWrapper.Tests;

using System;
using System.IO;

using FFmpegDotnetWrapper.Exceptions;
using FFmpegDotnetWrapper.Models;

using FluentAssertions;

using Xunit;

public class ConversionOptionsTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), "conversion-options-tests-" + Guid.NewGuid().ToString("N"));
    private readonly string _inputPath;

    public ConversionOptionsTests()
    {
        Directory.CreateDirectory(_tempDir);
        _inputPath = Path.Combine(_tempDir, "input.mp4");
        File.WriteAllText(_inputPath, "stub");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private ConversionOptions CreateValid() => new()
    {
        InputPath = _inputPath,
        OutputPath = Path.Combine(_tempDir, "output.mp4"),
    };

    // Applies one mutation to a valid options object and asserts that exactly one violation,
    // containing the expected text, is reported.
    private void AssertSingleViolation(Action<ConversionOptions> mutate, string expectedFragment)
    {
        var options = CreateValid();
        mutate(options);

        var violations = GetViolations(options);

        violations.Should().HaveCount(1);
        violations[0].Should().Contain(expectedFragment);
    }

    private static IReadOnlyList<string> GetViolations(ConversionOptions options)
    {
        Action act = () => options.Validate();
        return act.Should().Throw<OptionsValidationException>().Which.Violations;
    }

    [Fact]
    public void Validate_WhenOptionsAreValid_DoesNotThrow()
    {
        var options = CreateValid();

        Action act = () => options.Validate();

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenInputPathNotSet_ReportsInputPathMustBeSet(string? inputPath)
    {
        AssertSingleViolation(o => o.InputPath = inputPath, "InputPath must be set");
    }

    [Fact]
    public void Validate_WhenInputFileDoesNotExist_ReportsDoesNotExist()
    {
        var missing = Path.Combine(_tempDir, "missing.mp4");

        AssertSingleViolation(o => o.InputPath = missing, "InputPath does not exist");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_WhenOutputPathNotSet_ReportsOutputPathMustBeSet(string? outputPath)
    {
        AssertSingleViolation(o => o.OutputPath = outputPath, "OutputPath must be set");
    }

    [Fact]
    public void Validate_WhenOutputDirectoryMissingButCreatable_DoesNotThrow()
    {
        var options = CreateValid();
        options.OutputPath = Path.Combine(_tempDir, "nested", "deeper", "output.mp4");

        Action act = () => options.Validate();

        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WhenOutputDirectoryCannotBeCreated_ReportsCannotBeCreated()
    {
        // An ancestor of the output directory is a regular file, so the directory can never be created.
        var blocker = Path.Combine(_tempDir, "blocker");
        File.WriteAllText(blocker, "not a directory");

        AssertSingleViolation(
            o => o.OutputPath = Path.Combine(blocker, "sub", "output.mp4"),
            "Output directory does not exist and cannot be created");
    }

    [Fact]
    public void Validate_WhenOutputPathIsExistingDirectory_ReportsExistingDirectory()
    {
        AssertSingleViolation(o => o.OutputPath = _tempDir, "OutputPath is an existing directory");
    }

    [Fact]
    public void Validate_WhenOutputEqualsInput_ReportsOutputMustDifferFromInput()
    {
        AssertSingleViolation(o => o.OutputPath = _inputPath, "OutputPath must differ from InputPath");
    }

    [Fact]
    public void Validate_WhenVideoBitrateAndCrfBothSet_ReportsMutuallyExclusive()
    {
        AssertSingleViolation(o =>
        {
            o.VideoBitrateKbps = 2000;
            o.Crf = 23;
        }, "VideoBitrateKbps and Crf are mutually exclusive");
    }

    [Theory]
    [InlineData(FFmpegDotnetWrapper.Constants.FFmpegConstants.MinBitrate - 1)]
    [InlineData(FFmpegDotnetWrapper.Constants.FFmpegConstants.MaxBitrate + 1)]
    public void Validate_WhenVideoBitrateOutOfRange_ReportsRange(int bitrate)
    {
        AssertSingleViolation(o => o.VideoBitrateKbps = bitrate, "VideoBitrateKbps must be between");
    }

    [Theory]
    [InlineData(FFmpegDotnetWrapper.Constants.FFmpegConstants.MinBitrate)]
    [InlineData(FFmpegDotnetWrapper.Constants.FFmpegConstants.MaxBitrate)]
    public void Validate_WhenVideoBitrateAtBounds_DoesNotThrow(int bitrate)
    {
        var options = CreateValid();
        options.VideoBitrateKbps = bitrate;

        Action act = () => options.Validate();

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(52)]
    public void Validate_WhenCrfOutOfRange_ReportsRange(int crf)
    {
        AssertSingleViolation(o => o.Crf = crf, "Crf must be between 0 and 51");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    public void Validate_WhenCrfAtBounds_DoesNotThrow(int crf)
    {
        var options = CreateValid();
        options.Crf = crf;

        Action act = () => options.Validate();

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    [InlineData(1281)]
    public void Validate_WhenWidthNotPositiveEven_ReportsWidth(int width)
    {
        AssertSingleViolation(o => o.Width = width, "Width must be a positive even number");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    [InlineData(721)]
    public void Validate_WhenHeightNotPositiveEven_ReportsHeight(int height)
    {
        AssertSingleViolation(o => o.Height = height, "Height must be a positive even number");
    }

    [Fact]
    public void Validate_WhenResolutionIsPositiveAndEven_DoesNotThrow()
    {
        var options = CreateValid();
        options.Width = 1280;
        options.Height = 720;

        Action act = () => options.Validate();

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-30.0)]
    [InlineData(double.NaN)]
    public void Validate_WhenFrameRateNotPositive_ReportsFrameRate(double frameRate)
    {
        AssertSingleViolation(o => o.FrameRate = frameRate, "FrameRate must be greater than zero");
    }

    [Fact]
    public void Validate_WhenFrameRateIsFractional_DoesNotThrow()
    {
        var options = CreateValid();
        options.FrameRate = 23.976;

        Action act = () => options.Validate();

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-128)]
    public void Validate_WhenAudioBitrateNotPositive_ReportsAudioBitrate(int bitrate)
    {
        AssertSingleViolation(o => o.AudioBitrateKbps = bitrate, "AudioBitrateKbps must be positive");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-44100)]
    public void Validate_WhenAudioSampleRateNotPositive_ReportsAudioSampleRate(int sampleRate)
    {
        AssertSingleViolation(o => o.AudioSampleRate = sampleRate, "AudioSampleRate must be positive");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public void Validate_WhenAudioChannelsNotPositive_ReportsAudioChannels(int channels)
    {
        AssertSingleViolation(o => o.AudioChannels = channels, "AudioChannels must be positive");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenVideoCodecEmptyWhenSpecified_ReportsVideoCodec(string codec)
    {
        AssertSingleViolation(o => o.VideoCodec = codec, "VideoCodec must not be empty");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenAudioCodecEmptyWhenSpecified_ReportsAudioCodec(string codec)
    {
        AssertSingleViolation(o => o.AudioCodec = codec, "AudioCodec must not be empty");
    }

    [Fact]
    public void Validate_WhenCodecsAreNull_DoesNotThrow()
    {
        var options = CreateValid();

        Action act = () => options.Validate();

        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WhenStartTimeNegative_ReportsNegativeStart()
    {
        AssertSingleViolation(o => o.StartTime = TimeSpan.FromSeconds(-1), "StartTime must not be negative");
    }

    [Fact]
    public void Validate_WhenEndTimeEqualsStartTime_ReportsEndAfterStart()
    {
        AssertSingleViolation(o =>
        {
            o.StartTime = TimeSpan.FromSeconds(10);
            o.EndTime = TimeSpan.FromSeconds(10);
        }, "must be after StartTime");
    }

    [Fact]
    public void Validate_WhenEndTimeBeforeStartTime_ReportsEndAfterStart()
    {
        AssertSingleViolation(o =>
        {
            o.StartTime = TimeSpan.FromSeconds(10);
            o.EndTime = TimeSpan.FromSeconds(5);
        }, "must be after StartTime");
    }

    [Fact]
    public void Validate_WhenEndTimeZeroWithoutStartTime_ReportsEndAfterStart()
    {
        // StartTime defaults to zero, so an EndTime of zero is an empty range.
        AssertSingleViolation(o => o.EndTime = TimeSpan.Zero, "must be after StartTime");
    }

    [Fact]
    public void Validate_WhenTimeRangeWithinDuration_DoesNotThrow()
    {
        var options = CreateValid();
        options.StartTime = TimeSpan.FromSeconds(5);
        options.EndTime = TimeSpan.FromSeconds(60);
        options.SourceDuration = TimeSpan.FromSeconds(60);

        Action act = () => options.Validate();

        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WhenSourceDurationNotPositive_ReportsSourceDuration()
    {
        AssertSingleViolation(o => o.SourceDuration = TimeSpan.Zero, "SourceDuration must be greater than zero");
    }

    [Fact]
    public void Validate_WhenStartTimeAtOrAfterSourceDuration_ReportsStartExceedsDuration()
    {
        AssertSingleViolation(o =>
        {
            o.StartTime = TimeSpan.FromSeconds(60);
            o.SourceDuration = TimeSpan.FromSeconds(60);
        }, "StartTime (00:01:00) exceeds the source duration");
    }

    [Fact]
    public void Validate_WhenEndTimeAfterSourceDuration_ReportsEndExceedsDuration()
    {
        AssertSingleViolation(o =>
        {
            o.EndTime = TimeSpan.FromSeconds(61);
            o.SourceDuration = TimeSpan.FromSeconds(60);
        }, "EndTime (00:01:01) exceeds the source duration");
    }

    [Fact]
    public void Validate_WhenDurationUnknown_DoesNotCheckTimeRangeAgainstDuration()
    {
        var options = CreateValid();
        options.StartTime = TimeSpan.FromHours(10);
        options.EndTime = TimeSpan.FromHours(11);

        Action act = () => options.Validate();

        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WhenSeveralRulesViolated_ReportsAllInOneException()
    {
        var options = new ConversionOptions
        {
            InputPath = null,
            OutputPath = Path.Combine(_tempDir, "output.mp4"),
            Crf = 60,
            Width = 3,
            FrameRate = 0,
            AudioChannels = 0,
            VideoCodec = ""
        };

        var violations = GetViolations(options);

        violations.Should().HaveCount(6);
        violations.Should().Contain(v => v.Contains("InputPath must be set"));
        violations.Should().Contain(v => v.Contains("Crf must be between"));
        violations.Should().Contain(v => v.Contains("Width must be"));
        violations.Should().Contain(v => v.Contains("FrameRate must be"));
        violations.Should().Contain(v => v.Contains("AudioChannels must be"));
        violations.Should().Contain(v => v.Contains("VideoCodec must not be empty"));
    }

    [Fact]
    public void Validate_WhenSeveralRulesViolated_MessageListsEachViolationOnItsOwnLine()
    {
        var options = CreateValid();
        options.Crf = 60;
        options.Width = 3;

        Action act = () => options.Validate();

        var exception = act.Should().Throw<OptionsValidationException>().Which;
        exception.Message.Should().Contain("2 violation(s)");
        foreach (var violation in exception.Violations)
            exception.Message.Should().Contain("  - " + violation);
    }
}
