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
        Unknown,
        Transcode,
        Trim,
        Merge,
        Watermark,
        ExtractAudio,
        ExtractFrames,
        GenerateThumbnail,
        ResizeVideo,
        RotateVideo,
        FlipVideo,
        AdjustQuality,
        AddSubtitles,
        RemoveAudio,
        ChangeAspectRatio,
        CreatePlaylist
    }

    /// <summary>
    /// Enumeration of log levels for structured logging.
    /// </summary>
    public enum LogLevel
    {
        Trace,
        Debug,
        Information,
        Warning,
        Error,
        Critical,
        None
    }

    /// <summary>
    /// Enumeration of error codes for programmatic error handling.
    /// </summary>
    public enum ErrorCode
    {
        Success = 0,
        Unknown = -1,
        InputFileNotFound = 1000,
        OutputPathInvalid = 1001,
        UnsupportedFormat = 1002,
        InvalidArguments = 1003,
        OperationTimeout = 1004,
        InsufficientDiskSpace = 1005,
        PermissionDenied = 1006,
        FFmpegNotInstalled = 1007,
        FileIsLocked = 1008,
        InvalidCodec = 1009,
        RateLimitExceeded = 2000,
        ServiceUnavailable = 3000,
        InternalError = 9999
    }

    /// <summary>
    /// Operation state enumeration.
    /// </summary>
    public enum OperationState
    {
        Pending,
        Started,
        Processing,
        Paused,
        Completed,
        Failed,
        Cancelled
    }

    /// <summary>
    /// Constants related to codec support.
    /// </summary>
    public static class CodecConstants
    {
        public const string H264 = "h264";
        public const string H265 = "h265";
        public const string HEVC = "hevc";
        public const string VP8 = "vp8";
        public const string VP9 = "vp9";
        public const string AV1 = "av1";
        public const string MPEG2 = "mpeg2";

        public static readonly HashSet<string> SupportedVideoCodecs = new(StringComparer.OrdinalIgnoreCase)
        {
            H264, H265, HEVC, VP8, VP9, AV1, MPEG2
        };

        public const string AAC = "aac";
        public const string MP3 = "mp3";
        public const string OPUS = "opus";
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
        public const string MP4 = "mp4";
        public const string MKV = "mkv";
        public const string WEBM = "webm";
        public const string AVI = "avi";
        public const string MOV = "mov";
        public const string FLV = "flv";
        public const string TS = "ts";
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
        public const int VeryLow = 40;    // Poor quality, small file size
        public const int Low = 32;        // Lower quality
        public const int Medium = 23;     // Balanced quality (default)
        public const int High = 18;       // High quality
        public const int VeryHigh = 10;   // Very high quality, large file size
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
        public const int VideoLow = 1000;       // 1 Mbps
        public const int VideoMedium = 5000;    // 5 Mbps
        public const int VideoHigh = 10000;     // 10 Mbps
        public const int VideoVeryHigh = 25000; // 25 Mbps

        // Audio bitrates (Kbps)
        public const int AudioMono = 64;        // 64 Kbps
        public const int AudioStereo = 128;     // 128 Kbps
        public const int AudioHigh = 192;       // 192 Kbps
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
        public const string InputOption = "-i";
        public const string OutputFormat = "-f";
        public const string Codec = "-c:v";
        public const string AudioCodec = "-c:a";
        public const string Bitrate = "-b:v";
        public const string AudioBitrate = "-b:a";
        public const string CRF = "-crf";
        public const string Preset = "-preset";
        public const string FrameRate = "-r";
        public const string Resolution = "-s";
        public const string NoAudio = "-an";
        public const string NoVideo = "-vn";
        public const string Duration = "-t";
        public const string StartTime = "-ss";
        public const string Overwrite = "-y";
        public const string NoOverwrite = "-n";
        public const string Stats = "-stats";
        public const string HideLog = "-hide_banner";
        public const string ErrorLogLevel = "-loglevel error";
    }

    /// <summary>
    /// Constants related to temporary files and cleanup.
    /// </summary>
    public static class TempFileConstants
    {
        public const string TempFilePrefix = ".ffmpeg-";
        public const string TempFileExtension = ".tmp";
        public const string FFmpegTempDir = "ffmpeg-dotnet-temp";
        public const int MaxTempFileAge = 86400; // 24 hours in seconds
    }

    /// <summary>
    /// Constants related to timeouts and delays.
    /// </summary>
    public static class TimeoutConstants
    {
        public const int DefaultOperationTimeoutSeconds = 600; // 10 minutes
        public const int MaxOperationTimeoutSeconds = 3600;    // 1 hour
        public const int MinOperationTimeoutSeconds = 10;      // 10 seconds
        public const int ProbeTimeoutSeconds = 30;
        public const int WebhookTimeoutSeconds = 30;
        public const int HttpClientTimeoutSeconds = 60;
    }
}
