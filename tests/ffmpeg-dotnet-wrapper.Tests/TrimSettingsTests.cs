// =============================================================================
// Tests for TrimSettings
// =============================================================================

using System;
using FFmpegDotnetWrapper.Models;
using FFmpegDotnetWrapper.Exceptions;
using Xunit;

namespace FFmpegDotnetWrapper.Tests;

public class TrimSettingsTests
{
    // Helper to create a minimal MediaFile instance using reflection/dynamic.
    private static MediaFile CreateMediaFile(TimeSpan? duration)
    {
        // Assume MediaFile has a parameterless constructor.
        var mediaFile = (MediaFile)Activator.CreateInstance(typeof(MediaFile));
        // Set the Duration property via reflection (in case it's read‑only).
        var prop = typeof(MediaFile).GetProperty("Duration");
        prop?.SetValue(mediaFile, duration);
        // Ensure ValidateAsVideo does not throw for our test media.
        var method = typeof(MediaFile).GetMethod("ValidateAsVideo");
        method?.Invoke(mediaFile, null);
        return mediaFile;
    }

    [Fact]
    public void Clone_ShouldCreateEqualButDistinctInstance()
    {
        var original = new TrimSettings
        {
            StartTime = TimeSpan.FromSeconds(10),
            Duration = TimeSpan.FromSeconds(20),
            PreserveAudio = false,
            PreserveVideo = true,
            Keyframe = false
        };

        var clone = original.Clone();

        Assert.NotSame(original, clone);
        Assert.Equal(original.StartTime, clone.StartTime);
        Assert.Equal(original.Duration, clone.Duration);
        Assert.Equal(original.PreserveAudio, clone.PreserveAudio);
        Assert.Equal(original.PreserveVideo, clone.PreserveVideo);
        Assert.Equal(original.Keyframe, clone.Keyframe);
    }

    [Fact]
    public void CalculateEndTime_ReturnsEndTime_WhenSet()
    {
        var settings = new TrimSettings
        {
            StartTime = TimeSpan.FromSeconds(5),
            EndTime = TimeSpan.FromSeconds(15)
        };

        var end = settings.CalculateEndTime();

        Assert.Equal(TimeSpan.FromSeconds(15), end);
    }

    [Fact]
    public void CalculateEndTime_ReturnsStartPlusDuration_WhenDurationSet()
    {
        var settings = new TrimSettings
        {
            StartTime = TimeSpan.FromSeconds(5),
            Duration = TimeSpan.FromSeconds(12)
        };

        var end = settings.CalculateEndTime();

        Assert.Equal(TimeSpan.FromSeconds(17), end);
    }

    [Fact]
    public void CalculateEndTime_Throws_WhenNeitherEndTimeNorDurationSet()
    {
        var settings = new TrimSettings
        {
            StartTime = TimeSpan.FromSeconds(5)
        };

        Assert.Throws<InvalidOperationConfigurationException>(() => settings.CalculateEndTime());
    }

    [Fact]
    public void GetTrimmedDuration_ComputesCorrectSpan()
    {
        var settings = new TrimSettings
        {
            StartTime = TimeSpan.FromSeconds(3),
            Duration = TimeSpan.FromSeconds(7)
        };

        var trimmed = settings.GetTrimmedDuration();

        Assert.Equal(TimeSpan.FromSeconds(7), trimmed);
    }

    [Fact]
    public void Validate_Passes_ForValidMedia()
    {
        var media = CreateMediaFile(TimeSpan.FromSeconds(100));

        var settings = new TrimSettings
        {
            StartTime = TimeSpan.FromSeconds(10),
            Duration = TimeSpan.FromSeconds(20)
        };

        // Should not throw any exception.
        settings.Validate(media);
    }

    [Fact]
    public void Validate_Throws_WhenStartTimeExceedsMediaDuration()
    {
        var media = CreateMediaFile(TimeSpan.FromSeconds(30));

        var settings = new TrimSettings
        {
            StartTime = TimeSpan.FromSeconds(40)
        };

        var ex = Assert.Throws<InvalidOperationConfigurationException>(() => settings.Validate(media));
        Assert.Contains("Start time", ex.Message);
    }

    [Fact]
    public void Validate_Throws_WhenTrimEndExceedsMediaDuration()
    {
        var media = CreateMediaFile(TimeSpan.FromSeconds(50));

        var settings = new TrimSettings
        {
            StartTime = TimeSpan.FromSeconds(30),
            Duration = TimeSpan.FromSeconds(25) // end would be 55 > 50
        };

        var ex = Assert.Throws<InvalidOperationConfigurationException>(() => settings.Validate(media));
        Assert.Contains("Trim end time", ex.Message);
    }

    [Fact]
    public void Validate_Throws_WhenEndTimeBeforeOrEqualStartTime()
    {
        var media = CreateMediaFile(TimeSpan.FromSeconds(60));

        var settings = new TrimSettings
        {
            StartTime = TimeSpan.FromSeconds(20),
            EndTime = TimeSpan.FromSeconds(10) // earlier than start
        };

        var ex = Assert.Throws<InvalidOperationConfigurationException>(() => settings.Validate(media));
        Assert.Contains("End time must be after start time", ex.Message);
    }

    [Fact]
    public void Validate_Throws_WhenBothAudioAndVideoPreservedAreFalse()
    {
        var media = CreateMediaFile(TimeSpan.FromSeconds(60));

        var settings = new TrimSettings
        {
            PreserveAudio = false,
            PreserveVideo = false
        };

        var ex = Assert.Throws<InvalidOperationConfigurationException>(() => settings.Validate(media));
        Assert.Contains("At least audio or video must be preserved", ex.Message);
    }

    [Fact]
    public void Validate_Throws_WhenMediaFileIsNull()
    {
        var settings = new TrimSettings();

        Assert.Throws<NullReferenceException>(() => settings.Validate(null!));
    }
}
