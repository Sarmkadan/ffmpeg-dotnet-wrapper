// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Utilities;
using FluentAssertions;
using Xunit;

namespace FFmpegDotnetWrapper.Tests;

public class ValidationUtilitiesTests
{
    // -------------------------------------------------------------------------
    // IsValidBitrate
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(1)]
    [InlineData(128)]
    [InlineData(50000)]
    public void IsValidBitrate_WithinRange_ReturnsTrue(int bitrate)
    {
        ValidationUtilities.IsValidBitrate(bitrate).Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(50001)]
    public void IsValidBitrate_OutsideRange_ReturnsFalse(int bitrate)
    {
        ValidationUtilities.IsValidBitrate(bitrate).Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // IsValidCodec
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("h264")]
    [InlineData("H264")]
    [InlineData("hevc")]
    [InlineData("vp9")]
    [InlineData("av1")]
    public void IsValidCodec_SupportedCodec_ReturnsTrue(string codec)
    {
        ValidationUtilities.IsValidCodec(codec).Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("xvid")]
    [InlineData("divx")]
    public void IsValidCodec_UnsupportedOrEmpty_ReturnsFalse(string? codec)
    {
        ValidationUtilities.IsValidCodec(codec).Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // IsValidOutputFormat
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("mp4")]
    [InlineData("MKV")]
    [InlineData("webm")]
    [InlineData("ts")]
    public void IsValidOutputFormat_SupportedFormat_ReturnsTrue(string format)
    {
        ValidationUtilities.IsValidOutputFormat(format).Should().BeTrue();
    }

    [Fact]
    public void IsValidOutputFormat_UnrecognizedFormat_ReturnsFalse()
    {
        ValidationUtilities.IsValidOutputFormat("xyz").Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // ParseTimeToSeconds
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("01:30:00", 5400.0)]
    [InlineData("00:00:45", 45.0)]
    [InlineData("02:00:30", 7230.0)]
    public void ParseTimeToSeconds_HhMmSsFormat_ReturnsCorrectSeconds(string timeString, double expected)
    {
        var result = ValidationUtilities.ParseTimeToSeconds(timeString);

        result.Should().NotBeNull();
        result!.Value.Should().BeApproximately(expected, 0.001);
    }

    [Theory]
    [InlineData("90", 90.0)]
    [InlineData("3600", 3600.0)]
    [InlineData("0", 0.0)]
    public void ParseTimeToSeconds_PureSecondsString_ReturnsValue(string timeString, double expected)
    {
        var result = ValidationUtilities.ParseTimeToSeconds(timeString);

        result.Should().NotBeNull();
        result!.Value.Should().BeApproximately(expected, 0.001);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid")]
    [InlineData("1:2:3:4")]
    [InlineData("-5")]
    public void ParseTimeToSeconds_InvalidOrEmpty_ReturnsNull(string? timeString)
    {
        ValidationUtilities.ParseTimeToSeconds(timeString).Should().BeNull();
    }

    // -------------------------------------------------------------------------
    // FormatSecondsToTime
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(0, "00:00:00")]
    [InlineData(65, "00:01:05")]
    [InlineData(3661, "01:01:01")]
    public void FormatSecondsToTime_VariousValues_ReturnsHhMmSs(double seconds, string expected)
    {
        ValidationUtilities.FormatSecondsToTime(seconds).Should().Be(expected);
    }

    [Fact]
    public void FormatSecondsToTime_NegativeSeconds_ClampsToZero()
    {
        ValidationUtilities.FormatSecondsToTime(-10).Should().Be("00:00:00");
    }

    // -------------------------------------------------------------------------
    // IsValidResolution
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("1920x1080")]
    [InlineData("3840x2160")]
    [InlineData("640x480")]
    public void IsValidResolution_ValidFormat_ReturnsTrue(string resolution)
    {
        ValidationUtilities.IsValidResolution(resolution).Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("0x1080")]
    [InlineData("1920x0")]
    [InlineData("1920-1080")]
    [InlineData("abcxdef")]
    public void IsValidResolution_InvalidFormat_ReturnsFalse(string? resolution)
    {
        ValidationUtilities.IsValidResolution(resolution).Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // ValidateTrimTimes
    // -------------------------------------------------------------------------

    [Fact]
    public void ValidateTrimTimes_StartBeforeEnd_ReturnsTrue()
    {
        ValidationUtilities.ValidateTrimTimes(10.0, 60.0, null).Should().BeTrue();
    }

    [Fact]
    public void ValidateTrimTimes_StartGreaterThanEnd_ReturnsFalse()
    {
        ValidationUtilities.ValidateTrimTimes(90.0, 30.0, null).Should().BeFalse();
    }

    [Fact]
    public void ValidateTrimTimes_NegativeStart_ReturnsFalse()
    {
        ValidationUtilities.ValidateTrimTimes(-5.0, 60.0, null).Should().BeFalse();
    }

    [Fact]
    public void ValidateTrimTimes_WithDurationOnly_ReturnsTrue()
    {
        ValidationUtilities.ValidateTrimTimes(0.0, null, 30.0).Should().BeTrue();
    }

    [Fact]
    public void ValidateTrimTimes_NoEndOrDuration_ReturnsFalse()
    {
        ValidationUtilities.ValidateTrimTimes(10.0, null, null).Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // IsValidWatermarkScale / IsValidOpacity
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(0.01)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    public void IsValidWatermarkScale_ValidRange_ReturnsTrue(double scale)
    {
        ValidationUtilities.IsValidWatermarkScale(scale).Should().BeTrue();
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(1.01)]
    public void IsValidWatermarkScale_OutsideRange_ReturnsFalse(double scale)
    {
        ValidationUtilities.IsValidWatermarkScale(scale).Should().BeFalse();
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.75)]
    [InlineData(1.0)]
    public void IsValidOpacity_ValidRange_ReturnsTrue(double opacity)
    {
        ValidationUtilities.IsValidOpacity(opacity).Should().BeTrue();
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    public void IsValidOpacity_OutsideRange_ReturnsFalse(double opacity)
    {
        ValidationUtilities.IsValidOpacity(opacity).Should().BeFalse();
    }
}
