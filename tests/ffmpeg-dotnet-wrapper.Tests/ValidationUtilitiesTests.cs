// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Utilities;
using FluentAssertions;
using Xunit;

namespace FFmpegDotnetWrapper.Tests;

/// <summary>
/// Contains unit tests for the <see cref="ValidationUtilities"/> class.
/// Tests validation methods for bitrate, codec, output format, time parsing/formatting,
/// resolution, trim times, watermark scale, and opacity values.
/// </summary>
public class ValidationUtilitiesTests
{
    // -------------------------------------------------------------------------
    // IsValidBitrate
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that <see cref="ValidationUtilities.IsValidBitrate"/> returns true for valid bitrate values within the allowed range (1-50000).
    /// </summary>
    /// <param name="bitrate">The bitrate value to test.</param>
    [Theory]
    [InlineData(1)]
    [InlineData(128)]
    [InlineData(50000)]
    public void IsValidBitrate_WithinRange_ReturnsTrue(int bitrate)
    {
        ValidationUtilities.IsValidBitrate(bitrate).Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="ValidationUtilities.IsValidBitrate"/> returns false for invalid bitrate values outside the allowed range (1-50000).
    /// </summary>
    /// <param name="bitrate">The bitrate value to test.</param>
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

    /// <summary>
    /// Tests that <see cref="ValidationUtilities.IsValidCodec"/> returns true for supported video codec names.
    /// Supported codecs include: h264, hevc, vp9, and av1 (case-insensitive).
    /// </summary>
    /// <param name="codec">The codec name to test.</param>
    [Theory]
    [InlineData("h264")]
    [InlineData("H264")]
    [InlineData("hevc")]
    [InlineData("vp9")]
    [InlineData("av1")]
    public void IsValidCodec_SupportedCodec_ReturnsTrue(string codec)
    {
        ArgumentException.ThrowIfNullOrEmpty(codec);
        ValidationUtilities.IsValidCodec(codec).Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="ValidationUtilities.IsValidCodec"/> returns false for unsupported codec names or empty/null values.
    /// Unsupported codecs include: xvid, divx, and any null or empty strings.
    /// </summary>
    /// <param name="codec">The codec name to test.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("xvid")]
    [InlineData("divx")]
    public void IsValidCodec_UnsupportedOrEmpty_ReturnsFalse(string? codec)
    {
        if (codec != null)
        {
            ArgumentException.ThrowIfNullOrEmpty(codec);
        }
        ValidationUtilities.IsValidCodec(codec).Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // IsValidOutputFormat
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that <see cref="ValidationUtilities.IsValidOutputFormat"/> returns true for supported output format strings.
    /// Supported formats include: mp4, mkv, webm, and ts (case-insensitive).
    /// </summary>
    /// <param name="format">The output format string to test.</param>
    [Theory]
    [InlineData("mp4")]
    [InlineData("MKV")]
    [InlineData("webm")]
    [InlineData("ts")]
    public void IsValidOutputFormat_SupportedFormat_ReturnsTrue(string format)
    {
        ArgumentException.ThrowIfNullOrEmpty(format);
        ValidationUtilities.IsValidOutputFormat(format).Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="ValidationUtilities.IsValidOutputFormat"/> returns false for unrecognized output format strings.
    /// </summary>
    [Fact]
    public void IsValidOutputFormat_UnrecognizedFormat_ReturnsFalse()
    {
        ArgumentException.ThrowIfNullOrEmpty("xyz");
        ValidationUtilities.IsValidOutputFormat("xyz").Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // ParseTimeToSeconds
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that <see cref="ValidationUtilities.ParseTimeToSeconds"/> correctly parses time strings in hh:mm:ss format and returns the equivalent seconds.
    /// </summary>
    /// <param name="timeString">The time string in hh:mm:ss format to parse.</param>
    /// <param name="expected">The expected number of seconds.</param>
    [Theory]
    [InlineData("01:30:00", 5400.0)]
    [InlineData("00:00:45", 45.0)]
    [InlineData("02:00:30", 7230.0)]
    public void ParseTimeToSeconds_HhMmSsFormat_ReturnsCorrectSeconds(string timeString, double expected)
    {
        ArgumentException.ThrowIfNullOrEmpty(timeString);
        var result = ValidationUtilities.ParseTimeToSeconds(timeString);

        result.Should().NotBeNull();
        result!.Value.Should().BeApproximately(expected, 0.001);
    }

    /// <summary>
    /// Tests that <see cref="ValidationUtilities.ParseTimeToSeconds"/> correctly parses pure seconds strings (numeric values) and returns the equivalent seconds.
    /// </summary>
    /// <param name="timeString">The time string representing pure seconds to parse.</param>
    /// <param name="expected">The expected number of seconds.</param>
    [Theory]
    [InlineData("90", 90.0)]
    [InlineData("3600", 3600.0)]
    [InlineData("0", 0.0)]
    public void ParseTimeToSeconds_PureSecondsString_ReturnsValue(string timeString, double expected)
    {
        ArgumentException.ThrowIfNullOrEmpty(timeString);
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
    /// <summary>
    /// Tests that <see cref="ValidationUtilities.ParseTimeToSeconds"/> returns null for invalid or empty time strings.
    /// Invalid inputs include: null, empty strings, whitespace, malformed time strings, and negative numbers.
    /// </summary>
    /// <param name="timeString">The invalid time string to test.</param>
    /// <summary>
    /// Tests that <see cref="ValidationUtilities.ParseTimeToSeconds"/> returns null for invalid or empty time strings.
    /// Invalid inputs include: null, empty strings, whitespace, malformed time strings, and negative numbers.
    /// </summary>
    /// <param name="timeString">The invalid time string to test.</param>
    public void ParseTimeToSeconds_InvalidOrEmpty_ReturnsNull(string? timeString)
    {
        ValidationUtilities.ParseTimeToSeconds(timeString).Should().BeNull();
    }

    // -------------------------------------------------------------------------
    // FormatSecondsToTime
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that <see cref="ValidationUtilities.FormatSecondsToTime"/> correctly formats seconds into hh:mm:ss format strings.
    /// </summary>
    /// <param name="seconds">The number of seconds to format.</param>
    /// <param name="expected">The expected formatted time string in hh:mm:ss format.</param>
    [Theory]
    [InlineData(0, "00:00:00")]
    [InlineData(65, "00:01:05")]
    [InlineData(3661, "01:01:01")]
    public void FormatSecondsToTime_VariousValues_ReturnsHhMmSs(double seconds, string expected)
    {
        ValidationUtilities.FormatSecondsToTime(seconds).Should().Be(expected);
    }

    /// <summary>
    /// Tests that <see cref="ValidationUtilities.FormatSecondsToTime"/> clamps negative values to zero and returns "00:00:00".
    /// </summary>
    [Fact]
    public void FormatSecondsToTime_NegativeSeconds_ClampsToZero()
    {
        ValidationUtilities.FormatSecondsToTime(-10).Should().Be("00:00:00");
    }

    // -------------------------------------------------------------------------
    // IsValidResolution
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that <see cref="ValidationUtilities.IsValidResolution"/> returns true for valid resolution strings in WIDTHxHEIGHT format.
    /// Valid resolutions include common formats like 1920x1080, 3840x2160, and 640x480.
    /// </summary>
    /// <param name="resolution">The resolution string in WIDTHxHEIGHT format to test.</param>
    [Theory]
    [InlineData("1920x1080")]
    [InlineData("3840x2160")]
    [InlineData("640x480")]
    public void IsValidResolution_ValidFormat_ReturnsTrue(string resolution)
    {
        ValidationUtilities.IsValidResolution(resolution).Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="ValidationUtilities.IsValidResolution"/> returns false for invalid resolution strings.
    /// Invalid inputs include: null, empty strings, zero dimensions, malformed format (using dash instead of 'x'),
    /// and non-numeric values.
    /// </summary>
    /// <param name="resolution">The invalid resolution string to test.</param>
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

    /// <summary>
    /// Tests that <see cref="ValidationUtilities.ValidateTrimTimes"/> returns true when start time is before end time.
    /// </summary>
    [Fact]
    public void ValidateTrimTimes_StartBeforeEnd_ReturnsTrue()
    {
        ValidationUtilities.ValidateTrimTimes(10.0, 60.0, null).Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="ValidationUtilities.ValidateTrimTimes"/> returns false when start time is greater than end time.
    /// </summary>
    [Fact]
    public void ValidateTrimTimes_StartGreaterThanEnd_ReturnsFalse()
    {
        ValidationUtilities.ValidateTrimTimes(90.0, 30.0, null).Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="ValidationUtilities.ValidateTrimTimes"/> returns false when start time is negative.
    /// </summary>
    [Fact]
    public void ValidateTrimTimes_NegativeStart_ReturnsFalse()
    {
        ValidationUtilities.ValidateTrimTimes(-5.0, 60.0, null).Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="ValidationUtilities.ValidateTrimTimes"/> returns true when only duration is provided (start=0, end=null).
    /// </summary>
    [Fact]
    public void ValidateTrimTimes_WithDurationOnly_ReturnsTrue()
    {
        ValidationUtilities.ValidateTrimTimes(0.0, null, 30.0).Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="ValidationUtilities.ValidateTrimTimes"/> returns false when neither end time nor duration is provided.
    /// </summary>
    [Fact]
    public void ValidateTrimTimes_NoEndOrDuration_ReturnsFalse()
    {
        ValidationUtilities.ValidateTrimTimes(10.0, null, null).Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // IsValidWatermarkScale / IsValidOpacity
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that <see cref="ValidationUtilities.IsValidWatermarkScale"/> returns true for valid watermark scale values in the range (0.0, 1.0].
    /// Valid scales include: 0.01, 0.5, and 1.0.
    /// </summary>
    /// <param name="scale">The watermark scale value to test.</param>
    [Theory]
    [InlineData(0.01)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    public void IsValidWatermarkScale_ValidRange_ReturnsTrue(double scale)
    {
        ValidationUtilities.IsValidWatermarkScale(scale).Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="ValidationUtilities.IsValidWatermarkScale"/> returns false for invalid watermark scale values outside the range (0.0, 1.0].
    /// Invalid scales include: 0.0 (boundary) and 1.01 (above maximum).
    /// </summary>
    /// <param name="scale">The watermark scale value to test.</param>
    [Theory]
    [InlineData(0.0)]
    [InlineData(1.01)]
    public void IsValidWatermarkScale_OutsideRange_ReturnsFalse(double scale)
    {
        ValidationUtilities.IsValidWatermarkScale(scale).Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="ValidationUtilities.IsValidOpacity"/> returns true for valid opacity values in the range [0.0, 1.0].
    /// Valid opacity values include: 0.0 (fully transparent), 0.75, and 1.0 (fully opaque).
    /// </summary>
    /// <param name="opacity">The opacity value to test.</param>
    [Theory]
    [InlineData(0.0)]
    [InlineData(0.75)]
    [InlineData(1.0)]
    public void IsValidOpacity_ValidRange_ReturnsTrue(double opacity)
    {
        ValidationUtilities.IsValidOpacity(opacity).Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="ValidationUtilities.IsValidOpacity"/> returns false for invalid opacity values outside the range [0.0, 1.0].
    /// Invalid opacity values include: -0.01 (below minimum) and 1.01 (above maximum).
    /// </summary>
    /// <param name="opacity">The opacity value to test.</param>
    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    public void IsValidOpacity_OutsideRange_ReturnsFalse(double opacity)
    {
        ValidationUtilities.IsValidOpacity(opacity).Should().BeFalse();
    }
}
