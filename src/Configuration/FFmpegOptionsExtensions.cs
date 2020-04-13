// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace FFmpegDotnetWrapper.Configuration
{
    /// <summary>
    /// Extension methods for <see cref="FFmpegOptions"/> configuration.
    /// Provides convenient methods for common operations and validations.
    /// </summary>
    public static class FFmpegOptionsExtensions
    {
        /// <summary>
        /// Gets the effective FFmpeg path, falling back to auto-detection if not configured.
        /// </summary>
        /// <param name="options">The FFmpeg configuration options.</param>
        /// <returns>The FFmpeg executable path, or null if not available.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
        public static string? GetEffectiveFFmpegPath(this FFmpegOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            return string.IsNullOrWhiteSpace(options.FFmpegPath)
                ? FindFFmpegInPath()
                : options.FFmpegPath;
        }

        /// <summary>
        /// Gets the effective FFprobe path, falling back to auto-detection if not configured.
        /// </summary>
        /// <param name="options">The FFmpeg configuration options.</param>
        /// <returns>The FFprobe executable path, or null if not available.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
        public static string? GetEffectiveFFprobePath(this FFmpegOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            return string.IsNullOrWhiteSpace(options.FFprobePath)
                ? FindFFprobeInPath()
                : options.FFprobePath;
        }

        /// <summary>
        /// Determines if hardware acceleration is enabled and supported.
        /// </summary>
        /// <param name="options">The FFmpeg configuration options.</param>
        /// <returns>True if hardware acceleration is enabled; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
        public static bool IsHardwareAccelerationEnabled(this FFmpegOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            return options.EnableHardwareAcceleration &&
                   !string.IsNullOrWhiteSpace(options.EncodingPreset) &&
                   options.EncodingPreset.Contains("cuda", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the effective encoding preset for FFmpeg operations.
        /// </summary>
        /// <param name="options">The FFmpeg configuration options.</param>
        /// <returns>The encoding preset to use, or "medium" as default.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
        public static string GetEffectiveEncodingPreset(this FFmpegOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            return string.IsNullOrWhiteSpace(options.EncodingPreset)
                ? "medium"
                : options.EncodingPreset;
        }

        /// <summary>
        /// Gets the effective timeout for FFmpeg operations in milliseconds.
        /// </summary>
        /// <param name="options">The FFmpeg configuration options.</param>
        /// <returns>Timeout in milliseconds.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
        public static int GetTimeoutMilliseconds(this FFmpegOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            return options.OperationTimeoutSeconds * 1000;
        }

        /// <summary>
        /// Determines if concurrent operations are allowed based on configuration.
        /// </summary>
        /// <param name="options">The FFmpeg configuration options.</param>
        /// <returns>True if concurrent operations are allowed; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
        public static bool CanRunConcurrently(this FFmpegOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            return options.AllowConcurrentOperations &&
                   options.MaxConcurrentOperations >= 0;
        }

        /// <summary>
        /// Gets the maximum number of concurrent operations allowed.
        /// </summary>
        /// <param name="options">The FFmpeg configuration options.</param>
        /// <returns>Maximum concurrent operations allowed, or 0 for unlimited.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
        public static int GetMaxConcurrentOperations(this FFmpegOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            return options.MaxConcurrentOperations;
        }

        /// <summary>
        /// Determines if the specified format is supported by FFmpeg.
        /// </summary>
        /// <param name="options">The FFmpeg configuration options.</param>
        /// <param name="format">The format to check (e.g., "mp4", "webm").</param>
        /// <returns>True if the format is supported; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
        public static bool IsFormatSupported(this FFmpegOptions options, string format)
        {
            ArgumentNullException.ThrowIfNull(options);

            return !string.IsNullOrWhiteSpace(format) &&
                   options.SupportedFormats.Contains(format.Trim(), StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the list of all supported formats as a comma-separated string.
        /// </summary>
        /// <param name="options">The FFmpeg configuration options.</param>
        /// <returns>Comma-separated list of supported formats.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
        public static string GetSupportedFormatsString(this FFmpegOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            return string.Join(", ", options.SupportedFormats.OrderBy(f => f, StringComparer.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets the effective temporary directory path, falling back to system temp if not configured.
        /// </summary>
        /// <param name="options">The FFmpeg configuration options.</param>
        /// <returns>The temporary directory path.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
        public static string GetEffectiveTemporaryDirectory(this FFmpegOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            return string.IsNullOrWhiteSpace(options.TemporaryDirectory)
                ? System.IO.Path.GetTempPath()
                : options.TemporaryDirectory;
        }

        /// <summary>
        /// Determines if temporary files should be kept based on configuration.
        /// </summary>
        /// <param name="options">The FFmpeg configuration options.</param>
        /// <returns>True if temporary files should be kept; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
        public static bool ShouldKeepTemporaryFiles(this FFmpegOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            return options.KeepTemporaryFiles;
        }

        /// <summary>
        /// Gets the retry configuration as a tuple of attempts and delay.
        /// </summary>
        /// <param name="options">The FFmpeg configuration options.</param>
        /// <returns>Tuple containing retry attempts and delay in milliseconds.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
        public static (int Attempts, int DelayMs) GetRetryConfiguration(this FFmpegOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            return (options.RetryAttempts, options.RetryDelayMs);
        }

        /// <summary>
        /// Determines if verbose logging is enabled.
        /// </summary>
        /// <param name="options">The FFmpeg configuration options.</param>
        /// <returns>True if verbose logging is enabled; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
        public static bool IsVerboseLoggingEnabled(this FFmpegOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            return options.VerboseLogging;
        }

        /// <summary>
        /// Gets the default audio bitrate in kbps.
        /// </summary>
        /// <param name="options">The FFmpeg configuration options.</param>
        /// <returns>Default audio bitrate in kbps.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
        public static int GetDefaultAudioBitrate(this FFmpegOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            return options.DefaultAudioBitrate;
        }

        /// <summary>
        /// Gets the default video bitrate in kbps.
        /// </summary>
        /// <param name="options">The FFmpeg configuration options.</param>
        /// <returns>Default video bitrate in kbps, or 0 if auto.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
        public static int GetDefaultVideoBitrate(this FFmpegOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            return options.DefaultVideoBitrate;
        }

        /// <summary>
        /// Gets the default quality level for encoding.
        /// </summary>
        /// <param name="options">The FFmpeg configuration options.</param>
        /// <returns>Default quality level, or null if not configured.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
        public static int? GetDefaultQuality(this FFmpegOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            return options.DefaultQuality;
        }

        /// <summary>
        /// Determines if path validation is enabled.
        /// </summary>
        /// <param name="options">The FFmpeg configuration options.</param>
        /// <returns>True if path validation is enabled; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
        public static bool IsPathValidationEnabled(this FFmpegOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            return options.ValidatePaths;
        }

        /// <summary>
        /// Determines if output path validation is enabled.
        /// </summary>
        /// <param name="options">The FFmpeg configuration options.</param>
        /// <returns>True if output path validation is enabled; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
        public static bool IsOutputPathValidationEnabled(this FFmpegOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            return options.ValidateOutputPath;
        }

        // Helper methods for path detection
        private static string? FindFFmpegInPath()
        {
            var paths = Environment.GetEnvironmentVariable("PATH")?.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            var possibleNames = new[] { "ffmpeg", "ffmpeg.exe" };

            foreach (var path in paths)
            {
                if (string.IsNullOrWhiteSpace(path) || !System.IO.Directory.Exists(path))
                {
                    continue;
                }

                foreach (var name in possibleNames)
                {
                    var fullPath = System.IO.Path.Combine(path, name);
                    if (System.IO.File.Exists(fullPath))
                    {
                        return fullPath;
                    }
                }
            }

            return null;
        }

        private static string? FindFFprobeInPath()
        {
            var paths = Environment.GetEnvironmentVariable("PATH")?.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            var possibleNames = new[] { "ffprobe", "ffprobe.exe" };

            foreach (var path in paths)
            {
                if (string.IsNullOrWhiteSpace(path) || !System.IO.Directory.Exists(path))
                {
                    continue;
                }

                foreach (var name in possibleNames)
                {
                    var fullPath = System.IO.Path.Combine(path, name);
                    if (System.IO.File.Exists(fullPath))
                    {
                        return fullPath;
                    }
                }
            }

            return null;
        }
    }
}