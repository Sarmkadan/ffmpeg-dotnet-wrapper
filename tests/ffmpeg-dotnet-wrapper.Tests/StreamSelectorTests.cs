// =============================================================================
// Tests for StreamSelector
// =============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using FFmpegDotnetWrapper.Exceptions;
using FFmpegDotnetWrapper.Models;
using FFmpegDotnetWrapper.Services;
using FFmpegDotnetWrapper.Utilities;

using Xunit;

namespace FFmpegDotnetWrapper.Tests;

/// <summary>
/// End-to-end tests for <see cref="StreamSelector"/>.
/// These tests verify that -map arguments are generated correctly
/// and that validation works as expected.
/// </summary>
public class StreamSelectorTests : IDisposable
{
    private readonly string _tempDir;

    public StreamSelectorTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"ffmpeg-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            foreach (var file in Directory.GetFiles(_tempDir))
            {
                try { File.Delete(file); } catch { }
            }
            try { Directory.Delete(_tempDir); } catch { }
        }
    }

    [Fact]
    public void SelectStream_ByIndex_GeneratesCorrectMapArgument()
    {
        // Arrange
        var selector = new StreamSelector();
        selector.SelectStream(0, 2); // Select stream 0:2

        // Act
        var mapArgs = selector.BuildMapArguments();

        // Assert
        Assert.Single(mapArgs);
        Assert.Contains(mapArgs, arg => arg == "-map 0:2");
    }

    [Fact]
    public void SelectAllAudio_GeneratesCorrectMapArgument()
    {
        // Arrange
        var selector = new StreamSelector();
        selector.SelectAllAudio(); // Select all audio from input 0

        // Act
        var mapArgs = selector.BuildMapArguments();

        // Assert
        Assert.Single(mapArgs);
        Assert.Contains(mapArgs, arg => arg == "-map 0:a");
    }

    [Fact]
    public void SelectAllVideo_GeneratesCorrectMapArgument()
    {
        // Arrange
        var selector = new StreamSelector();
        selector.SelectAllVideo(); // Select all video from input 0

        // Act
        var mapArgs = selector.BuildMapArguments();

        // Assert
        Assert.Single(mapArgs);
        Assert.Contains(mapArgs, arg => arg == "-map 0:v");
    }

    [Fact]
    public void SelectAudioByLanguage_GeneratesCorrectMapArgument()
    {
        // Arrange
        var selector = new StreamSelector();
        selector.SelectAudioByLanguage("eng"); // Select audio with English language

        // Act
        var mapArgs = selector.BuildMapArguments();

        // Assert
        Assert.Single(mapArgs);
        // -map 0:a:m:language:eng selects audio stream with language=eng
        Assert.Contains(mapArgs, arg => arg == "-map 0:a:m:language:eng");
    }

    [Fact]
    public void SelectSubtitleByLanguage_GeneratesCorrectMapArgument()
    {
        // Arrange
        var selector = new StreamSelector();
        selector.SelectSubtitleByLanguage("eng"); // Select subtitle with English language

        // Act
        var mapArgs = selector.BuildMapArguments();

        // Assert
        Assert.Single(mapArgs);
        // -map 0:s:m:language:eng selects subtitle stream with language=eng
        Assert.Contains(mapArgs, arg => arg == "-map 0:s:m:language:eng");
    }

    [Fact]
    public void SelectVideoByIndex_GeneratesCorrectMapArgument()
    {
        // Arrange
        var selector = new StreamSelector();
        selector.SelectVideoByIndex(0); // Select first video stream

        // Act
        var mapArgs = selector.BuildMapArguments();

        // Assert
        Assert.Single(mapArgs);
        Assert.Contains(mapArgs, arg => arg == "-map 0:v:0");
    }

    [Fact]
    public void SelectAudioByIndex_GeneratesCorrectMapArgument()
    {
        // Arrange
        var selector = new StreamSelector();
        selector.SelectAudioByIndex(1); // Select second audio stream

        // Act
        var mapArgs = selector.BuildMapArguments();

        // Assert
        Assert.Single(mapArgs);
        Assert.Contains(mapArgs, arg => arg == "-map 0:a:1");
    }

    [Fact]
    public void SelectSubtitleByIndex_GeneratesCorrectMapArgument()
    {
        // Arrange
        var selector = new StreamSelector();
        selector.SelectSubtitleByIndex(0); // Select first subtitle stream

        // Act
        var mapArgs = selector.BuildMapArguments();

        // Assert
        Assert.Single(mapArgs);
        Assert.Contains(mapArgs, arg => arg == "-map 0:s:0");
    }

    [Fact]
    public void MultipleSelections_GeneratesMultipleMapArguments()
    {
        // Arrange
        var selector = new StreamSelector();
        selector.SelectStream(0, 0);      // video stream 0
        selector.SelectStream(0, 2);      // audio stream 2
        selector.SelectAllSubtitles();     // all subtitles

        // Act
        var mapArgs = selector.BuildMapArguments();

        // Assert
        Assert.Equal(3, mapArgs.Count);
        Assert.Contains(mapArgs, arg => arg == "-map 0:0");   // video
        Assert.Contains(mapArgs, arg => arg == "-map 0:2");   // audio
        Assert.Contains(mapArgs, arg => arg == "-map 0:s");   // all subtitles
    }

    [Fact]
    public void SelectStream_WithNegativeInputIndex_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        var selector = new StreamSelector();
        Assert.Throws<ArgumentOutOfRangeException>(() => selector.SelectStream(-1, 0));
    }

    [Fact]
    public void SelectStream_WithNegativeStreamIndex_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        var selector = new StreamSelector();
        Assert.Throws<ArgumentOutOfRangeException>(() => selector.SelectStream(0, -1));
    }

    [Fact]
    public void SelectVideoByIndex_WithNegativeVideoStreamIndex_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        var selector = new StreamSelector();
        Assert.Throws<ArgumentOutOfRangeException>(() => selector.SelectVideoByIndex(-1, 0));
    }

    [Fact]
    public void SelectVideoByIndex_WithNegativeInputIndex_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        var selector = new StreamSelector();
        Assert.Throws<ArgumentOutOfRangeException>(() => selector.SelectVideoByIndex(0, -1));
    }

    [Fact]
    public void SelectAudioByLanguage_WithEmptyLanguage_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var selector = new StreamSelector();
        Assert.Throws<ArgumentException>(() => selector.SelectAudioByLanguage(""));
    }

    [Fact]
    public void SelectAudioByLanguage_WithTwoCharLanguage_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var selector = new StreamSelector();
        Assert.Throws<ArgumentException>(() => selector.SelectAudioByLanguage("en"));
    }

    [Fact]
    public void SelectAudioByLanguage_WithSevenCharLanguage_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var selector = new StreamSelector();
        Assert.Throws<ArgumentException>(() => selector.SelectAudioByLanguage("englis"));
    }

    [Fact]
    public void SelectSubtitleByLanguage_WithEmptyLanguage_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var selector = new StreamSelector();
        Assert.Throws<ArgumentException>(() => selector.SelectSubtitleByLanguage(""));
    }

    [Fact]
    public void SelectSubtitleByLanguage_WithTwoCharLanguage_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var selector = new StreamSelector();
        Assert.Throws<ArgumentException>(() => selector.SelectSubtitleByLanguage("en"));
    }

    [Fact]
    public void Validate_WithNullMediaFile_ThrowsArgumentNullException()
    {
        // Arrange
        var selector = new StreamSelector();
        selector.SelectStream(0, 0);

        // Act & Assert
        // Note: Validate always checks mediaFile for null first, before validating selections
        Assert.Throws<ArgumentNullException>(() => selector.Validate(null!));
    }

    [Fact]
    public void Clone_ShouldCreateEqualButDistinctInstance()
    {
        // Arrange
        var selector = new StreamSelector();
        selector.SelectStream(0, 0);
        selector.SelectAudioByLanguage("eng");

        // Act
        var clone = selector.Clone();

        // Assert
        Assert.NotSame(selector, clone);
        Assert.Equal(selector.Count, clone.Count);
        // The clone should have the same selections
        var cloneArgs = clone.BuildMapArguments();
        var originalArgs = selector.BuildMapArguments();
        Assert.Equal(originalArgs.Count, cloneArgs.Count);
    }

    [Fact]
    public void SelectStream_ByIndex_Throws_WhenNegativeInputIndex()
    {
        // Arrange & Act & Assert
        var selector = new StreamSelector();
        Assert.Throws<ArgumentOutOfRangeException>(() => selector.SelectStream(-1, 0));
    }

    [Fact]
    public void SelectStream_ByIndex_Throws_WhenNegativeStreamIndex()
    {
        // Arrange & Act & Assert
        var selector = new StreamSelector();
        Assert.Throws<ArgumentOutOfRangeException>(() => selector.SelectStream(0, -1));
    }

    [Fact]
    public void SelectAudioByLanguage_Throws_WhenLanguageNot3Chars()
    {
        // Arrange & Act & Assert
        var selector = new StreamSelector();
        Assert.Throws<ArgumentException>(() => selector.SelectAudioByLanguage("en")); // 2 chars
    }

    [Fact]
    public void SelectAudioByLanguage_Throws_WhenLanguageIs4Chars()
    {
        // Arrange & Act & Assert
        var selector = new StreamSelector();
        Assert.Throws<ArgumentException>(() => selector.SelectAudioByLanguage("englis")); // 7 chars
    }

    [Fact]
    public void SelectStream_AllOfType_WithDifferentInputIndexes()
    {
        // Arrange
        var selector = new StreamSelector();
        selector.SelectAllVideo(0);
        selector.SelectAllVideo(1); // Different input index

        // Act
        var mapArgs = selector.BuildMapArguments();

        // Assert
        Assert.Equal(2, mapArgs.Count);
        Assert.Contains(mapArgs, arg => arg == "-map 0:v");
        Assert.Contains(mapArgs, arg => arg == "-map 1:v");
    }
}