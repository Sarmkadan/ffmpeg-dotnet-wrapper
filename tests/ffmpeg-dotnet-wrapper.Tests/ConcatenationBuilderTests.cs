using FFmpegDotnetWrapper.Constants;
using FFmpegDotnetWrapper.Exceptions;
using FFmpegDotnetWrapper.Models;
using FluentAssertions;
using Xunit;

namespace FFmpegDotnetWrapper.Tests;

/// <summary>
/// Contains unit tests for the <see cref="ConcatenationBuilder"/> class.
/// </summary>
public class ConcatenationBuilderTests : IDisposable
{
    private readonly List<string> _tempFiles = [];

    /// <summary>
    /// Initializes temporary video files that are used by the tests.
    /// Each file contains placeholder text and is stored in the system temporary directory.
    /// </summary>
    public ConcatenationBuilderTests()
    {
        for (var i = 0; i < 4; i++)
        {
            var path = Path.Combine(Path.GetTempPath(), $"test_concat_{Guid.NewGuid()}.mp4");
            File.WriteAllText(path, "fake video data");
            _tempFiles.Add(path);
        }
    }

    /// <summary>
    /// Deletes any temporary files that were created during the test run.
    /// </summary>
    public void Dispose()
    {
        foreach (var f in _tempFiles.Where(File.Exists))
            File.Delete(f);
    }

    /// <summary>
    /// Verifies that adding a single file results in one segment with the correct absolute path.
    /// </summary>
    [Fact]
    public void Add_SingleFile_AddsToSegments()
    {
        var builder = new ConcatenationBuilder();
        builder.Add(_tempFiles[0]);

        builder.SegmentCount.Should().Be(1);
        builder.Segments[0].FilePath.Should().Be(Path.GetFullPath(_tempFiles[0]));
    }

    /// <summary>
    /// Verifies that adding multiple files preserves the order in which they were added.
    /// </summary>
    [Fact]
    public void Add_MultipleFiles_PreservesOrder()
    {
        var builder = new ConcatenationBuilder()
            .Add(_tempFiles[0])
            .Add(_tempFiles[1])
            .Add(_tempFiles[2]);

        builder.SegmentCount.Should().Be(3);
        builder.Segments[0].FilePath.Should().Be(Path.GetFullPath(_tempFiles[0]));
        builder.Segments[1].FilePath.Should().Be(Path.GetFullPath(_tempFiles[1]));
        builder.Segments[2].FilePath.Should().Be(Path.GetFullPath(_tempFiles[2]));
    }

    /// <summary>
    /// Ensures that providing trim start and duration values sets the corresponding segment properties.
    /// </summary>
    [Fact]
    public void Add_WithTrimParameters_SetsSegmentProperties()
    {
        var start = TimeSpan.FromSeconds(5);
        var duration = TimeSpan.FromSeconds(30);

        var builder = new ConcatenationBuilder()
            .Add(_tempFiles[0], trimStart: start, trimDuration: duration);

        var segment = builder.Segments[0];
        segment.TrimStart.Should().Be(start);
        segment.TrimDuration.Should().Be(duration);
        segment.TrimEnd.Should().BeNull();
        segment.HasTrim.Should().BeTrue();
    }

    /// <summary>
    /// Confirms that specifying both <c>trimEnd</c> and <c>trimDuration</c> throws an <see cref="InvalidOperationConfigurationException"/>.
    /// </summary>
    [Fact]
    public void Add_WithBothTrimEndAndDuration_ThrowsException()
    {
        var builder = new ConcatenationBuilder();

        var act = () => builder.Add(
            _tempFiles[0],
            trimEnd: TimeSpan.FromSeconds(30),
            trimDuration: TimeSpan.FromSeconds(20));

        act.Should().Throw<InvalidOperationConfigurationException>()
           .WithMessage("*TrimEnd*TrimDuration*");
    }

    /// <summary>
    /// Verifies that attempting to add a non‑existent file results in an <see cref="InvalidOperationConfigurationException"/>.
    /// </summary>
    [Fact]
    public void Add_WithNonexistentFile_ThrowsException()
    {
        var builder = new ConcatenationBuilder();

        var act = () => builder.Add("/nonexistent/video.mp4");

        act.Should().Throw<InvalidOperationConfigurationException>()
           .WithMessage("*does not exist*");
    }

    /// <summary>
    /// Checks that inserting a segment at a valid index places it at the correct position.
    /// </summary>
    [Fact]
    public void Insert_AtValidIndex_InsertsAtPosition()
    {
        var builder = new ConcatenationBuilder()
            .Add(_tempFiles[0])
            .Add(_tempFiles[2]);

        builder.Insert(1, _tempFiles[1]);

        builder.SegmentCount.Should().Be(3);
        builder.Segments[1].FilePath.Should().Be(Path.GetFullPath(_tempFiles[1]));
    }

    /// <summary>
    /// Ensures that removing a segment eliminates the matching file from the collection.
    /// </summary>
    [Fact]
    public void Remove_RemovesMatchingSegment()
    {
        var builder = new ConcatenationBuilder()
            .Add(_tempFiles[0])
            .Add(_tempFiles[1])
            .Add(_tempFiles[2]);

        builder.Remove(_tempFiles[1]);

        builder.SegmentCount.Should().Be(2);
        builder.Segments.Should().NotContain(s => s.FilePath == Path.GetFullPath(_tempFiles[1]));
    }

