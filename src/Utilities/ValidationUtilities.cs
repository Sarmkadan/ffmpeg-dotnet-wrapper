// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
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
        private const int MinimumBitrateKbps = 1;
        private const int MaximumBitrateKbps = 50000;
        private const int MinimumTimeComponent = 0;
        private const int MinutesPerHour = 60;
        private const int SecondsPerMinute = 60;
        private const int SecondsPerHour = 3600;
        private const int MinimumCrf = 0;
        private const int MaximumCrf = 51;
        private const int MinimumWatermarkXPosition = -4096;
        private const int MaximumWatermarkXPosition = 4096;
        private const int MinimumWatermarkYPosition = -2160;
        private const int MaximumWatermarkYPosition = 2160;
        private const double MinimumWatermarkScale = 0.01;
        private const double MaximumWatermarkScale = 1.0;
        private const double MinimumOpacity = 0.0;
        private const double MaximumOpacity = 1.0;
        private const int MinimumResolutionDimension = 0;
        private const int MaximumResolutionWidth = 7680;
        private const int MaximumResolutionHeight = 4320;
        private const double MinimumFrameRate = 0;
        private const double MaximumFrameRate = 240;
        private const decimal MinimumAspectRatioDimension = 0;
        private const double MinimumTrimTimeSeconds = 0;
        private const double MinimumTrimDurationSeconds = 0;

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
            return bitratekbps >= MinimumBitrateKbps && bitratekbps <= MaximumBitrateKbps; // 1k to 50Mbps
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
            if (double.TryParse(timeString.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds))
            {
                return seconds >= MinimumTimeComponent ? seconds : null;
            }

            // Try parsing as HH:MM:SS
            var parts = timeString.Split(':');
            if (parts.Length == 3 &&
                int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var hours) &&
                int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var minutes) &&
                double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var secs))
            {
                if (hours >= MinimumTimeComponent && minutes >= MinimumTimeComponent &&
                    minutes < MinutesPerHour && secs >= MinimumTimeComponent &&
                    secs < SecondsPerMinute)
                {
                    return hours * SecondsPerHour + minutes * SecondsPerMinute + secs;
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
            if (seconds < MinimumTimeComponent)
                seconds = MinimumTimeComponent;

            var timeSpan = TimeSpan.FromSeconds(seconds);
            return $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }

        /// <summary>
        /// Validates quality settings are within encoder-specific ranges.
        /// H264 typically uses 0-51 CRF, other codecs have different scales.
        /// </summary>
        public static bool IsValidQualitySetting(int quality, string? codec = null)
        {
            return quality >= MinimumCrf && quality <= MaximumCrf; // Standard CRF range
        }

        /// <summary>
        /// Validates watermark position coordinates are reasonable.
        /// Allows for off-screen positioning but prevents extreme values.
        /// </summary>
        public static bool IsValidWatermarkPosition(int x, int y)
        {
            // Allow positioning slightly outside video bounds
            return x >= MinimumWatermarkXPosition && x <= MaximumWatermarkXPosition &&
                y >= MinimumWatermarkYPosition && y <= MaximumWatermarkYPosition;
        }

        /// <summary>
        /// Validates watermark scale is between 0.01 (1%) and 1.0 (100%).
        /// Prevents watermarks from being too small or larger than video.
        /// </summary>
        public static bool IsValidWatermarkScale(double scale)
        {
            return scale >= MinimumWatermarkScale && scale <= MaximumWatermarkScale;
        }

        /// <summary>
        /// Validates opacity setting for watermark or other transparency effects.
        /// Returns true for values between 0.0 (transparent) and 1.0 (opaque).
        /// </summary>
        public static bool IsValidOpacity(double opacity)
        {
            return opacity >= MinimumOpacity && opacity <= MaximumOpacity;
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

            if (int.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var width) &&
                int.TryParse(match.Groups[2].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var height))
            {
                return width > MinimumResolutionDimension && width <= MaximumResolutionWidth &&
                    height > MinimumResolutionDimension && height <= MaximumResolutionHeight;
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
            return fps > MinimumFrameRate && fps <= MaximumFrameRate; // Max 240 fps for extreme slow-mo
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

            if (decimal.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var w) &&
                decimal.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var h))
            {
                return w > MinimumAspectRatioDimension && h > MinimumAspectRatioDimension;
            }

            return false;
        }

        /// <summary>
        /// Validates that trim times are logically consistent.
        /// Ensures start &lt; end, and start is not negative.
        /// </summary>
        public static bool ValidateTrimTimes(double? startSeconds, double? endSeconds, double? durationSeconds)
        {
            if (startSeconds.HasValue && startSeconds.Value < MinimumTrimTimeSeconds)
                return false;

            if (durationSeconds.HasValue && durationSeconds.Value <= MinimumTrimDurationSeconds)
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
