// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Constants;

namespace FFmpegDotnetWrapper.Models;

/// <summary>
/// Configuration settings for GIF export operations.
/// </summary>
public class GifExportSettings
{
    private int _fps = 10;
    private int _width = 640;

    /// <summary>
    /// Gets or sets the frames-per-second for the output GIF.
    /// </summary>
    public int Fps
    {
        get => _fps;
        set
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value), "FPS must be greater than 0");
            _fps = value;
        }
    }

    /// <summary>
    /// Gets or sets the target width of the GIF (height is scaled to preserve aspect ratio).
    /// </summary>
    public int Width
    {
        get => _width;
        set
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Width must be greater than 0");
            _width = value;
        }
    }

    /// <summary>
    /// Gets or sets the quality preset for GIF export.
    /// </summary>
    public GifQualityPreset Quality { get; set; } = GifQualityPreset.Medium;

    /// <summary>
    /// Creates a new instance with default settings.
    /// </summary>
    public GifExportSettings() { }

    /// <summary>
    /// Creates a new instance with specified quality preset.
    /// </summary>
    /// <param name="quality">The quality preset to use.</param>
    public GifExportSettings(GifQualityPreset quality)
    {
        Quality = quality;
        ApplyQualityPreset(quality);
    }

    /// <summary>
    /// Applies a quality preset to the current settings.
    /// </summary>
    /// <param name="preset">The quality preset to apply.</param>
    public void ApplyQualityPreset(GifQualityPreset preset)
    {
        Quality = preset;

        switch (preset)
        {
            case GifQualityPreset.Low:
                Fps = 8;
                Width = 480;
                break;
            case GifQualityPreset.Medium:
                Fps = 10;
                Width = 640;
                break;
            case GifQualityPreset.High:
                Fps = 15;
                Width = 800;
                break;
        }
    }
}

/// <summary>
/// Video quality preset enumeration for GIF export.
/// </summary>
public enum GifQualityPreset
{
    /// <summary>Low quality preset with smaller file size and lower quality.</summary>
    Low,

    /// <summary>Medium quality preset with balanced file size and quality.</summary>
    Medium,

    /// <summary>High quality preset with better quality but larger file size.</summary>
    High
}