using System;
using System.IO;
using FFmpegDotnetWrapper.Models;
using Xunit;

namespace FFmpegDotnetWrapper.Tests;

public class ConversionResultExtensionsTests
{
    #region GetProcessingSpeedFps

    [Fact]
    public void GetProcessingSpeedFps_ReturnsFrameRate_WhenAllValuesAreValid()
    {
        // Arrange
        var mediaFile = new MediaFile { FrameRate = 30.0 };
        var result = new ConversionResult
        {
            Duration = TimeSpan.FromSeconds(10),
            OutputMedia = mediaFile
        };

        // Act
        var speed = result.GetProcessingSpeedFps();

        // Assert
        Assert.Equal(30.0, speed);
    }

    [Fact]
    public void GetProcessingSpeedFps_ReturnsNull_WhenDurationIsZero()
    {
        // Arrange
        var mediaFile = new MediaFile { FrameRate = 30.0 };
        var result = new ConversionResult
        {
            Duration = TimeSpan.Zero,
            OutputMedia = mediaFile
        };

        // Act
        var speed = result.GetProcessingSpeedFps();

        // Assert
        Assert.Null(speed);
    }

    [Fact]
    public void GetProcessingSpeedFps_ReturnsNull_WhenDurationIsNegative()
    {
        // Arrange
        var mediaFile = new MediaFile { FrameRate = 30.0 };
        var result = new ConversionResult
        {
            Duration = TimeSpan.FromSeconds(-1),
            OutputMedia = mediaFile
        };

        // Act
        var speed = result.GetProcessingSpeedFps();

        // Assert
        Assert.Null(speed);
    }

    [Fact]
    public void GetProcessingSpeedFps_ReturnsNull_WhenFrameRateIsNull()
    {
        // Arrange
        var mediaFile = new MediaFile { FrameRate = null };
        var result = new ConversionResult
        {
            Duration = TimeSpan.FromSeconds(10),
            OutputMedia = mediaFile
        };

        // Act
        var speed = result.GetProcessingSpeedFps();

        // Assert
        Assert.Null(speed);
    }

    [Fact]
    public void GetProcessingSpeedFps_ReturnsNull_WhenFrameRateIsZero()
    {
        // Arrange
        var mediaFile = new MediaFile { FrameRate = 0 };
        var result = new ConversionResult
        {
            Duration = TimeSpan.FromSeconds(10),
            OutputMedia = mediaFile
        };

        // Act
        var speed = result.GetProcessingSpeedFps();

        // Assert
        Assert.Null(speed);
    }

    [Fact]
    public void GetProcessingSpeedFps_ReturnsNull_WhenOutputMediaIsNull()
    {
        // Arrange
        var result = new ConversionResult
        {
            Duration = TimeSpan.FromSeconds(10),
            OutputMedia = null
        };

        // Act
        var speed = result.GetProcessingSpeedFps();

        // Assert
        Assert.Null(speed);
    }

