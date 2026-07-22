using System;
using FFmpegDotnetWrapper.Exceptions;
using Xunit;

namespace FFmpegDotnetWrapper.Tests;

public class ProcessExecutionExceptionTests
{
    [Fact]
    public void Constructor_WithMessageOnly_ReturnsExceptionWithMessage()
    {
        // Arrange
        var message = "Test message";

        // Act
        var exception = new ProcessExecutionException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void Constructor_WithMessageAndExitCode_ReturnsExceptionWithMessageAndExitCode()
    {
        // Arrange
        var message = "Test message";
        var exitCode = 123;

        // Act
        var exception = new ProcessExecutionException(message, exitCode);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal(exitCode, exception.ExitCode);
    }

    [Fact]
    public void Constructor_WithMessageExitCodeAndErrorOutput_ReturnsExceptionWithMessageExitCodeAndErrorOutput()
    {
        // Arrange
        var message = "Test message";
        var exitCode = 123;
        var errorOutput = "Test error output";

        // Act
        var exception = new ProcessExecutionException(message, exitCode, errorOutput);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal(exitCode, exception.ExitCode);
        Assert.Equal(errorOutput, exception.ErrorOutput);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_ReturnsExceptionWithMessageAndInnerException()
    {
        // Arrange
        var message = "Test message";
        var innerException = new InvalidOperationException("Inner exception");

        // Act
        var exception = new ProcessExecutionException(message, innerException);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Same(innerException, exception.InnerException);
    }

    [Fact]
    public void Constructor_WithMessageExitCodeErrorOutputAndInnerException_ReturnsExceptionWithMessageExitCodeErrorOutputAndInnerException()
    {
        // Arrange
        var message = "Test message";
        var exitCode = 123;
        var errorOutput = "Test error output";
        var innerException = new InvalidOperationException("Inner exception");

        // Act
        var exception = new ProcessExecutionException(message, exitCode, errorOutput, innerException);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal(exitCode, exception.ExitCode);
        Assert.Equal(errorOutput, exception.ErrorOutput);
        Assert.Same(innerException, exception.InnerException);
    }

    [Fact]
    public void ExitCode_Property_ReturnsExitCode()
    {
        // Arrange
        var exitCode = 123;
        var exception = new ProcessExecutionException("Test message", exitCode);

        // Act
        var result = exception.ExitCode;

        // Assert
        Assert.Equal(exitCode, result);
    }

    [Fact]
    public void ErrorOutput_Property_ReturnsErrorOutput()
    {
        // Arrange
        var errorOutput = "Test error output";
        var exception = new ProcessExecutionException("Test message", 123, errorOutput);

        // Act
        var result = exception.ErrorOutput;

        // Assert
        Assert.Equal(errorOutput, result);
    }
}
