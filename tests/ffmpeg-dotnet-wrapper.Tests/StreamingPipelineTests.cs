// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Models;
using FluentAssertions;
using Xunit;

namespace FFmpegDotnetWrapper.Tests;

/// <summary>
/// Unit tests for the argument guards on <see cref="StreamingPipelineSettings"/> and <see cref="StreamingPipelineResult"/>.
/// </summary>
public class StreamingPipelineTests
{
    /// <summary>
    /// Tests that assigning a null profile list is rejected with <see cref="ArgumentNullException"/>.
    /// </summary>
    [Fact]
    public void Profiles_ShouldThrowArgumentNullException_WhenNull()
    {
        var settings = new StreamingPipelineSettings
        {
            InputFilePath = "/path/to/input.mp4",
            OutputDirectory = "/path/to/output"
        };

        var act = () => settings.Profiles = null!;

        act.Should().Throw<ArgumentNullException>().WithParameterName("value");
    }

    /// <summary>
    /// Tests that assigning an empty profile list is rejected with <see cref="ArgumentException"/>.
    /// </summary>
    [Fact]
    public void Profiles_ShouldThrowArgumentException_WhenEmpty()
    {
        var settings = new StreamingPipelineSettings
        {
            InputFilePath = "/path/to/input.mp4",
            OutputDirectory = "/path/to/output"
        };

        var act = () => settings.Profiles = [];

        act.Should().Throw<ArgumentException>().WithParameterName("value");
    }

    /// <summary>
    /// Tests that a profile list containing a null entry is rejected with <see cref="ArgumentException"/>.
    /// </summary>
    [Fact]
    public void Profiles_ShouldThrowArgumentException_WhenContainsNullEntry()
    {
        var settings = new StreamingPipelineSettings
        {
            InputFilePath = "/path/to/input.mp4",
            OutputDirectory = "/path/to/output"
        };

        var act = () => settings.Profiles = [StreamingProfile.HD, null!];

        act.Should().Throw<ArgumentException>().WithParameterName("value");
    }

    /// <summary>
    /// Tests that the default ladder is used when no profiles are assigned.
    /// </summary>
    [Fact]
    public void Profiles_ShouldDefaultToLadder_WhenNotAssigned()
    {
        var settings = new StreamingPipelineSettings
        {
            InputFilePath = "/path/to/input.mp4",
            OutputDirectory = "/path/to/output"
        };

        settings.Profiles.Should().HaveCount(4);
    }

    /// <summary>
    /// Tests that a null input path is rejected with <see cref="ArgumentNullException"/>.
    /// </summary>
    [Fact]
    public void InputFilePath_ShouldThrowArgumentNullException_WhenNull()
    {
        var act = () => new StreamingPipelineSettings
        {
            InputFilePath = null!,
            OutputDirectory = "/path/to/output"
        };

        act.Should().Throw<ArgumentNullException>().WithParameterName("value");
    }

    /// <summary>
    /// Tests that a whitespace input path is rejected with <see cref="ArgumentException"/>.
    /// </summary>
    [Fact]
    public void InputFilePath_ShouldThrowArgumentException_WhenWhitespace()
    {
        var act = () => new StreamingPipelineSettings
        {
            InputFilePath = "   ",
            OutputDirectory = "/path/to/output"
        };

        act.Should().Throw<ArgumentException>().WithParameterName("value");
    }

    /// <summary>
    /// Tests that a null output directory is rejected with <see cref="ArgumentNullException"/>.
    /// </summary>
    [Fact]
    public void OutputDirectory_ShouldThrowArgumentNullException_WhenNull()
    {
        var act = () => new StreamingPipelineSettings
        {
            InputFilePath = "/path/to/input.mp4",
            OutputDirectory = null!
        };

        act.Should().Throw<ArgumentNullException>().WithParameterName("value");
    }

    /// <summary>
    /// Tests that an empty output directory is rejected with <see cref="ArgumentException"/>.
    /// </summary>
    [Fact]
    public void OutputDirectory_ShouldThrowArgumentException_WhenEmpty()
    {
        var act = () => new StreamingPipelineSettings
        {
            InputFilePath = "/path/to/input.mp4",
            OutputDirectory = string.Empty
        };

        act.Should().Throw<ArgumentException>().WithParameterName("value");
    }

    /// <summary>
    /// Tests that <see cref="StreamingPipelineResult.AddSegment"/> rejects a null segment.
    /// </summary>
    [Fact]
    public void AddSegment_ShouldThrowArgumentNullException_WhenSegmentIsNull()
    {
        var result = new StreamingPipelineResult { PipelineId = "test" };

        var act = () => result.AddSegment(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("segment");
    }

    /// <summary>
    /// Tests that <see cref="StreamingPipelineResult.RecordSwitch"/> rejects a null switch event.
    /// </summary>
    [Fact]
    public void RecordSwitch_ShouldThrowArgumentNullException_WhenSwitchIsNull()
    {
        var result = new StreamingPipelineResult { PipelineId = "test" };

        var act = () => result.RecordSwitch(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("bitrateSwitch");
    }
}
