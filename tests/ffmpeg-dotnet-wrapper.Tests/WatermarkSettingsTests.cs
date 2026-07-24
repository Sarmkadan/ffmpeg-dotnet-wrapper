using System;
using FFmpegDotnetWrapper.Models;
using FFmpegDotnetWrapper.Exceptions;
using Xunit;

namespace FFmpegDotnetWrapper.Tests;

public class WatermarkSettingsTests
{
    private static MediaFile CreateMediaFile(int width = 1920, int height = 1080, TimeSpan? duration = null)
    {
        var mediaFile = new MediaFile();
        mediaFile.Width = width;
        mediaFile.Height = height;
        mediaFile.Duration = duration ?? TimeSpan.FromSeconds(100);
        return mediaFile;
    }

    [Fact]
    public void Constructor_InitializesWithDefaultValues()
    {
        var settings = new WatermarkSettings();
        Assert.Equal(WatermarkPosition.TopRight, settings.Position);
        Assert.Equal(10, settings.XOffset);
        Assert.Equal(10, settings.YOffset);
        Assert.Equal(0.2, settings.Scale);
        Assert.True(settings.PreserveAspectRatio);
        Assert.False(settings.AnimateIn);
        Assert.Null(settings.StartTime);
        Assert.Null(settings.Duration);
        Assert.Null(settings.AnimateInDuration);
    }

    [Fact]
    public void Clone_CreatesEqualButDistinctInstance()
    {
        var original = new WatermarkSettings
        {
            Position = WatermarkPosition.BottomLeft,
            XOffset = 20,
            YOffset = 30,
            Scale = 0.5,
            PreserveAspectRatio = false,
            StartTime = TimeSpan.FromSeconds(10),
            Duration = TimeSpan.FromSeconds(20),
            AnimateIn = true,
            AnimateInDuration = TimeSpan.FromSeconds(5)
        };
        var clone = original.Clone();
        Assert.NotSame(original, clone);
        Assert.Equal(original.Position, clone.Position);
        Assert.Equal(original.XOffset, clone.XOffset);
        Assert.Equal(original.YOffset, clone.YOffset);
        Assert.Equal(original.Scale, clone.Scale);
        Assert.Equal(original.PreserveAspectRatio, clone.PreserveAspectRatio);
        Assert.Equal(original.StartTime, clone.StartTime);
        Assert.Equal(original.Duration, clone.Duration);
        Assert.Equal(original.AnimateIn, clone.AnimateIn);
        Assert.Equal(original.AnimateInDuration, clone.AnimateInDuration);
    }

    [Theory]
    [InlineData(WatermarkPosition.TopLeft, 5, 8, 5, 8)]
    [InlineData(WatermarkPosition.TopRight, 10, 15, 1910, 15)]
    [InlineData(WatermarkPosition.BottomLeft, 5, 8, 5, 1072)]
    [InlineData(WatermarkPosition.BottomRight, 10, 15, 1910, 1065)]
    [InlineData(WatermarkPosition.Center, 0, 0, 960, 540)]
    public void CalculatePosition_ReturnsCorrectCoordinates(WatermarkPosition position, int xOffset, int yOffset, int expectedX, int expectedY)
    {
        // Arrange
        var settings = new WatermarkSettings { Position = position, XOffset = xOffset, YOffset = yOffset };

        // Act
        var positionResult = settings.CalculatePosition(1920, 1080);

        // Assert
        Assert.Equal((expectedX, expectedY), positionResult);
    }

    [Fact]
    public void CalculatePosition_WithNullOffsets_UsesZero()
    {
        // Arrange
        var settings = new WatermarkSettings { Position = WatermarkPosition.TopLeft, XOffset = null, YOffset = null };

        // Act
        var position = settings.CalculatePosition(1920, 1080);

        // Assert
        Assert.Equal((0, 0), position);
    }

    [Fact]
    public void Validate_WithValidMediaFile_DoesNotThrow()
    {
        var mediaFile = CreateMediaFile();
        var settings = new WatermarkSettings { Scale = 0.5 };
        settings.Validate(mediaFile);
    }

