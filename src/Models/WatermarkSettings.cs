// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Exceptions;
using FFmpegDotnetWrapper.Utilities;

namespace FFmpegDotnetWrapper.Models;

/// <summary>
/// Configuration settings for applying watermarks to videos.
/// </summary>
public class WatermarkSettings
{
    private string _watermarkPath = string.Empty;
    private double _opacity = 1.0;

    public string WatermarkPath
    {
        get => _watermarkPath;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationConfigurationException("Watermark path cannot be null or empty");

            if (!File.Exists(value))
                throw new InvalidOperationConfigurationException($"Watermark file does not exist: {value}");

            // Validate that the watermark path stays within the current directory
                // Use the executable's directory as a safe base directory
                var baseDirectory = AppContext.BaseDirectory;
                _watermarkPath = PathValidation.ValidateExistingFileWithinBaseDirectory(value, baseDirectory, nameof(WatermarkPath));
        }
    }

    public double Opacity
    {
        get => _opacity;
        set
        {
            if (value < 0 || value > 1)
                throw new InvalidOperationConfigurationException("Opacity must be between 0 and 1");
            _opacity = value;
        }
    }

    public WatermarkPosition Position { get; set; } = WatermarkPosition.TopRight;
    public int? XOffset { get; set; } = 10;
    public int? YOffset { get; set; } = 10;
    public double? Scale { get; set; } = 0.2; // 20% of video width
    public bool PreserveAspectRatio { get; set; } = true;
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? Duration { get; set; }
    public bool AnimateIn { get; set; } = false;
    public TimeSpan? AnimateInDuration { get; set; }

    public override string ToString() => $"WatermarkSettings {{ Position = {Position}, XOffset = {XOffset}, YOffset = {YOffset}, Scale = {Scale}, PreserveAspectRatio = {PreserveAspectRatio}, StartTime = {StartTime} }}";

    /// <summary>
    /// Validates the watermark settings.
    /// </summary>
    public void Validate(MediaFile mediaFile)
    {
        mediaFile.ValidateAsVideo();

        if (!mediaFile.Width.HasValue || !mediaFile.Height.HasValue)
            throw new InvalidOperationConfigurationException("Video dimensions are required for watermark validation");

        if (Scale.HasValue && (Scale < 0.01 || Scale > 1))
            throw new InvalidOperationConfigurationException("Scale must be between 0.01 and 1");

        if (StartTime.HasValue && StartTime.Value < TimeSpan.Zero)
            throw new InvalidOperationConfigurationException("Start time cannot be negative");

        if (Duration.HasValue && Duration.Value <= TimeSpan.Zero)
            throw new InvalidOperationConfigurationException("Duration must be greater than zero");

        if (AnimateIn && !AnimateInDuration.HasValue)
            throw new InvalidOperationConfigurationException("AnimateInDuration is required when AnimateIn is enabled");

        if (AnimateInDuration.HasValue && AnimateInDuration.Value <= TimeSpan.Zero)
            throw new InvalidOperationConfigurationException("AnimateInDuration must be greater than zero");
    }

    /// <summary>
    /// Calculates the watermark position coordinates.
    /// </summary>
    public (int X, int Y) CalculatePosition(int videoWidth, int videoHeight)
    {
        int x = XOffset ?? 0;
        int y = YOffset ?? 0;

        return Position switch
        {
            WatermarkPosition.TopLeft => (x, y),
            WatermarkPosition.TopRight => (videoWidth - x, y),
            WatermarkPosition.BottomLeft => (x, videoHeight - y),
            WatermarkPosition.BottomRight => (videoWidth - x, videoHeight - y),
            WatermarkPosition.Center => (videoWidth / 2 - x, videoHeight / 2 - y),
            _ => (x, y)
        };
    }

    /// <summary>
    /// Creates a clone of the current settings.
    /// </summary>
    public WatermarkSettings Clone()
    {
        return new WatermarkSettings
        {
            _watermarkPath = _watermarkPath,
            _opacity = _opacity,
            Position = Position,
            XOffset = XOffset,
            YOffset = YOffset,
            Scale = Scale,
            PreserveAspectRatio = PreserveAspectRatio,
            StartTime = StartTime,
            Duration = Duration,
            AnimateIn = AnimateIn,
            AnimateInDuration = AnimateInDuration
        };
    }
}

/// <summary>
/// Watermark position enumeration.
/// </summary>
public enum WatermarkPosition
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
    Center
}
