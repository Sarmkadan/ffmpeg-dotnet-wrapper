// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Extension methods for WatermarkSettings providing common operations and
// convenience methods for watermark configuration.
// =============================================================================

using System;

namespace FFmpegDotnetWrapper.Models;

/// <summary>
/// Provides extension methods for <see cref="WatermarkSettings"/> to simplify
/// common watermark configuration scenarios.
/// </summary>
public static class WatermarkSettingsExtensions
{
    /// <summary>
    /// Creates a new WatermarkSettings instance with TopLeft position and default offsets.
    /// </summary>
    /// <param name="settings">The source settings to copy non-position properties from. Cannot be null.</param>
    /// <returns>A new WatermarkSettings instance configured for top-left position.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> is null.</exception>
    public static WatermarkSettings WithTopLeftPosition(this WatermarkSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var result = settings.Clone();
        result.Position = WatermarkPosition.TopLeft;
        return result;
    }

    /// <summary>
    /// Creates a new WatermarkSettings instance with Center position and zero offsets.
    /// </summary>
    /// <param name="settings">The source settings to copy non-position properties from. Cannot be null.</param>
    /// <returns>A new WatermarkSettings instance configured for center position.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> is null.</exception>
    public static WatermarkSettings WithCenterPosition(this WatermarkSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var result = settings.Clone();
        result.Position = WatermarkPosition.Center;
        result.XOffset = 0;
        result.YOffset = 0;
        return result;
    }

    /// <summary>
    /// Creates a new WatermarkSettings instance with the specified scale percentage.
    /// </summary>
    /// <param name="settings">The source settings to copy from. Cannot be null.</param>
    /// <param name="scalePercentage">The scale percentage (0-100).</param>
    /// <returns>A new WatermarkSettings instance with the specified scale.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="scalePercentage"/> is not between 0 and 100.</exception>
    public static WatermarkSettings WithScale(this WatermarkSettings settings, double scalePercentage)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (scalePercentage < 0 || scalePercentage > 100)
            throw new ArgumentOutOfRangeException(nameof(scalePercentage), "Scale percentage must be between 0 and 100");

        var result = settings.Clone();
        result.Scale = scalePercentage / 100.0;
        return result;
    }

    /// <summary>
    /// Creates a new WatermarkSettings instance with animation enabled and specified duration.
    /// </summary>
    /// <param name="settings">The source settings to copy from. Cannot be null.</param>
    /// <param name="duration">The animation duration. Cannot be negative.</param>
    /// <returns>A new WatermarkSettings instance with animation enabled.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="duration"/> is negative.</exception>
    public static WatermarkSettings WithAnimation(this WatermarkSettings settings, TimeSpan duration)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (duration < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(duration), "Duration cannot be negative");

        var result = settings.Clone();
        result.AnimateIn = true;
        result.AnimateInDuration = duration;
        return result;
    }

    /// <summary>
    /// Creates a new WatermarkSettings instance with the specified start time and duration.
    /// </summary>
    /// <param name="settings">The source settings to copy from. Cannot be null.</param>
    /// <param name="startTime">The start time for the watermark. Cannot be negative.</param>
    /// <param name="duration">The duration the watermark should appear. Must be positive.</param>
    /// <returns>A new WatermarkSettings instance with time constraints.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="startTime"/> is negative or <paramref name="duration"/> is not positive.</exception>
    public static WatermarkSettings WithTimeConstraints(this WatermarkSettings settings, TimeSpan startTime, TimeSpan duration)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (startTime < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(startTime), "Start time cannot be negative");

        if (duration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be positive");

        var result = settings.Clone();
        result.StartTime = startTime;
        result.Duration = duration;
        return result;
    }

    /// <summary>
    /// Adjusts the opacity of the watermark settings.
    /// </summary>
    /// <param name="settings">The source settings to copy from. Cannot be null.</param>
    /// <param name="opacity">The opacity value (0.0 to 1.0).</param>
    /// <returns>A new WatermarkSettings instance with the specified opacity.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="opacity"/> is not between 0.0 and 1.0.</exception>
    public static WatermarkSettings WithOpacity(this WatermarkSettings settings, double opacity)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (opacity is < 0.0 or > 1.0)
            throw new ArgumentOutOfRangeException(nameof(opacity), "Opacity must be between 0.0 and 1.0");

        var result = settings.Clone();
        result.Opacity = opacity;
        return result;
    }
}