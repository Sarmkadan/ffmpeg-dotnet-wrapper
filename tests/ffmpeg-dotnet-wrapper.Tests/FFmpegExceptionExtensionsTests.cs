using Xunit;
using FluentAssertions;
using FFmpegDotnetWrapper.Exceptions;

namespace FFmpegDotnetWrapper.Tests;

public class FFmpegExceptionExtensionsTests
{
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

    [Fact]
    public void ToDetailedErrorMessage_InvalidMediaFileException_ReturnsFormattedMessageWithFilePath()
    {
        var ex = new InvalidMediaFileException("Invalid File", "path/to/file.mp4");
        
        var result = ex.ToDetailedErrorMessage();
        
        result.Should().Contain("FFmpeg Error: InvalidMediaFileException");
        result.Should().Contain("File Path: path/to/file.mp4");
    }

    [Fact]
    public void ToDetailedErrorMessage_InvalidOperationConfigurationException_ReturnsFormattedMessageWithConfigKey()
    {
        var ex = new InvalidOperationConfigurationException("Invalid Config", "myKey");
        
        var result = ex.ToDetailedErrorMessage();
        
        result.Should().Contain("FFmpeg Error: InvalidOperationConfigurationException");
        result.Should().Contain("Configuration Key: myKey");
    }

    [Fact]
    public void ToDetailedErrorMessage_FFmpegProcessException_ReturnsFormattedMessageWithTimeout()
    {
        var ex = new FFmpegProcessException("Timeout", TimeSpan.FromSeconds(30));
        
        var result = ex.ToDetailedErrorMessage();
        
        result.Should().Contain("FFmpeg Error: FFmpegProcessException");
        result.Should().Contain("Timeout: 30 seconds");
    }

    [Fact]
    public void ToDetailedErrorMessage_NullInput_ThrowsArgumentNullException()
    {
        FFmpegException? ex = null;
        
        Action act = () => ex!.ToDetailedErrorMessage();
        
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void IsProcessFailure_FFmpegProcessException_ReturnsTrue()
    {
        var ex = new FFmpegProcessException();
        ex.IsProcessFailure().Should().BeTrue();
    }

    [Fact]
    public void IsProcessFailure_OtherException_ReturnsFalse()
    {
        var ex = new FFmpegException();
        ex.IsProcessFailure().Should().BeFalse();
    }

    [Fact]
    public void IsInvalidMediaFileError_InvalidMediaFileException_ReturnsTrue()
    {
        var ex = new InvalidMediaFileException();
        ex.IsInvalidMediaFileError().Should().BeTrue();
    }

    [Fact]
    public void IsInvalidConfigurationError_InvalidOperationConfigurationException_ReturnsTrue()
    {
        var ex = new InvalidOperationConfigurationException();
        ex.IsInvalidConfigurationError().Should().BeTrue();
    }

    [Fact]
    public void IsUnsupportedOperationError_UnsupportedOperationException_ReturnsTrue()
    {
        var ex = new UnsupportedOperationException();
        ex.IsUnsupportedOperationError().Should().BeTrue();
    }
}
