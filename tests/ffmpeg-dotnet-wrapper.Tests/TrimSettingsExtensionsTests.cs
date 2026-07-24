namespace FFmpegDotnetWrapper.Tests;

using Xunit;
using FluentAssertions;
using FFmpegDotnetWrapper.Models;
using System;
using FFmpegDotnetWrapper.Exceptions;

public class TrimSettingsExtensionsTests
{
    [Fact]
    public void WithStartTimeOffset_WithValidSettings_AdjustsStartTimeByOffset()
    {
        // Arrange
        var settings = new TrimSettings { StartTime = TimeSpan.FromSeconds(10) };
        var offset = TimeSpan.FromSeconds(5);

        // Act
        var result = settings.WithStartTimeOffset(offset);

        // Assert
        result.Should().NotBeSameAs(settings);
        result.StartTime.Should().Be(TimeSpan.FromSeconds(15));
    }

    [Fact]
    public void WithStartTimeOffset_NullSettings_ThrowsArgumentNullException()
    {
        // Arrange
        TrimSettings settings = null!;
        var offset = TimeSpan.FromSeconds(5);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => settings.WithStartTimeOffset(offset));
    }

    [Fact]
    public void WithDurationAdjustment_WithDuration_AdjustsDuration()
    {
        // Arrange
        var settings = new TrimSettings { Duration = TimeSpan.FromSeconds(20) };
        var adjustment = TimeSpan.FromSeconds(5);

        // Act
        var result = settings.WithDurationAdjustment(adjustment);

        // Assert
        result.Should().NotBeSameAs(settings);
        result.Duration.Should().Be(TimeSpan.FromSeconds(25));
    }

    [Fact]
    public void WithDurationAdjustment_NullSettings_ThrowsArgumentNullException()
    {
        // Arrange
        TrimSettings settings = null!;
        var adjustment = TimeSpan.FromSeconds(5);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => settings.WithDurationAdjustment(adjustment));
    }

    [Fact]
    public void PreservesBothStreams_WhenBothPreserved_ReturnsTrue()
    {
        // Arrange
        var settings = new TrimSettings { PreserveAudio = true, PreserveVideo = true };

        // Act
        var result = settings.PreservesBothStreams();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void PreservesOnlyAudio_WhenOnlyAudioPreserved_ReturnsTrue()
    {
        // Arrange
        var settings = new TrimSettings { PreserveAudio = true, PreserveVideo = false };

        // Act
        var result = settings.PreservesOnlyAudio();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void PreservesOnlyVideo_WhenOnlyVideoPreserved_ReturnsTrue()
    {
        // Arrange
        var settings = new TrimSettings { PreserveAudio = false, PreserveVideo = true };

        // Act
        var result = settings.PreservesOnlyVideo();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void PreservesOnlyVideo_NullSettings_ThrowsArgumentNullException()
    {
        // Arrange
        TrimSettings settings = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => settings.PreservesOnlyVideo());
    }

    [Fact]
    public void GetEndTime_WithDuration_ReturnsStartTimePlusDuration()
    {
        // Arrange
        var settings = new TrimSettings { StartTime = TimeSpan.FromSeconds(10), Duration = TimeSpan.FromSeconds(5) };

        // Act
        var result = settings.GetEndTime();

        // Assert
        result.Should().Be(TimeSpan.FromSeconds(15));
    }

    [Fact]
    public void GetEndTime_NeitherDurationNorEndTimeSet_ThrowsInvalidOperationConfigurationException()
    {
        // Arrange
        var settings = new TrimSettings { StartTime = TimeSpan.FromSeconds(10) };

        // Act & Assert
        Assert.Throws<InvalidOperationConfigurationException>(() => settings.GetEndTime());
    }

    [Fact]
    public void GetTrimmedDurationOrZero_WithValidDuration_ReturnsTrimmedDuration()
    {
        // Arrange
        var settings = new TrimSettings { StartTime = TimeSpan.FromSeconds(10), Duration = TimeSpan.FromSeconds(5) };

        // Act
        var result = settings.GetTrimmedDurationOrZero();

        // Assert
        result.Should().Be(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GetTrimmedDurationOrZero_NeitherDurationNorEndTimeSet_ReturnsZero()
    {
        // Arrange
        var settings = new TrimSettings { StartTime = TimeSpan.FromSeconds(10) };

        // Act
        var result = settings.GetTrimmedDurationOrZero();

        // Assert
        result.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void TrimToEnd_ClearsDurationAndEndTime()
    {
        // Arrange
        var settings = new TrimSettings {
            StartTime = TimeSpan.FromSeconds(10),
            Duration = TimeSpan.FromSeconds(5),
            EndTime = TimeSpan.FromSeconds(20)
        };

        // Act
        var result = settings.TrimToEnd();

        // Assert
        result.Should().NotBeSameAs(settings);
        result.Duration.Should().BeNull();
        result.EndTime.Should().BeNull();
    }

    [Fact]
    public void TrimToEnd_NullSettings_ThrowsArgumentNullException()
    {
        // Arrange
        TrimSettings settings = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => settings.TrimToEnd());
    }

    [Fact]
    public void RequiresKeyframes_WhenKeyframeTrue_ReturnsTrue()
    {
        // Arrange
        var settings = new TrimSettings { Keyframe = true };

        // Act
        var result = settings.RequiresKeyframes();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void RequiresKeyframes_NullSettings_ThrowsArgumentNullException()
    {
        // Arrange
        TrimSettings settings = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => settings.RequiresKeyframes());
    }
}