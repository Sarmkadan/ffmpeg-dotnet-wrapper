// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using FFmpegDotnetWrapper.Models;

namespace FFmpegDotnetWrapper.Services;

/// <summary>
/// Convenience service for common watermark scenarios.
/// Wraps <see cref="IFFmpegService"/> with pre-configured <see cref="WatermarkSettings"/>.
/// </summary>
public interface IWatermarkService
{
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
    Task<ConversionResult> ApplyWatermarkAsync(
        MediaFile inputMedia,
        string outputPath,
        string watermarkPath,
        WatermarkPosition position = WatermarkPosition.TopRight,
        int margin = 10,
        double opacity = 0.8,
        double scale = 0.2,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies a watermark to a video with custom settings.
    /// </summary>
    /// <param name="inputMedia">The source video file.</param>
    /// <param name="outputPath">Full path for the output video file.</param>
    /// <param name="settings">Watermark configuration settings.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="ConversionResult"/> with output file info.</returns>
    Task<ConversionResult> ApplyWatermarkAsync(
        MediaFile inputMedia,
        string outputPath,
        WatermarkSettings settings,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a watermark settings object configured for the specified corner position.
    /// </summary>
    /// <param name="position">The corner position for the watermark.</param>
    /// <param name="margin">Margin in pixels from the edge. Defaults to 10.</param>
    /// <param name="opacity">Watermark opacity (0.0 to 1.0). Defaults to 0.8.</param>
    /// <param name="scale">Scale factor relative to video width (0.0 to 1.0). Defaults to 0.2.</param>
    /// <returns>A configured <see cref="WatermarkSettings"/> instance.</returns>
    WatermarkSettings CreateSettings(
        WatermarkPosition position = WatermarkPosition.TopRight,
        int margin = 10,
        double opacity = 0.8,
        double scale = 0.2);
}