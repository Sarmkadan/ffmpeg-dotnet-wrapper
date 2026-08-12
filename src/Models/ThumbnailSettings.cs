// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Exceptions;

namespace FFmpegDotnetWrapper.Models;

/// <summary>
/// Output image format for extracted thumbnails.
/// </summary>
public enum ThumbnailFormat
{
    /// <summary>JPEG format — smaller file size, configurable quality.</summary>
    Jpeg,
    /// <summary>PNG format — lossless, larger file size.</summary>
    Png
}

/// <summary>
/// Configuration settings for extracting thumbnail images from a video file.
/// Supports extracting frames at specific timestamps or evenly distributed across the video.
/// </summary>
public class ThumbnailSettings
{
    private int _count = 1;
    private int _jpegQuality = 2;

    /// <summary>
    /// Explicit list of timestamps at which to capture frames.
    /// When non-empty, takes precedence over <see cref="Count"/>.
    /// </summary>
    public List<TimeSpan> Times { get; set; } = [];

    /// <summary>
    /// Number of thumbnails to extract when <see cref="Times"/> is empty.
    /// The frames are distributed evenly across the video duration.
    /// Must be between 1 and 500.
    /// </summary>
    public int Count
    {
        get => _count;
        set
        {
            if (value < 1 || value > 500)
                throw new InvalidOperationConfigurationException("Count must be between 1 and 500");
            _count = value;
        }
    }

    /// <summary>
    /// Output image format. Defaults to <see cref="ThumbnailFormat.Jpeg"/>.
    /// </summary>
    public ThumbnailFormat Format { get; set; } = ThumbnailFormat.Jpeg;

    /// <summary>
    /// Output width in pixels. Set to <c>-1</c> to derive from <see cref="Height"/> while
    /// preserving the aspect ratio. Leave <c>null</c> to use the source width.
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// Output height in pixels. Set to <c>-1</c> to derive from <see cref="Width"/> while
    /// preserving the aspect ratio. Leave <c>null</c> to use the source height.
    /// </summary>
    public int? Height { get; set; }

    /// <summary>
    /// JPEG quality factor (1 = best quality / largest file, 31 = worst quality / smallest file).
    /// Defaults to <c>2</c> (near-lossless). Only applies when <see cref="Format"/> is
    /// <see cref="ThumbnailFormat.Jpeg"/>.
    /// </summary>
    public int? JpegQuality
    {
        get => _jpegQuality;
        set
        {
            if (value.HasValue && (value < 1 || value > 31))
                throw new InvalidOperationConfigurationException("JpegQuality must be between 1 and 31");
            _jpegQuality = value ?? 2;
        }
    }

    public override string ToString() => $"ThumbnailSettings {{ Times = [{string.Join(", ", Times)}], Format = {Format}, Width = {Width}, Height = {Height} }}";

    /// <summary>
    /// Validates the settings against the source video before an operation is executed.
    /// </summary>
    /// <param name="inputMedia">The video file that thumbnails will be extracted from.</param>
    public void Validate(MediaFile inputMedia)
    {
        if (Times.Count == 0 && _count < 1)
            throw new InvalidOperationConfigurationException("Count must be at least 1 when no explicit timestamps are provided");

        if (inputMedia.Duration.HasValue)
        {
            foreach (var t in Times)
            {
                if (t < TimeSpan.Zero)
                    throw new InvalidOperationConfigurationException($"Timestamp {t} cannot be negative");

                if (t > inputMedia.Duration.Value)
                    throw new InvalidOperationConfigurationException(
                        $"Timestamp {t} exceeds video duration {inputMedia.Duration.Value}");
            }
        }

        if (Width.HasValue && Width.Value != -1 && Width.Value < 1)
            throw new InvalidOperationConfigurationException("Width must be greater than 0 (or -1 for auto)");

        if (Height.HasValue && Height.Value != -1 && Height.Value < 1)
            throw new InvalidOperationConfigurationException("Height must be greater than 0 (or -1 for auto)");
    }

    /// <summary>
    /// Creates a deep copy of the current settings.
    /// </summary>
    public ThumbnailSettings Clone() =>
        new()
        {
            Times = new List<TimeSpan>(Times),
            _count = _count,
            Format = Format,
            Width = Width,
            Height = Height,
            _jpegQuality = _jpegQuality
        };
}
