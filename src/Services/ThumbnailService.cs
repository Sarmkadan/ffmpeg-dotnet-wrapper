// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Models;
using Microsoft.Extensions.Logging;

namespace FFmpegDotnetWrapper.Services;

/// <summary>
/// Convenience service for common thumbnail extraction scenarios.
/// Wraps <see cref="IFFmpegService"/> with pre-configured <see cref="ThumbnailSettings"/>.
/// </summary>
public class ThumbnailService
{
    private readonly IFFmpegService _ffmpegService;
    private readonly ILogger<ThumbnailService> _logger;

    public ThumbnailService(IFFmpegService ffmpegService, ILogger<ThumbnailService> logger)
    {
        _ffmpegService = ffmpegService ?? throw new ArgumentNullException(nameof(ffmpegService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Extracts a single thumbnail at the given timestamp.
    /// </summary>
    /// <param name="inputMedia">The source video file.</param>
    /// <param name="outputPath">Full path for the output image file.</param>
    /// <param name="at">Timestamp within the video to capture. Defaults to the 5-second mark.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="ThumbnailResult"/> containing the extracted image path.</returns>
    public async Task<ThumbnailResult> ExtractSingleAsync(
        MediaFile inputMedia,
        string outputPath,
        TimeSpan? at = null,
        CancellationToken cancellationToken = default)
    {
        var settings = new ThumbnailSettings { Format = ThumbnailFormat.Jpeg };
        settings.Times.Add(at ?? TimeSpan.FromSeconds(5));

        _logger.LogInformation("Extracting single thumbnail from {File} at {Time}", inputMedia.Name, settings.Times[0]);
        return await _ffmpegService.ExtractThumbnailsAsync(inputMedia, outputPath, settings, cancellationToken);
    }

    /// <summary>
    /// Extracts multiple evenly-distributed thumbnails from a video.
    /// </summary>
    /// <param name="inputMedia">The source video file.</param>
    /// <param name="outputPattern">
    /// Output path pattern with <c>%03d</c> placeholder for sequential numbering,
    /// e.g. <c>/output/thumb_%03d.jpg</c>.
    /// </param>
    /// <param name="count">Number of thumbnails to extract. Defaults to 10.</param>
    /// <param name="width">Optional output width in pixels.</param>
    /// <param name="height">Optional output height in pixels (use <c>-1</c> with a set width to auto-scale).</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="ThumbnailResult"/> containing paths of all extracted images.</returns>
    public async Task<ThumbnailResult> ExtractStoryboardAsync(
        MediaFile inputMedia,
        string outputPattern,
        int count = 10,
        int? width = null,
        int? height = null,
        CancellationToken cancellationToken = default)
    {
        var settings = new ThumbnailSettings
        {
            Count = count,
            Format = ThumbnailFormat.Jpeg,
            Width = width,
            Height = height
        };

        if (inputMedia.Duration.HasValue)
        {
            var step = inputMedia.Duration.Value.TotalSeconds / (count + 1);
            for (var i = 1; i <= count; i++)
                settings.Times.Add(TimeSpan.FromSeconds(step * i));
        }

        _logger.LogInformation(
            "Extracting {Count} storyboard thumbnails from {File}",
            count, inputMedia.Name);

        return await _ffmpegService.ExtractThumbnailsAsync(inputMedia, outputPattern, settings, cancellationToken);
    }

    /// <summary>
    /// Extracts thumbnails at each of the provided explicit timestamps.
    /// </summary>
    /// <param name="inputMedia">The source video file.</param>
    /// <param name="outputPattern">Output path pattern with <c>%03d</c> placeholder.</param>
    /// <param name="timestamps">Specific timestamps to capture.</param>
    /// <param name="format">Image format for the output thumbnails.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="ThumbnailResult"/> with one image per timestamp.</returns>
    public async Task<ThumbnailResult> ExtractAtTimestampsAsync(
        MediaFile inputMedia,
        string outputPattern,
        IEnumerable<TimeSpan> timestamps,
        ThumbnailFormat format = ThumbnailFormat.Jpeg,
        CancellationToken cancellationToken = default)
    {
        var settings = new ThumbnailSettings { Format = format };
        settings.Times.AddRange(timestamps);

        _logger.LogInformation(
            "Extracting {Count} thumbnails at explicit timestamps from {File}",
            settings.Times.Count, inputMedia.Name);

        return await _ffmpegService.ExtractThumbnailsAsync(inputMedia, outputPattern, settings, cancellationToken);
    }
}
