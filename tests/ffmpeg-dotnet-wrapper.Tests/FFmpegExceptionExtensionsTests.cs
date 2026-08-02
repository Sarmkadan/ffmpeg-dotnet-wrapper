using Xunit;
using FluentAssertions;
using FFmpegDotnetWrapper.Exceptions;

namespace FFmpegDotnetWrapper.Tests;

/// <summary>
/// Provides unit tests for <see cref="FFmpegExceptionExtensions"/>.
/// </summary>
public class FFmpegExceptionExtensionsTests
{
    /// <summary>
    /// Verifies that <see cref="FFmpegExceptionExtensions.ToDetailedErrorMessage(FFmpegException)"/> returns a formatted error message for a general <see cref="FFmpegException"/>.
    /// </summary>
    [Fact]
    public void ToDetailedErrorMessage_FFmpegException_ReturnsFormattedMessage()
    {
        var ex = new FFmpegException("Test Message", 1, "Error Output");
        
        var result = ex.ToDetailedErrorMessage();
        
        result.Should().Contain("FFmpeg Error: FFmpegException");
        result.Should().Contain("Message: Test Message");
        result.Should().Contain("Exit Code: 1");
        result.Should().Contain("Error Output: Error Output");
    }

    /// <summary>
    /// Verifies that <see cref="FFmpegExceptionExtensions.ToDetailedErrorMessage(InvalidMediaFileException)"/> returns a formatted error message including the file path.
    /// </summary>
    [Fact]
    public void ToDetailedErrorMessage_InvalidMediaFileException_ReturnsFormattedMessageWithFilePath()
    {
        var ex = new InvalidMediaFileException("Invalid File", "path/to/file.mp4");
        
        var result = ex.ToDetailedErrorMessage();
        
        result.Should().Contain("FFmpeg Error: InvalidMediaFileException");
        result.Should().Contain("File Path: path/to/file.mp4");
    }

    /// <summary>
    /// Verifies that <see cref="FFmpegExceptionExtensions.ToDetailedErrorMessage(InvalidOperationConfigurationException)"/> returns a formatted error message including the configuration key.
    /// </summary>
    [Fact]
    public void ToDetailedErrorMessage_InvalidOperationConfigurationException_ReturnsFormattedMessageWithConfigKey()
    {
        var ex = new InvalidOperationConfigurationException("Invalid Config", "myKey");
        
        var result = ex.ToDetailedErrorMessage();
        
        result.Should().Contain("FFmpeg Error: InvalidOperationConfigurationException");
        result.Should().Contain("Configuration Key: myKey");
    }

    /// <summary>
    /// Verifies that <see cref="FFmpegExceptionExtensions.ToDetailedErrorMessage(FFmpegProcessException)"/> returns a formatted error message including the timeout duration.
    /// </summary>
    [Fact]
    public void ToDetailedErrorMessage_FFmpegProcessException_ReturnsFormattedMessageWithTimeout()
    {
        var ex = new FFmpegProcessException("Timeout", TimeSpan.FromSeconds(30));
        
        var result = ex.ToDetailedErrorMessage();
        
        result.Should().Contain("FFmpeg Error: FFmpegProcessException");
        result.Should().Contain("Timeout: 30 seconds");
    }

    /// <summary>
    /// Verifies that <see cref="FFmpegExceptionExtensions.ToDetailedErrorMessage(FFmpegException)"/> throws an <see cref="ArgumentNullException"/> when the input is null.
    /// </summary>
    [Fact]
    public void ToDetailedErrorMessage_NullInput_ThrowsArgumentNullException()
    {
        FFmpegException? ex = null;
        
        Action act = () => ex!.ToDetailedErrorMessage();
        
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that <see cref="FFmpegExceptionExtensions.IsProcessFailure(FFmpegException)"/> returns true for a <see cref="FFmpegProcessException"/>.
    /// </summary>
    [Fact]
    public void IsProcessFailure_FFmpegProcessException_ReturnsTrue()
    {
        var ex = new FFmpegProcessException();
        ex.IsProcessFailure().Should().BeTrue();
    }

    /// <summary>
    /// Verifies that <see cref="FFmpegExceptionExtensions.IsProcessFailure(FFmpegException)"/> returns false for a general <see cref="FFmpegException"/>.
    /// </summary>
    [Fact]
    public void IsProcessFailure_OtherException_ReturnsFalse()
    {
        var ex = new FFmpegException();
        ex.IsProcessFailure().Should().BeFalse();
    }

    /// <summary>
    /// Verifies that <see cref="FFmpegExceptionExtensions.IsInvalidMediaFileError(FFmpegException)"/> returns true for an <see cref="InvalidMediaFileException"/>.
    /// </summary>
    [Fact]
    public void IsInvalidMediaFileError_InvalidMediaFileException_ReturnsTrue()
    {
        var ex = new InvalidMediaFileException();
        ex.IsInvalidMediaFileError().Should().BeTrue();
    }

    /// <summary>
    /// Verifies that <see cref="FFmpegExceptionExtensions.IsInvalidConfigurationError(FFmpegException)"/> returns true for an <see cref="InvalidOperationConfigurationException"/>.
    /// </summary>
    [Fact]
    public void IsInvalidConfigurationError_InvalidOperationConfigurationException_ReturnsTrue()
    {
        var ex = new InvalidOperationConfigurationException();
        ex.IsInvalidConfigurationError().Should().BeTrue();
    }

    /// <summary>
    /// Verifies that <see cref="FFmpegExceptionExtensions.IsUnsupportedOperationError(FFmpegException)"/> returns true for an <see cref="UnsupportedOperationException"/>.
    /// </summary>
    [Fact]
    public void IsUnsupportedOperationError_UnsupportedOperationException_ReturnsTrue()
    {
        var ex = new UnsupportedOperationException();
        ex.IsUnsupportedOperationError().Should().BeTrue();
    }
}
