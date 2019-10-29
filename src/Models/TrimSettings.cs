// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Exceptions;

namespace FFmpegDotnetWrapper.Models;

/// <summary>
/// Configuration settings for trimming/cutting media files.
/// </summary>
public class TrimSettings
{
    private TimeSpan _startTime = TimeSpan.Zero;
    private TimeSpan? _duration;

    public TimeSpan StartTime
    {
        get => _startTime;
        set
        {
            if (value < TimeSpan.Zero)
                throw new InvalidOperationConfigurationException("Start time cannot be negative");
            _startTime = value;
        }
    }

    public TimeSpan? Duration
    {
        get => _duration;
        set
        {
            if (value.HasValue && value.Value <= TimeSpan.Zero)
                throw new InvalidOperationConfigurationException("Duration must be greater than zero");
            _duration = value;
        }
    }

    public TimeSpan? EndTime { get; set; }

    public bool PreserveAudio { get; set; } = true;
    public bool PreserveVideo { get; set; } = true;
    public bool Keyframe { get; set; } = true;

    /// <summary>
    /// Validates the trim settings for consistency.
    /// </summary>
    public void Validate(MediaFile mediaFile)
    {
        mediaFile.ValidateAsVideo();

        if (StartTime >= mediaFile.Duration)
            throw new InvalidOperationConfigurationException(
                $"Start time ({StartTime.TotalSeconds}s) exceeds media duration ({mediaFile.Duration?.TotalSeconds}s)");

        if (Duration.HasValue && StartTime + Duration.Value > mediaFile.Duration)
            throw new InvalidOperationConfigurationException(
                $"Trim end time exceeds media duration");

        if (EndTime.HasValue && EndTime.Value <= StartTime)
            throw new InvalidOperationConfigurationException("End time must be after start time");

        if (!PreserveAudio && !PreserveVideo)
            throw new InvalidOperationConfigurationException("At least audio or video must be preserved");
    }

    /// <summary>
    /// Calculates the end time based on start time and duration.
    /// </summary>
    public TimeSpan CalculateEndTime()
    {
        if (EndTime.HasValue)
            return EndTime.Value;

        if (Duration.HasValue)
            return StartTime + Duration.Value;

        throw new InvalidOperationConfigurationException("Either Duration or EndTime must be set");
    }

    /// <summary>
    /// Gets the duration of the trimmed segment.
    /// </summary>
    public TimeSpan GetTrimmedDuration()
    {
        return CalculateEndTime() - StartTime;
    }

    /// <summary>
    /// Creates a clone of the current settings.
    /// </summary>
    public TrimSettings Clone()
    {
        return new TrimSettings
        {
            StartTime = StartTime,
            Duration = Duration,
            EndTime = EndTime,
            PreserveAudio = PreserveAudio,
            PreserveVideo = PreserveVideo,
            Keyframe = Keyframe
        };
    }
}
