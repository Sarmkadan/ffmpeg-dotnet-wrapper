using System;
using FFmpegDotnetWrapper.Exceptions;
using FluentAssertions;
using Xunit;

namespace FFmpegDotnetWrapper.Tests.Exceptions;

/// <summary>
/// Contains unit tests for the FileOperationExceptionExtensions class.
/// </summary>
public class FileOperationExceptionExtensionsTests
{
    /// <summary>
    /// Tests that GetFileName returns the file name when a valid file path is provided.
    /// </summary>
    [Fact]
    public void GetFileName_WithValidFilePath_ReturnsFileName()
    {
        // Arrange
        var exception = new FileOperationException("Test error", "/path/to/file.txt");

        // Act
        var result = exception.GetFileName();

        // Assert
        result.Should().Be("file.txt");
    }

    /// <summary>
    /// Tests that GetFileName returns an empty string when the file path is null.
    /// </summary>
    [Fact]
    public void GetFileName_WithNullFilePath_ReturnsEmptyString()
    {
        // Arrange
        var exception = new FileOperationException("Test error", string.Empty);

        // Act
        var result = exception.GetFileName();

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that GetFileName returns an empty string when the file path is empty.
    /// </summary>
    [Fact]
    public void GetFileName_WithEmptyFilePath_ReturnsEmptyString()
    {
        // Arrange
        var exception = new FileOperationException("Test error", string.Empty);

        // Act
        var result = exception.GetFileName();

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that GetFileName returns an empty string when the file path contains only whitespace.
    /// </summary>
    [Fact]
    public void GetFileName_WithWhitespaceFilePath_ReturnsEmptyString()
    {
        // Arrange
        var exception = new FileOperationException("Test error", "   ");

        // Act
        var result = exception.GetFileName();

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that GetFileName returns an empty string when the path does not contain a file name.
    /// </summary>
    [Fact]
    public void GetFileName_WithPathWithoutFileName_ReturnsEmptyString()
    {
        // Arrange
        var exception = new FileOperationException("Test error", "/path/to/");

        // Act
        var result = exception.GetFileName();

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that GetFileName returns the file name from a deeply nested path.
    /// </summary>
    [Fact]
    public void GetFileName_WithNestedPath_ReturnsFileName()
    {
        // Arrange
        var exception = new FileOperationException("Test error", "/very/long/path/to/deeply/nested/directory/file.log");

        // Act
        var result = exception.GetFileName();

        // Assert
        result.Should().Be("file.log");
    }

    /// <summary>
    /// Tests that GetFileName throws an ArgumentNullException when the exception is null.
    /// </summary>
    [Fact]
    public void GetFileName_WithNullException_ThrowsArgumentNullException()
    {
        // Arrange
        FileOperationException exception = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => exception.GetFileName());
    }

    /// <summary>
    /// Tests that HasFilePath returns true when a valid file path is provided.
    /// </summary>
    [Fact]
    public void HasFilePath_WithValidFilePath_ReturnsTrue()
    {
        // Arrange
        var exception = new FileOperationException("Test error", "/path/to/file.txt");

        // Act
        var result = exception.HasFilePath();

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that HasFilePath returns false when the file path is null.
    /// </summary>
    [Fact]
    public void HasFilePath_WithNullFilePath_ReturnsFalse()
    {
        // Arrange
        var exception = new FileOperationException("Test error", string.Empty);

        // Act
        var result = exception.HasFilePath();

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that HasFilePath returns false when the file path is empty.
    /// </summary>
    [Fact]
    public void HasFilePath_WithEmptyFilePath_ReturnsFalse()
    {
        // Arrange
        var exception = new FileOperationException("Test error", string.Empty);

        // Act
        var result = exception.HasFilePath();

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that HasFilePath returns false when the file path contains only whitespace.
    /// </summary>
    [Fact]
    public void HasFilePath_WithWhitespaceFilePath_ReturnsFalse()
    {
        // Arrange
        var exception = new FileOperationException("Test error", "   ");

        // Act
        var result = exception.HasFilePath();

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that HasFilePath throws an ArgumentNullException when the exception is null.
    /// </summary>
    [Fact]
    public void HasFilePath_WithNullException_ThrowsArgumentNullException()
    {
        // Arrange
        FileOperationException exception = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => exception.HasFilePath());
    }

    /// <summary>
    /// Tests that ToLogString returns a formatted string including the file path when it is provided.
    /// </summary>
    [Fact]
    public void ToLogString_WithFilePath_ReturnsFormattedStringWithFilePath()
    {
        // Arrange
        var exception = new FileOperationException("File not found", "/path/to/missing.txt");

        // Act
        var result = exception.ToLogString();

        // Assert
        result.Should().Be("Error: File not found (File: /path/to/missing.txt)");
    }

    /// <summary>
    /// Tests that ToLogString returns a formatted string without the file path when it is not provided.
    /// </summary>
    [Fact]
    public void ToLogString_WithoutFilePath_ReturnsFormattedStringWithoutFilePath()
    {
        // Arrange
        var exception = new FileOperationException("Generic error");

        // Act
        var result = exception.ToLogString();

        // Assert
        result.Should().Be("Error: Generic error");
    }

    /// <summary>
    /// Tests that ToLogString returns a formatted string without the file path when it is null.
    /// </summary>
    [Fact]
    public void ToLogString_WithNullFilePath_ReturnsFormattedStringWithoutFilePath()
    {
        // Arrange
        var exception = new FileOperationException("Generic error", string.Empty);

        // Act
        var result = exception.ToLogString();

        // Assert
        result.Should().Be("Error: Generic error");
    }

    /// <summary>
    /// Tests that ToLogString returns a formatted string without the file path when it is empty.
    /// </summary>
    [Fact]
    public void ToLogString_WithEmptyFilePath_ReturnsFormattedStringWithoutFilePath()
    {
        // Arrange
        var exception = new FileOperationException("Generic error", string.Empty);

        // Act
        var result = exception.ToLogString();

        // Assert
        result.Should().Be("Error: Generic error");
    }

    /// <summary>
    /// Tests that ToLogString returns a formatted string without the file path when it contains only whitespace.
    /// </summary>
    [Fact]
    public void ToLogString_WithWhitespaceFilePath_ReturnsFormattedStringWithoutFilePath()
    {
        // Arrange
        var exception = new FileOperationException("Generic error", "   ");

        // Act
        var result = exception.ToLogString();

        // Assert
        result.Should().Be("Error: Generic error");
    }

    /// <summary>
    /// Tests that ToLogString throws an ArgumentNullException when the exception is null.
    /// </summary>
    [Fact]
    public void ToLogString_WithNullException_ThrowsArgumentNullException()
    {
        // Arrange
        FileOperationException exception = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => exception.ToLogString());
    }

    /// <summary>
    /// Tests that WithAdditionalInfo returns a new exception with a combined message when valid additional info is provided.
    /// </summary>
    [Fact]
    public void WithAdditionalInfo_WithValidAdditionalInfo_ReturnsNewExceptionWithCombinedMessage()
    {
        // Arrange
        var originalException = new FileOperationException("Original error", "/path/to/file.txt");
        var additionalInfo = "Additional context about the error";

        // Act
        var result = originalException.WithAdditionalInfo(additionalInfo);

        // Assert
        result.Should().NotBeSameAs(originalException);
        result.Message.Should().Be("Original error - Additional context about the error");
        result.FilePath.Should().Be("/path/to/file.txt");
        result.InnerException.Should().BeSameAs(originalException);
    }

    /// <summary>
    /// Tests that WithAdditionalInfo preserves an empty file path when provided.
    /// </summary>
    [Fact]
    public void WithAdditionalInfo_WithNullFilePath_PreservesNullFilePath()
    {
        // Arrange
        var originalException = new FileOperationException("Original error", string.Empty);
        var additionalInfo = "Additional context";

        // Act
        var result = originalException.WithAdditionalInfo(additionalInfo);

        // Assert
        result.FilePath.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that WithAdditionalInfo preserves an empty file path when provided.
    /// </summary>
    [Fact]
    public void WithAdditionalInfo_WithEmptyFilePath_PreservesEmptyFilePath()
    {
        // Arrange
        var originalException = new FileOperationException("Original error", string.Empty);
        var additionalInfo = "Additional context";

        // Act
        var result = originalException.WithAdditionalInfo(additionalInfo);

        // Assert
        result.FilePath.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that WithAdditionalInfo throws an ArgumentNullException when additional info is null.
    /// </summary>
    [Fact]
    public void WithAdditionalInfo_WithNullAdditionalInfo_ThrowsArgumentNullException()
    {
        // Arrange
        var originalException = new FileOperationException("Original error", "/path/to/file.txt");
        string additionalInfo = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => originalException.WithAdditionalInfo(additionalInfo));
    }

    /// <summary>
    /// Tests that WithAdditionalInfo throws an ArgumentNullException when the exception is null.
    /// </summary>
    [Fact]
    public void WithAdditionalInfo_WithNullException_ThrowsArgumentNullException()
    {
        // Arrange
        FileOperationException originalException = null!;
        var additionalInfo = "Additional context";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => originalException.WithAdditionalInfo(additionalInfo));
    }

    /// <summary>
    /// Tests that WithAdditionalInfo preserves the original inner exception when creating a new exception.
    /// </summary>
    [Fact]
    public void WithAdditionalInfo_WithInnerException_PreservesInnerException()
    {
        // Arrange
        var innerException = new Exception("Inner error");
        var originalException = new FileOperationException("Original error", "/path/to/file.txt", innerException);
        var additionalInfo = "Additional context";

        // Act
        var result = originalException.WithAdditionalInfo(additionalInfo);

        // Assert
        result.InnerException.Should().BeSameAs(originalException);
    }

    /// <summary>
    /// Tests that WithAdditionalInfo correctly combines complex messages.
    /// </summary>
    [Fact]
    public void WithAdditionalInfo_WithComplexMessage_CombinesMessagesCorrectly()
    {
        // Arrange
        var originalException = new FileOperationException("Failed to read file: data.json", "/data/file.json");
        var additionalInfo = "Operation: ImportData, Attempt: 3";

        // Act
        var result = originalException.WithAdditionalInfo(additionalInfo);

        // Assert
        result.Message.Should().Be("Failed to read file: data.json - Operation: ImportData, Attempt: 3");
    }
}
