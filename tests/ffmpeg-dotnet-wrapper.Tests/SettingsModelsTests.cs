using FFmpegDotnetWrapper.Constants;
using FFmpegDotnetWrapper.Exceptions;
using FFmpegDotnetWrapper.Models;
using FluentAssertions;
using Xunit;

namespace FFmpegDotnetWrapper.Tests;

public class TranscodeSettingsTests
{
    [Fact]
    public void Constructor_CreatesDefaultSettings()
    {
        var settings = new TranscodeSettings();

        settings.VideoCodec.Should().Be(VideoCodec.H264);
        settings.AudioCodec.Should().Be(AudioCodec.AAC);
        settings.Container.Should().Be(ContainerFormat.MP4);
        settings.VideoBitrate.Should().Be(FFmpegConstants.DefaultBitrate);
        settings.AudioBitrate.Should().Be(FFmpegConstants.DefaultAudioBitrate);
        settings.FrameRate.Should().Be(FFmpegConstants.DefaultFrameRate);
        settings.Quality.Should().Be(QualityPreset.Medium);
        settings.EnableAutoScale.Should().BeTrue();
        settings.PreserveAspectRatio.Should().BeTrue();
        settings.TwoPass.Should().BeFalse();
        settings.HardwareAcceleration.Should().Be(HwAccel.None);
    }

    [Theory]
    [InlineData(FFmpegConstants.MinBitrate)]
    [InlineData(FFmpegConstants.DefaultBitrate)]
    [InlineData(FFmpegConstants.MaxBitrate)]
    public void VideoBitrate_WithValidValue_AcceptsValue(int validBitrate)
    {
        var settings = new TranscodeSettings { VideoBitrate = validBitrate };

        settings.VideoBitrate.Should().Be(validBitrate);
    }

    [Theory]
    [InlineData(FFmpegConstants.MinBitrate - 1)]
    [InlineData(0)]
    [InlineData(FFmpegConstants.MaxBitrate + 1)]
    public void VideoBitrate_OutsideValidRange_ThrowsException(int invalidBitrate)
    {
        var settings = new TranscodeSettings();

        var act = () => settings.VideoBitrate = invalidBitrate;

        act.Should().Throw<InvalidOperationConfigurationException>();
    }

    [Theory]
    [InlineData(FFmpegConstants.MinAudioBitrate)]
    [InlineData(FFmpegConstants.DefaultAudioBitrate)]
    [InlineData(FFmpegConstants.MaxAudioBitrate)]
    public void AudioBitrate_WithValidValue_AcceptsValue(int validBitrate)
    {
        var settings = new TranscodeSettings { AudioBitrate = validBitrate };

        settings.AudioBitrate.Should().Be(validBitrate);
    }

    [Theory]
    [InlineData(FFmpegConstants.MinAudioBitrate - 1)]
    [InlineData(0)]
    [InlineData(FFmpegConstants.MaxAudioBitrate + 1)]
    public void AudioBitrate_OutsideValidRange_ThrowsException(int invalidBitrate)
    {
        var settings = new TranscodeSettings();

        var act = () => settings.AudioBitrate = invalidBitrate;

        act.Should().Throw<InvalidOperationConfigurationException>();
    }

    [Theory]
    [InlineData(FFmpegConstants.MinFrameRate)]
    [InlineData(FFmpegConstants.DefaultFrameRate)]
    [InlineData(FFmpegConstants.MaxFrameRate)]
    public void FrameRate_WithValidValue_AcceptsValue(int validFrameRate)
    {
        var settings = new TranscodeSettings { FrameRate = validFrameRate };

        settings.FrameRate.Should().Be(validFrameRate);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(FFmpegConstants.MaxFrameRate + 1)]
    public void FrameRate_OutsideValidRange_ThrowsException(int invalidFrameRate)
    {
        var settings = new TranscodeSettings();

        var act = () => settings.FrameRate = invalidFrameRate;

        act.Should().Throw<InvalidOperationConfigurationException>();
    }

