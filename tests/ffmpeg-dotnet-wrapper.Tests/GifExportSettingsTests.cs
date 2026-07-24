using Xunit;
using FFmpegDotnetWrapper.Models;

namespace FFmpegDotnetWrapper.Tests;

public class GifExportSettingsTests
{
    [Fact]
    public void Constructor_Defaults_SetsExpectedValues()
    {
        // Arrange & Act
        var settings = new GifExportSettings();

        // Assert
        Assert.Equal(GifQualityPreset.Medium, settings.Quality);
        Assert.Equal(10, settings.Fps);
        Assert.Equal(640, settings.Width);
        Assert.Equal(-1, settings.Loop);
        Assert.Equal(DitherMode.Sierra2_4a, settings.DitherMode);
    }

    [Fact]
    public void Constructor_WithPreset_SetsQualityAndDimensions()
    {
        // Arrange & Act
        var settings = new GifExportSettings(GifQualityPreset.High);

        // Assert
        Assert.Equal(GifQualityPreset.High, settings.Quality);
        Assert.Equal(15, settings.Fps);
        Assert.Equal(800, settings.Width);
    }

    [Fact]
    public void ApplyQualityPreset_OverridesCurrentSettings()
    {
        // Arrange
        var settings = new GifExportSettings(GifQualityPreset.High);

        // Act
        settings.ApplyQualityPreset(GifQualityPreset.Low);

        // Assert
        Assert.Equal(GifQualityPreset.Low, settings.Quality);
        Assert.Equal(8, settings.Fps);
        Assert.Equal(480, settings.Width);
    }

    [Fact]
    public void Fps_SetToZero_ThrowsException()
    {
        // Arrange
        var settings = new GifExportSettings();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => settings.Fps = 0);
    }

    [Fact]
    public void Width_SetToNegative_ThrowsException()
    {
        // Arrange
        var settings = new GifExportSettings();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => settings.Width = -1);
    }

    [Fact]
    public void Loop_SetToLessThanNegativeOne_ThrowsException()
    {
        // Arrange
        var settings = new GifExportSettings();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => settings.Loop = -2);
    }
}
