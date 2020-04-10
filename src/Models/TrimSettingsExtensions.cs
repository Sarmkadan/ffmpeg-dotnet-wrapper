// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using FFmpegDotnetWrapper.Exceptions;

namespace FFmpegDotnetWrapper.Models;

/// <summary>
/// Extension methods for <see cref="TrimSettings"/> providing additional functionality
/// for working with media trimming operations.
/// </summary>
public static class TrimSettingsExtensions
{
    /// <summary>
    /// Creates a new <see cref="TrimSettings"/> with the same settings as this instance
    /// but with the start time adjusted by the specified offset.
    /// </summary>
    /// <param name="settings">The original trim settings.</param>
    /// <param name="offset">The time offset to apply to the start time.</param>
    /// <returns>A new <see cref="TrimSettings"/> instance with adjusted start time.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="settings"/> is <see langword="null"/>.</exception>
    public static TrimSettings WithStartTimeOffset(this TrimSettings settings, TimeSpan offset)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var clone = settings.Clone();
        clone.StartTime += offset;
        return clone;
    }

    /// <summary>
    /// Creates a new <see cref="TrimSettings"/> with the same settings as this instance
    /// but with the duration adjusted by the specified amount.
    /// </summary>
    /// <param name="settings">The original trim settings.</param>
    /// <param name="durationAdjustment">The amount to adjust the duration by (can be positive or negative).</param>
    /// <returns>A new <see cref="TrimSettings"/> instance with adjusted duration.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="settings"/> is <see langword="null"/>.</exception>
    public static TrimSettings WithDurationAdjustment(this TrimSettings settings, TimeSpan durationAdjustment)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var clone = settings.Clone();
        if (clone.Duration.HasValue)
        {
            clone.Duration += durationAdjustment;
            if (clone.Duration.Value <= TimeSpan.Zero)
                clone.Duration = null;
        }
        else if (durationAdjustment > TimeSpan.Zero)
        {
            clone.Duration = durationAdjustment;
        }

        return clone;
    }

    /// <summary>
    /// Determines whether this trim operation preserves both audio and video.
    /// </summary>
    /// <param name="settings">The trim settings.</param>
    /// <returns>True if both audio and video are preserved; otherwise false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="settings"/> is <see langword="null"/>.</exception>
    public static bool PreservesBothStreams(this TrimSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return settings.PreserveAudio && settings.PreserveVideo;
    }

    /// <summary>
    /// Determines whether this trim operation preserves only audio.
    /// </summary>
    /// <param name="settings">The trim settings.</param>
    /// <returns>True if only audio is preserved; otherwise false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="settings"/> is <see langword="null"/>.</exception>
    public static bool PreservesOnlyAudio(this TrimSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return settings.PreserveAudio && !settings.PreserveVideo;
    }

    /// <summary>
    /// Determines whether this trim operation preserves only video.
    /// </summary>
    /// <param name="settings">The trim settings.</param>
    /// <returns>True if only video is preserved; otherwise false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="settings"/> is <see langword="null"/>.</exception>
    public static bool PreservesOnlyVideo(this TrimSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return !settings.PreserveAudio && settings.PreserveVideo;
    }

    /// <summary>
    /// Gets the end time of the trim operation.
    /// </summary>
    /// <param name="settings">The trim settings.</param>
    /// <returns>The calculated end time.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="settings"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationConfigurationException">Neither <see cref="TrimSettings.EndTime"/> nor <see cref="TrimSettings.Duration"/> is set.</exception>
    public static TimeSpan GetEndTime(this TrimSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return settings.CalculateEndTime();
    }

    /// <summary>
    /// Gets the duration of the trimmed segment, or <see cref="TimeSpan.Zero"/> if the trim would result in no content.
    /// </summary>
    /// <param name="settings">The trim settings.</param>
    /// <returns>The duration of the trimmed segment, or <see cref="TimeSpan.Zero"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="settings"/> is <see langword="null"/>.</exception>
    public static TimeSpan GetTrimmedDurationOrZero(this TrimSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        try
        {
            var duration = settings.GetTrimmedDuration();
            return duration > TimeSpan.Zero ? duration : TimeSpan.Zero;
        }
        catch (InvalidOperationConfigurationException)
        {
            return TimeSpan.Zero;
        }
    }

    /// <summary>
    /// Creates a new <see cref="TrimSettings"/> that trims from the current start time to the end of the media.
    /// </summary>
    /// <param name="settings">The original trim settings.</param>
    /// <returns>A new <see cref="TrimSettings"/> instance that trims to the end of the media.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="settings"/> is <see langword="null"/>.</exception>
    public static TrimSettings TrimToEnd(this TrimSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var clone = settings.Clone();
        clone.EndTime = null;
        clone.Duration = null;
        return clone;
    }

    /// <summary>
    /// Determines whether this trim operation requires keyframes (default behavior).
    /// </summary>
    /// <param name="settings">The trim settings.</param>
    /// <returns>True if keyframes are required; otherwise false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="settings"/> is <see langword="null"/>.</exception>
    public static bool RequiresKeyframes(this TrimSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        return settings.Keyframe;
    }
}