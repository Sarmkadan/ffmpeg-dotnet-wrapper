// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Globalization;
using System.Text;

namespace FFmpegDotnetWrapper.Utilities
{
    /// <summary>
    /// String formatting and output utilities for consistent formatting across the library.
    /// Handles time formatting, byte size formatting, and command-line output formatting.
    /// Used by logging, CLI, and API response formatting.
    /// </summary>
    public static class FormattingUtilities
    {
        /// <summary>
        /// Formats a timespan into a human-readable duration string (HH:MM:SS format).
        /// Used in logging and progress reporting for FFmpeg operations.
        /// </summary>
        public static string FormatDuration(TimeSpan timeSpan)
        {
            if (timeSpan.TotalHours >= 1)
            {
                return $"{(int)timeSpan.TotalHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            }
            else if (timeSpan.TotalMinutes >= 1)
            {
                return $"00:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            }
            else
            {
                return $"00:00:{timeSpan.Seconds:D2}";
            }
        }

        /// <summary>
        /// Formats byte size into human-readable format (B, KB, MB, GB, TB).
        /// Used for displaying file sizes in logs and API responses.
        /// </summary>
        public static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len.ToString("0.##", CultureInfo.InvariantCulture)} {sizes[order]}";
        }

        /// <summary>
        /// Formats bitrate in kbps to human-readable format (Kbps, Mbps, Gbps).
        /// Used in logging and API responses for bitrate information.
        /// </summary>
        public static string FormatBitrate(int kbps)
        {
            if (kbps >= 1000000)
            {
                return $"{((double)kbps / 1000000).ToString("0.##", CultureInfo.InvariantCulture)} Gbps";
            }
            else if (kbps >= 1000)
            {
                return $"{((double)kbps / 1000).ToString("0.##", CultureInfo.InvariantCulture)} Mbps";
            }
            else
            {
                return $"{kbps} Kbps";
            }
        }

        /// <summary>
        /// Formats an FFmpeg command line for display in logs.
        /// Masks sensitive paths and creates readable multi-line output.
        /// Useful for debugging but should mask sensitive information.
        /// </summary>
        public static string FormatFFmpegCommand(string executable, string arguments)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{executable} \\");

            var args = arguments.Split(' ');
            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];

                // Mask file paths (replace with placeholders)
                if (arg.EndsWith(".mp4") || arg.EndsWith(".mkv") || arg.EndsWith(".avi"))
                {
                    arg = $"<{System.IO.Path.GetFileName(arg)}>";
                }

                sb.Append("  ");
                sb.Append(arg);

                if (i < args.Length - 1)
                {
                    sb.AppendLine(" \\");
                }
                else
                {
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Parses FFmpeg progress output to extract current processing information.
        /// Extracts frame count, bitrate, speed, and time processed.
        /// Used for progress tracking and ETA calculation.
        /// </summary>
        public static string ExtractProgressSummary(string ffmpegOutput)
        {
            var lines = ffmpegOutput.Split('\n');
            var lastProgressLine = lines.FirstOrDefault(l => l.Contains("frame=")) ?? "";

            if (string.IsNullOrEmpty(lastProgressLine))
                return "Starting...";

            var sb = new StringBuilder();

            // Extract frame count
            var frameMatch = System.Text.RegularExpressions.Regex.Match(lastProgressLine, @"frame=\s*(\d+)");
            if (frameMatch.Success)
            {
                sb.Append($"Frame: {frameMatch.Groups[1].Value} | ");
            }

            // Extract speed
            var speedMatch = System.Text.RegularExpressions.Regex.Match(lastProgressLine, @"speed=\s*([\d.]+)x");
            if (speedMatch.Success)
            {
                sb.Append($"Speed: {speedMatch.Groups[1].Value}x | ");
            }

            // Extract FPS
            var fpsMatch = System.Text.RegularExpressions.Regex.Match(lastProgressLine, @"fps=\s*([\d.]+)");
            if (fpsMatch.Success)
            {
                sb.Append($"FPS: {fpsMatch.Groups[1].Value} | ");
            }

            // Extract bitrate
            var bitrateMatch = System.Text.RegularExpressions.Regex.Match(lastProgressLine, @"bitrate=\s*([\d.]+)(\w+)");
            if (bitrateMatch.Success)
            {
                sb.Append($"Bitrate: {bitrateMatch.Groups[1].Value}{bitrateMatch.Groups[2].Value}");
            }

            return sb.ToString().TrimEnd('|', ' ');
        }

        /// <summary>
        /// Formats elapsed and remaining time for progress display.
        /// Shows "2:15:30 / 5:45:00" format for progress reporting.
        /// </summary>
        public static string FormatProgressTime(TimeSpan elapsed, TimeSpan estimated)
        {
            return $"{FormatDuration(elapsed)} / {FormatDuration(estimated)}";
        }

        /// <summary>
        /// Calculates and formats ETA (Estimated Time to Arrival) for operation completion.
        /// Returns estimated completion time based on progress and elapsed time.
        /// </summary>
        public static string FormatETA(TimeSpan elapsed, double progressPercentage)
        {
            if (progressPercentage <= 0)
                return "Calculating...";

            var totalSeconds = (elapsed.TotalSeconds / progressPercentage) * 100;
            var remainingSeconds = totalSeconds - elapsed.TotalSeconds;

            if (remainingSeconds < 0)
                remainingSeconds = 0;

            var remainingTimespan = TimeSpan.FromSeconds(remainingSeconds);
            return $"~{FormatDuration(remainingTimespan)} remaining";
        }

        /// <summary>
        /// Formats a timestamp for consistent logging format (ISO 8601).
        /// Used in log messages for better searchability and machine readability.
        /// </summary>
        public static string FormatTimestamp(DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

        /// <summary>
        /// Formats resolution (width x height) to standard format.
        /// Ensures consistent formatting across API responses and logs.
        /// </summary>
        public static string FormatResolution(int width, int height)
        {
            return $"{width}x{height}";
        }

        /// <summary>
        /// Formats a percentage with one decimal place.
        /// Used for displaying progress percentages consistently.
        /// </summary>
        public static string FormatPercentage(double percentage)
        {
            return $"{percentage.ToString("0.0", CultureInfo.InvariantCulture)}%";
        }

        /// <summary>
        /// Truncates a string to a maximum length and adds ellipsis if needed.
        /// Used for displaying long file paths in logs without line wrapping.
        /// </summary>
        public static string TruncateString(string? input, int maxLength = 80)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            if (input.Length <= maxLength)
                return input;

            return input.Substring(0, maxLength - 3) + "...";
        }

        /// <summary>
        /// Sanitizes a string for safe display in console output.
        /// Removes or escapes control characters and invalid UTF-8 sequences.
        /// </summary>
        public static string SanitizeForDisplay(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            var sb = new StringBuilder();
            foreach (var c in input)
            {
                if (char.IsControl(c) && c != '\n' && c != '\r')
                {
                    // Skip control characters except newlines
                    continue;
                }

                sb.Append(c);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts a kebab-case string to Title Case for display.
        /// Example: "output-format" becomes "Output Format".
        /// </summary>
        public static string TitleCase(string input)
        {
            var parts = input.Split('-', '_');
            var titleParts = parts.Select(p =>
                p.Length > 0 ? char.ToUpperInvariant(p[0]) + (p.Length > 1 ? p.Substring(1) : "") : ""
            );
            return string.Join(" ", titleParts);
        }
    }
}
