using System;
using FFmpegDotnetWrapper.Exceptions;
using Xunit;

namespace FFmpegDotnetWrapper.Tests;

public class ServiceExceptionExtensionsTests
{
    [Fact]
    public void WithServiceName_WithValidParameters_ReturnsNewExceptionWithNewServiceName()
    {
        // Arrange
        var originalMessage = "Service failed to start";
        var originalServiceName = "OriginalService";
        var innerException = new InvalidOperationException("Inner error");
        var originalException = new ServiceException(originalMessage, originalServiceName, innerException);
        var newServiceName = "NewService";

        // Act
        var result = originalException.WithServiceName(newServiceName);

        // Assert
        Assert.NotSame(originalException, result);
        Assert.Equal(originalMessage, result.Message);
        Assert.Equal(newServiceName, result.ServiceName);
        Assert.Same(innerException, result.InnerException);
    }

    [Fact]
    public void WithServiceName_WithNullOriginalException_ThrowsArgumentNullException()
    {
        // Arrange
        ServiceException originalException = null!;
        var newServiceName = "NewService";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => originalException.WithServiceName(newServiceName));
    }

    [Fact]
    public void WithServiceName_WithNullNewServiceName_ThrowsArgumentNullException()
    {
        // Arrange
        var originalException = new ServiceException("Error message", "OriginalService");
        string newServiceName = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => originalException.WithServiceName(newServiceName));
    }

    [Fact]
    public void WithServiceName_WithEmptyNewServiceName_ReturnsNewExceptionWithEmptyServiceName()
    {
        // Arrange
        var originalException = new ServiceException("Error message", "OriginalService");
        var newServiceName = string.Empty;

        // Act
        var result = originalException.WithServiceName(newServiceName);

        // Assert
        Assert.NotSame(originalException, result);
        Assert.Equal("Error message", result.Message);
        Assert.Empty(result.ServiceName);
    }

    [Fact]
    public void GetMessageWithService_WithExceptionWithServiceName_ReturnsFormattedStringWithServiceName()
    {
        // Arrange
        var message = "Service failed to start";
        var serviceName = "FFmpeg.Core";
        var exception = new ServiceException(message, serviceName);

        // Act
        var result = exception.GetMessageWithService();

        // Assert
        Assert.Equal($"{serviceName}: {message}", result);
    }

    [Fact]
    public void GetMessageWithService_WithExceptionWithoutServiceName_ReturnsOnlyMessage()
    {
        // Arrange
        var message = "Service failed to start";
        var exception = new ServiceException(message);

        // Act
        var result = exception.GetMessageWithService();

        // Assert
        Assert.Equal(message, result);
    }

    [Fact]
    public void HasServiceContext_WithExceptionWithServiceName_ReturnsTrue()
    {
        // Arrange
        var exception = new ServiceException("Error message", "FFmpeg.Core");

        // Act
        var result = exception.HasServiceContext();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasServiceContext_WithExceptionWithoutServiceName_ReturnsFalse()
    {
        // Arrange
        var exception = new ServiceException("Error message");

        // Act
        var result = exception.HasServiceContext();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasServiceContext_WithExceptionWithNullServiceName_ReturnsFalse()
    {
        // Arrange
        var exception = new ServiceException("Error message", serviceName: null);

        // Act
        var result = exception.HasServiceContext();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasServiceContext_WithExceptionWithEmptyServiceName_ReturnsFalse()
    {
        // Arrange
        var exception = new ServiceException("Error message", serviceName: string.Empty);

        // Act
        var result = exception.HasServiceContext();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasServiceContext_WithNullException_ThrowsArgumentNullException()
    {
        // Arrange
        ServiceException exception = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => exception.HasServiceContext());
    }

    [Fact]
    public void WithServiceName_WithExceptionWithoutInnerException_PreservesNullInnerException()
    {
        // Arrange
        var originalException = new ServiceException("Error message", "OriginalService");
        var newServiceName = "NewService";

        // Act
        var result = originalException.WithServiceName(newServiceName);

        // Assert
        Assert.Null(result.InnerException);
    }

    [Fact]
    public void GetMessageWithService_WithExceptionWithInnerException_ReturnsFormattedMessage()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner error");
        var exception = new ServiceException("Outer error", "FFmpeg.Core", innerException);

        // Act
        var result = exception.GetMessageWithService();

        // Assert
        Assert.Equal("FFmpeg.Core: Outer error", result);
    }
}