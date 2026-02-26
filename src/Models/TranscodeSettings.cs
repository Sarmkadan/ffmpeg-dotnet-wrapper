// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Constants;
using FFmpegDotnetWrapper.Exceptions;

namespace FFmpegDotnetWrapper.Models;

/// <summary>
/// Configuration settings for video transcoding operations.
/// </summary>
public class TranscodeSettings
{
    private int _videoBitrate = FFmpegConstants.DefaultBitrate;
    private int _audioBitrate = FFmpegConstants.DefaultAudioBitrate;
    private int _frameRate = FFmpegConstants.DefaultFrameRate;

    public VideoCodec VideoCodec { get; set; } = VideoCodec.H264;
    public AudioCodec AudioCodec { get; set; } = AudioCodec.AAC;
    public ContainerFormat Container { get; set; } = ContainerFormat.MP4;

    public int VideoBitrate
    {
        get => _videoBitrate;
        set
        {
            if (value < FFmpegConstants.MinBitrate || value > FFmpegConstants.MaxBitrate)
                throw new InvalidOperationConfigurationException(
                    $"Video bitrate must be between {FFmpegConstants.MinBitrate} and {FFmpegConstants.MaxBitrate} kbps");
            _videoBitrate = value;
        }
    }

    public int AudioBitrate
    {
        get => _audioBitrate;
        set
        {
            if (value < FFmpegConstants.MinAudioBitrate || value > FFmpegConstants.MaxAudioBitrate)
                throw new InvalidOperationConfigurationException(
                    $"Audio bitrate must be between {FFmpegConstants.MinAudioBitrate} and {FFmpegConstants.MaxAudioBitrate} kbps");
            _audioBitrate = value;
        }
    }

    public int FrameRate
    {
        get => _frameRate;
        set
        {
            if (value < FFmpegConstants.MinFrameRate || value > FFmpegConstants.MaxFrameRate)
                throw new InvalidOperationConfigurationException(
                    $"Frame rate must be between {FFmpegConstants.MinFrameRate} and {FFmpegConstants.MaxFrameRate}");
            _frameRate = value;
        }
    }

    public int? Width { get; set; }
    public int? Height { get; set; }
    public QualityPreset Quality { get; set; } = QualityPreset.Medium;
    public bool EnableAutoScale { get; set; } = true;
    public int? MaxWidth { get; set; } = 1920;
    public int? MaxHeight { get; set; } = 1080;
    public ScalingMode ScalingMode { get; set; } = ScalingMode.Lanczos;
    public bool PreserveAspectRatio { get; set; } = true;
    public bool EnableAudioNormalization { get; set; } = false;
    public double? TargetLoudness { get; set; } = -23.0;
    public bool TwoPass { get; set; } = false;
    public string? CustomFFmpegArgs { get; set; }

    /// <summary>
    /// Validates the transcode settings for consistency and compatibility.
    /// </summary>
    public void Validate()
    {
        if (Width.HasValue && Width < 1)
            throw new InvalidOperationConfigurationException("Width must be greater than 0");

        if (Height.HasValue && Height < 1)
            throw new InvalidOperationConfigurationException("Height must be greater than 0");

        if (MaxWidth.HasValue && MaxWidth < 1)
            throw new InvalidOperationConfigurationException("MaxWidth must be greater than 0");

        if (MaxHeight.HasValue && MaxHeight < 1)
            throw new InvalidOperationConfigurationException("MaxHeight must be greater than 0");

        if (EnableAutoScale && MaxWidth.HasValue && MaxHeight.HasValue)
        {
            if (MaxWidth < 320 || MaxHeight < 180)
                throw new InvalidOperationConfigurationException("MaxWidth/MaxHeight are too small for auto-scaling");
        }

        if (EnableAudioNormalization && TargetLoudness.HasValue)
        {
            if (TargetLoudness < -40 || TargetLoudness > -5)
                throw new InvalidOperationConfigurationException("Target loudness must be between -40 and -5 LUFS");
        }
    }

    /// <summary>
    /// Creates a clone of the current settings.
    /// </summary>
    public TranscodeSettings Clone()
    {
        return new TranscodeSettings
        {
            VideoCodec = VideoCodec,
            AudioCodec = AudioCodec,
            Container = Container,
            VideoBitrate = VideoBitrate,
            AudioBitrate = AudioBitrate,
            FrameRate = FrameRate,
            Width = Width,
            Height = Height,
            Quality = Quality,
            EnableAutoScale = EnableAutoScale,
            MaxWidth = MaxWidth,
            MaxHeight = MaxHeight,
            ScalingMode = ScalingMode,
            PreserveAspectRatio = PreserveAspectRatio,
            EnableAudioNormalization = EnableAudioNormalization,
            TargetLoudness = TargetLoudness,
            TwoPass = TwoPass,
            CustomFFmpegArgs = CustomFFmpegArgs
        };
    }
}
