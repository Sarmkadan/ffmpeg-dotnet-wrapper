// =============================================================================
// Tests for ConcatenationSegmentJsonExtensions
// =============================================================================

using System;
using FFmpegDotnetWrapper.Models;
using Xunit;

namespace FFmpegDotnetWrapper.Tests;

public class ConcatenationSegmentJsonExtensionsTests : IDisposable
{
    private static readonly string _testFilePath = "/tmp/test-video.mp4";
    private static readonly string _testFileContent = "dummy video content";

    static ConcatenationSegmentJsonExtensionsTests()
    {
        File.WriteAllText(_testFilePath, _testFileContent);
    }

    public ConcatenationSegmentJsonExtensionsTests()
    {
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

    #region ToJson

    [Fact]
    public void ToJson_SerializesSegmentWithDefaultOptions()
    {
        var segment = new ConcatenationSegment(_testFilePath);
        var json = segment.ToJson();

        Assert.NotNull(json);
        Assert.Contains(_testFilePath, json);
        Assert.Contains("filePath", json);
        Assert.DoesNotContain("\n", json);
    }

    [Fact]
    public void ToJson_SerializesSegmentWithIndentedOptions()
    {
        var segment = new ConcatenationSegment(_testFilePath);
        var json = segment.ToJson(indented: true);

        Assert.NotNull(json);
        Assert.Contains(_testFilePath, json);
        Assert.Contains("filePath", json);
        Assert.Contains("\n", json);
    }

    [Fact]
    public void ToJson_SerializesSegmentWithTrimStart()
    {
        var segment = new ConcatenationSegment(_testFilePath) { TrimStart = TimeSpan.FromSeconds(5) };
        var json = segment.ToJson();

        Assert.NotNull(json);
        Assert.Contains("filePath", json);
        Assert.Contains("trimStart", json);
        Assert.Contains("5", json);
    }

    [Fact]
    public void ToJson_SerializesSegmentWithAllProperties()
    {
        var segment = new ConcatenationSegment(_testFilePath)
        {
            TrimStart = TimeSpan.FromSeconds(10),
            TrimEnd = TimeSpan.FromSeconds(60),
            Label = "test-segment"
        };
        var json = segment.ToJson();

        Assert.NotNull(json);
        Assert.Contains(_testFilePath, json);
        Assert.Contains("filePath", json);
        Assert.Contains("trimStart", json);
        Assert.Contains("trimEnd", json);
        Assert.Contains("label", json);
        Assert.Contains("test-segment", json);
    }

    [Fact]
    public void ToJson_ThrowsArgumentNullException_WhenValueIsNull()
    {
        ConcatenationSegment? segment = null;
        Assert.Throws<ArgumentNullException>(() => segment!.ToJson());
    }

    #endregion

    #region FromJson

    [Fact]
    public void FromJson_DeserializesValidJson()
    {
        var segment = new ConcatenationSegment(_testFilePath);
        var json = segment.ToJson();
        var result = ConcatenationSegmentJsonExtensions.FromJson(json);

        Assert.NotNull(result);
        Assert.Equal(_testFilePath, result.FilePath);
    }

    [Fact]
    public void FromJson_DeserializesJsonWithTrimStart()
    {
        var segment = new ConcatenationSegment(_testFilePath) { TrimStart = TimeSpan.FromSeconds(15) };
        var json = segment.ToJson();
        var result = ConcatenationSegmentJsonExtensions.FromJson(json);

        Assert.NotNull(result);
        Assert.Equal(_testFilePath, result.FilePath);
        Assert.Equal(TimeSpan.FromSeconds(15), result.TrimStart);
    }

    [Fact]
    public void FromJson_ReturnsNull_WhenJsonIsWhitespace()
    {
        var result = ConcatenationSegmentJsonExtensions.FromJson("   \n\t  ");
        Assert.Null(result);
    }

    [Fact]
    public void FromJson_ThrowsArgumentNullException_WhenJsonIsNull()
    {
        string? json = null;
        Assert.Throws<ArgumentNullException>(() => ConcatenationSegmentJsonExtensions.FromJson(json!));
    }

    [Fact]
    public void FromJson_ThrowsArgumentException_WhenJsonIsEmptyString()
    {
        var json = string.Empty;
        Assert.Throws<ArgumentException>(() => ConcatenationSegmentJsonExtensions.FromJson(json));
    }

    [Fact]
    public void FromJson_ThrowsJsonException_WhenJsonIsInvalid()
    {
        var json = "invalid json {{{";
        Assert.Throws<System.Text.Json.JsonException>(() => ConcatenationSegmentJsonExtensions.FromJson(json));
    }

    #endregion

    #region TryFromJson

    [Fact]
    public void TryFromJson_ReturnsTrueAndDeserializes_WhenJsonIsValid()
    {
        var segment = new ConcatenationSegment(_testFilePath);
        var json = segment.ToJson();
        var result = ConcatenationSegmentJsonExtensions.TryFromJson(json, out var deserialized);

        Assert.True(result);
        Assert.NotNull(deserialized);
        Assert.Equal(_testFilePath, deserialized.FilePath);
    }

    [Fact]
    public void TryFromJson_ReturnsFalseAndNull_WhenJsonIsInvalid()
    {
        var json = "invalid json {{{";
        var result = ConcatenationSegmentJsonExtensions.TryFromJson(json, out var deserialized);

        Assert.False(result);
        Assert.Null(deserialized);
    }

    [Fact]
    public void TryFromJson_ReturnsFalseAndNull_WhenJsonIsWhitespace()
    {
        var result = ConcatenationSegmentJsonExtensions.TryFromJson("   \n\t  ", out var deserialized);
        Assert.False(result);
        Assert.Null(deserialized);
    }

    [Fact]
    public void TryFromJson_ThrowsArgumentNullException_WhenJsonIsNull()
    {
        string? json = null;
        Assert.Throws<ArgumentNullException>(() => ConcatenationSegmentJsonExtensions.TryFromJson(json!, out _));
    }

    [Fact]
    public void TryFromJson_ThrowsArgumentException_WhenJsonIsEmptyString()
    {
        var json = string.Empty;
        Assert.Throws<ArgumentException>(() => ConcatenationSegmentJsonExtensions.TryFromJson(json, out _));
    }

    [Fact]
    public void TryFromJson_DeserializesComplexSegment()
    {
        var segment = new ConcatenationSegment(_testFilePath)
        {
            TrimStart = TimeSpan.FromSeconds(5),
            TrimDuration = TimeSpan.FromSeconds(20),
            Label = "complex-segment"
        };
        var json = segment.ToJson();
        var result = ConcatenationSegmentJsonExtensions.TryFromJson(json, out var deserialized);

        Assert.True(result);
        Assert.NotNull(deserialized);
        Assert.Equal(_testFilePath, deserialized.FilePath);
        Assert.Equal(TimeSpan.FromSeconds(5), deserialized.TrimStart);
        Assert.Equal(TimeSpan.FromSeconds(20), deserialized.TrimDuration);
        Assert.Equal("complex-segment", deserialized.Label);
    }

    #endregion
}
