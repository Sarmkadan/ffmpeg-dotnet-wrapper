// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Models;

namespace FFmpegDotnetWrapper.Services;

/// <summary>
/// Main interface for FFmpeg operations orchestration.
/// </summary>
public interface IFFmpegService
{
    /// <summary>
    /// Transcodes a media file with specified settings.
    /// </summary>
    Task<ConversionResult> TranscodeAsync(
        MediaFile inputMedia,
        string outputPath,
        TranscodeSettings settings,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Trims a media file.
    /// </summary>
    Task<ConversionResult> TrimAsync(
        MediaFile inputMedia,
        string outputPath,
        TrimSettings settings,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Merges multiple media files.
    /// </summary>
    Task<ConversionResult> MergeAsync(
        IEnumerable<string> inputFiles,
        string outputPath,
        MergeSettings settings,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies a watermark to a video.
    /// </summary>
    Task<ConversionResult> AddWatermarkAsync(
        MediaFile inputMedia,
        string outputPath,
        WatermarkSettings settings,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyzes a media file and extracts metadata.
    /// </summary>
    Task<MediaFile> AnalyzeMediaAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a custom FFmpeg operation.
    /// </summary>
    Task<ConversionResult> ExecuteCustomOperationAsync(
        FFmpegOperation operation,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the FFmpeg version information.
    /// </summary>
    Task<string> GetFFmpegVersionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if FFmpeg is installed and available.
    /// </summary>
    Task<bool> IsFFmpegAvailableAsync(CancellationToken cancellationToken = default);
}
