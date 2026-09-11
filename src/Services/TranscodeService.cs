// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Constants;
using FFmpegDotnetWrapper.Exceptions;
using FFmpegDotnetWrapper.Models;
using FFmpegDotnetWrapper.Repository;
using Microsoft.Extensions.Logging;
using System;

namespace FFmpegDotnetWrapper.Services;

/// <summary>
/// Specialized service for handling transcoding operations.
/// </summary>
public class TranscodeService : ITranscodeService
{
    private readonly IFFmpegService _ffmpegService;
    private readonly ILogger<TranscodeService> _logger;

    // Web preset constants
    private const int WebPresetVideoBitrate = 2500;
    private const int WebPresetAudioBitrate = 128;
    private const int WebPresetFrameRate = 30;
    private const int WebPresetMaxWidth = 1280;
    private const int WebPresetMaxHeight = 720;

    // H265 preset constants
    private const int H265PresetVideoBitrate = 1500;
    private const int H265PresetAudioBitrate = 128;
    private const int H265PresetFrameRate = 30;
    private const int H265PresetMaxWidth = 1920;
    private const int H265PresetMaxHeight = 1080;

    // Mobile preset constants
    private const int MobilePresetVideoBitrate = 1000;
    private const int MobilePresetAudioBitrate = 96;
    private const int MobilePresetFrameRate = 25;
    private const int MobilePresetMaxWidth = 720;
    private const int MobilePresetMaxHeight = 480;

    // High quality preset constants
    private const int HighQualityPresetVideoBitrate = 8000;
    private const int HighQualityPresetAudioBitrate = 320;
    private const int HighQualityPresetFrameRate = 30;
    private const double HighQualityPresetTargetLoudness = -23.0;

    // Resize preset constants
    private const int ResizePresetVideoBitrate = 3000;
    private const int ResizePresetAudioBitrate = 128;

    // Extract audio preset constants
    private const int ExtractAudioAudioBitrate = 192;

    // Default constants
    private const int DefaultFrameRate = 30;

    public TranscodeService(IFFmpegService ffmpegService, ILogger<TranscodeService> logger)
    {
        _ffmpegService = ffmpegService ?? throw new ArgumentNullException(nameof(ffmpegService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Transcodes to H.264 format optimized for web.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="inputMedia"/> is null.</exception>
    public async Task<ConversionResult> TranscodeToWebAsync(
        MediaFile inputMedia,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputMedia);

        var settings = new TranscodeSettings
        {
            VideoCodec = VideoCodec.H264,
            AudioCodec = AudioCodec.AAC,
            Container = ContainerFormat.MP4,
            VideoBitrate = WebPresetVideoBitrate,
            AudioBitrate = WebPresetAudioBitrate,
            FrameRate = WebPresetFrameRate,
            Quality = QualityPreset.Fast,
            EnableAutoScale = true,
            MaxWidth = WebPresetMaxWidth,
            MaxHeight = WebPresetMaxHeight,
            PreserveAspectRatio = true
        };

        _logger.LogInformation("Transcoding {File} to web format (H.264)", inputMedia.Name);
        return await _ffmpegService.TranscodeAsync(inputMedia, outputPath, settings, cancellationToken);
    }

    /// <summary>
    /// Transcodes to H.265 format for better compression.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="inputMedia"/> is null.</exception>
    public async Task<ConversionResult> TranscodeToH265Async(
        MediaFile inputMedia,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputMedia);

        var settings = new TranscodeSettings
        {
            VideoCodec = VideoCodec.H265,
            AudioCodec = AudioCodec.AAC,
            Container = ContainerFormat.MP4,
            VideoBitrate = H265PresetVideoBitrate,
            AudioBitrate = H265PresetAudioBitrate,
            FrameRate = H265PresetFrameRate,
            Quality = QualityPreset.Medium,
            EnableAutoScale = true,
            MaxWidth = H265PresetMaxWidth,
            MaxHeight = H265PresetMaxHeight
        };

        _logger.LogInformation("Transcoding {File} to H.265 format", inputMedia.Name);
        return await _ffmpegService.TranscodeAsync(inputMedia, outputPath, settings, cancellationToken);
    }

    /// <summary>
    /// Transcodes to mobile-friendly format.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="inputMedia"/> is null.</exception>
    public async Task<ConversionResult> TranscodeToMobileAsync(
        MediaFile inputMedia,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputMedia);

        var settings = new TranscodeSettings
        {
            VideoCodec = VideoCodec.H264,
            AudioCodec = AudioCodec.AAC,
            Container = ContainerFormat.MP4,
            VideoBitrate = MobilePresetVideoBitrate,
            AudioBitrate = MobilePresetAudioBitrate,
            FrameRate = MobilePresetFrameRate,
            Quality = QualityPreset.Fast,
            EnableAutoScale = true,
            MaxWidth = MobilePresetMaxWidth,
            MaxHeight = MobilePresetMaxHeight,
            PreserveAspectRatio = true
        };

        _logger.LogInformation("Transcoding {File} to mobile format", inputMedia.Name);
        return await _ffmpegService.TranscodeAsync(inputMedia, outputPath, settings, cancellationToken);
    }

