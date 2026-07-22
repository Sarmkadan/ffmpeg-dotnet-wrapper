using Xunit;
using FFmpegDotnetWrapper.Exceptions;

namespace FFmpegDotnetWrapper.Tests;

public class ServiceExceptionTests
{
    [Fact]
    public void Constructor_MessageOnly_SetsMessageAndNullServiceName()
    {
        // Arrange
        var message = "Service failed to start.";

        // Act
        var exception = new ServiceException(message);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Null(exception.ServiceName);
    }

    [Fact]
    public void Constructor_MessageAndServiceName_SetsMessageAndServiceName()
    {
        // Arrange
        var message = "Service failed to start.";
        var serviceName = "FFmpeg.Core";

        // Act
        var exception = new ServiceException(message, serviceName);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal(serviceName, exception.ServiceName);
    }

    [Fact]
    public void Constructor_MessageAndInnerException_SetsMessageAndInnerException()
    {
        // Arrange
        var message = "Service failed to start.";
        var innerException = new InvalidOperationException("Invalid operation");

        // Act
        var exception = new ServiceException(message, innerException);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Same(innerException, exception.InnerException);
        Assert.Null(exception.ServiceName);
    }

    [Fact]
    public void Constructor_MessageServiceNameAndInnerException_SetsAllProperties()
    {
        // Arrange
        var message = "Service failed to start.";
        var serviceName = "FFmpeg.Core";
        var innerException = new InvalidOperationException("Invalid operation");

        // Act
        var exception = new ServiceException(message, serviceName, innerException);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal(serviceName, exception.ServiceName);
        Assert.Same(innerException, exception.InnerException);
    }

    [Fact]
    public void Constructor_MessageExitCodeAndErrorOutput_SetsMessageAndNullServiceName()
    {
        // Arrange
        var message = "Process exited with error.";
        var exitCode = 1;
        var errorOutput = "File not found";

        // Act
        var exception = new ServiceException(message, exitCode, errorOutput);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Null(exception.ServiceName);
    }
}
