// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Constants;
using FFmpegDotnetWrapper.Exceptions;
using FFmpegDotnetWrapper.Models;
using System.Threading;
using System.Threading.Tasks;

namespace FFmpegDotnetWrapper.Services;

/// <summary>
/// Contract for specialized service handling transcoding operations.
/// </summary>
public interface ITranscodeService
{
    /// <summary>
    /// Transcodes to H.264 format optimized for web.
    /// </summary>
    Task<ConversionResult> TranscodeToWebAsync(
        MediaFile inputMedia,
        string outputPath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Transcodes to H.265 format for better compression.
    /// </summary>
    Task<ConversionResult> TranscodeToH265Async(
        MediaFile inputMedia,
        string outputPath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Transcodes to mobile-friendly format.
    /// </summary>
    Task<ConversionResult> TranscodeToMobileAsync(
        MediaFile inputMedia,
        string outputPath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Transcodes to high-quality format for archival.
    /// </summary>
    Task<ConversionResult> TranscodeToHighQualityAsync(
        MediaFile inputMedia,
        string outputPath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a custom transcode with specified bitrate.
    /// </summary>
    Task<ConversionResult> TranscodeWithBitrateAsync(
        MediaFile inputMedia,
        string outputPath,
        int videoBitrate,
        int audioBitrate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts audio from a video file.
    /// </summary>
    Task<ConversionResult> ExtractAudioAsync(
        MediaFile inputMedia,
        string outputPath,
        AudioCodec audioCodec = AudioCodec.MP3,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Converts video to a specific resolution.
    /// </summary>
    Task<ConversionResult> ResizeVideoAsync(
        MediaFile inputMedia,
        string outputPath,
        int width,
        int height,
        CancellationToken cancellationToken = default);
}