// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using FFmpegDotnetWrapper.Constants;

namespace FFmpegDotnetWrapper.Models;

/// <summary>
/// Configuration settings for GIF export operations.
/// </summary>
public class GifExportSettings
{
    private int _fps = 10;
    private int _width = 640;
    private int? _maxWidth;
    private DitherMode _ditherMode = DitherMode.Sierra2_4a;
    private int _loop = -1; // -1 means infinite loop

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
    /// When MaxWidth is set, this value is ignored and MaxWidth is used instead.
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
    /// Gets or sets the maximum width of the output GIF. Height will be scaled to preserve aspect ratio.
    /// If set, this takes precedence over the Width property.
    /// </summary>
    public int? MaxWidth
    {
        get => _maxWidth;
        set
        {
            if (value.HasValue && value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value), "MaxWidth must be greater than 0");
            _maxWidth = value;
        }
    }

    /// <summary>
    /// Gets or sets the dithering mode for palette conversion.
    /// </summary>
    public DitherMode DitherMode
    {
        get => _ditherMode;
        set => _ditherMode = value;
    }

    /// <summary>
    /// Gets or sets the number of times the GIF should loop. -1 for infinite loop (default).
    /// </summary>
    public int Loop
    {
        get => _loop;
        set
        {
            if (value < -1)
                throw new ArgumentOutOfRangeException(nameof(value), "Loop must be -1 (infinite) or a positive number");
            _loop = value;
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

    /// <summary>
    /// Validates the GIF export settings.
    /// </summary>
    /// <returns>A list of validation error messages, or empty list if valid.</returns>
    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();

        if (Fps <= 0)
            errors.Add("FPS must be greater than 0.");

        if (Width <= 0)
            errors.Add("Width must be greater than 0.");

        if (MaxWidth.HasValue && MaxWidth <= 0)
            errors.Add("MaxWidth must be greater than 0 or null.");

        if (MaxWidth.HasValue && MaxWidth < 160)
            errors.Add("MaxWidth should be at least 160 pixels for reasonable quality.");

        if (Loop < -1)
            errors.Add("Loop must be -1 (infinite) or a positive number.");

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the current settings are valid.
    /// </summary>
    /// <returns><c>true</c> if the settings are valid; otherwise, <c>false</c>.</returns>
    public bool IsValid()
    {
        return Validate().Count == 0;
    }

    /// <summary>
    /// Ensures the current settings are valid, throwing an exception if not.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    public void EnsureValid()
    {
        var errors = Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"GifExportSettings validation failed with {errors.Count} error(s):{Environment.NewLine}" +
                string.Join(Environment.NewLine, errors.Select((error, index) => $" {index + 1}. {error}")));
        }
    }

    /// <summary>
    /// Gets the effective width to use for scaling, considering MaxWidth if set.
    /// </summary>
    /// <returns>The width value to use.</returns>
    public int GetEffectiveWidth()
    {
        return MaxWidth ?? Width;
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

/// <summary>
/// Dithering mode enumeration for palette conversion in GIF export.
/// </summary>
public enum DitherMode
{
    /// <summary>No dithering (0).</summary>
    None = 0,

    /// <summary>Ordered 8x8 bayer dithering (deterministic).</summary>
    Bayer = 1,

    /// <summary>Dithering as defined by Paul Heckbert in 1982 (simple error diffusion).</summary>
    Heckbert = 2,

    /// <summary>Floyd and Steingberg dithering (error diffusion).</summary>
    FloydSteinberg = 3,

    /// <summary>Frankie Sierra dithering v2 (error diffusion).</summary>
    Sierra2 = 4,

    /// <summary>Frankie Sierra dithering v2 "Lite" (error diffusion) - default.</summary>
    Sierra2_4a = 5,

    /// <summary>Frankie Sierra dithering v3 (error diffusion).</summary>
    Sierra3 = 6,

    /// <summary>Burkes dithering (error diffusion).</summary>
    Burkes = 7,

    /// <summary>Atkinson dithering by Bill Atkinson at Apple Computer (error diffusion).</summary>
    Atkinson = 8
}