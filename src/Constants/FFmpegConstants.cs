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
    /// <summary>The FFmpeg executable name.</summary>
    public const string FFmpegExecutableName = "ffmpeg";
    /// <summary>The FFprobe executable name.</summary>
    public const string FFprobeExecutableName = "ffprobe";

    /// <summary>The default operation timeout in seconds.</summary>
    public const int DefaultTimeoutSeconds = 300;
    /// <summary>The maximum operation timeout in seconds.</summary>
    public const int MaxTimeoutSeconds = 3600;
    /// <summary>The minimum operation timeout in seconds.</summary>
    public const int MinTimeoutSeconds = 10;

    /// <summary>The default video bitrate in kilobits per second.</summary>
    public const int DefaultBitrate = 5000; // kbps
    /// <summary>The minimum video bitrate in kilobits per second.</summary>
    public const int MinBitrate = 100;
    /// <summary>The maximum video bitrate in kilobits per second.</summary>
    public const int MaxBitrate = 50000;

    /// <summary>The default audio bitrate in kilobits per second.</summary>
    public const int DefaultAudioBitrate = 128; // kbps
    /// <summary>The minimum audio bitrate in kilobits per second.</summary>
    public const int MinAudioBitrate = 32;
    /// <summary>The maximum audio bitrate in kilobits per second.</summary>
    public const int MaxAudioBitrate = 320;

    /// <summary>The default video frame rate.</summary>
    public const int DefaultFrameRate = 30;
    /// <summary>The minimum video frame rate.</summary>
    public const int MinFrameRate = 1;
    /// <summary>The maximum video frame rate.</summary>
    public const int MaxFrameRate = 120;

    public static class FileExtensions
    {
        /// <summary>The MP4 file extension.</summary>
        public const string MP4 = ".mp4";
        /// <summary>The Matroska file extension.</summary>
        public const string MKV = ".mkv";
        /// <summary>The AVI file extension.</summary>
        public const string AVI = ".avi";
        /// <summary>The QuickTime movie file extension.</summary>
        public const string MOV = ".mov";
        /// <summary>The Flash video file extension.</summary>
        public const string FLV = ".flv";
        /// <summary>The WebM file extension.</summary>
        public const string WEBM = ".webm";
        /// <summary>The WAV file extension.</summary>
        public const string WAV = ".wav";
        /// <summary>The MP3 file extension.</summary>
        public const string MP3 = ".mp3";
        /// <summary>The AAC file extension.</summary>
        public const string AAC = ".aac";
        /// <summary>The FLAC file extension.</summary>
        public const string FLAC = ".flac";
    }

    public static class VideoCodecNames
    {
        /// <summary>The FFmpeg H.264 codec name.</summary>
        public const string H264 = "h264";
        /// <summary>The FFmpeg H.265 codec name.</summary>
        public const string H265 = "hevc";
        /// <summary>The FFmpeg VP8 codec name.</summary>
        public const string VP8 = "vp8";
        /// <summary>The FFmpeg VP9 codec name.</summary>
        public const string VP9 = "vp9";
        /// <summary>The FFmpeg AV1 codec name.</summary>
        public const string AV1 = "av1";
        /// <summary>The FFmpeg MPEG-2 codec name.</summary>
        public const string MPEG2 = "mpeg2video";
    }

    public static class AudioCodecNames
    {
        /// <summary>The FFmpeg AAC codec name.</summary>
        public const string AAC = "aac";
        /// <summary>The FFmpeg MP3 codec name.</summary>
        public const string MP3 = "libmp3lame";
        /// <summary>The FFmpeg Opus codec name.</summary>
        public const string OPUS = "libopus";
        /// <summary>The FFmpeg FLAC codec name.</summary>
        public const string FLAC = "flac";
        /// <summary>The FFmpeg PCM codec name.</summary>
        public const string PCM = "pcm_s16le";
        /// <summary>The FFmpeg Vorbis codec name.</summary>
        public const string VORBIS = "libvorbis";
    }

    public static class PresetLevels
    {
        /// <summary>The ultrafast encoding preset.</summary>
        public const string Ultrafast = "ultrafast";
        /// <summary>The superfast encoding preset.</summary>
        public const string Superfast = "superfast";
        /// <summary>The very fast encoding preset.</summary>
        public const string Veryfast = "veryfast";
        /// <summary>The faster encoding preset.</summary>
        public const string Faster = "faster";
        /// <summary>The fast encoding preset.</summary>
        public const string Fast = "fast";
        /// <summary>The medium encoding preset.</summary>
        public const string Medium = "medium";
        /// <summary>The slow encoding preset.</summary>
        public const string Slow = "slow";
        /// <summary>The slower encoding preset.</summary>
        public const string Slower = "slower";
        /// <summary>The very slow encoding preset.</summary>
        public const string Veryslow = "veryslow";
    }
}