    /// <summary>
    /// Validates that configuring a cross‑fade transition updates the resulting <see cref="MergeSettings"/>.
    /// </summary>
    [Fact]
    public void WithTransition_SetsCrossfade()
    {
        var builder = new ConcatenationBuilder()
            .Add(_tempFiles[0])
            .Add(_tempFiles[1])
            .WithTransition(ConcatTransition.Crossfade, duration: 0.75);

        var settings = builder.Build();

        settings.Crossfade.Should().BeTrue();
        settings.CrossfadeDuration.Should().Be(0.75);
    }

    /// <summary>
    /// Confirms that specifying a zero duration for a transition throws an <see cref="InvalidOperationConfigurationException"/>.
    /// </summary>
    [Fact]
    public void WithTransition_ZeroDuration_ThrowsException()
    {
        var builder = new ConcatenationBuilder();

        var act = () => builder.WithTransition(ConcatTransition.Crossfade, duration: 0);

        act.Should().Throw<InvalidOperationConfigurationException>()
           .WithMessage("*duration*greater than zero*");
    }

    /// <summary>
    /// Checks that enabling re‑encoding sets the appropriate flag on the generated <see cref="MergeSettings"/>.
    /// </summary>
    [Fact]
    public void WithReencode_SetsTranscodeOnMerge()
    {
        var builder = new ConcatenationBuilder()
            .Add(_tempFiles[0])
            .Add(_tempFiles[1])
            .WithReencode(true);

        var settings = builder.Build();

        settings.TranscodeOnMerge.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that building with fewer than two segments throws an <see cref="InvalidOperationConfigurationException"/>.
    /// </summary>
    [Fact]
    public void Build_WithLessThanTwoSegments_ThrowsException()
    {
        var builder = new ConcatenationBuilder()
            .Add(_tempFiles[0]);

        var act = () => builder.Build();

        act.Should().Throw<InvalidOperationConfigurationException>()
           .WithMessage("*At least two segments*");
    }

    /// <summary>
    /// Ensures that building with exactly two segments produces a valid <see cref="MergeSettings"/> instance.
    /// </summary>
    [Fact]
    public void Build_WithTwoSegments_ReturnsValidMergeSettings()
    {
        var settings = new ConcatenationBuilder()
            .Add(_tempFiles[0])
            .Add(_tempFiles[1])
            .Build();

        settings.Should().NotBeNull();
        settings.InputFiles.Should().HaveCount(2);
        settings.InputFiles[0].Should().Be(Path.GetFullPath(_tempFiles[0]));
        settings.InputFiles[1].Should().Be(Path.GetFullPath(_tempFiles[1]));
        settings.PreserveAudio.Should().BeTrue();
        settings.PreserveVideo.Should().BeTrue();
    }

    /// <summary>
    /// Confirms that calling <c>Reset</c> clears all segments and any configured options.
    /// </summary>
    [Fact]
    public void Reset_ClearsAllSegmentsAndOptions()
    {
        var builder = new ConcatenationBuilder()
            .Add(_tempFiles[0])
            .Add(_tempFiles[1])
            .WithTransition(ConcatTransition.Crossfade)
            .WithReencode(true);

        builder.Reset();

        builder.SegmentCount.Should().Be(0);
    }

    /// <summary>
    /// Verifies that custom transcode settings are propagated to the resulting <see cref="MergeSettings"/>.
    /// </summary>
    [Fact]
    public void Build_WithCustomTranscodeSettings_PropagatesSettings()
    {
        var transcodeSettings = new TranscodeSettings
        {
            VideoCodec = VideoCodec.H265,
            AudioCodec = AudioCodec.AAC
        };

        var settings = new ConcatenationBuilder()
            .Add(_tempFiles[0])
            .Add(_tempFiles[1])
            .WithTranscodeSettings(transcodeSettings)
            .Build();

        settings.TranscodeOnMerge.Should().BeTrue();
        settings.TranscodeSettings.Should().NotBeNull();
        settings.TranscodeSettings!.VideoCodec.Should().Be(VideoCodec.H265);
    }

    /// <summary>
    /// Ensures that constructing a <see cref="ConcatenationSegment"/> with a null path throws an <see cref="InvalidOperationConfigurationException"/>.
    /// </summary>
    [Fact]
    public void ConcatenationSegment_WithNullPath_ThrowsException()
    {
        var act = () => new ConcatenationSegment(null!);

        act.Should().Throw<InvalidOperationConfigurationException>();
    }

    /// <summary>
    /// Tests that fluent chaining of builder methods produces the expected settings, including a disabled transition.
    /// </summary>
    [Fact]
    public void FluentChaining_BuildsCorrectly()
    {
        var settings = new ConcatenationBuilder()
            .Add(_tempFiles[0])
            .Add(_tempFiles[1], trimStart: TimeSpan.FromSeconds(5))
            .Add(_tempFiles[2])
            .WithTransition(ConcatTransition.None)
            .Build();

        settings.InputFiles.Should().HaveCount(3);
        settings.Crossfade.Should().BeFalse();
    }
}
