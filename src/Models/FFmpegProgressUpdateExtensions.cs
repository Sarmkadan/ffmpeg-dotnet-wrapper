// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace FFmpegDotnetWrapper.Models;

/// <summary>
/// Provides useful extension methods for <see cref="FFmpegProgressUpdate"/> to simplify common
/// operations and calculations when working with FFmpeg progress updates.
/// </summary>
public static class FFmpegProgressUpdateExtensions
{
    /// <summary>
    /// Calculates the remaining duration based on the current progress percentage.
    /// </summary>
    /// <param name="update">The progress update instance.</param>
    /// <returns>The estimated remaining duration as a TimeSpan.</returns>
    public static TimeSpan GetRemainingDuration(this FFmpegProgressUpdate update)
    {
        if (update.ProgressPercentage >= 100.0)
        {
            return TimeSpan.Zero;
        }

        if (update.TotalDuration.TotalSeconds <= 0)
        {
            return TimeSpan.Zero;
        }

        var processedSeconds = update.ProcessedDuration.TotalSeconds;
        var totalSeconds = update.TotalDuration.TotalSeconds;
        var remainingSeconds = totalSeconds - processedSeconds;

        return TimeSpan.FromSeconds(remainingSeconds);
    }

    /// <summary>
    /// Determines whether the FFmpeg operation has completed based on the progress percentage.
    /// </summary>
    /// <param name="update">The progress update instance.</param>
    /// <returns>True if the operation has completed (100% or more); otherwise, false.</returns>
    public static bool IsCompleted(this FFmpegProgressUpdate update)
    {
        return update.ProgressPercentage >= 100.0;
    }

    /// <summary>
    /// Gets the formatted progress percentage as a string with optional precision control.
    /// </summary>
    /// <param name="update">The progress update instance.</param>
    /// <param name="decimalPlaces">Number of decimal places to display (default: 1).</param>
    /// <returns>Formatted percentage string (e.g., "75.5%" or "100%").</returns>
    public static string GetFormattedPercentage(this FFmpegProgressUpdate update, int decimalPlaces = 1)
    {
        return update.ProgressPercentage.ToString($"F{decimalPlaces}%");
    }

    /// <summary>
    /// Calculates the estimated completion time based on the current encoding speed and elapsed time.
    /// </summary>
    /// <param name="update">The progress update instance.</param>
    /// <returns>The estimated completion DateTime, or DateTime.MinValue if calculation is not possible.</returns>
    public static DateTime GetEstimatedCompletionTime(this FFmpegProgressUpdate update)
    {
        if (update.EncodingSpeed <= 0 || update.ElapsedWallTime.TotalSeconds <= 0)
        {
            return DateTime.MinValue;
        }

        if (update.TotalDuration.TotalSeconds <= 0)
        {
            return DateTime.MinValue;
        }

        var totalSecondsNeeded = update.TotalDuration.TotalSeconds / update.EncodingSpeed;
        var secondsRemaining = totalSecondsNeeded - update.ElapsedWallTime.TotalSeconds;

        if (secondsRemaining <= 0)
        {
            return DateTime.UtcNow;
        }

        return DateTime.UtcNow.AddSeconds(secondsRemaining);
    }
}