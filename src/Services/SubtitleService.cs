// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Models;
using Microsoft.Extensions.Logging;

namespace FFmpegDotnetWrapper.Services;

/// <summary>
/// Convenience service for subtitle embedding workflows.
/// Wraps <see cref="IFFmpegService"/> with pre-configured settings for common subtitle scenarios.
/// </summary>
public class SubtitleService
{
    private readonly IFFmpegService _ffmpegService;
    private readonly ILogger<SubtitleService> _logger;

    public SubtitleService(IFFmpegService ffmpegService, ILogger<SubtitleService> logger)
    {
        _ffmpegService = ffmpegService ?? throw new ArgumentNullException(nameof(ffmpegService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Soft-embeds a subtitle file as a selectable stream inside the output container.
    /// The original video and audio streams are copied without re-encoding.
    /// </summary>
    /// <param name="inputMedia">The source video file.</param>
    /// <param name="subtitlePath">Path to the subtitle file (.srt, .ass, .vtt, …).</param>
    /// <param name="outputPath">Destination file path for the output with the embedded subtitle track.</param>
    /// <param name="language">Optional ISO 639-1 language code stored in the stream metadata.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="ConversionResult"/> describing the outcome.</returns>
    public async Task<ConversionResult> EmbedSoftSubtitlesAsync(
        MediaFile inputMedia,
        string subtitlePath,
        string outputPath,
        string? language = null,
        CancellationToken cancellationToken = default)
    {
        var settings = new SubtitleSettings
        {
            SubtitlePath = subtitlePath,
            HardEmbed = false,
            Language = language
        };

        _logger.LogInformation("Soft-embedding subtitles from {Sub} into {File}", subtitlePath, inputMedia.Name);
        var result = await _ffmpegService.EmbedSubtitlesAsync(inputMedia, outputPath, settings, cancellationToken);

        if (result.IsSuccess)
            _logger.LogInformation("Soft-embedding completed for {File}", inputMedia.Name);
        else
            _logger.LogWarning("Soft-embedding failed for {File}: {Error}", inputMedia.Name, result.ErrorMessage);

        return result;
    }

    /// <summary>
    /// Hard-burns subtitles directly into the video frames.
    /// The output video will always display the subtitles regardless of the player.
    /// </summary>
    /// <param name="inputMedia">The source video file.</param>
    /// <param name="subtitlePath">Path to the subtitle file.</param>
    /// <param name="outputPath">Destination path for the burned-in output.</param>
    /// <param name="fontName">Font face used for rendering (default: Arial).</param>
    /// <param name="fontSize">Font size in points (default: 24).</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="ConversionResult"/> describing the outcome.</returns>
    public async Task<ConversionResult> BurnSubtitlesAsync(
        MediaFile inputMedia,
        string subtitlePath,
        string outputPath,
        string fontName = "Arial",
        int fontSize = 24,
        CancellationToken cancellationToken = default)
    {
        var settings = new SubtitleSettings
        {
            SubtitlePath = subtitlePath,
            HardEmbed = true,
            FontName = fontName,
            FontSize = fontSize
        };

        _logger.LogInformation("Burning subtitles from {Sub} into {File}", subtitlePath, inputMedia.Name);
        return await _ffmpegService.EmbedSubtitlesAsync(inputMedia, outputPath, settings, cancellationToken);
    }
}
