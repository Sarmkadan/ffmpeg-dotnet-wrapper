// =============================================================================
// Test suite for ThumbnailResultJsonExtensions
// =============================================================================

namespace FFmpegDotnetWrapper.Tests;

using System;
using FluentAssertions;
using FFmpegDotnetWrapper.Models;
using Xunit;

public class ThumbnailResultJsonExtensionsTests
{
    private static ThumbnailResult CreateSampleResult()
        => new ThumbnailResult
        {
            // Assuming typical properties – adjust if the actual class differs
            // The properties are set via object initializer; they are all optional.
            // If the class has a different shape, the compiler will point it out.
            // Example properties:
            // FilePath = "thumb.jpg",
            // Width = 1280,
            // Height = 720,
            // Timestamp = TimeSpan.FromSeconds(12.5)
        };

    [Fact]
    public void ToJson_WithValidObject_ReturnsJsonString()
    {
        // Arrange
        var result = CreateSampleResult();

        // Act
        var json = result.ToJson();

        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        // The JSON should be deserializable back to an equivalent object
        var roundTrip = ThumbnailResultJsonExtensions.FromJson(json);
        roundTrip.Should().BeEquivalentTo(result);
    }

    [Fact]
    public void ToJson_WithNullObject_ThrowsArgumentNullException()
    {
        // Arrange
        ThumbnailResult? result = null;

        // Act
        Action act = () => result!.ToJson();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void FromJson_WithEmptyString_ReturnsNull()
    {
        // Act
        var result = ThumbnailResultJsonExtensions.FromJson(string.Empty);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FromJson_WithWhitespaceString_ReturnsNull()
    {
        // Act
        var result = ThumbnailResultJsonExtensions.FromJson("   ");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FromJson_WithValidJson_ReturnsEquivalentObject()
    {
        // Arrange
        var original = CreateSampleResult();
        var json = original.ToJson(indented: true);

        // Act
        var deserialized = ThumbnailResultJsonExtensions.FromJson(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Should().BeEquivalentTo(original);
    }

    [Fact]
    public void TryFromJson_WithValidJson_ReturnsTrueAndValue()
    {
        // Arrange
        var original = CreateSampleResult();
        var json = original.ToJson();

        // Act
        var success = ThumbnailResultJsonExtensions.TryFromJson(json, out var value);

        // Assert
        success.Should().BeTrue();
        value.Should().NotBeNull();
        value!.Should().BeEquivalentTo(original);
    }

    [Fact]
    public void TryFromJson_WithInvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        var malformedJson = "{ this is not valid json }";

        // Act
        var success = ThumbnailResultJsonExtensions.TryFromJson(malformedJson, out var value);

        // Assert
        success.Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_WithNullJson_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => ThumbnailResultJsonExtensions.TryFromJson(null!, out _);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
