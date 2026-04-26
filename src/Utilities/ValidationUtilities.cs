// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FFmpegDotnetWrapper.Utilities
{
    /// <summary>
    /// Validation utility methods for common video-related validations.
    /// Provides checks for video codecs, bitrates, durations, and format compatibility.
    /// Used by API validation middleware and service layers to ensure data integrity.
    /// </summary>
    public static class ValidationUtilities
    {
        // Supported video codecs
        private static readonly HashSet<string> SupportedVideoCodecs = new(StringComparer.OrdinalIgnoreCase)
        {
            "h264", "h265", "hevc", "vp8", "vp9", "av1", "mpeg2", "mpeg4"
        };

        // Supported output formats
        private static readonly HashSet<string> SupportedFormats = new(StringComparer.OrdinalIgnoreCase)
        {
            "mp4", "mkv", "webm", "avi", "mov", "flv", "wmv", "3gp", "ts"
        };

        // Common video file extensions
        private static readonly HashSet<string> VideoFileExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            "mp4", "mkv", "avi", "mov", "flv", "wmv", "webm", "3gp", "ts",
            "m3u8", "mts", "m2ts", "ogv", "asf", "vob", "f4v", "mpg", "mpeg"
        };

        /// <summary>
        /// Validates a bitrate value is within acceptable ranges for video encoding.
        /// FFmpeg typically supports 1k to 50Mbps for practical use.
        /// </summary>
        public static bool IsValidBitrate(int bitratekbps)
        {
            return bitratekbps >= 1 && bitratekbps <= 50000; // 1k to 50Mbps
        }

        /// <summary>
        /// Validates a video codec is recognized and supported.
        /// Used to prevent invalid codec specifications from reaching FFmpeg.
        /// </summary>
        public static bool IsValidCodec(string? codec)
        {
            return !string.IsNullOrEmpty(codec) && SupportedVideoCodecs.Contains(codec);
        }

        /// <summary>
        /// Validates an output format is supported and recognized by FFmpeg.
        /// Prevents attempts to use invalid or unsupported container formats.
        /// </summary>
        public static bool IsValidOutputFormat(string? format)
        {
            return !string.IsNullOrEmpty(format) && SupportedFormats.Contains(format);
        }

        /// <summary>
        /// Validates a file is a video file based on extension.
        /// Note: Extension check only, not full format validation (use FFprobe for that).
        /// </summary>
        public static bool IsVideoFile(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            var extension = System.IO.Path.GetExtension(filePath)
                .TrimStart('.')
                .ToLowerInvariant();

            return VideoFileExtensions.Contains(extension);
        }

        /// <summary>
        /// Parses a time string in format "HH:MM:SS" or "SS" to total seconds.
        /// Returns null if format is invalid.
        /// </summary>
        public static double? ParseTimeToSeconds(string? timeString)
        {
            if (string.IsNullOrWhiteSpace(timeString))
                return null;

            // Try parsing as pure seconds
            if (double.TryParse(timeString.Trim(), out var seconds))
            {
                return seconds >= 0 ? seconds : null;
            }

            // Try parsing as HH:MM:SS
            var parts = timeString.Split(':');
            if (parts.Length == 3 &&
                int.TryParse(parts[0], out var hours) &&
                int.TryParse(parts[1], out var minutes) &&
                double.TryParse(parts[2], out var secs))
            {
                if (hours >= 0 && minutes >= 0 && minutes < 60 && secs >= 0 && secs < 60)
                {
                    return hours * 3600 + minutes * 60 + secs;
                }
            }

            return null;
        }

        /// <summary>
        /// Converts seconds to HH:MM:SS format for display.
        /// Used in logging and API responses for human-readable duration display.
        /// </summary>
        public static string FormatSecondsToTime(double seconds)
        {
            if (seconds < 0)
                seconds = 0;

            var timeSpan = TimeSpan.FromSeconds(seconds);
            return $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }

        /// <summary>
        /// Validates quality settings are within encoder-specific ranges.
        /// H264 typically uses 0-51 CRF, other codecs have different scales.
        /// </summary>
        public static bool IsValidQualitySetting(int quality, string? codec = null)
        {
            return quality >= 0 && quality <= 51; // Standard CRF range
        }

        /// <summary>
        /// Validates watermark position coordinates are reasonable.
        /// Allows for off-screen positioning but prevents extreme values.
        /// </summary>
        public static bool IsValidWatermarkPosition(int x, int y)
        {
            // Allow positioning slightly outside video bounds
            return x >= -4096 && x <= 4096 && y >= -2160 && y <= 2160;
        }

        /// <summary>
        /// Validates watermark scale is between 0.01 (1%) and 1.0 (100%).
        /// Prevents watermarks from being too small or larger than video.
        /// </summary>
        public static bool IsValidWatermarkScale(double scale)
        {
            return scale >= 0.01 && scale <= 1.0;
        }

        /// <summary>
        /// Validates opacity setting for watermark or other transparency effects.
        /// Returns true for values between 0.0 (transparent) and 1.0 (opaque).
        /// </summary>
        public static bool IsValidOpacity(double opacity)
        {
            return opacity >= 0.0 && opacity <= 1.0;
        }

        /// <summary>
        /// Validates a resolution string in format "WIDTHxHEIGHT".
        /// Common values: "1920x1080", "1280x720", "3840x2160", etc.
        /// </summary>
        public static bool IsValidResolution(string? resolution)
        {
            if (string.IsNullOrEmpty(resolution))
                return false;

            var match = Regex.Match(resolution, @"^(\d+)x(\d+)$");
            if (!match.Success)
                return false;

            if (int.TryParse(match.Groups[1].Value, out var width) &&
                int.TryParse(match.Groups[2].Value, out var height))
            {
                return width > 0 && width <= 7680 && height > 0 && height <= 4320;
            }

            return false;
        }

        /// <summary>
        /// Validates a frame rate value is realistic.
        /// Common values: 24, 25, 30, 50, 60 fps.
        /// Supports fractional rates like 23.976 (3000/1001).
        /// </summary>
        public static bool IsValidFrameRate(double fps)
        {
            return fps > 0 && fps <= 240; // Max 240 fps for extreme slow-mo
        }

        /// <summary>
        /// Validates an aspect ratio string in format "W:H".
        /// Common values: "16:9", "4:3", "21:9", etc.
        /// </summary>
        public static bool IsValidAspectRatio(string? ratio)
        {
            if (string.IsNullOrEmpty(ratio))
                return false;

            var parts = ratio.Split(':');
            if (parts.Length != 2)
                return false;

            if (decimal.TryParse(parts[0], out var w) &&
                decimal.TryParse(parts[1], out var h))
            {
                return w > 0 && h > 0;
            }

            return false;
        }

        /// <summary>
        /// Validates that trim times are logically consistent.
        /// Ensures start &lt; end, and start is not negative.
        /// </summary>
        public static bool ValidateTrimTimes(double? startSeconds, double? endSeconds, double? durationSeconds)
        {
            if (startSeconds.HasValue && startSeconds.Value < 0)
                return false;

            if (durationSeconds.HasValue && durationSeconds.Value <= 0)
                return false;

            if (startSeconds.HasValue && endSeconds.HasValue && startSeconds.Value >= endSeconds.Value)
                return false;

            // At least one of endSeconds or durationSeconds must be specified
            return endSeconds.HasValue || durationSeconds.HasValue;
        }

        /// <summary>
        /// Gets all supported codec names for API documentation and validation.
        /// Returns a copy to prevent external modification.
        /// </summary>
        public static IEnumerable<string> GetSupportedCodecs()
        {
            return SupportedVideoCodecs.ToList();
        }

        /// <summary>
        /// Gets all supported output formats for API documentation.
        /// Returns a copy to prevent external modification.
        /// </summary>
        public static IEnumerable<string> GetSupportedFormats()
        {
            return SupportedFormats.ToList();
        }
    }
}
