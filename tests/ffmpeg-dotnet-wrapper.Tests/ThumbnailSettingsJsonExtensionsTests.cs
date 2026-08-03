using System;
using FluentAssertions;
using FFmpegDotnetWrapper.Models;
using Xunit;

namespace FFmpegDotnetWrapper.Models.Tests
{
    /// <summary>
    /// Unit tests for <see cref="ThumbnailSettingsJsonExtensions"/>.
    /// Covers happy paths, edge cases, and error handling for JSON (de)serialization.
    /// </summary>
    public class ThumbnailSettingsJsonExtensionsTests
    {
        [Fact]
        public void ToJson_WithValidSettings_ReturnsNonEmptyJson()
        {
            // Arrange
            var settings = new ThumbnailSettings();

            // Act
            var json = settings.ToJson();

            // Assert
            json.Should().NotBeNullOrWhiteSpace();

            // The JSON should be deserializable back to an equivalent object.
            var deserialized = ThumbnailSettingsJsonExtensions.FromJson(json);
            deserialized.Should().NotBeNull();
        }

        [Fact]
        public void ToJson_NullSettings_ThrowsArgumentNullException()
        {
            // Arrange
            ThumbnailSettings? settings = null;

            // Act
            Action act = () => settings!.ToJson();

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void FromJson_NullString_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => ThumbnailSettingsJsonExtensions.FromJson(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void FromJson_EmptyString_ThrowsArgumentException()
        {
            // Act
            Action act = () => ThumbnailSettingsJsonExtensions.FromJson(string.Empty);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void TryFromJson_WithInvalidJson_ReturnsFalseAndNullValue()
        {
            // Arrange
            var invalidJson = "{ this is not valid json }";

            // Act
            var result = ThumbnailSettingsJsonExtensions.TryFromJson(invalidJson, out var value);

            // Assert
            result.Should().BeFalse();
            value.Should().BeNull();
        }

        [Fact]
        public void TryFromJson_WithValidJson_ReturnsTrueAndDeserializedValue()
        {
            // Arrange
            var original = new ThumbnailSettings();
            var json = original.ToJson();

            // Act
            var result = ThumbnailSettingsJsonExtensions.TryFromJson(json, out var value);

            // Assert
            result.Should().BeTrue();
            value.Should().NotBeNull();
        }
    }
}
