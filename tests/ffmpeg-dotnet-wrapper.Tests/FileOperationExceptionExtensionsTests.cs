using System;
using FFmpegDotnetWrapper.Exceptions;
using FluentAssertions;
using Xunit;

namespace FFmpegDotnetWrapper.Tests.Exceptions;

public class FileOperationExceptionExtensionsTests
{
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

    [Fact]
    public void GetFileName_WithNullException_ThrowsArgumentNullException()
    {
        // Arrange
        FileOperationException exception = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => exception.GetFileName());
    }

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

    [Fact]
    public void HasFilePath_WithNullException_ThrowsArgumentNullException()
    {
        // Arrange
        FileOperationException exception = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => exception.HasFilePath());
    }

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

    [Fact]
    public void ToLogString_WithNullException_ThrowsArgumentNullException()
    {
        // Arrange
        FileOperationException exception = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => exception.ToLogString());
    }

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

    [Fact]
    public void WithAdditionalInfo_WithNullAdditionalInfo_ThrowsArgumentNullException()
    {
        // Arrange
        var originalException = new FileOperationException("Original error", "/path/to/file.txt");
        string additionalInfo = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => originalException.WithAdditionalInfo(additionalInfo));
    }

    [Fact]
    public void WithAdditionalInfo_WithNullException_ThrowsArgumentNullException()
    {
        // Arrange
        FileOperationException originalException = null!;
        var additionalInfo = "Additional context";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => originalException.WithAdditionalInfo(additionalInfo));
    }

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
