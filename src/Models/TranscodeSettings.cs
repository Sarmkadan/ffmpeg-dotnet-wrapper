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

    /// <summary>
    /// Video codec to use for encoding. Defaults to <see cref="VideoCodec.H264"/>.
    /// </summary>
    public VideoCodec VideoCodec { get; set; } = VideoCodec.H264;
    /// <summary>
    /// Audio codec to use for encoding. Defaults to <see cref="AudioCodec.AAC"/>.
    /// </summary>
    public AudioCodec AudioCodec { get; set; } = AudioCodec.AAC;
    /// <summary>
    /// Container format for the output file. Defaults to <see cref="ContainerFormat.MP4"/>.
    /// </summary>
    public ContainerFormat Container { get; set; } = ContainerFormat.MP4;

    /// <summary>
    /// Target video bitrate in kbps.
    /// Must be between <see cref="FFmpegConstants.MinBitrate"/> and <see cref="FFmpegConstants.MaxBitrate"/>.
    /// Defaults to <see cref="FFmpegConstants.DefaultBitrate"/>.
    /// </summary>
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

    /// <summary>
    /// Target audio bitrate in kbps.
    /// Must be between <see cref="FFmpegConstants.MinAudioBitrate"/> and <see cref="FFmpegConstants.MaxAudioBitrate"/>.
    /// Defaults to <see cref="FFmpegConstants.DefaultAudioBitrate"/>.
    /// </summary>
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

    /// <summary>
    /// Target frame rate (fps).
    /// Must be between <see cref="FFmpegConstants.MinFrameRate"/> and <see cref="FFmpegConstants.MaxFrameRate"/>.
    /// Defaults to <see cref="FFmpegConstants.DefaultFrameRate"/>.
    /// </summary>
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

    /// <summary>
    /// Optional output width in pixels. If null, the source width is used.
    /// </summary>
    public int? Width { get; set; }
    /// <summary>
    /// Optional output height in pixels. If null, the source height is used.
    /// </summary>
    public int? Height { get; set; }
    /// <summary>
    /// Encoding quality preset. Defaults to <see cref="QualityPreset.Medium"/>.
    /// </summary>
    public QualityPreset Quality { get; set; } = QualityPreset.Medium;
    /// <summary>
    /// Whether to automatically scale the output to fit within <see cref="MaxWidth"/> and <see cref="MaxHeight"/>.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool EnableAutoScale { get; set; } = true;
    /// <summary>
    /// Maximum width for auto-scaling. Defaults to <c>1920</c>.
    /// </summary>
    public int? MaxWidth { get; set; } = 1920;
    /// <summary>
    /// Maximum height for auto-scaling. Defaults to <c>1080</c>.
    /// </summary>
    public int? MaxHeight { get; set; } = 1080;
    /// <summary>
    /// Scaling algorithm to use when resizing. Defaults to <see cref="ScalingMode.Lanczos"/>.
    /// </summary>
    public ScalingMode ScalingMode { get; set; } = ScalingMode.Lanczos;
    /// <summary>
    /// Whether to preserve the source aspect ratio when resizing. Defaults to <c>true</c>.
    /// </summary>
    public bool PreserveAspectRatio { get; set; } = true;
    /// <summary>
    /// Whether to enable audio normalization. Defaults to <c>false</c>.
    /// </summary>
    public bool EnableAudioNormalization { get; set; } = false;
    /// <summary>
    /// Target loudness in LUFS for audio normalization. Defaults to <c>-23.0</c>.
    /// Only applies when <see cref="EnableAudioNormalization"/> is <c>true</c>.
    /// </summary>
    public double? TargetLoudness { get; set; } = -23.0;
    /// <summary>
    /// Whether to use two-pass encoding for better quality. Defaults to <c>false</c>.
    /// </summary>
    public bool TwoPass { get; set; } = false;
    /// <summary>
    /// Additional custom FFmpeg command-line arguments to pass to the encoder.
    /// </summary>
    public string? CustomFFmpegArgs { get; set; }

    /// <summary>
    /// Hardware acceleration backend to use for encoding.
    /// Set to <see cref="HwAccel.None"/> (default) for software encoding.
    /// <see cref="HwAccel.Auto"/> lets FFmpeg probe and select the best available accelerator.
    /// </summary>
    public HwAccel HardwareAcceleration { get; set; } = HwAccel.None;

    /// <summary>
    /// Known-good video codec / container combinations. Entries not listed here
    /// will be rejected during validation to prevent silent FFmpeg failures.
    /// </summary>
    private static readonly Dictionary<ContainerFormat, HashSet<VideoCodec>> ContainerVideoCodecMap = new()
    {
        [ContainerFormat.MP4]  = [VideoCodec.H264, VideoCodec.H265, VideoCodec.AV1],
        [ContainerFormat.Matroska] = [VideoCodec.H264, VideoCodec.H265, VideoCodec.VP8, VideoCodec.VP9, VideoCodec.AV1],
        [ContainerFormat.WebM] = [VideoCodec.VP8, VideoCodec.VP9, VideoCodec.AV1],
    };

    /// <summary>
    /// Validates the transcode settings for consistency, compatibility, and
    /// codec/container support.
    /// </summary>
    public void Validate()
    {
        ValidateCodecContainerCompatibility();

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

    private void ValidateCodecContainerCompatibility()
    {
        if (!ContainerVideoCodecMap.TryGetValue(Container, out var supportedCodecs))
            return; // unknown containers are allowed (user may use CustomFFmpegArgs)

        if (!supportedCodecs.Contains(VideoCodec))
        {
            var allowed = string.Join(", ", supportedCodecs);
            throw new InvalidOperationConfigurationException(
                $"{VideoCodec} is not supported in {Container} container. " +
                $"Supported video codecs for {Container}: {allowed}");
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
            CustomFFmpegArgs = CustomFFmpegArgs,
            HardwareAcceleration = HardwareAcceleration
        };
    }
    public override string ToString() => $"TranscodeSettings {{ VideoCodec = {VideoCodec}, AudioCodec = {AudioCodec}, Container = {Container}, Width = {Width}, Height = {Height}, Quality = {Quality} }}";
}