    [Fact]
    public void Width_WithPositiveValue_AcceptsValue()
    {
        var settings = new TranscodeSettings { Width = 1920 };

        settings.Width.Should().Be(1920);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Width_WithZeroOrNegative_ThrowsException(int invalidWidth)
    {
        var settings = new TranscodeSettings();

        var act = () => settings.Validate();
        settings.Width = invalidWidth;

        // Width validation occurs in Validate() method
        var exception = Record.Exception(() => settings.Validate());
        exception.Should().BeOfType<InvalidOperationConfigurationException>();
    }

    [Fact]
    public void Validate_H264InMP4_IsValid()
    {
        var settings = new TranscodeSettings
        {
            VideoCodec = VideoCodec.H264,
            Container = ContainerFormat.MP4
        };

        var act = () => settings.Validate();

        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_VP9InWebM_IsValid()
    {
        var settings = new TranscodeSettings
        {
            VideoCodec = VideoCodec.VP9,
            Container = ContainerFormat.WebM
        };

        var act = () => settings.Validate();

        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_H264InWebM_ThrowsException()
    {
        var settings = new TranscodeSettings
        {
            VideoCodec = VideoCodec.H264,
            Container = ContainerFormat.WebM
        };

        var act = () => settings.Validate();

        act.Should().Throw<InvalidOperationConfigurationException>()
            .WithMessage("*H264*not supported*WebM*");
    }

    [Fact]
    public void Validate_AutoScaleWithTooSmallMaxDimensions_ThrowsException()
    {
        var settings = new TranscodeSettings
        {
            EnableAutoScale = true,
            MaxWidth = 200,
            MaxHeight = 100
        };

        var act = () => settings.Validate();

        act.Should().Throw<InvalidOperationConfigurationException>()
            .WithMessage("*too small*");
    }

    [Fact]
    public void Validate_AudioNormalizationWithValidLoudness_IsValid()
    {
        var settings = new TranscodeSettings
        {
            EnableAudioNormalization = true,
            TargetLoudness = -23.0
        };

        var act = () => settings.Validate();

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(-50.0)]
    [InlineData(0.0)]
    public void Validate_AudioNormalizationWithInvalidLoudness_ThrowsException(double loudness)
    {
        var settings = new TranscodeSettings
        {
            EnableAudioNormalization = true,
            TargetLoudness = loudness
        };

        var act = () => settings.Validate();

        act.Should().Throw<InvalidOperationConfigurationException>()
            .WithMessage("*loudness*");
    }

    [Fact]
    public void Clone_CreatesIndependentCopy()
    {
        var original = new TranscodeSettings
        {
            VideoCodec = VideoCodec.VP9,
            VideoBitrate = 8000,
            Width = 1280,
            TwoPass = true
        };

        var clone = original.Clone();

        clone.VideoCodec.Should().Be(VideoCodec.VP9);
        clone.VideoBitrate.Should().Be(8000);
        clone.Width.Should().Be(1280);
        clone.TwoPass.Should().BeTrue();

        clone.VideoBitrate = 6000;
        original.VideoBitrate.Should().Be(8000);
    }

    [Fact]
    public void HardwareAcceleration_SupportsAllValues()
    {
        var settings = new TranscodeSettings { HardwareAcceleration = HwAccel.NVENC };
        settings.HardwareAcceleration.Should().Be(HwAccel.NVENC);

        settings.HardwareAcceleration = HwAccel.VAAPI;
        settings.HardwareAcceleration.Should().Be(HwAccel.VAAPI);

        settings.HardwareAcceleration = HwAccel.Auto;
        settings.HardwareAcceleration.Should().Be(HwAccel.Auto);
    }

    [Fact]
    public void CustomFFmpegArgs_AllowsArbitraryArguments()
    {
        var customArgs = "-custom -args -here";
        var settings = new TranscodeSettings { CustomFFmpegArgs = customArgs };

        settings.CustomFFmpegArgs.Should().Be(customArgs);
    }
}

public class TrimSettingsTests
{
    private string _tempFile = null!;

    public TrimSettingsTests()
    {
        _tempFile = Path.Combine(Path.GetTempPath(), $"test_trim_{Guid.NewGuid()}.mp4");
        File.WriteAllText(_tempFile, "fake video");
    }

    ~TrimSettingsTests()
    {
        if (File.Exists(_tempFile))
            File.Delete(_tempFile);
    }

    [Fact]
    public void Constructor_CreatesDefaultSettings()
    {
        var settings = new TrimSettings();

        settings.StartTime.Should().Be(TimeSpan.Zero);
        settings.Duration.Should().BeNull();
        settings.EndTime.Should().BeNull();
        settings.PreserveAudio.Should().BeTrue();
        settings.PreserveVideo.Should().BeTrue();
        settings.Keyframe.Should().BeTrue();
    }

    [Fact]
    public void StartTime_WithPositiveValue_AcceptsValue()
    {
        var timespan = TimeSpan.FromSeconds(30);
        var settings = new TrimSettings { StartTime = timespan };

        settings.StartTime.Should().Be(timespan);
    }

    [Fact]
    public void StartTime_WithNegativeValue_ThrowsException()
    {
        var settings = new TrimSettings();

        var act = () => settings.StartTime = TimeSpan.FromSeconds(-5);

        act.Should().Throw<InvalidOperationConfigurationException>()
            .WithMessage("*negative*");
    }

    [Fact]
    public void Duration_WithPositiveValue_AcceptsValue()
    {
        var duration = TimeSpan.FromSeconds(60);
        var settings = new TrimSettings { Duration = duration };

        settings.Duration.Should().Be(duration);
    }

    [Fact]
    public void Duration_WithZeroOrNegativeValue_ThrowsException()
    {
        var settings = new TrimSettings();

        var act = () => settings.Duration = TimeSpan.Zero;

        act.Should().Throw<InvalidOperationConfigurationException>()
            .WithMessage("*greater than zero*");
    }

    [Fact]
    public void CalculateEndTime_WithDuration_ReturnsStartPlusDuration()
    {
        var settings = new TrimSettings
        {
            StartTime = TimeSpan.FromSeconds(10),
            Duration = TimeSpan.FromSeconds(60)
        };

        var endTime = settings.CalculateEndTime();

        endTime.Should().Be(TimeSpan.FromSeconds(70));
    }

    [Fact]
    public void CalculateEndTime_WithExplicitEndTime_ReturnsEndTime()
    {
        var settings = new TrimSettings
        {
            StartTime = TimeSpan.FromSeconds(10),
            EndTime = TimeSpan.FromSeconds(100)
        };

        var endTime = settings.CalculateEndTime();

        endTime.Should().Be(TimeSpan.FromSeconds(100));
    }

    [Fact]
    public void CalculateEndTime_WithoutDurationOrEndTime_ThrowsException()
    {
        var settings = new TrimSettings { StartTime = TimeSpan.FromSeconds(10) };

        var act = () => settings.CalculateEndTime();

        act.Should().Throw<InvalidOperationConfigurationException>()
            .WithMessage("*Duration*EndTime*");
    }

    [Fact]
    public void GetTrimmedDuration_ReturnsEndMinusStart()
    {
        var settings = new TrimSettings
        {
            StartTime = TimeSpan.FromSeconds(10),
            Duration = TimeSpan.FromSeconds(60)
        };

        var trimmedDuration = settings.GetTrimmedDuration();

        trimmedDuration.Should().Be(TimeSpan.FromSeconds(60));
    }

    [Fact]
    public void Validate_StartTimeExceedsMediaDuration_ThrowsException()
    {
        var mediaFile = new MediaFile(_tempFile) { Width = 1920, Height = 1080, Duration = TimeSpan.FromSeconds(100) };
        var settings = new TrimSettings { StartTime = TimeSpan.FromSeconds(150) };

        var act = () => settings.Validate(mediaFile);

        act.Should().Throw<InvalidOperationConfigurationException>()
            .WithMessage("*exceeds media duration*");
    }

    [Fact]
    public void Validate_TrimEndExceedsMediaDuration_ThrowsException()
    {
        var mediaFile = new MediaFile(_tempFile) { Width = 1920, Height = 1080, Duration = TimeSpan.FromSeconds(100) };
        var settings = new TrimSettings
        {
            StartTime = TimeSpan.FromSeconds(60),
            Duration = TimeSpan.FromSeconds(60)
        };

        var act = () => settings.Validate(mediaFile);

        act.Should().Throw<InvalidOperationConfigurationException>()
            .WithMessage("*exceeds media duration*");
    }

    [Fact]
    public void Validate_EndTimeBeforeStartTime_ThrowsException()
    {
        var mediaFile = new MediaFile(_tempFile) { Width = 1920, Height = 1080, Duration = TimeSpan.FromSeconds(100) };
        var settings = new TrimSettings
        {
            StartTime = TimeSpan.FromSeconds(50),
            EndTime = TimeSpan.FromSeconds(30)
        };

        var act = () => settings.Validate(mediaFile);

        act.Should().Throw<InvalidOperationConfigurationException>()
            .WithMessage("*after start time*");
    }

    [Fact]
    public void Validate_BothAudioAndVideoPreserved_IsValid()
    {
        var mediaFile = new MediaFile(_tempFile) { Width = 1920, Height = 1080, Duration = TimeSpan.FromSeconds(100) };
        var settings = new TrimSettings
        {
            StartTime = TimeSpan.FromSeconds(10),
            Duration = TimeSpan.FromSeconds(50),
            PreserveAudio = true,
            PreserveVideo = true
        };

        var act = () => settings.Validate(mediaFile);

        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_PreserveNeitherAudioNorVideo_ThrowsException()
    {
        var mediaFile = new MediaFile(_tempFile) { Width = 1920, Height = 1080, Duration = TimeSpan.FromSeconds(100) };
        var settings = new TrimSettings
        {
            StartTime = TimeSpan.FromSeconds(10),
            Duration = TimeSpan.FromSeconds(50),
            PreserveAudio = false,
            PreserveVideo = false
        };

        var act = () => settings.Validate(mediaFile);

        act.Should().Throw<InvalidOperationConfigurationException>()
            .WithMessage("*must be preserved*");
    }

    [Fact]
    public void Clone_CreatesIndependentCopy()
    {
        var original = new TrimSettings
        {
            StartTime = TimeSpan.FromSeconds(10),
            Duration = TimeSpan.FromSeconds(60),
            Keyframe = false
        };

        var clone = original.Clone();

        clone.StartTime.Should().Be(TimeSpan.FromSeconds(10));
        clone.Duration.Should().Be(TimeSpan.FromSeconds(60));
        clone.Keyframe.Should().BeFalse();

        clone.StartTime = TimeSpan.FromSeconds(20);
        original.StartTime.Should().Be(TimeSpan.FromSeconds(10));
    }
}

public class WatermarkSettingsTests
{
    private string _tempWatermarkFile = null!;

    public WatermarkSettingsTests()
    {
        _tempWatermarkFile = Path.Combine(Path.GetTempPath(), $"watermark_{Guid.NewGuid()}.png");
        File.WriteAllText(_tempWatermarkFile, "fake png");
    }

    ~WatermarkSettingsTests()
    {
        if (File.Exists(_tempWatermarkFile))
            File.Delete(_tempWatermarkFile);
    }

    [Fact]
    public void Constructor_CreatesDefaultSettings()
    {
        var settings = new WatermarkSettings { WatermarkPath = _tempWatermarkFile };

        settings.Position.Should().Be(WatermarkPosition.TopRight);
        settings.Opacity.Should().Be(1.0);
        settings.Scale.Should().Be(0.2);
        settings.PreserveAspectRatio.Should().BeTrue();
        settings.AnimateIn.Should().BeFalse();
    }

    [Fact]
    public void WatermarkPath_WithValidFile_AcceptsPath()
    {
        var settings = new WatermarkSettings { WatermarkPath = _tempWatermarkFile };

        settings.WatermarkPath.Should().NotBeEmpty();
        File.Exists(settings.WatermarkPath).Should().BeTrue();
    }

    [Fact]
    public void WatermarkPath_WithNonexistentFile_ThrowsException()
    {
        var settings = new WatermarkSettings();

        var act = () => settings.WatermarkPath = "/nonexistent/watermark.png";

        act.Should().Throw<InvalidOperationConfigurationException>()
            .WithMessage("*does not exist*");
    }

    [Fact]
    public void WatermarkPath_WithEmptyString_ThrowsException()
    {
        var settings = new WatermarkSettings();

        var act = () => settings.WatermarkPath = string.Empty;

        act.Should().Throw<InvalidOperationConfigurationException>()
            .WithMessage("*cannot be null or empty*");
    }

    [Fact]
    public void Opacity_WithValidRange_AcceptsValue()
    {
        var settings = new WatermarkSettings { WatermarkPath = _tempWatermarkFile };

        settings.Opacity = 0.5;
        settings.Opacity.Should().Be(0.5);

        settings.Opacity = 1.0;
        settings.Opacity.Should().Be(1.0);

        settings.Opacity = 0.0;
        settings.Opacity.Should().Be(0.0);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void Opacity_OutsideRange_ThrowsException(double invalidOpacity)
    {
        var settings = new WatermarkSettings { WatermarkPath = _tempWatermarkFile };

        var act = () => settings.Opacity = invalidOpacity;

        act.Should().Throw<InvalidOperationConfigurationException>()
            .WithMessage("*between 0 and 1*");
    }

    [Fact]
    public void CalculatePosition_TopRight_ReturnsCorrectCoordinates()
    {
        var settings = new WatermarkSettings
        {
            WatermarkPath = _tempWatermarkFile,
            Position = WatermarkPosition.TopRight,
            XOffset = 10,
            YOffset = 10
        };

        var (x, y) = settings.CalculatePosition(1920, 1080);

        x.Should().Be(1910);
        y.Should().Be(10);
    }

    [Fact]
    public void CalculatePosition_BottomLeft_ReturnsCorrectCoordinates()
    {
        var settings = new WatermarkSettings
        {
            WatermarkPath = _tempWatermarkFile,
            Position = WatermarkPosition.BottomLeft,
            XOffset = 10,
            YOffset = 10
        };

        var (x, y) = settings.CalculatePosition(1920, 1080);

        x.Should().Be(10);
        y.Should().Be(1070);
    }

    [Fact]
    public void CalculatePosition_Center_ReturnsCorrectCoordinates()
    {
        var settings = new WatermarkSettings
        {
            WatermarkPath = _tempWatermarkFile,
            Position = WatermarkPosition.Center,
            XOffset = 0,
            YOffset = 0
        };

        var (x, y) = settings.CalculatePosition(1920, 1080);

        x.Should().Be(960);
        y.Should().Be(540);
    }

    [Fact]
    public void Validate_WithValidSettings_DoesNotThrow()
    {
        var mediaFile = new MediaFile { Width = 1920, Height = 1080, Duration = TimeSpan.FromSeconds(100) };
        var settings = new WatermarkSettings
        {
            WatermarkPath = _tempWatermarkFile,
            Scale = 0.15,
            Opacity = 0.7
        };

        var act = () => settings.Validate(mediaFile);

        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_ScaleTooSmall_ThrowsException()
    {
        var mediaFile = new MediaFile { Width = 1920, Height = 1080, Duration = TimeSpan.FromSeconds(100) };
        var settings = new WatermarkSettings
        {
            WatermarkPath = _tempWatermarkFile,
            Scale = 0.001
        };

        var act = () => settings.Validate(mediaFile);

        act.Should().Throw<InvalidOperationConfigurationException>()
            .WithMessage("*Scale*");
    }

    [Fact]
    public void Validate_AnimateInWithoutDuration_ThrowsException()
    {
        var mediaFile = new MediaFile { Width = 1920, Height = 1080, Duration = TimeSpan.FromSeconds(100) };
        var settings = new WatermarkSettings
        {
            WatermarkPath = _tempWatermarkFile,
            AnimateIn = true
        };

        var act = () => settings.Validate(mediaFile);

        act.Should().Throw<InvalidOperationConfigurationException>()
            .WithMessage("*AnimateInDuration*");
    }

    [Fact]
    public void Clone_CreatesIndependentCopy()
    {
        var original = new WatermarkSettings
        {
            WatermarkPath = _tempWatermarkFile,
            Position = WatermarkPosition.BottomRight,
            Opacity = 0.8,
            Scale = 0.25
        };

        var clone = original.Clone();

        clone.Position.Should().Be(WatermarkPosition.BottomRight);
        clone.Opacity.Should().Be(0.8);
        clone.Scale.Should().Be(0.25);
    }
}

public class MergeSettingsTests
{
    private string _tempFile1 = null!;
    private string _tempFile2 = null!;

    public MergeSettingsTests()
    {
        _tempFile1 = Path.Combine(Path.GetTempPath(), $"video1_{Guid.NewGuid()}.mp4");
        _tempFile2 = Path.Combine(Path.GetTempPath(), $"video2_{Guid.NewGuid()}.mp4");
        File.WriteAllText(_tempFile1, "fake video");
        File.WriteAllText(_tempFile2, "fake video");
    }

    ~MergeSettingsTests()
    {
        foreach (var file in new[] { _tempFile1, _tempFile2 })
        {
            if (File.Exists(file))
                File.Delete(file);
        }
    }

    [Fact]
    public void Constructor_RequiresInputFiles()
    {
        var act = () => new MergeSettings { InputFiles = new() };

        act.Should().Throw<InvalidOperationConfigurationException>()
            .WithMessage("*required*");
    }

    [Fact]
    public void InputFiles_WithEmptyList_ThrowsException()
    {
        var settings = new MergeSettings { InputFiles = new List<string> { _tempFile1 } };

        var act = () => settings.InputFiles = new();

        act.Should().Throw<InvalidOperationConfigurationException>()
            .WithMessage("*required*");
    }

    [Fact]
    public void AddInputFile_WithValidFile_AddsToList()
    {
        var settings = new MergeSettings { InputFiles = new() { _tempFile1 } };

        settings.AddInputFile(_tempFile2);

        settings.InputFiles.Should().Contain(_tempFile2);
    }

    [Fact]
    public void AddInputFile_WithNonexistentFile_ThrowsException()
    {
        var settings = new MergeSettings { InputFiles = new() { _tempFile1 } };

        var act = () => settings.AddInputFile("/nonexistent/file.mp4");

        act.Should().Throw<InvalidOperationConfigurationException>()
            .WithMessage("*does not exist*");
    }

    [Fact]
    public void RemoveInputFile_RemovesFromList()
    {
        var settings = new MergeSettings { InputFiles = new() { _tempFile1, _tempFile2 } };

        settings.RemoveInputFile(_tempFile1);

        settings.InputFiles.Should().NotContain(_tempFile1);
        settings.InputFiles.Should().Contain(_tempFile2);
    }

    [Fact]
    public void Validate_WithTwoFilesOrMore_IsValid()
    {
        var settings = new MergeSettings { InputFiles = new() { _tempFile1, _tempFile2 } };

        var act = () => settings.Validate();

        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WithSingleFile_ThrowsException()
    {
        var settings = new MergeSettings { InputFiles = new() { _tempFile1 } };

        var act = () => settings.Validate();

        act.Should().Throw<InvalidOperationConfigurationException>()
            .WithMessage("*two input files*");
    }

    [Fact]
    public void Validate_WithNonexistentFile_ThrowsException()
    {
        var settings = new MergeSettings { InputFiles = new() { _tempFile1, "/nonexistent/file.mp4" } };

        var act = () => settings.Validate();

        act.Should().Throw<InvalidOperationConfigurationException>()
            .WithMessage("*does not exist*");
    }

    [Fact]
    public void Validate_PreserveNeitherAudioNorVideo_ThrowsException()
    {
        var settings = new MergeSettings
        {
            InputFiles = new() { _tempFile1, _tempFile2 },
            PreserveAudio = false,
            PreserveVideo = false
        };

        var act = () => settings.Validate();

        act.Should().Throw<InvalidOperationConfigurationException>()
            .WithMessage("*must be preserved*");
    }

    [Fact]
    public void Validate_TranscodeOnMergeWithoutSettings_ThrowsException()
    {
        var settings = new MergeSettings
        {
            InputFiles = new() { _tempFile1, _tempFile2 },
            TranscodeOnMerge = true,
            TranscodeSettings = null
        };

        var act = () => settings.Validate();

        act.Should().Throw<InvalidOperationConfigurationException>()
            .WithMessage("*TranscodeSettings*required*");
    }

    [Fact]
    public void Validate_CrossfadeWithZeroDuration_ThrowsException()
    {
        var settings = new MergeSettings
        {
            InputFiles = new() { _tempFile1, _tempFile2 },
            Crossfade = true,
            CrossfadeDuration = 0
        };

        var act = () => settings.Validate();

        act.Should().Throw<InvalidOperationConfigurationException>()
            .WithMessage("*greater than zero*");
    }

    [Fact]
    public void GetInputFileCount_ReturnsCorrectCount()
    {
        var settings = new MergeSettings { InputFiles = new() { _tempFile1, _tempFile2 } };

        settings.GetInputFileCount().Should().Be(2);
    }

    [Fact]
    public void ClearInputFiles_EmptiesTheList()
    {
        var settings = new MergeSettings { InputFiles = new() { _tempFile1, _tempFile2 } };

        settings.ClearInputFiles();

        settings.InputFiles.Should().BeEmpty();
    }

    [Fact]
    public void Clone_CreatesIndependentCopy()
    {
        var original = new MergeSettings
        {
            InputFiles = new() { _tempFile1, _tempFile2 },
            Crossfade = true,
            CrossfadeDuration = 1.5
        };

        var clone = original.Clone();

        clone.InputFiles.Should().HaveCount(2);
        clone.Crossfade.Should().BeTrue();
        clone.CrossfadeDuration.Should().Be(1.5);

        clone.InputFiles.Add(_tempFile1);
        original.InputFiles.Should().HaveCount(2);
    }
}
