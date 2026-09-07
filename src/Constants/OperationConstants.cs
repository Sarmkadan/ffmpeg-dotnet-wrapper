// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace FFmpegDotnetWrapper.Constants
{
    /// <summary>
    /// Enumeration of supported operation types.
    /// </summary>
    public enum OperationType
    {
        /// <summary>Represents an unknown operation type.</summary>
        Unknown,
        /// <summary>Represents a media transcoding operation.</summary>
        Transcode,
        /// <summary>Represents a media trimming operation.</summary>
        Trim,
        /// <summary>Represents a media merging operation.</summary>
        Merge,
        /// <summary>Represents a watermarking operation.</summary>
        Watermark,
        /// <summary>Represents an audio extraction operation.</summary>
        ExtractAudio,
        /// <summary>Represents a frame extraction operation.</summary>
        ExtractFrames,
        /// <summary>Represents a thumbnail generation operation.</summary>
        GenerateThumbnail,
        /// <summary>Represents a video resizing operation.</summary>
        ResizeVideo,
        /// <summary>Represents a video rotation operation.</summary>
        RotateVideo,
        /// <summary>Represents a video flipping operation.</summary>
        FlipVideo,
        /// <summary>Represents a quality adjustment operation.</summary>
        AdjustQuality,
        /// <summary>Represents an operation that adds subtitles.</summary>
        AddSubtitles,
        /// <summary>Represents an operation that removes audio.</summary>
        RemoveAudio,
        /// <summary>Represents an aspect ratio change operation.</summary>
        ChangeAspectRatio,
        /// <summary>Represents a playlist creation operation.</summary>
        CreatePlaylist
    }

    /// <summary>
    /// Enumeration of log levels for structured logging.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>Specifies trace-level logging.</summary>
        Trace,
        /// <summary>Specifies debug-level logging.</summary>
        Debug,
        /// <summary>Specifies informational logging.</summary>
        Information,
        /// <summary>Specifies warning-level logging.</summary>
        Warning,
        /// <summary>Specifies error-level logging.</summary>
        Error,
        /// <summary>Specifies critical-level logging.</summary>
        Critical,
        /// <summary>Disables logging.</summary>
        None
    }

    /// <summary>
    /// Enumeration of error codes for programmatic error handling.
    /// </summary>
    public enum ErrorCode
    {
        /// <summary>Indicates successful completion.</summary>
        Success = 0,
        /// <summary>Indicates an unknown error.</summary>
        Unknown = -1,
        /// <summary>Indicates that the input file was not found.</summary>
        InputFileNotFound = 1000,
        /// <summary>Indicates that the output path is invalid.</summary>
        OutputPathInvalid = 1001,
        /// <summary>Indicates that the media format is unsupported.</summary>
        UnsupportedFormat = 1002,
        /// <summary>Indicates that one or more arguments are invalid.</summary>
        InvalidArguments = 1003,
        /// <summary>Indicates that the operation timed out.</summary>
        OperationTimeout = 1004,
        /// <summary>Indicates that insufficient disk space is available.</summary>
        InsufficientDiskSpace = 1005,
        /// <summary>Indicates that access was denied.</summary>
        PermissionDenied = 1006,
        /// <summary>Indicates that FFmpeg is not installed.</summary>
        FFmpegNotInstalled = 1007,
        /// <summary>Indicates that a required file is locked.</summary>
        FileIsLocked = 1008,
        /// <summary>Indicates that the specified codec is invalid.</summary>
        InvalidCodec = 1009,
        /// <summary>Indicates that a rate limit was exceeded.</summary>
        RateLimitExceeded = 2000,
        /// <summary>Indicates that the service is unavailable.</summary>
        ServiceUnavailable = 3000,
        /// <summary>Indicates an internal error.</summary>
        InternalError = 9999
    }

    /// <summary>
    /// Operation state enumeration.
    /// </summary>
    public enum OperationState
    {
        /// <summary>Indicates that the operation is pending.</summary>
        Pending,
        /// <summary>Indicates that the operation has started.</summary>
        Started,
        /// <summary>Indicates that the operation is processing.</summary>
        Processing,
        /// <summary>Indicates that the operation is paused.</summary>
        Paused,
        /// <summary>Indicates that the operation completed successfully.</summary>
        Completed,
        /// <summary>Indicates that the operation failed.</summary>
        Failed,
        /// <summary>Indicates that the operation was cancelled.</summary>
        Cancelled
    }

    /// <summary>
    /// Constants related to codec support.
    /// </summary>
    public static class CodecConstants
    {
        /// <summary>The H.264 video codec identifier.</summary>
        public const string H264 = "h264";
        /// <summary>The H.265 video codec identifier.</summary>
        public const string H265 = "h265";
        /// <summary>The HEVC video codec identifier.</summary>
        public const string HEVC = "hevc";
        /// <summary>The VP8 video codec identifier.</summary>
        public const string VP8 = "vp8";
        /// <summary>The VP9 video codec identifier.</summary>
        public const string VP9 = "vp9";
        /// <summary>The AV1 video codec identifier.</summary>
        public const string AV1 = "av1";
        /// <summary>The MPEG-2 video codec identifier.</summary>
        public const string MPEG2 = "mpeg2";

        public static readonly HashSet<string> SupportedVideoCodecs = new(StringComparer.OrdinalIgnoreCase)
        {
            H264, H265, HEVC, VP8, VP9, AV1, MPEG2
        };

        /// <summary>The AAC audio codec identifier.</summary>
        public const string AAC = "aac";
        /// <summary>The MP3 audio codec identifier.</summary>
        public const string MP3 = "mp3";
        /// <summary>The Opus audio codec identifier.</summary>
        public const string OPUS = "opus";
        /// <summary>The Vorbis audio codec identifier.</summary>
        public const string VORBIS = "vorbis";

        public static readonly HashSet<string> SupportedAudioCodecs = new(StringComparer.OrdinalIgnoreCase)
        {
            AAC, MP3, OPUS, VORBIS
        };
    }

    /// <summary>
    /// Constants related to output formats.
    /// </summary>
    public static class FormatConstants
    {
        /// <summary>The MP4 container format identifier.</summary>
        public const string MP4 = "mp4";
        /// <summary>The Matroska container format identifier.</summary>
        public const string MKV = "mkv";
        /// <summary>The WebM container format identifier.</summary>
        public const string WEBM = "webm";
        /// <summary>The AVI container format identifier.</summary>
        public const string AVI = "avi";
        /// <summary>The QuickTime container format identifier.</summary>
        public const string MOV = "mov";
        /// <summary>The Flash Video container format identifier.</summary>
        public const string FLV = "flv";
        /// <summary>The MPEG transport stream format identifier.</summary>
        public const string TS = "ts";
        /// <summary>The M3U8 playlist format identifier.</summary>
        public const string M3U8 = "m3u8";

        public static readonly HashSet<string> SupportedFormats = new(StringComparer.OrdinalIgnoreCase)
        {
            MP4, MKV, WEBM, AVI, MOV, FLV, TS, M3U8
        };
    }

    /// <summary>
    /// Constants related to quality presets.
    /// </summary>
    public static class QualityPresets
    {
        /// <summary>The constant rate factor for very low quality.</summary>
        public const int VeryLow = 40;    // Poor quality, small file size
        /// <summary>The constant rate factor for low quality.</summary>
        public const int Low = 32;        // Lower quality
        /// <summary>The constant rate factor for medium quality.</summary>
        public const int Medium = 23;     // Balanced quality (default)
        /// <summary>The constant rate factor for high quality.</summary>
        public const int High = 18;       // High quality
        /// <summary>The constant rate factor for very high quality.</summary>
        public const int VeryHigh = 10;   // Very high quality, large file size
        /// <summary>The constant rate factor for lossless encoding.</summary>
        public const int Lossless = 0;    // Lossless encoding

        public static string GetPresetName(int crf)
        {
            return crf switch
            {
                >= 40 => "Very Low",
                >= 32 => "Low",
                >= 23 => "Medium",
                >= 18 => "High",
                >= 10 => "Very High",
                >= 0 => "Lossless",
                _ => "Unknown"
            };
        }
    }

    /// <summary>
    /// Constants related to bitrate.
    /// </summary>
    public static class BitrateConstants
    {
        // Video bitrates (Kbps)
        /// <summary>The low video bitrate in kilobits per second.</summary>
        public const int VideoLow = 1000;       // 1 Mbps
        /// <summary>The medium video bitrate in kilobits per second.</summary>
        public const int VideoMedium = 5000;    // 5 Mbps
        /// <summary>The high video bitrate in kilobits per second.</summary>
        public const int VideoHigh = 10000;     // 10 Mbps
        /// <summary>The very high video bitrate in kilobits per second.</summary>
        public const int VideoVeryHigh = 25000; // 25 Mbps

        // Audio bitrates (Kbps)
        /// <summary>The mono audio bitrate in kilobits per second.</summary>
        public const int AudioMono = 64;        // 64 Kbps
        /// <summary>The stereo audio bitrate in kilobits per second.</summary>
        public const int AudioStereo = 128;     // 128 Kbps
        /// <summary>The high audio bitrate in kilobits per second.</summary>
        public const int AudioHigh = 192;       // 192 Kbps
        /// <summary>The high-definition audio bitrate in kilobits per second.</summary>
        public const int AudioHD = 320;         // 320 Kbps

        // Common resolution bitrate recommendations
        public static int GetRecommendedBitrate(int width, int height)
        {
            var pixelCount = (long)width * height;

            return pixelCount switch
            {
                <= 1920 * 1080 => 5000,      // HD: 5 Mbps
                <= 3840 * 2160 => 15000,     // 4K: 15 Mbps
                <= 7680 * 4320 => 50000,     // 8K: 50 Mbps
                _ => 25000                    // Default: 25 Mbps
            };
        }
    }

    /// <summary>
    /// Constants related to FFmpeg command-line options.
    /// </summary>
    public static class FFmpegCommandConstants
    {
        /// <summary>The FFmpeg input option.</summary>
        public const string InputOption = "-i";
        /// <summary>The FFmpeg output format option.</summary>
        public const string OutputFormat = "-f";
        /// <summary>The FFmpeg video codec option.</summary>
        public const string Codec = "-c:v";
        /// <summary>The FFmpeg audio codec option.</summary>
        public const string AudioCodec = "-c:a";
        /// <summary>The FFmpeg video bitrate option.</summary>
        public const string Bitrate = "-b:v";
        /// <summary>The FFmpeg audio bitrate option.</summary>
        public const string AudioBitrate = "-b:a";
        /// <summary>The FFmpeg constant rate factor option.</summary>
        public const string CRF = "-crf";
        /// <summary>The FFmpeg encoding preset option.</summary>
        public const string Preset = "-preset";
        /// <summary>The FFmpeg frame rate option.</summary>
        public const string FrameRate = "-r";
        /// <summary>The FFmpeg resolution option.</summary>
        public const string Resolution = "-s";
        /// <summary>The FFmpeg option that disables audio output.</summary>
        public const string NoAudio = "-an";
        /// <summary>The FFmpeg option that disables video output.</summary>
        public const string NoVideo = "-vn";
        /// <summary>The FFmpeg duration option.</summary>
        public const string Duration = "-t";
        /// <summary>The FFmpeg start time option.</summary>
        public const string StartTime = "-ss";
        /// <summary>The FFmpeg option that enables output overwriting.</summary>
        public const string Overwrite = "-y";
        /// <summary>The FFmpeg option that disables output overwriting.</summary>
        public const string NoOverwrite = "-n";
        /// <summary>The FFmpeg option that displays encoding statistics.</summary>
        public const string Stats = "-stats";
        /// <summary>The FFmpeg option that hides the startup banner.</summary>
        public const string HideLog = "-hide_banner";
        /// <summary>The FFmpeg option that limits logging to errors.</summary>
        public const string ErrorLogLevel = "-loglevel error";
    }

    /// <summary>
    /// Constants related to temporary files and cleanup.
    /// </summary>
    public static class TempFileConstants
    {
        /// <summary>The prefix used for temporary FFmpeg files.</summary>
        public const string TempFilePrefix = ".ffmpeg-";
        /// <summary>The extension used for temporary FFmpeg files.</summary>
        public const string TempFileExtension = ".tmp";
        /// <summary>The name of the FFmpeg temporary directory.</summary>
        public const string FFmpegTempDir = "ffmpeg-dotnet-temp";
        /// <summary>The maximum temporary file age in seconds.</summary>
        public const int MaxTempFileAge = 86400; // 24 hours in seconds
    }

    /// <summary>
    /// Constants related to timeouts and delays.
    /// </summary>
    public static class TimeoutConstants
    {
        /// <summary>The default operation timeout in seconds.</summary>
        public const int DefaultOperationTimeoutSeconds = 600; // 10 minutes
        /// <summary>The maximum operation timeout in seconds.</summary>
        public const int MaxOperationTimeoutSeconds = 3600;    // 1 hour
        /// <summary>The minimum operation timeout in seconds.</summary>
        public const int MinOperationTimeoutSeconds = 10;      // 10 seconds
        /// <summary>The media probe timeout in seconds.</summary>
        public const int ProbeTimeoutSeconds = 30;
        /// <summary>The webhook request timeout in seconds.</summary>
        public const int WebhookTimeoutSeconds = 30;
        /// <summary>The HTTP client timeout in seconds.</summary>
        public const int HttpClientTimeoutSeconds = 60;
    }
}
