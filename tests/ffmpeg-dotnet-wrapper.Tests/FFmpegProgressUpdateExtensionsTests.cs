using FFmpegDotnetWrapper.Models;
using FluentAssertions;
using Xunit;

namespace ffmpeg_dotnet_wrapper_tests;

public class FFmpegProgressUpdateExtensionsTests
{
    [Fact]
    public void GetRemainingDuration_ReturnsExpectedDuration()
    {
        // Arrange
        var update = new FFmpegProgressUpdate
        {
            ProgressPercentage = 50.0,
            ProcessedDuration = TimeSpan.FromSeconds(50),
            TotalDuration = TimeSpan.FromSeconds(100)
        };

        // Act
        var result = update.GetRemainingDuration();

        // Assert
        result.Should().Be(TimeSpan.FromSeconds(50));
    }

    [Fact]
    public void GetRemainingDuration_ReturnsZeroWhenCompleted()
    {
        // Arrange
        var update = new FFmpegProgressUpdate
        {
            ProgressPercentage = 100.0,
            ProcessedDuration = TimeSpan.FromSeconds(100),
            TotalDuration = TimeSpan.FromSeconds(100)
        };

        // Act
        var result = update.GetRemainingDuration();

        // Assert
        result.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void IsCompleted_ReturnsTrueWhenPercentageIs100()
    {
        // Arrange
        var update = new FFmpegProgressUpdate { ProgressPercentage = 100.0 };

        // Act
        var result = update.IsCompleted();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsCompleted_ReturnsFalseWhenPercentageIsBelow100()
    {
        // Arrange
        var update = new FFmpegProgressUpdate { ProgressPercentage = 99.9 };

        // Act
        var result = update.IsCompleted();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetFormattedPercentage_ReturnsFormattedString()
    {
        // Arrange
        var update = new FFmpegProgressUpdate { ProgressPercentage = 75.56 };

        // Act
        var result = update.GetFormattedPercentage(1);

        // Assert
        result.Should().Be("75.6%");
    }

    [Fact]
    public void GetEstimatedCompletionTime_ReturnsDateTimeInFuture()
    {
        // Arrange
        var update = new FFmpegProgressUpdate
        {
            EncodingSpeed = 1.0,
            ElapsedWallTime = TimeSpan.FromSeconds(10),
            TotalDuration = TimeSpan.FromSeconds(20),
            ProcessedDuration = TimeSpan.FromSeconds(10)
        };

        // Act
        var result = update.GetEstimatedCompletionTime();

        // Assert
        result.Should().BeOnOrAfter(DateTime.UtcNow.AddSeconds(9));
    }

    [Fact]
    public void GetRemainingDuration_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => ((FFmpegProgressUpdate)null!).GetRemainingDuration();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
