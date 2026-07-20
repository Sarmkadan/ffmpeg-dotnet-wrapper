// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using FFmpegDotnetWrapper.Models;
using Microsoft.Extensions.Logging;

namespace FFmpegDotnetWrapper.Services;

/// <summary>
/// Convenience service for common watermark scenarios.
/// Wraps <see cref="IFFmpegService"/> with pre-configured <see cref="WatermarkSettings"/>.
/// </summary>
public class WatermarkService : IWatermarkService
{
    private readonly IFFmpegService _ffmpegService;
    private readonly ILogger<WatermarkService> _logger;

    public WatermarkService(IFFmpegService ffmpegService, ILogger<WatermarkService> logger)
    {
        _ffmpegService = ffmpegService ?? throw new ArgumentNullException(nameof(ffmpegService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Applies a watermark to a video at the specified position.
    /// </summary>
    /// <param name="inputMedia">The source video file.</param>
    /// <param name="outputPath">Full path for the output video file.</param>
    /// <param name="watermarkPath">Path to the watermark image file.</param>
    /// <param name="position">Corner position for the watermark (TopLeft/TopRight/BottomLeft/BottomRight).</param>
    /// <param name="margin">Margin in pixels from the edge. Defaults to 10.</param>
    /// <param name="opacity">Watermark opacity (0.0 to 1.0). Defaults to 0.8.</param>
    /// <param name="scale">Scale factor relative to video width (0.0 to 1.0). Defaults to 0.2.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="ConversionResult"/> with output file info.</returns>
    public async Task<ConversionResult> ApplyWatermarkAsync(
        MediaFile inputMedia,
        string outputPath,
        string watermarkPath,
        WatermarkPosition position = WatermarkPosition.TopRight,
        int margin = 10,
        double opacity = 0.8,
        double scale = 0.2,
        CancellationToken cancellationToken = default)
    {
        var settings = CreateSettings(position, margin, opacity, scale);
        settings.WatermarkPath = watermarkPath;

        _logger.LogInformation(
            "Applying watermark to {File} at {Position} with margin {Margin}px, opacity {Opacity}, scale {Scale}%",
            inputMedia.Name,
            position,
            margin,
            opacity,
            scale * 100);

        return await _ffmpegService.AddWatermarkAsync(inputMedia, outputPath, settings, cancellationToken);
    }

    /// <summary>
    /// Applies a watermark to a video with custom settings.
    /// </summary>
    /// <param name="inputMedia">The source video file.</param>
    /// <param name="outputPath">Full path for the output video file.</param>
    /// <param name="settings">Watermark configuration settings.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="ConversionResult"/> with output file info.</returns>
    public async Task<ConversionResult> ApplyWatermarkAsync(
        MediaFile inputMedia,
        string outputPath,
        WatermarkSettings settings,
        CancellationToken cancellationToken = default)
    {
        if (settings is null)
        {
            throw new ArgumentNullException(nameof(settings));
        }

        settings.Validate(inputMedia);

        _logger.LogInformation(
            "Applying watermark to {File} with custom settings: position={Position}, margin=({XOffset},{YOffset}), opacity={Opacity}, scale={Scale}",
            inputMedia.Name,
            settings.Position,
            settings.XOffset,
            settings.YOffset,
            settings.Opacity,
            settings.Scale);

        return await _ffmpegService.AddWatermarkAsync(inputMedia, outputPath, settings, cancellationToken);
    }

    /// <summary>
    /// Creates a watermark settings object configured for the specified corner position.
    /// </summary>
    /// <param name="position">The corner position for the watermark.</param>
    /// <param name="margin">Margin in pixels from the edge. Defaults to 10.</param>
    /// <param name="opacity">Watermark opacity (0.0 to 1.0). Defaults to 0.8.</param>
    /// <param name="scale">Scale factor relative to video width (0.0 to 1.0). Defaults to 0.2.</param>
    /// <returns>A configured <see cref="WatermarkSettings"/> instance.</returns>
    public WatermarkSettings CreateSettings(
        WatermarkPosition position = WatermarkPosition.TopRight,
        int margin = 10,
        double opacity = 0.8,
        double scale = 0.2)
    {
        return new WatermarkSettings
        {
            Position = position,
            XOffset = margin,
            YOffset = margin,
            Opacity = opacity,
            Scale = scale,
            PreserveAspectRatio = true
        };
    }
}