    /// <summary>
    /// Transcodes to high-quality format for archival.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="inputMedia"/> is null.</exception>
    public async Task<ConversionResult> TranscodeToHighQualityAsync(
        MediaFile inputMedia,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputMedia);

        var settings = new TranscodeSettings
        {
            VideoCodec = VideoCodec.H264,
            AudioCodec = AudioCodec.FLAC,
            Container = ContainerFormat.MP4,
            VideoBitrate = HighQualityPresetVideoBitrate,
            AudioBitrate = HighQualityPresetAudioBitrate,
            FrameRate = HighQualityPresetFrameRate,
            Quality = QualityPreset.Slower,
            TwoPass = true,
            EnableAudioNormalization = true,
            TargetLoudness = HighQualityPresetTargetLoudness
        };

        _logger.LogInformation("Transcoding {File} to high-quality format", inputMedia.Name);
        return await _ffmpegService.TranscodeAsync(inputMedia, outputPath, settings, cancellationToken);
    }

    /// <summary>
    /// Creates a custom transcode with specified bitrate.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="inputMedia"/> is null.</exception>
    public async Task<ConversionResult> TranscodeWithBitrateAsync(
        MediaFile inputMedia,
        string outputPath,
        int videoBitrate,
        int audioBitrate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputMedia);

        var settings = new TranscodeSettings
        {
            VideoCodec = VideoCodec.H264,
            AudioCodec = AudioCodec.AAC,
            Container = ContainerFormat.MP4,
            VideoBitrate = videoBitrate,
            AudioBitrate = audioBitrate,
            FrameRate = DefaultFrameRate,
            Quality = QualityPreset.Medium
        };

        settings.Validate();

        _logger.LogInformation(
            "Transcoding {File} with custom bitrate: V={VBit}k A={ABit}k",
            inputMedia.Name,
            videoBitrate,
            audioBitrate);

        return await _ffmpegService.TranscodeAsync(inputMedia, outputPath, settings, cancellationToken);
    }

    /// <summary>
    /// Extracts audio from a video file.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="inputMedia"/> is null.</exception>
    public async Task<ConversionResult> ExtractAudioAsync(
        MediaFile inputMedia,
        string outputPath,
        AudioCodec audioCodec = AudioCodec.MP3,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputMedia);
        inputMedia.ValidateAsVideo();

        var settings = new TranscodeSettings
        {
            AudioCodec = audioCodec,
            Container = audioCodec == AudioCodec.MP3 ? ContainerFormat.MP3 : ContainerFormat.AAC,
            AudioBitrate = 192,
            CustomFFmpegArgs = "-vn" // No video
        };

        _logger.LogInformation("Extracting audio from {File} as {Codec}", inputMedia.Name, audioCodec);
        return await _ffmpegService.TranscodeAsync(inputMedia, outputPath, settings, cancellationToken);
    }

    /// <summary>
    /// Converts video to a specific resolution.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="inputMedia"/> is null.</exception>
    public async Task<ConversionResult> ResizeVideoAsync(
        MediaFile inputMedia,
        string outputPath,
        int width,
        int height,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputMedia);

        if (width < 1 || height < 1)
            throw new InvalidOperationConfigurationException("Width and height must be greater than 0");

        var settings = new TranscodeSettings
        {
            VideoCodec = VideoCodec.H264,
            AudioCodec = AudioCodec.AAC,
            Container = ContainerFormat.MP4,
            Width = width,
            Height = height,
            VideoBitrate = 3000,
            AudioBitrate = 128,
            PreserveAspectRatio = true
        };

        _logger.LogInformation("Resizing {File} to {Width}x{Height}", inputMedia.Name, width, height);
        return await _ffmpegService.TranscodeAsync(inputMedia, outputPath, settings, cancellationToken);
    }
}