/// <summary>
/// Video codec enumeration for strongly-typed codec selection.
/// </summary>
public enum VideoCodec
{
    /// <summary>The H.264 video codec.</summary>
    H264,
    /// <summary>The H.265 video codec.</summary>
    H265,
    /// <summary>The VP8 video codec.</summary>
    VP8,
    /// <summary>The VP9 video codec.</summary>
    VP9,
    /// <summary>The AV1 video codec.</summary>
    AV1,
    /// <summary>The MPEG-2 video codec.</summary>
    MPEG2
}

/// <summary>
/// Audio codec enumeration for strongly-typed codec selection.
/// </summary>
public enum AudioCodec
{
    /// <summary>The AAC audio codec.</summary>
    AAC,
    /// <summary>The MP3 audio codec.</summary>
    MP3,
    /// <summary>The Opus audio codec.</summary>
    OPUS,
    /// <summary>The FLAC audio codec.</summary>
    FLAC,
    /// <summary>The PCM audio codec.</summary>
    PCM,
    /// <summary>The Vorbis audio codec.</summary>
    VORBIS
}

/// <summary>
/// Container format enumeration.
/// </summary>
public enum ContainerFormat
{
    /// <summary>The MP4 container format.</summary>
    MP4,
    /// <summary>The Matroska container format.</summary>
    Matroska,
    /// <summary>The AVI container format.</summary>
    AVI,
    /// <summary>The QuickTime container format.</summary>
    QuickTime,
    /// <summary>The WebM container format.</summary>
    WebM,
    /// <summary>The Flash video container format.</summary>
    FLV,
    /// <summary>The WAV audio container format.</summary>
    WAV,
    /// <summary>The MP3 audio container format.</summary>
    MP3,
    /// <summary>The AAC audio container format.</summary>
    AAC,
    /// <summary>The FLAC audio container format.</summary>
    FLAC,
    /// <summary>HLS (HTTP Live Streaming) — produces a <c>.m3u8</c> playlist and <c>.ts</c> segments.</summary>
    HLS
}

/// <summary>
/// Video quality preset enumeration for encoding efficiency.
/// </summary>
public enum QualityPreset
{
    /// <summary>The ultrafast quality preset.</summary>
    Ultrafast,
    /// <summary>The superfast quality preset.</summary>
    Superfast,
    /// <summary>The very fast quality preset.</summary>
    Veryfast,
    /// <summary>The faster quality preset.</summary>
    Faster,
    /// <summary>The fast quality preset.</summary>
    Fast,
    /// <summary>The medium quality preset.</summary>
    Medium,
    /// <summary>The slow quality preset.</summary>
    Slow,
    /// <summary>The slower quality preset.</summary>
    Slower,
    /// <summary>The very slow quality preset.</summary>
    Veryslow
}

/// <summary>
/// Video scaling mode enumeration.
/// </summary>
public enum ScalingMode
{
    /// <summary>Uses bilinear scaling.</summary>
    Bilinear,
    /// <summary>Uses bicubic scaling.</summary>
    Bicubic,
    /// <summary>Uses Lanczos scaling.</summary>
    Lanczos,
    /// <summary>Uses nearest-neighbor scaling.</summary>
    Neighbor,
    /// <summary>Uses area-based scaling.</summary>
    Area
}

/// <summary>
/// Audio sample rate enumeration.
/// </summary>
public enum AudioSampleRate
{
    /// <summary>An 8,000 Hz audio sample rate.</summary>
    Hz8000 = 8000,
    /// <summary>A 16,000 Hz audio sample rate.</summary>
    Hz16000 = 16000,
    /// <summary>A 22,050 Hz audio sample rate.</summary>
    Hz22050 = 22050,
    /// <summary>A 44,100 Hz audio sample rate.</summary>
    Hz44100 = 44100,
    /// <summary>A 48,000 Hz audio sample rate.</summary>
    Hz48000 = 48000,
    /// <summary>A 96,000 Hz audio sample rate.</summary>
    Hz96000 = 96000,
    /// <summary>A 192,000 Hz audio sample rate.</summary>
    Hz192000 = 192000
}

/// <summary>
/// Audio channel configuration enumeration.
/// </summary>
public enum AudioChannels
{
    /// <summary>Mono audio with one channel.</summary>
    Mono = 1,
    /// <summary>Stereo audio with two channels.</summary>
    Stereo = 2,
    /// <summary>Surround audio with five channels.</summary>
    Surround5 = 5,
    /// <summary>5.1 surround audio with six channels.</summary>
    Surround51 = 6,
    /// <summary>Surround audio with seven channels.</summary>
    Surround7 = 7,
    /// <summary>7.1 surround audio with eight channels.</summary>
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
