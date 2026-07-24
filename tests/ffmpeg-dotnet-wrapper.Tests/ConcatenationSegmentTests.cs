// =============================================================================
// Tests for ConcatenationSegment
// =====================================================================

using System;
using FFmpegDotnetWrapper.Exceptions;
using FFmpegDotnetWrapper.Models;
using Xunit;

namespace FFmpegDotnetWrapper.Tests;

/// <summary>
/// Contains unit tests for the <see cref="ConcatenationSegment"/> class.
/// </summary>
public class ConcatenationSegmentTests : IDisposable
{
    private static readonly string _testFilePath = Path.Combine(Path.GetTempPath(), $"test-segment-{Guid.NewGuid()}.mp4");
    private static readonly string _testFileContent = "dummy video content";

    static ConcatenationSegmentTests()
    {
        File.WriteAllText(_testFilePath, _testFileContent);
    }

    public ConcatenationSegmentTests()
    {
        // Clean up in case previous test failed
        if (File.Exists(_testFilePath))
        {
            File.Delete(_testFilePath);
        }
        File.WriteAllText(_testFilePath, _testFileContent);
    }

    public void Dispose()
    {
        if (File.Exists(_testFilePath))
        {
            File.Delete(_testFilePath);
        }
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidFilePath_CreatesSegment()
    {
        var segment = new ConcatenationSegment(_testFilePath);

        Assert.NotNull(segment);
        Assert.Equal(Path.GetFullPath(_testFilePath), segment.FilePath);
        Assert.Equal(Path.GetFileNameWithoutExtension(_testFilePath), segment.Label);
        Assert.Null(segment.TrimStart);
        Assert.Null(segment.TrimEnd);
        Assert.Null(segment.TrimDuration);
        Assert.False(segment.HasTrim);
    }

    [Fact]
    public void Constructor_WithNullFilePath_ThrowsException()
    {
        var exception = Assert.Throws<InvalidOperationConfigurationException>(() => new ConcatenationSegment(null!));
        Assert.Contains("Segment file path cannot be null or empty", exception.Message);
    }

    [Fact]
    public void Constructor_WithEmptyFilePath_ThrowsException()
    {
        var exception = Assert.Throws<InvalidOperationConfigurationException>(() => new ConcatenationSegment(""));
        Assert.Contains("Segment file path cannot be null or empty", exception.Message);
    }

    [Fact]
    public void Constructor_WithWhitespaceFilePath_ThrowsException()
    {
        var exception = Assert.Throws<InvalidOperationConfigurationException>(() => new ConcatenationSegment("   "));
        Assert.Contains("Segment file path cannot be null or empty", exception.Message);
    }

    [Fact]
    public void Constructor_WithNonExistentFilePath_ThrowsException()
    {
        var nonExistentPath = "/tmp/nonexistent-video-file-12345.mp4";
        var exception = Assert.Throws<InvalidOperationConfigurationException>(() => new ConcatenationSegment(nonExistentPath));
        Assert.Contains("Segment file does not exist", exception.Message);
        Assert.Contains(nonExistentPath, exception.Message);
    }

    [Fact]
    public void Constructor_NormalizesPathToAbsolute()
    {
        var relativePath = Path.GetFileName(_testFilePath);
        var fullPath = Path.GetFullPath(relativePath);
        File.WriteAllText(fullPath, _testFileContent);

        try
        {
            var segment = new ConcatenationSegment(relativePath);

            Assert.True(Path.IsPathRooted(segment.FilePath));
            Assert.Equal(fullPath, segment.FilePath);
        }
        finally
        {
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }

    #endregion

    #region Property Tests

    [Fact]
    public void FilePath_IsReadOnlyProperty()
    {
        var segment = new ConcatenationSegment(_testFilePath);

        // FilePath has only a getter, so it can't be modified after construction
        var propertyInfo = typeof(ConcatenationSegment).GetProperty("FilePath");
        Assert.False(propertyInfo?.CanWrite);
    }

    [Fact]
    public void Label_IsInitializedFromFileName()
    {
        var segment = new ConcatenationSegment(_testFilePath);

        Assert.Equal(Path.GetFileNameWithoutExtension(_testFilePath), segment.Label);
    }

    [Fact]
    public void Label_CanBeSetViaInit()
    {
        var segment = new ConcatenationSegment(_testFilePath) { Label = "test-label" };
        Assert.Equal("test-label", segment.Label);
    }

    [Fact]
    public void Label_CanBeNull()
    {
        var segment = new ConcatenationSegment(_testFilePath) { Label = null };
        Assert.Null(segment.Label);
    }

    [Fact]
    public void TrimStart_IsNullableTimeSpan()
    {
        var segment = new ConcatenationSegment(_testFilePath);
        Assert.Null(segment.TrimStart);

        segment = new ConcatenationSegment(_testFilePath) { TrimStart = TimeSpan.FromSeconds(5) };
        Assert.Equal(TimeSpan.FromSeconds(5), segment.TrimStart);
    }

    [Fact]
    public void TrimEnd_IsNullableTimeSpan()
    {
        var segment = new ConcatenationSegment(_testFilePath);
        Assert.Null(segment.TrimEnd);

        segment = new ConcatenationSegment(_testFilePath) { TrimEnd = TimeSpan.FromSeconds(30) };
        Assert.Equal(TimeSpan.FromSeconds(30), segment.TrimEnd);
    }

    [Fact]
    public void TrimDuration_IsNullableTimeSpan()
    {
        var segment = new ConcatenationSegment(_testFilePath);
        Assert.Null(segment.TrimDuration);

        segment = new ConcatenationSegment(_testFilePath) { TrimDuration = TimeSpan.FromSeconds(10) };
        Assert.Equal(TimeSpan.FromSeconds(10), segment.TrimDuration);
    }

    [Fact]
    public void HasTrim_WithNoTrims_ReturnsFalse()
    {
        var segment = new ConcatenationSegment(_testFilePath);
        Assert.False(segment.HasTrim);
    }

    [Fact]
    public void HasTrim_WithTrimStart_ReturnsTrue()
    {
        var segment = new ConcatenationSegment(_testFilePath) { TrimStart = TimeSpan.FromSeconds(5) };
        Assert.True(segment.HasTrim);
    }

    [Fact]
    public void HasTrim_WithTrimEnd_ReturnsTrue()
    {
        var segment = new ConcatenationSegment(_testFilePath) { TrimEnd = TimeSpan.FromSeconds(30) };
        Assert.True(segment.HasTrim);
    }

    [Fact]
    public void HasTrim_WithTrimDuration_ReturnsTrue()
    {
        var segment = new ConcatenationSegment(_testFilePath) { TrimDuration = TimeSpan.FromSeconds(15) };
        Assert.True(segment.HasTrim);
    }

    [Fact]
    public void HasTrim_WithAllTrims_ReturnsTrue()
    {
        var segment = new ConcatenationSegment(_testFilePath)
        {
            TrimStart = TimeSpan.FromSeconds(5),
            TrimEnd = TimeSpan.FromSeconds(30),
            TrimDuration = TimeSpan.FromSeconds(25)
        };
        Assert.True(segment.HasTrim);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Constructor_WithFilePathContainingSpaces_WorksCorrectly()
    {
        var spacedPath = Path.Combine(Path.GetTempPath(), "test file with spaces.mp4");
        File.WriteAllText(spacedPath, _testFileContent);

        try
        {
            var segment = new ConcatenationSegment(spacedPath);
            Assert.Equal(Path.GetFullPath(spacedPath), segment.FilePath);
        }
        finally
        {
            if (File.Exists(spacedPath))
            {
                File.Delete(spacedPath);
            }
        }
    }

    [Fact]
    public void Constructor_WithFilePathContainingSpecialChars_WorksCorrectly()
    {
        var specialPath = Path.Combine(Path.GetTempPath(), "test-(special)_file&123.mp4");
        File.WriteAllText(specialPath, _testFileContent);

        try
        {
            var segment = new ConcatenationSegment(specialPath);
            Assert.Equal(Path.GetFullPath(specialPath), segment.FilePath);
        }
        finally
        {
            if (File.Exists(specialPath))
            {
                File.Delete(specialPath);
            }
        }
    }

    [Fact]
    public void Constructor_WithVeryLongFileName_WorksCorrectly()
    {
        var longName = new string('a', 200) + ".mp4";
        var longPath = Path.Combine(Path.GetTempPath(), longName);
        File.WriteAllText(longPath, _testFileContent);

        try
        {
            var segment = new ConcatenationSegment(longPath);
            Assert.Equal(Path.GetFullPath(longPath), segment.FilePath);
        }
        finally
        {
            if (File.Exists(longPath))
            {
                File.Delete(longPath);
            }
        }
    }

    #endregion
}