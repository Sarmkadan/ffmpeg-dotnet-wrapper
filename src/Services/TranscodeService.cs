// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Constants;
using FFmpegDotnetWrapper.Exceptions;
using FFmpegDotnetWrapper.Models;
using FFmpegDotnetWrapper.Repository;
using Microsoft.Extensions.Logging;

namespace FFmpegDotnetWrapper.Services;

/// <summary>
/// Specialized service for handling transcoding operations.
/// </summary>
public class TranscodeService : ITranscodeService
{
    private readonly IFFmpegService _ffmpegService;
    private readonly ILogger<TranscodeService> _logger;

    public TranscodeService(IFFmpegService ffmpegService, ILogger<TranscodeService> logger)
    {
        _ffmpegService = ffmpegService ?? throw new ArgumentNullException(nameof(ffmpegService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Transcodes to H.264 format optimized for web.
    /// </summary>
    public async Task<ConversionResult> TranscodeToWebAsync(
        MediaFile inputMedia,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        var settings = new TranscodeSettings
        {
            VideoCodec = VideoCodec.H264,
            AudioCodec = AudioCodec.AAC,
            Container = ContainerFormat.MP4,
            VideoBitrate = 2500,
            AudioBitrate = 128,
            FrameRate = 30,
            Quality = QualityPreset.Fast,
            EnableAutoScale = true,
            MaxWidth = 1280,
            MaxHeight = 720,
            PreserveAspectRatio = true
        };

        _logger.LogInformation("Transcoding {File} to web format (H.264)", inputMedia.Name);
        return await _ffmpegService.TranscodeAsync(inputMedia, outputPath, settings, cancellationToken);
    }

    /// <summary>
    /// Transcodes to H.265 format for better compression.
    /// </summary>
    public async Task<ConversionResult> TranscodeToH265Async(
        MediaFile inputMedia,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        var settings = new TranscodeSettings
        {
            VideoCodec = VideoCodec.H265,
            AudioCodec = AudioCodec.AAC,
            Container = ContainerFormat.MP4,
            VideoBitrate = 1500,
            AudioBitrate = 128,
            FrameRate = 30,
            Quality = QualityPreset.Medium,
            EnableAutoScale = true,
            MaxWidth = 1920,
            MaxHeight = 1080
        };

        _logger.LogInformation("Transcoding {File} to H.265 format", inputMedia.Name);
        return await _ffmpegService.TranscodeAsync(inputMedia, outputPath, settings, cancellationToken);
    }

    /// <summary>
    /// Transcodes to mobile-friendly format.
    /// </summary>
    public async Task<ConversionResult> TranscodeToMobileAsync(
        MediaFile inputMedia,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        var settings = new TranscodeSettings
        {
            VideoCodec = VideoCodec.H264,
            AudioCodec = AudioCodec.AAC,
            Container = ContainerFormat.MP4,
            VideoBitrate = 1000,
            AudioBitrate = 96,
            FrameRate = 25,
            Quality = QualityPreset.Fast,
            EnableAutoScale = true,
            MaxWidth = 720,
            MaxHeight = 480,
            PreserveAspectRatio = true
        };

        _logger.LogInformation("Transcoding {File} to mobile format", inputMedia.Name);
        return await _ffmpegService.TranscodeAsync(inputMedia, outputPath, settings, cancellationToken);
    }

    /// <summary>
    /// Transcodes to high-quality format for archival.
    /// </summary>
    public async Task<ConversionResult> TranscodeToHighQualityAsync(
        MediaFile inputMedia,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        var settings = new TranscodeSettings
        {
            VideoCodec = VideoCodec.H264,
            AudioCodec = AudioCodec.FLAC,
            Container = ContainerFormat.MP4,
            VideoBitrate = 8000,
            AudioBitrate = 320,
            FrameRate = 30,
            Quality = QualityPreset.Slower,
            TwoPass = true,
            EnableAudioNormalization = true,
            TargetLoudness = -23.0
        };

        _logger.LogInformation("Transcoding {File} to high-quality format", inputMedia.Name);
        return await _ffmpegService.TranscodeAsync(inputMedia, outputPath, settings, cancellationToken);
    }

    /// <summary>
    /// Creates a custom transcode with specified bitrate.
    /// </summary>
    public async Task<ConversionResult> TranscodeWithBitrateAsync(
        MediaFile inputMedia,
        string outputPath,
        int videoBitrate,
        int audioBitrate,
        CancellationToken cancellationToken = default)
    {
        var settings = new TranscodeSettings
        {
            VideoCodec = VideoCodec.H264,
            AudioCodec = AudioCodec.AAC,
            Container = ContainerFormat.MP4,
            VideoBitrate = videoBitrate,
            AudioBitrate = audioBitrate,
            FrameRate = 30,
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
    public async Task<ConversionResult> ExtractAudioAsync(
        MediaFile inputMedia,
        string outputPath,
        AudioCodec audioCodec = AudioCodec.MP3,
        CancellationToken cancellationToken = default)
    {
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
    public async Task<ConversionResult> ResizeVideoAsync(
        MediaFile inputMedia,
        string outputPath,
        int width,
        int height,
        CancellationToken cancellationToken = default)
    {
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
