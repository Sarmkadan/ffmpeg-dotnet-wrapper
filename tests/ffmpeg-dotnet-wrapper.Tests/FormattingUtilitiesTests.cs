// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Utilities;
using FluentAssertions;
using Xunit;

namespace FFmpegDotnetWrapper.Tests;

public class FormattingUtilitiesTests
{
    // -------------------------------------------------------------------------
    // FormatDuration
    // -------------------------------------------------------------------------

    [Fact]
    public void FormatDuration_LessThanOneMinute_ReturnsZeroHoursAndMinutes()
    {
        var duration = TimeSpan.FromSeconds(45);

        FormattingUtilities.FormatDuration(duration).Should().Be("00:00:45");
    }

    [Fact]
    public void FormatDuration_BetweenOneAndSixtyMinutes_ReturnsZeroHours()
    {
        var duration = TimeSpan.FromSeconds(90);

        FormattingUtilities.FormatDuration(duration).Should().Be("00:01:30");
    }

    [Fact]
    public void FormatDuration_MoreThanOneHour_IncludesHours()
    {
        var duration = TimeSpan.FromSeconds(3661);

        FormattingUtilities.FormatDuration(duration).Should().Be("01:01:01");
    }

    // -------------------------------------------------------------------------
    // FormatBytes
    // -------------------------------------------------------------------------

    [Fact]
    public void FormatBytes_LessThanOneKilobyte_ReturnsByteSuffix()
    {
        FormattingUtilities.FormatBytes(512).Should().Be("512 B");
    }

    [Fact]
    public void FormatBytes_ExactMegabyte_ReturnsMbSuffix()
    {
        FormattingUtilities.FormatBytes(1024 * 1024).Should().Be("1 MB");
    }

    [Fact]
    public void FormatBytes_LargeGigabyteValue_ReturnsGbSuffix()
    {
        FormattingUtilities.FormatBytes(2L * 1024 * 1024 * 1024).Should().Be("2 GB");
    }

    // -------------------------------------------------------------------------
    // FormatBitrate
    // -------------------------------------------------------------------------

    [Fact]
    public void FormatBitrate_BelowOneThousand_ReturnsKbps()
    {
        FormattingUtilities.FormatBitrate(500).Should().Be("500 Kbps");
    }

    [Fact]
    public void FormatBitrate_Thousands_ReturnsMbps()
    {
        FormattingUtilities.FormatBitrate(5000).Should().Be("5 Mbps");
    }

    [Fact]
    public void FormatBitrate_Millions_ReturnsGbps()
    {
        FormattingUtilities.FormatBitrate(2_000_000).Should().Be("2 Gbps");
    }

    // -------------------------------------------------------------------------
    // TruncateString
    // -------------------------------------------------------------------------

    [Fact]
    public void TruncateString_BelowMaxLength_ReturnsUnchanged()
    {
        const string input = "short string";

        FormattingUtilities.TruncateString(input, 80).Should().Be(input);
    }

    [Fact]
    public void TruncateString_ExceedsMaxLength_AppendsEllipsis()
    {
        var input = new string('a', 100);

        var result = FormattingUtilities.TruncateString(input, 80);

        result.Should().HaveLength(80);
        result.Should().EndWith("...");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void TruncateString_NullOrEmpty_ReturnsEmptyString(string? input)
    {
        FormattingUtilities.TruncateString(input).Should().BeEmpty();
    }

    // -------------------------------------------------------------------------
    // TitleCase
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("output-format", "Output Format")]
    [InlineData("video_codec", "Video Codec")]
    [InlineData("transcode", "Transcode")]
    public void TitleCase_KebabOrSnakeCase_ReturnsTitleCase(string input, string expected)
    {
        FormattingUtilities.TitleCase(input).Should().Be(expected);
    }

    // -------------------------------------------------------------------------
    // FormatPercentage
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(0.0, "0.0%")]
    [InlineData(50.0, "50.0%")]
    [InlineData(100.0, "100.0%")]
    public void FormatPercentage_VariousValues_ReturnsOneDecimalPlace(double percentage, string expected)
    {
        FormattingUtilities.FormatPercentage(percentage).Should().Be(expected);
    }

    // -------------------------------------------------------------------------
    // FormatETA
    // -------------------------------------------------------------------------

    [Fact]
    public void FormatETA_ZeroProgress_ReturnsCalculatingMessage()
    {
        FormattingUtilities.FormatETA(TimeSpan.FromSeconds(10), 0.0)
            .Should().Be("Calculating...");
    }

    [Fact]
    public void FormatETA_HalfwayThrough_ReturnsRemainingTimeEstimate()
    {
        var elapsed = TimeSpan.FromSeconds(60);
        const double progress = 50.0;

        var result = FormattingUtilities.FormatETA(elapsed, progress);

        result.Should().StartWith("~").And.Contain("remaining");
    }

    // -------------------------------------------------------------------------
    // SanitizeForDisplay
    // -------------------------------------------------------------------------

    [Fact]
    public void SanitizeForDisplay_StringWithControlChars_RemovesThem()
    {
        var input = "hello\x01\x02world";

        FormattingUtilities.SanitizeForDisplay(input).Should().Be("helloworld");
    }

    [Fact]
    public void SanitizeForDisplay_StringWithNewline_PreservesNewline()
    {
        var input = "line1\nline2";

        FormattingUtilities.SanitizeForDisplay(input).Should().Be("line1\nline2");
    }

    // -------------------------------------------------------------------------
    // FormatResolution
    // -------------------------------------------------------------------------

    [Fact]
    public void FormatResolution_StandardHd_ReturnsWidthXHeight()
    {
        FormattingUtilities.FormatResolution(1920, 1080).Should().Be("1920x1080");
    }
}
