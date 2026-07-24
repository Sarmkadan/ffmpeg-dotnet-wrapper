namespace FFmpegDotnetWrapper.Tests;

using System;
using System.Text.Json;
using FluentAssertions;
using FFmpegDotnetWrapper.Models;
using Xunit;

public class TranscodeSettingsJsonExtensionsTests
{
    [Fact]
    public void ToJson_WithValidSettings_ReturnsCamelCaseJson()
    {
        // Arrange
        var settings = new TranscodeSettings
        {
            // Assuming TranscodeSettings has a few simple properties.
            // If the class has no writable properties, this object will still serialize.
        };

        // Act
        var json = settings.ToJson();

        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        // The JSON should be camel‑cased (e.g., "someProperty" instead of "SomeProperty").
        // We verify by deserializing with a case‑sensitive contract and checking a property name.
        var doc = JsonDocument.Parse(json);
        foreach (var prop in doc.RootElement.EnumerateObject())
        {
            // Property names should start with a lower‑case letter.
            char first = prop.Name[0];
            first.Should().Match<char>(c => char.IsLower(c));
        }
    }

    [Fact]
    public void ToJson_NullSettings_ThrowsArgumentNullException()
    {
        // Arrange
        TranscodeSettings? settings = null;

        // Act
        Action act = () => settings!.ToJson();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToJson_WithIndentation_ProducesIndentedJson()
    {
        // Arrange
        var settings = new TranscodeSettings();

        // Act
        var json = settings.ToJson(indented: true);

        // Assert
        json.Should().Contain("\n"); // Indented JSON contains line breaks.
    }

    [Fact]
    public void FromJson_ValidJson_ReturnsObject()
    {
        // Arrange
        var original = new TranscodeSettings();
        var json = original.ToJson();

        // Act
        var result = TranscodeSettingsJsonExtensions.FromJson(json);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<TranscodeSettings>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FromJson_NullOrWhiteSpace_ThrowsArgumentException(string json)
    {
        // Act
        Action act = () => TranscodeSettingsJsonExtensions.FromJson(json!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndObject()
    {
        // Arrange
        var original = new TranscodeSettings();
        var json = original.ToJson();

        // Act
        var success = TranscodeSettingsJsonExtensions.TryFromJson(json, out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().NotBeNull();
        result.Should().BeOfType<TranscodeSettings>();
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        var invalidJson = "{ this is not valid json }";

        // Act
        var success = TranscodeSettingsJsonExtensions.TryFromJson(invalidJson, out var result);

        // Assert
        success.Should().BeFalse();
        result.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryFromJson_NullOrWhiteSpace_ThrowsArgumentException(string json)
    {
        // Act
        Action act = () => TranscodeSettingsJsonExtensions.TryFromJson(json!, out _);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
