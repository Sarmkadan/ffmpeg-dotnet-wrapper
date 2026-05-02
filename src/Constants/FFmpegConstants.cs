// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace FFmpegDotnetWrapper.Constants;

/// <summary>
/// Global constants for FFmpeg operations and configuration.
/// </summary>
public static class FFmpegConstants
{
    public const string FFmpegExecutableName = "ffmpeg";
    public const string FFprobeExecutableName = "ffprobe";

    public const int DefaultTimeoutSeconds = 300;
    public const int MaxTimeoutSeconds = 3600;
    public const int MinTimeoutSeconds = 10;

    public const int DefaultBitrate = 5000; // kbps
    public const int MinBitrate = 100;
    public const int MaxBitrate = 50000;

    public const int DefaultAudioBitrate = 128; // kbps
    public const int MinAudioBitrate = 32;
    public const int MaxAudioBitrate = 320;

    public const int DefaultFrameRate = 30;
    public const int MinFrameRate = 1;
    public const int MaxFrameRate = 120;

    public static class FileExtensions
    {
        public const string MP4 = ".mp4";
        public const string MKV = ".mkv";
        public const string AVI = ".avi";
        public const string MOV = ".mov";
        public const string FLV = ".flv";
        public const string WEBM = ".webm";
        public const string WAV = ".wav";
        public const string MP3 = ".mp3";
        public const string AAC = ".aac";
        public const string FLAC = ".flac";
    }

    public static class VideoCodecNames
    {
        public const string H264 = "h264";
        public const string H265 = "hevc";
        public const string VP8 = "vp8";
        public const string VP9 = "vp9";
        public const string AV1 = "av1";
        public const string MPEG2 = "mpeg2video";
    }

    public static class AudioCodecNames
    {
        public const string AAC = "aac";
        public const string MP3 = "libmp3lame";
        public const string OPUS = "libopus";
        public const string FLAC = "flac";
        public const string PCM = "pcm_s16le";
        public const string VORBIS = "libvorbis";
    }

    public static class PresetLevels
    {
        public const string Ultrafast = "ultrafast";
        public const string Superfast = "superfast";
        public const string Veryfast = "veryfast";
        public const string Faster = "faster";
        public const string Fast = "fast";
        public const string Medium = "medium";
        public const string Slow = "slow";
        public const string Slower = "slower";
        public const string Veryslow = "veryslow";
    }
}

/// <summary>
/// Video codec enumeration for strongly-typed codec selection.
/// </summary>
public enum VideoCodec
{
    H264,
    H265,
    VP8,
    VP9,
    AV1,
    MPEG2
}

/// <summary>
/// Audio codec enumeration for strongly-typed codec selection.
/// </summary>
public enum AudioCodec
{
    AAC,
    MP3,
    OPUS,
    FLAC,
    PCM,
    VORBIS
}

/// <summary>
/// Container format enumeration.
/// </summary>
public enum ContainerFormat
{
    MP4,
    Matroska,
    AVI,
    QuickTime,
    WebM,
    FLV,
    WAV,
    MP3,
    AAC,
    FLAC,
    /// <summary>HLS (HTTP Live Streaming) — produces a <c>.m3u8</c> playlist and <c>.ts</c> segments.</summary>
    HLS
}

/// <summary>
/// Video quality preset enumeration for encoding efficiency.
/// </summary>
public enum QualityPreset
{
    Ultrafast,
    Superfast,
    Veryfast,
    Faster,
    Fast,
    Medium,
    Slow,
    Slower,
    Veryslow
}

/// <summary>
/// Video scaling mode enumeration.
/// </summary>
public enum ScalingMode
{
    Bilinear,
    Bicubic,
    Lanczos,
    Neighbor,
    Area
}

/// <summary>
/// Audio sample rate enumeration.
/// </summary>
public enum AudioSampleRate
{
    Hz8000 = 8000,
    Hz16000 = 16000,
    Hz22050 = 22050,
    Hz44100 = 44100,
    Hz48000 = 48000,
    Hz96000 = 96000,
    Hz192000 = 192000
}

/// <summary>
/// Audio channel configuration enumeration.
/// </summary>
public enum AudioChannels
{
    Mono = 1,
    Stereo = 2,
    Surround5 = 5,
    Surround51 = 6,
    Surround7 = 7,
    Surround71 = 8
}

/// <summary>
/// Hardware acceleration backend enumeration for video encoding.
/// </summary>
public enum HwAccel
{
    /// <summary>No hardware acceleration; use CPU-based encoding.</summary>
    None,
    /// <summary>NVIDIA NVENC hardware encoder (requires NVIDIA GPU and drivers).</summary>
    NVENC,
    /// <summary>Intel/AMD VAAPI hardware encoder (Linux only).</summary>
    VAAPI,
    /// <summary>Intel Quick Sync Video encoder (Intel iGPU/dGPU required).</summary>
    QSV,
    /// <summary>
    /// Let FFmpeg select the best available hardware accelerator automatically
    /// (<c>-hwaccel auto</c>). Falls back to software if none is available.
    /// </summary>
    Auto
}