    [Fact]
    public void GetProcessingSpeedFps_ThrowsArgumentNullException_WhenResultIsNull()
    {
        // Arrange
        ConversionResult? result = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => result!.GetProcessingSpeedFps());
    }

    #endregion

    #region GetOutputFileSizeMb

    [Fact]
    public void GetOutputFileSizeMb_ReturnsFileSize_WhenOutputMediaExists()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        File.WriteAllBytes(tempFile, new byte[1024 * 1024 * 500]); // 500 MB
        var mediaFile = new MediaFile(tempFile);
        var result = new ConversionResult
        {
            OutputMedia = mediaFile
        };

        // Act
        var size = result.GetOutputFileSizeMb();

        // Assert
        Assert.Equal(500.0, size);

        // Cleanup
        File.Delete(tempFile);
    }

    [Fact]
    public void GetOutputFileSizeMb_ReturnsNull_WhenOutputMediaIsNull()
    {
        // Arrange
        var result = new ConversionResult
        {
            OutputMedia = null
        };

        // Act
        var size = result.GetOutputFileSizeMb();

        // Assert
        Assert.Null(size);
    }

    [Fact]
    public void GetOutputFileSizeMb_ReturnsRoundedValue_WhenFileSizeIsNotExact()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        File.WriteAllBytes(tempFile, new byte[1024 * 1024 * 150 + 512 * 1024]); // 150.5 MB
        var mediaFile = new MediaFile(tempFile);
        var result = new ConversionResult
        {
            OutputMedia = mediaFile
        };

        // Act
        var size = result.GetOutputFileSizeMb();

        // Assert
        Assert.Equal(150.5, size);

        // Cleanup
        File.Delete(tempFile);
    }

    [Fact]
    public void GetOutputFileSizeMb_ThrowsArgumentNullException_WhenResultIsNull()
    {
        // Arrange
        ConversionResult? result = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => result!.GetOutputFileSizeMb());
    }

    #endregion

    #region HasWarnings

    [Fact]
    public void HasWarnings_ReturnsTrue_WhenWarningMessageIsNotEmpty()
    {
        // Arrange
        var result = new ConversionResult
        {
            WarningMessage = "Some warning occurred"
        };

        // Act
        var hasWarnings = result.HasWarnings();

        // Assert
        Assert.True(hasWarnings);
    }

    [Fact]
    public void HasWarnings_ReturnsTrue_WhenWarningMessageIsWhitespace()
    {
        // Arrange
        var result = new ConversionResult
        {
            WarningMessage = "   "
        };

        // Act
        var hasWarnings = result.HasWarnings();

        // Assert
        Assert.True(hasWarnings);
    }

    [Fact]
    public void HasWarnings_ReturnsFalse_WhenWarningMessageIsNull()
    {
        // Arrange
        var result = new ConversionResult
        {
            WarningMessage = null
        };

        // Act
        var hasWarnings = result.HasWarnings();

        // Assert
        Assert.False(hasWarnings);
    }

    [Fact]
    public void HasWarnings_ReturnsFalse_WhenWarningMessageIsEmpty()
    {
        // Arrange
        var result = new ConversionResult
        {
            WarningMessage = string.Empty
        };

        // Act
        var hasWarnings = result.HasWarnings();

        // Assert
        Assert.False(hasWarnings);
    }

    [Fact]
    public void HasWarnings_ThrowsArgumentNullException_WhenResultIsNull()
    {
        // Arrange
        ConversionResult? result = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => result!.HasWarnings());
    }

    #endregion

    #region GetFormattedDuration

    [Fact]
    public void GetFormattedDuration_ReturnsSecondsOnly_WhenDurationIsLessThanOneMinute()
    {
        // Arrange
        var result = new ConversionResult
        {
            Duration = TimeSpan.FromSeconds(45)
        };

        // Act
        var formatted = result.GetFormattedDuration();

        // Assert
        Assert.Equal("45s", formatted);
    }

    [Fact]
    public void GetFormattedDuration_ReturnsMinutesAndSeconds_WhenDurationIsOneMinuteOrMore()
    {
        // Arrange
        var result = new ConversionResult
        {
            Duration = TimeSpan.FromSeconds(90) // 1m 30s
        };

        // Act
        var formatted = result.GetFormattedDuration();

        // Assert
        Assert.Equal("1m 30s", formatted);
    }

    [Fact]
    public void GetFormattedDuration_ReturnsMinutesAndSeconds_WhenDurationIsExactlyOneMinute()
    {
        // Arrange
        var result = new ConversionResult
        {
            Duration = TimeSpan.FromSeconds(60)
        };

        // Act
        var formatted = result.GetFormattedDuration();

        // Assert
        Assert.Equal("1m 0s", formatted);
    }

    [Fact]
    public void GetFormattedDuration_ReturnsMinutesAndSeconds_WhenDurationIsMultipleMinutes()
    {
        // Arrange
        var result = new ConversionResult
        {
            Duration = TimeSpan.FromSeconds(390) // 6m 30s
        };

        // Act
        var formatted = result.GetFormattedDuration();

        // Assert
        Assert.Equal("6m 30s", formatted);
    }

    [Fact]
    public void GetFormattedDuration_ThrowsArgumentNullException_WhenResultIsNull()
    {
        // Arrange
        ConversionResult? result = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => result!.GetFormattedDuration());
    }

    #endregion

    #region AddPerformanceMetrics

    [Fact]
    public void AddPerformanceMetrics_AddsMetricsToResult()
    {
        // Arrange
        var result = new ConversionResult();

        // Act
        result.AddPerformanceMetrics(75.5, 256.8);

        // Assert
        Assert.Equal(75.5, result.GetCpuUsage());
        Assert.Equal(256.8, result.GetMemoryUsageMb());
    }

    [Fact]
    public void AddPerformanceMetrics_RoundsValues_WhenValuesHaveDecimals()
    {
        // Arrange
        var result = new ConversionResult();

        // Act
        result.AddPerformanceMetrics(75.555, 256.888);

        // Assert
        Assert.Equal(75.56, result.GetCpuUsage());
        Assert.Equal(256.89, result.GetMemoryUsageMb());
    }

    [Fact]
    public void AddPerformanceMetrics_ThrowsArgumentNullException_WhenResultIsNull()
    {
        // Arrange
        ConversionResult? result = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => result!.AddPerformanceMetrics(50.0, 128.0));
    }

    #endregion

    #region GetCpuUsage

    [Fact]
    public void GetCpuUsage_ReturnsCpuUsage_WhenMetricExists()
    {
        // Arrange
        var result = new ConversionResult();
        result.SetMetric("CPU_Usage_Percent", 85.5);

        // Act
        var cpuUsage = result.GetCpuUsage();

        // Assert
        Assert.Equal(85.5, cpuUsage);
    }

    [Fact]
    public void GetCpuUsage_ReturnsZero_WhenMetricDoesNotExist()
    {
        // Arrange
        var result = new ConversionResult();

        // Act
        var cpuUsage = result.GetCpuUsage();

        // Assert
        Assert.Equal(0.0, cpuUsage);
    }

    [Fact]
    public void GetCpuUsage_ReturnsZero_WhenMetricIsWrongType()
    {
        // Arrange
        var result = new ConversionResult();
        result.SetMetric("CPU_Usage_Percent", "85.5"); // Wrong type

        // Act
        var cpuUsage = result.GetCpuUsage();

        // Assert
        Assert.Equal(0.0, cpuUsage);
    }

    [Fact]
    public void GetCpuUsage_ThrowsArgumentNullException_WhenResultIsNull()
    {
        // Arrange
        ConversionResult? result = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => result!.GetCpuUsage());
    }

    #endregion

    #region GetMemoryUsageMb

    [Fact]
    public void GetMemoryUsageMb_ReturnsMemoryUsage_WhenMetricExists()
    {
        // Arrange
        var result = new ConversionResult();
        result.SetMetric("Memory_Usage_MB", 512.75);

        // Act
        var memoryUsage = result.GetMemoryUsageMb();

        // Assert
        Assert.Equal(512.75, memoryUsage);
    }

    [Fact]
    public void GetMemoryUsageMb_ReturnsZero_WhenMetricDoesNotExist()
    {
        // Arrange
        var result = new ConversionResult();

        // Act
        var memoryUsage = result.GetMemoryUsageMb();

        // Assert
        Assert.Equal(0.0, memoryUsage);
    }

    [Fact]
    public void GetMemoryUsageMb_ReturnsZero_WhenMetricIsWrongType()
    {
        // Arrange
        var result = new ConversionResult();
        result.SetMetric("Memory_Usage_MB", "512.75"); // Wrong type

        // Act
        var memoryUsage = result.GetMemoryUsageMb();

        // Assert
        Assert.Equal(0.0, memoryUsage);
    }

    [Fact]
    public void GetMemoryUsageMb_ThrowsArgumentNullException_WhenResultIsNull()
    {
        // Arrange
        ConversionResult? result = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => result!.GetMemoryUsageMb());
    }

    #endregion

    #region CompletedWithinThreshold

    [Fact]
    public void CompletedWithinThreshold_ReturnsTrue_WhenDurationIsLessThanMax()
    {
        // Arrange
        var result = new ConversionResult
        {
            Duration = TimeSpan.FromSeconds(5)
        };
        var maxDuration = TimeSpan.FromSeconds(10);

        // Act
        var completed = result.CompletedWithinThreshold(maxDuration);

        // Assert
        Assert.True(completed);
    }

    [Fact]
    public void CompletedWithinThreshold_ReturnsTrue_WhenDurationEqualsMax()
    {
        // Arrange
        var result = new ConversionResult
        {
            Duration = TimeSpan.FromSeconds(10)
        };
        var maxDuration = TimeSpan.FromSeconds(10);

        // Act
        var completed = result.CompletedWithinThreshold(maxDuration);

        // Assert
        Assert.True(completed);
    }

    [Fact]
    public void CompletedWithinThreshold_ReturnsFalse_WhenDurationExceedsMax()
    {
        // Arrange
        var result = new ConversionResult
        {
            Duration = TimeSpan.FromSeconds(15)
        };
        var maxDuration = TimeSpan.FromSeconds(10);

        // Act
        var completed = result.CompletedWithinThreshold(maxDuration);

        // Assert
        Assert.False(completed);
    }

    [Fact]
    public void CompletedWithinThreshold_ThrowsArgumentNullException_WhenResultIsNull()
    {
        // Arrange
        ConversionResult? result = null;
        var maxDuration = TimeSpan.FromSeconds(10);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => result!.CompletedWithinThreshold(maxDuration));
    }

    #endregion

    #region GetMetricsSummary

    [Fact]
    public void GetMetricsSummary_ReturnsMetricsSummary_WhenMetricsExist()
    {
        // Arrange
        var result = new ConversionResult();
        result.SetMetric("CPU_Usage_Percent", 75.5);
        result.SetMetric("Memory_Usage_MB", 256.8);
        result.SetMetric("Frames_Per_Second", 30.0);

        // Act
        var summary = result.GetMetricsSummary();

        // Assert
        Assert.Contains("Conversion Metrics:", summary);
        Assert.Contains("CPU_Usage_Percent: 75.5", summary);
        Assert.Contains("Memory_Usage_MB: 256.8", summary);
        Assert.Contains("Frames_Per_Second: 30", summary);
    }

    [Fact]
    public void GetMetricsSummary_ReturnsNoMetricsMessage_WhenNoMetricsExist()
    {
        // Arrange
        var result = new ConversionResult();

        // Act
        var summary = result.GetMetricsSummary();

        // Assert
        Assert.Equal("No metrics available\n", summary);
    }

    [Fact]
    public void GetMetricsSummary_ThrowsArgumentNullException_WhenResultIsNull()
    {
        // Arrange
        ConversionResult? result = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => result!.GetMetricsSummary());
    }

    #endregion
}