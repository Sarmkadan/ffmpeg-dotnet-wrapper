namespace FFmpegDotnetWrapper.Tests;

using Xunit;
using FluentAssertions;
using FFmpegDotnetWrapper.Repository;
using FFmpegDotnetWrapper.Models;
using System;

public class OperationRepositoryJsonExtensionsTests
{
    [Fact]
    public void ToJson_WithValidObject_ReturnsJsonString()
    {
        // Arrange
        var repo = new OperationRepository();

        // Act
        var json = repo.ToJson();

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().StartWith("{").And.EndWith("}");
    }

    [Fact]
    public void ToJson_WithIndentedTrue_ReturnsFormattedJson()
    {
        // Arrange
        var repo = new OperationRepository();

        // Act
        var json = repo.ToJson(indented: true);

        // Assert
        json.Should().Contain(Environment.NewLine);
        json.Should().Contain("  ");
    }

    [Fact]
    public void ToJson_WithNullObject_ThrowsArgumentNullException()
    {
        // Arrange
        OperationRepository repo = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => repo.ToJson());
    }

    [Fact]
    public void FromJson_WithValidJson_ReturnsObject()
    {
        // Arrange
        var repo = new OperationRepository();
        var json = repo.ToJson();

        // Act
        var result = OperationRepositoryJsonExtensions.FromJson(json);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void FromJson_WithNullInput_ThrowsArgumentException()
    {
        // Arrange
        string json = null!;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => OperationRepositoryJsonExtensions.FromJson(json));
    }

    [Fact]
    public void FromJson_WithEmptyInput_ThrowsArgumentException()
    {
        // Arrange
        var json = string.Empty;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => OperationRepositoryJsonExtensions.FromJson(json));
    }

    [Fact]
    public void FromJson_WithWhitespaceInput_ReturnsNull()
    {
        // Arrange
        var json = "   ";

        // Act
        var result = OperationRepositoryJsonExtensions.FromJson(json);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_WithValidJson_ReturnsTrueAndObject()
    {
        // Arrange
        var repo = new OperationRepository();
        var json = repo.ToJson();

        // Act
        var success = OperationRepositoryJsonExtensions.TryFromJson(json, out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().NotBeNull();
    }

    [Fact]
    public void TryFromJson_WithInvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        var json = "{ not valid json }";

        // Act
        var success = OperationRepositoryJsonExtensions.TryFromJson(json, out var result);

        // Assert
        success.Should().BeFalse();
        result.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_WithNullInput_ThrowsArgumentException()
    {
        // Arrange
        string json = null!;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => OperationRepositoryJsonExtensions.TryFromJson(json, out _));
    }
}
