// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using FFmpegDotnetWrapper.Models;

namespace FFmpegDotnetWrapper.Services;

/// <summary>
/// Extension methods for <see cref="ThumbnailService"/> providing additional convenience methods
/// for common thumbnail extraction scenarios.
/// </summary>
public static class ThumbnailServiceExtensions
{
    /// <summary>
    /// Extracts a single thumbnail at the beginning of the video (first frame).
    /// </summary>
    /// <param name="service">The thumbnail service instance. Cannot be <see langword="null"/>.</param>
    /// <param name="inputMedia">The source video file. Cannot be <see langword="null"/>.</param>
    /// <param name="outputPath">Full path for the output image file. Cannot be <see langword="null"/> or empty.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="ThumbnailResult"/> containing the extracted image path.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> or <paramref name="inputMedia"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="outputPath"/> is empty.</exception>
    public static async Task<ThumbnailResult> ExtractFirstFrameAsync(
        this ThumbnailService service,
        MediaFile inputMedia,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(inputMedia);
        ArgumentException.ThrowIfNullOrEmpty(outputPath);

        return await service.ExtractSingleAsync(inputMedia, outputPath, TimeSpan.Zero, cancellationToken);
    }

    /// <summary>
    /// Extracts a single thumbnail at the end of the video (last frame).
    /// </summary>
    /// <param name="service">The thumbnail service instance. Cannot be <see langword="null"/>.</param>
    /// <param name="inputMedia">The source video file. Cannot be <see langword="null"/>.</param>
    /// <param name="outputPath">Full path for the output image file. Cannot be <see langword="null"/> or empty.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="ThumbnailResult"/> containing the extracted image path.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> or <paramref name="inputMedia"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="outputPath"/> is empty or media file lacks duration information.</exception>
    public static async Task<ThumbnailResult> ExtractLastFrameAsync(
        this ThumbnailService service,
        MediaFile inputMedia,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(inputMedia);
        ArgumentException.ThrowIfNullOrEmpty(outputPath);

        if (!inputMedia.Duration.HasValue)
        {
            throw new ArgumentException("Media file must have duration information to extract last frame", nameof(inputMedia));
        }

        var lastFrameTime = inputMedia.Duration.Value.Subtract(TimeSpan.FromMilliseconds(1));
        return await service.ExtractSingleAsync(inputMedia, outputPath, lastFrameTime, cancellationToken);
    }

    /// <summary>
    /// Extracts a single thumbnail at the middle timestamp of the video.
    /// </summary>
    /// <param name="service">The thumbnail service instance. Cannot be <see langword="null"/>.</param>
    /// <param name="inputMedia">The source video file. Cannot be <see langword="null"/>.</param>
    /// <param name="outputPath">Full path for the output image file. Cannot be <see langword="null"/> or empty.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="ThumbnailResult"/> containing the extracted image path.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> or <paramref name="inputMedia"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="outputPath"/> is empty or media file lacks duration information.</exception>
    public static async Task<ThumbnailResult> ExtractMiddleFrameAsync(
        this ThumbnailService service,
        MediaFile inputMedia,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(inputMedia);
        ArgumentException.ThrowIfNullOrEmpty(outputPath);

        if (!inputMedia.Duration.HasValue)
        {
            throw new ArgumentException("Media file must have duration information to extract middle frame", nameof(inputMedia));
        }

        var middleTime = TimeSpan.FromSeconds(inputMedia.Duration.Value.TotalSeconds / 2);
        return await service.ExtractSingleAsync(inputMedia, outputPath, middleTime, cancellationToken);
    }

    /// <summary>
    /// Extracts a single thumbnail at a specific percentage position in the video.
    /// </summary>
    /// <param name="service">The thumbnail service instance. Cannot be <see langword="null"/>.</param>
    /// <param name="inputMedia">The source video file. Cannot be <see langword="null"/>.</param>
    /// <param name="outputPath">Full path for the output image file. Cannot be <see langword="null"/> or empty.</param>
    /// <param name="percentage">Percentage position in the video (0-100).</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="ThumbnailResult"/> containing the extracted image path.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> or <paramref name="inputMedia"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="outputPath"/> is empty or media file lacks duration information.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="percentage"/> is outside 0-100 range.</exception>
    public static async Task<ThumbnailResult> ExtractAtPercentageAsync(
        this ThumbnailService service,
        MediaFile inputMedia,
        string outputPath,
        double percentage,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(inputMedia);
        ArgumentException.ThrowIfNullOrEmpty(outputPath);

        if (percentage < 0 || percentage > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(percentage), "Percentage must be between 0 and 100");
        }

        if (!inputMedia.Duration.HasValue)
        {
            throw new ArgumentException("Media file must have duration information to extract at percentage", nameof(inputMedia));
        }

        var timePosition = TimeSpan.FromSeconds(inputMedia.Duration.Value.TotalSeconds * (percentage / 100));
        return await service.ExtractSingleAsync(inputMedia, outputPath, timePosition, cancellationToken);
    }
}