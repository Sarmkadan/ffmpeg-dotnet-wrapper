// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Utilities;
using FluentAssertions;
using Xunit;

/// <summary>
/// Tests for the FormattingUtilities class.
/// </summary>
public class FormattingUtilitiesTests
{
    // -------------------------------------------------------------------------
    // FormatDuration
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that FormatDuration returns the correct string representation of a TimeSpan less than one minute.
    /// </summary>
    [Fact]
    public void FormatDuration_LessThanOneMinute_ReturnsZeroHoursAndMinutes()
    {
        var duration = TimeSpan.FromSeconds(45);

        FormattingUtilities.FormatDuration(duration).Should().Be("00:00:45");
    }

    /// <summary>
    /// Verifies that FormatDuration returns the correct string representation of a TimeSpan between one and sixty minutes.
    /// </summary>
    [Fact]
    public void FormatDuration_BetweenOneAndSixtyMinutes_ReturnsZeroHours()
    {
        var duration = TimeSpan.FromSeconds(90);

        FormattingUtilities.FormatDuration(duration).Should().Be("00:01:30");
    }

    /// <summary>
    /// Verifies that FormatDuration returns the correct string representation of a TimeSpan more than one hour.
    /// </summary>
    [Fact]
    public void FormatDuration_MoreThanOneHour_IncludesHours()
    {
        var duration = TimeSpan.FromSeconds(3661);

        FormattingUtilities.FormatDuration(duration).Should().Be("01:01:01");
    }

    // -------------------------------------------------------------------------
    // FormatBytes
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that FormatBytes returns the correct string representation of a byte count less than one kilobyte.
    /// </summary>
    [Fact]
    public void FormatBytes_LessThanOneKilobyte_ReturnsByteSuffix()
    {
        FormattingUtilities.FormatBytes(512).Should().Be("512 B");
    }

    /// <summary>
    /// Verifies that FormatBytes returns the correct string representation of a byte count exactly one megabyte.
    /// </summary>
    [Fact]
    public void FormatBytes_ExactMegabyte_ReturnsMbSuffix()
    {
        FormattingUtilities.FormatBytes(1024 * 1024).Should().Be("1 MB");
    }

    /// <summary>
    /// Verifies that FormatBytes returns the correct string representation of a large byte count in gigabytes.
    /// </summary>
    [Fact]
    public void FormatBytes_LargeGigabyteValue_ReturnsGbSuffix()
    {
        FormattingUtilities.FormatBytes(2L * 1024 * 1024 * 1024).Should().Be("2 GB");
    }

    // -------------------------------------------------------------------------
    // FormatBitrate
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that FormatBitrate returns the correct string representation of a bitrate below one thousand.
    /// </summary>
    [Fact]
    public void FormatBitrate_BelowOneThousand_ReturnsKbps()
    {
        FormattingUtilities.FormatBitrate(500).Should().Be("500 Kbps");
    }

    /// <summary>
    /// Verifies that FormatBitrate returns the correct string representation of a bitrate in thousands.
    /// </summary>
    [Fact]
    public void FormatBitrate_Thousands_ReturnsMbps()
    {
        FormattingUtilities.FormatBitrate(5000).Should().Be("5 Mbps");
    }

    /// <summary>
    /// Verifies that FormatBitrate returns the correct string representation of a bitrate in millions.
    /// </summary>
    [Fact]
    public void FormatBitrate_Millions_ReturnsGbps()
    {
        FormattingUtilities.FormatBitrate(2_000_000).Should().Be("2 Gbps");
    }

    // -------------------------------------------------------------------------
    // TruncateString
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that TruncateString returns the original string when its length is below the maximum allowed length.
    /// </summary>
    [Fact]
    public void TruncateString_BelowMaxLength_ReturnsUnchanged()
    {
        const string input = "short string";

        FormattingUtilities.TruncateString(input, 80).Should().Be(input);
    }

    /// <summary>
    /// Verifies that TruncateString appends an ellipsis to the string when its length exceeds the maximum allowed length.
    /// </summary>
    [Fact]
    public void TruncateString_ExceedsMaxLength_AppendsEllipsis()
    {
        var input = new string('a', 100);

        var result = FormattingUtilities.TruncateString(input, 80);

        result.Should().HaveLength(80);
        result.Should().EndWith("...");
    }

    /// <summary>
    /// Verifies that TruncateString returns an empty string when the input is null or empty.
    /// </summary>
    /// <param name="input">The input string to truncate.</param>
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

    /// <summary>
    /// Verifies that TitleCase correctly converts kebab or snake case strings to title case.
    /// </summary>
    /// <param name="input">The input string to convert to title case.</param>
    /// <param name="expected">The expected title case string.</param>
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

    /// <summary>
    /// Verifies that FormatPercentage returns the correct string representation of a percentage value.
    /// </summary>
    /// <param name="percentage">The percentage value to format.</param>
    /// <param name="expected">The expected formatted string.</param>
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

    /// <summary>
    /// Verifies that FormatETA returns the correct string representation of the estimated time to completion when the progress is zero.
    /// </summary>
    [Fact]
    public void FormatETA_ZeroProgress_ReturnsCalculatingMessage()
    {
        FormattingUtilities.FormatETA(TimeSpan.FromSeconds(10), 0.0)
            .Should().Be("Calculating...");
    }

    /// <summary>
    /// Verifies that FormatETA returns the correct string representation of the estimated time to completion when the progress is halfway through.
    /// </summary>
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

    /// <summary>
    /// Verifies that SanitizeForDisplay removes control characters from the input string.
    /// </summary>
    [Fact]
    public void SanitizeForDisplay_StringWithControlChars_RemovesThem()
    {
        var input = "hello\x01\x02world";

        FormattingUtilities.SanitizeForDisplay(input).Should().Be("helloworld");
    }

    /// <summary>
    /// Verifies that SanitizeForDisplay preserves newline characters in the input string.
    /// </summary>
    [Fact]
    public void SanitizeForDisplay_StringWithNewline_PreservesNewline()
    {
        var input = "line1\nline2";

        FormattingUtilities.SanitizeForDisplay(input).Should().Be("line1\nline2");
    }

    // -------------------------------------------------------------------------
    // FormatResolution
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that FormatResolution returns the correct string representation of a resolution.
    /// </summary>
    [Fact]
    public void FormatResolution_StandardHd_ReturnsWidthXHeight()
    {
        FormattingUtilities.FormatResolution(1920, 1080).Should().Be("1920x1080");
    }
}