    [Fact]
    public void Validate_WithNullMediaFile_ThrowsNullReferenceException()
    {
        var settings = new WatermarkSettings();
        Assert.Throws<NullReferenceException>(() => settings.Validate(null!));
    }

    [Fact]
    public void Validate_WithNullVideoDimensions_ThrowsInvalidMediaFileException()
    {
        var mediaFile = new MediaFile();
        mediaFile.Duration = TimeSpan.FromSeconds(100);
        var settings = new WatermarkSettings();
        var ex = Assert.Throws<InvalidMediaFileException>(() => settings.Validate(mediaFile));
        Assert.Contains("missing dimensions", ex.Message);
    }

    [Theory]
    [InlineData(0.005)][InlineData(1.5)]
    public void Validate_WithInvalidScale_ThrowsInvalidOperationConfigurationException(double invalidScale)
    {
        var mediaFile = CreateMediaFile();
        var settings = new WatermarkSettings { Scale = invalidScale };
        var ex = Assert.Throws<InvalidOperationConfigurationException>(() => settings.Validate(mediaFile));
        Assert.Contains("Scale must be between 0.01 and 1", ex.Message);
    }

    [Fact]
    public void Validate_WithNegativeStartTime_ThrowsInvalidOperationConfigurationException()
    {
        var mediaFile = CreateMediaFile();
        var settings = new WatermarkSettings { StartTime = TimeSpan.FromSeconds(-1) };
        var ex = Assert.Throws<InvalidOperationConfigurationException>(() => settings.Validate(mediaFile));
        Assert.Contains("Start time cannot be negative", ex.Message);
    }

    [Fact]
    public void Validate_WithNonPositiveDuration_ThrowsInvalidOperationConfigurationException()
    {
        var mediaFile = CreateMediaFile();
        var settings = new WatermarkSettings { Duration = TimeSpan.Zero };
        var ex = Assert.Throws<InvalidOperationConfigurationException>(() => settings.Validate(mediaFile));
        Assert.Contains("Duration must be greater than zero", ex.Message);
    }

    [Fact]
    public void Validate_WithAnimateInTrueAndNullAnimateInDuration_ThrowsInvalidOperationConfigurationException()
    {
        var mediaFile = CreateMediaFile();
        var settings = new WatermarkSettings { AnimateIn = true };
        var ex = Assert.Throws<InvalidOperationConfigurationException>(() => settings.Validate(mediaFile));
        Assert.Contains("AnimateInDuration is required when AnimateIn is enabled", ex.Message);
    }

    [Fact]
    public void Validate_WithNonPositiveAnimateInDuration_ThrowsInvalidOperationConfigurationException()
    {
        var mediaFile = CreateMediaFile();
        var settings = new WatermarkSettings { AnimateIn = true, AnimateInDuration = TimeSpan.Zero };
        var ex = Assert.Throws<InvalidOperationConfigurationException>(() => settings.Validate(mediaFile));
        Assert.Contains("AnimateInDuration must be greater than zero", ex.Message);
    }

    [Fact]
    public void Properties_CanBeSetAndRetrieved()
    {
        var settings = new WatermarkSettings();
        settings.Position = WatermarkPosition.Center;
        settings.XOffset = 25;
        settings.YOffset = 30;
        settings.Scale = 0.75;
        settings.PreserveAspectRatio = false;
        settings.StartTime = TimeSpan.FromSeconds(15);
        settings.Duration = TimeSpan.FromSeconds(30);
        settings.AnimateIn = true;
        settings.AnimateInDuration = TimeSpan.FromSeconds(5);
        Assert.Equal(WatermarkPosition.Center, settings.Position);
        Assert.Equal(25, settings.XOffset);
        Assert.Equal(30, settings.YOffset);
        Assert.Equal(0.75, settings.Scale);
        Assert.False(settings.PreserveAspectRatio);
        Assert.Equal(TimeSpan.FromSeconds(15), settings.StartTime);
        Assert.Equal(TimeSpan.FromSeconds(30), settings.Duration);
        Assert.True(settings.AnimateIn);
        Assert.Equal(TimeSpan.FromSeconds(5), settings.AnimateInDuration);
    }
}