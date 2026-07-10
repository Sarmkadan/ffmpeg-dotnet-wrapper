// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Extension methods for WatermarkSettings providing common operations and
// convenience methods for watermark configuration.
// =============================================================================

using FFmpegDotnetWrapper.Models;

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
    /// <param name="settings">The source settings to copy non-position properties from.</param>
    /// <returns>A new WatermarkSettings instance configured for top-left position.</returns>
    public static WatermarkSettings WithTopLeftPosition(this WatermarkSettings settings)
    {
        var result = settings.Clone();
        result.Position = WatermarkPosition.TopLeft;
        return result;
    }

    /// <summary>
    /// Creates a new WatermarkSettings instance with Center position and zero offsets.
    /// </summary>
    /// <param name="settings">The source settings to copy non-position properties from.</param>
    /// <returns>A new WatermarkSettings instance configured for center position.</returns>
    public static WatermarkSettings WithCenterPosition(this WatermarkSettings settings)
    {
        var result = settings.Clone();
        result.Position = WatermarkPosition.Center;
        result.XOffset = 0;
        result.YOffset = 0;
        return result;
    }

    /// <summary>
    /// Creates a new WatermarkSettings instance with the specified scale percentage.
    /// </summary>
    /// <param name="settings">The source settings to copy from.</param>
    /// <param name="scalePercentage">The scale percentage (0-100).</param>
    /// <returns>A new WatermarkSettings instance with the specified scale.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when scalePercentage is not between 0 and 100.</exception>
    public static WatermarkSettings WithScale(this WatermarkSettings settings, double scalePercentage)
    {
        if (scalePercentage < 0 || scalePercentage > 100)
            throw new ArgumentOutOfRangeException(nameof(scalePercentage), "Scale percentage must be between 0 and 100");

        var result = settings.Clone();
        result.Scale = scalePercentage / 100.0;
        return result;
    }

    /// <summary>
    /// Creates a new WatermarkSettings instance with animation enabled and specified duration.
    /// </summary>
    /// <param name="settings">The source settings to copy from.</param>
    /// <param name="duration">The animation duration.</param>
    /// <returns>A new WatermarkSettings instance with animation enabled.</returns>
    public static WatermarkSettings WithAnimation(this WatermarkSettings settings, TimeSpan duration)
    {
        var result = settings.Clone();
        result.AnimateIn = true;
        result.AnimateInDuration = duration;
        return result;
    }

    /// <summary>
    /// Creates a new WatermarkSettings instance with the specified start time and duration.
    /// </summary>
    /// <param name="settings">The source settings to copy from.</param>
    /// <param name="startTime">The start time for the watermark.</param>
    /// <param name="duration">The duration the watermark should appear.</param>
    /// <returns>A new WatermarkSettings instance with time constraints.</returns>
    public static WatermarkSettings WithTimeConstraints(this WatermarkSettings settings, TimeSpan startTime, TimeSpan duration)
    {
        var result = settings.Clone();
        result.StartTime = startTime;
        result.Duration = duration;
        return result;
    }

    /// <summary>
    /// Adjusts the opacity of the watermark settings.
    /// </summary>
    /// <param name="settings">The source settings to copy from.</param>
    /// <param name="opacity">The opacity value (0.0 to 1.0).</param>
    /// <returns>A new WatermarkSettings instance with the specified opacity.</returns>
    public static WatermarkSettings WithOpacity(this WatermarkSettings settings, double opacity)
    {
        var result = settings.Clone();
        result.Opacity = opacity;
        return result;
    }
}