using FFmpegDotnetWrapper.Constants;
using FFmpegDotnetWrapper.Exceptions;
using FFmpegDotnetWrapper.Models;
using FluentAssertions;
using Xunit;

namespace FFmpegDotnetWrapper.Tests;

/// <summary>
/// Contains unit tests for the <see cref="TranscodeSettings"/> class.
/// Tests various properties, validation rules, and behaviors of transcoding configuration.
/// </summary>
public class TranscodeSettingsTests
{
	/// <summary>
	/// Tests that the default constructor creates settings with expected default values.
	/// </summary>
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

	/// <summary>
	/// Tests that video bitrate can be set to valid values within the allowed range.
	/// </summary>
	/// <param name="validBitrate">A valid bitrate value within the allowed range.</param>
	[Theory]
	[InlineData(FFmpegConstants.MinBitrate)]
	[InlineData(FFmpegConstants.DefaultBitrate)]
	[InlineData(FFmpegConstants.MaxBitrate)]
	public void VideoBitrate_WithValidValue_AcceptsValue(int validBitrate)
	{
		var settings = new TranscodeSettings { VideoBitrate = validBitrate };

		settings.VideoBitrate.Should().Be(validBitrate);
	}

	/// <summary>
	/// Tests that video bitrate throws exception when set to values outside the valid range.
	/// </summary>
	/// <param name="invalidBitrate">An invalid bitrate value outside the allowed range.</param>
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

	/// <summary>
	/// Tests that audio bitrate can be set to valid values within the allowed range.
	/// </summary>
	/// <param name="validBitrate">A valid audio bitrate value within the allowed range.</param>
	[Theory]
	[InlineData(FFmpegConstants.MinAudioBitrate)]
	[InlineData(FFmpegConstants.DefaultAudioBitrate)]
	[InlineData(FFmpegConstants.MaxAudioBitrate)]
	public void AudioBitrate_WithValidValue_AcceptsValue(int validBitrate)
	{
		var settings = new TranscodeSettings { AudioBitrate = validBitrate };

		settings.AudioBitrate.Should().Be(validBitrate);
	}

	/// <summary>
	/// Tests that audio bitrate throws exception when set to values outside the valid range.
	/// </summary>
	/// <param name="invalidBitrate">An invalid audio bitrate value outside the allowed range.</param>
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

	/// <summary>
	/// Tests that frame rate can be set to valid values within the allowed range.
	/// </summary>
	/// <param name="validFrameRate">A valid frame rate value within the allowed range.</param>
	[Theory]
	[InlineData(FFmpegConstants.MinFrameRate)]
	[InlineData(FFmpegConstants.DefaultFrameRate)]
	[InlineData(FFmpegConstants.MaxFrameRate)]
	public void FrameRate_WithValidValue_AcceptsValue(int validFrameRate)
	{
		var settings = new TranscodeSettings { FrameRate = validFrameRate };

		settings.FrameRate.Should().Be(validFrameRate);
	}

	/// <summary>
	/// Tests that frame rate throws exception when set to invalid values.
	/// </summary>
	/// <param name="invalidFrameRate">An invalid frame rate value (zero, negative, or exceeding maximum).</param>
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

	/// <summary>
	/// Tests that width can be set to a positive value.
	/// </summary>
	[Fact]
	public void Width_WithPositiveValue_AcceptsValue()
	{
		var settings = new TranscodeSettings { Width = 1920 };

		settings.Width.Should().Be(1920);
	}

	/// <summary>
	/// Tests that width throws exception when set to zero or negative values.
	/// </summary>
	/// <param name="invalidWidth">A zero or negative width value.</param>
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

	/// <summary>
	/// Tests that H264 codec in MP4 container is considered valid configuration.
	/// </summary>
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

	/// <summary>
	/// Tests that VP9 codec in WebM container is considered valid configuration.
	/// </summary>
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

	/// <summary>
	/// Tests that H264 codec in WebM container throws exception as it's not supported.
	/// </summary>
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

	/// <summary>
	/// Tests that auto-scaling with too small max dimensions throws exception.
	/// </summary>
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

	/// <summary>
	/// Tests that audio normalization with valid loudness level is considered valid.
	/// </summary>
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

	/// <summary>
	/// Tests that audio normalization with invalid loudness level throws exception.
	/// </summary>
	/// <param name="loudness">An invalid loudness value (too low or zero).</param>
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

	/// <summary>
	/// Tests that Clone method creates an independent copy of the settings.
	/// </summary>
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

	/// <summary>
	/// Tests that hardware acceleration property supports all valid acceleration types.
	/// </summary>
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

	/// <summary>
	/// Tests that custom FFmpeg arguments can be set to arbitrary values.
	/// </summary>
	[Fact]
	public void CustomFFmpegArgs_AllowsArbitraryArguments()
	{
		var customArgs = "-custom -args -here";
		var settings = new TranscodeSettings { CustomFFmpegArgs = customArgs };

		settings.CustomFFmpegArgs.Should().Be(customArgs);
	}
}