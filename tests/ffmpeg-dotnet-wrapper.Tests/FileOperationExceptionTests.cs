using System;
using FFmpegDotnetWrapper.Exceptions;
using Xunit;

namespace FFmpegDotnetWrapper.Tests;

public class FileOperationExceptionTests
{
    [Fact]
    public void FilePath_ReturnsFilePath()
    {
        // Arrange
        var filePath = "path/to/file";
        var exception = new FileOperationException("Error message", filePath);

        // Act
        var result = exception.FilePath;

        // Assert
        Assert.Equal(filePath, result);
    }

    [Fact]
    public void FilePath_ReturnsNull_WhenNotSpecified()
    {
        // Arrange
        var exception = new FileOperationException("Error message");

        // Act
        var result = exception.FilePath;

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Constructor_WithMessageAndFilePath_CreatesException()
    {
        // Arrange
        var message = "Error message";
        var filePath = "path/to/file";

        // Act
        var exception = new FileOperationException(message, filePath);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal(filePath, exception.FilePath);
    }

    [Fact]
    public void Constructor_WithMessageAndFilePathAndInnerException_CreatesException()
    {
        // Arrange
        var message = "Error message";
        var filePath = "path/to/file";
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new FileOperationException(message, filePath, innerException);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal(filePath, exception.FilePath);
        Assert.Same(innerException, exception.InnerException);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_CreatesException()
    {
        // Arrange
        var message = "Error message";
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new FileOperationException(message, innerException);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Null(exception.FilePath);
        Assert.Same(innerException, exception.InnerException);
    }

    [Fact]
    public void Constructor_WithNullMessage_ThrowsArgumentNullException()
    {
        // Arrange
        string message = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new FileOperationException(message));
    }

    [Fact]
    public void Constructor_WithNullFilePath_ThrowsArgumentNullException()
    {
        // Arrange
        string filePath = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new FileOperationException("Error message", filePath));
    }

    [Fact]
    public void Constructor_WithNullInnerException_ThrowsArgumentNullException()
    {
        // Arrange
        Exception innerException = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new FileOperationException("Error message", innerException));
    }
}
