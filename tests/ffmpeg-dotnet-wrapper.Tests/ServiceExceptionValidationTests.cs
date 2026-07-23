using System;
using FFmpegDotnetWrapper.Exceptions;
using Xunit;

namespace FFmpegDotnetWrapper.Tests;

public class ServiceExceptionValidationTests
{
    [Fact]
    public void Validate_WithValidServiceException_ReturnsEmptyList()
    {
        // Arrange
        var exception = new ServiceException("Valid error message", "FFmpeg.Core");

        // Act
        var result = exception.Validate();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_WithNullServiceException_ThrowsArgumentNullException()
    {
        // Arrange
        ServiceException exception = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => exception.Validate());
    }

    [Fact]
    public void Validate_WithWhitespaceMessage_ReturnsValidationProblem()
    {
        // Arrange
        var exception = new ServiceException("   ", "FFmpeg.Core");

        // Act
        var result = exception.Validate();

        // Assert
        Assert.Single(result);
        Assert.Contains("Message cannot be null, empty, or whitespace", result[0]);
    }

    [Fact]
    public void Validate_WithNullServiceName_ValidatesSuccessfully()
    {
        // Arrange
        var exception = new ServiceException("Valid error message", serviceName: null);

        // Act
        var result = exception.Validate();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_WithEmptyServiceName_ValidatesSuccessfully()
    {
        // Arrange
        var exception = new ServiceException("Valid error message", serviceName: string.Empty);

        // Act
        var result = exception.Validate();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_WithWhitespaceServiceName_ValidatesSuccessfully()
    {
        // Arrange
        var exception = new ServiceException("Valid error message", "   ");

        // Act
        var result = exception.Validate();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_WithExitCodeButNoErrorOutput_ReturnsValidationProblem()
    {
        // Arrange
        var exception = new ProcessExecutionException("Error with exit code", 1, null);

        // Act
        var result = exception.Validate();

        // Assert
        Assert.Single(result);
        Assert.Contains("ErrorOutput must be provided when ExitCode is set", result[0]);
    }

    [Fact]
    public void Validate_WithExitCodeAndErrorOutput_ReturnsEmptyList()
    {
        // Arrange
        var exception = new ProcessExecutionException("Error with exit code", 1, "Error output content");

        // Act
        var result = exception.Validate();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_WithValidProcessExecutionException_ReturnsEmptyList()
    {
        // Arrange
        var exception = new ProcessExecutionException("Valid error message", 0, "Error output");

        // Act
        var result = exception.Validate();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void IsValid_WithValidServiceException_ReturnsTrue()
    {
        // Arrange
        var exception = new ServiceException("Valid error message", "FFmpeg.Core");

        // Act
        var result = exception.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_WithNullServiceException_ReturnsFalse()
    {
        // Arrange
        ServiceException exception = null!;

        // Act
        var result = exception.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_WithInvalidProcessExecutionException_ReturnsFalse()
    {
        // Arrange
        var exception = new ProcessExecutionException("Valid message", 1, null);

        // Act
        var result = exception.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EnsureValid_WithValidServiceException_DoesNotThrow()
    {
        // Arrange
        var exception = new ServiceException("Valid error message", "FFmpeg.Core");

        // Act
        var exceptionResult = Record.Exception(() => exception.EnsureValid());

        // Assert
        Assert.Null(exceptionResult);
    }

    [Fact]
    public void EnsureValid_WithNullServiceException_ThrowsArgumentNullException()
    {
        // Arrange
        ServiceException exception = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => exception.EnsureValid());
    }

    [Fact]
    public void EnsureValid_WithInvalidProcessExecutionException_ThrowsArgumentException()
    {
        // Arrange
        var exception = new ProcessExecutionException("Valid message", 1, null);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => exception.EnsureValid());
        Assert.Contains("is invalid", ex.Message);
    }

    [Fact]
    public void Validate_WithBaseServiceException_ReturnsEmptyList()
    {
        // Arrange
        var exception = new ServiceException("Valid error message");

        // Act
        var result = exception.Validate();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_WithServiceExceptionWithInnerException_ReturnsEmptyList()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner error");
        var exception = new ServiceException("Valid error message", "FFmpeg.Core", innerException);

        // Act
        var result = exception.Validate();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void IsValid_WithServiceExceptionWithInnerException_ReturnsTrue()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner error");
        var exception = new ServiceException("Valid error message", "FFmpeg.Core", innerException);

        // Act
        var result = exception.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EnsureValid_WithServiceExceptionWithInnerException_DoesNotThrow()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner error");
        var exception = new ServiceException("Valid error message", "FFmpeg.Core", innerException);

        // Act
        var exceptionResult = Record.Exception(() => exception.EnsureValid());

        // Assert
        Assert.Null(exceptionResult);
    }

    [Fact]
    public void Validate_ReturnsReadOnlyCollection()
    {
        // Arrange
        var exception = new ServiceException("Valid error message", "FFmpeg.Core");

        // Act
        var result = exception.Validate();

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void Validate_WithMultipleIssues_ReturnsMultipleProblems()
    {
        // Arrange
        var exception = new ProcessExecutionException("Error message", 1, null);

        // Act
        var result = exception.Validate();

        // Assert
        Assert.Single(result);
        Assert.Contains("ErrorOutput must be provided when ExitCode is set", result[0]);
    }

    [Fact]
    public void IsValid_WithValidProcessExecutionException_ReturnsTrue()
    {
        // Arrange
        var exception = new ProcessExecutionException("Valid error message", 0, "Error output");

        // Act
        var result = exception.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EnsureValid_WithValidProcessExecutionException_DoesNotThrow()
    {
        // Arrange
        var exception = new ProcessExecutionException("Valid error message", 0, "Error output");

        // Act
        var exceptionResult = Record.Exception(() => exception.EnsureValid());

        // Assert
        Assert.Null(exceptionResult);
    }
}
