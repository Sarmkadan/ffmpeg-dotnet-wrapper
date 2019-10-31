// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FFmpegDotnetWrapper.Api.DTOs;

namespace FFmpegDotnetWrapper.Utilities
{
    /// <summary>
    /// Extension methods providing convenient operations on standard types.
    /// Adds utility methods to strings, collections, and other types.
    /// Improves code readability and reduces boilerplate throughout the library.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Safely appends command-line arguments to a command.
        /// Handles null or empty arguments gracefully and maintains proper spacing.
        /// Used in fluent FFmpeg command building.
        /// </summary>
        public static StringBuilder AppendArgument(this StringBuilder sb, string? argument)
        {
            if (!string.IsNullOrEmpty(argument))
            {
                if (sb.Length > 0)
                    sb.Append(' ');
                sb.Append(argument);
            }
            return sb;
        }

        /// <summary>
        /// Appends multiple arguments as a space-separated list.
        /// Skips null or empty arguments automatically.
        /// Useful for building complex FFmpeg command lines.
        /// </summary>
        public static StringBuilder AppendArguments(this StringBuilder sb, params string?[] arguments)
        {
            foreach (var arg in arguments.Where(a => !string.IsNullOrEmpty(a)))
            {
                sb.AppendArgument(arg);
            }
            return sb;
        }

        /// <summary>
        /// Checks if a string is null or whitespace.
        /// Cleaner alternative to string.IsNullOrWhiteSpace for fluent style.
        /// </summary>
        public static bool IsNullOrWhiteSpace(this string? value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// Checks if a string has actual content (not null, empty, or whitespace).
        /// Useful for guard clauses and validation checks.
        /// </summary>
        public static bool HasValue(this string? value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// Repeats a string a specified number of times.
        /// Example: "ab".Repeat(3) returns "ababab".
        /// </summary>
        public static string Repeat(this string value, int count)
        {
            if (count <= 0)
                return string.Empty;
            if (count == 1)
                return value;

            var sb = new StringBuilder(value.Length * count);
            for (int i = 0; i < count; i++)
            {
                sb.Append(value);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Joins collection items into a string with a specified separator.
        /// Generic alternative to string.Join for more fluent usage.
        /// </summary>
        public static string Join<T>(this IEnumerable<T> values, string separator = ", ")
        {
            return string.Join(separator, values);
        }

        /// <summary>
        /// Joins collection items using a selector function and separator.
        /// Allows transforming items before joining.
        /// </summary>
        public static string Join<T>(this IEnumerable<T> values, Func<T, string> selector, string separator = ", ")
        {
            return string.Join(separator, values.Select(selector));
        }

        /// <summary>
        /// Returns the item if the collection contains only one item, otherwise null.
        /// Useful for single-result validation.
        /// </summary>
        public static T? SingleOrNull<T>(this IEnumerable<T?> source) where T : class
        {
            return source.Where(x => x != null).SingleOrDefault();
        }

        /// <summary>
        /// Checks if the collection is null or contains no elements.
        /// </summary>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source)
        {
            return source == null || !source.Any();
        }

        /// <summary>
        /// Batches a collection into groups of specified size.
        /// Useful for processing large video file lists in chunks.
        /// </summary>
        public static IEnumerable<List<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
        {
            var batch = new List<T>(batchSize);
            foreach (var item in source)
            {
                batch.Add(item);
                if (batch.Count == batchSize)
                {
                    yield return batch;
                    batch = new List<T>(batchSize);
                }
            }

            if (batch.Count > 0)
                yield return batch;
        }

        /// <summary>
        /// Converts TimeSpan to total seconds with precision.
        /// Cleaner than accessing TotalSeconds property repeatedly.
        /// </summary>
        public static double ToSeconds(this TimeSpan timeSpan)
        {
            return timeSpan.TotalSeconds;
        }

        /// <summary>
        /// Converts TimeSpan to total milliseconds as long integer.
        /// Useful for timeout specifications in milliseconds.
        /// </summary>
        public static long ToMilliseconds(this TimeSpan timeSpan)
        {
            return (long)timeSpan.TotalMilliseconds;
        }

        /// <summary>
        /// Converts a timespan to a formatted string (HH:MM:SS).
        /// Shorthand for FormattingUtilities.FormatDuration.
        /// </summary>
        public static string FormatAsTime(this TimeSpan timeSpan)
        {
            return FormattingUtilities.FormatDuration(timeSpan);
        }

        /// <summary>
        /// Checks if a time string is in valid format and can be parsed.
        /// Returns the parsed seconds if valid, null otherwise.
        /// </summary>
        public static double? TryParseTime(this string? timeString)
        {
            return ValidationUtilities.ParseTimeToSeconds(timeString);
        }

        /// <summary>
        /// Converts a file size in bytes to human-readable format.
        /// </summary>
        public static string FormatAsSize(this long bytes)
        {
            return FormattingUtilities.FormatBytes(bytes);
        }

        /// <summary>
        /// Converts a bitrate in kbps to human-readable format.
        /// </summary>
        public static string FormatAsBitrate(this int kbps)
        {
            return FormattingUtilities.FormatBitrate(kbps);
        }

        /// <summary>
        /// Converts a file path to a properly formatted file name (basename without directory).
        /// </summary>
        public static string GetFileName(this string filePath)
        {
            return System.IO.Path.GetFileName(filePath);
        }

        /// <summary>
        /// Gets the directory path from a full file path.
        /// </summary>
        public static string GetDirectoryPath(this string filePath)
        {
            return System.IO.Path.GetDirectoryName(filePath) ?? string.Empty;
        }

        /// <summary>
        /// Gets the file extension without the dot (e.g., "mp4" instead of ".mp4").
        /// </summary>
        public static string GetFileExtension(this string filePath)
        {
            var ext = System.IO.Path.GetExtension(filePath);
            return ext.StartsWith(".") ? ext.Substring(1) : ext;
        }

        /// <summary>
        /// Adds a request ID to an API response if not already set.
        /// Ensures all responses have correlation IDs for tracking.
        /// </summary>
        public static ApiResponse<T> WithRequestId<T>(this ApiResponse<T> response, string requestId)
        {
            response.RequestId ??= requestId;
            return response;
        }

        /// <summary>
        /// Sets a custom timestamp on an API response.
        /// Used for testing or syncing with external time sources.
        /// </summary>
        public static ApiResponse<T> WithTimestamp<T>(this ApiResponse<T> response, DateTime timestamp)
        {
            response.Timestamp = timestamp;
            return response;
        }

        /// <summary>
        /// Adds a stack trace to an API response (for development environments only).
        /// Should never be used in production responses.
        /// </summary>
        public static ApiResponse<T> WithStackTrace<T>(this ApiResponse<T> response, string? stackTrace)
        {
            response.StackTrace = stackTrace;
            return response;
        }

        /// <summary>
        /// Ensures a collection is not null by returning an empty collection instead.
        /// Prevents null reference exceptions when iterating over potentially null collections.
        /// </summary>
        public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T>? source)
        {
            return source ?? Enumerable.Empty<T>();
        }

        /// <summary>
        /// Safely executes an action and returns a result, with fallback value on exception.
        /// Used for safe property access in LINQ queries.
        /// </summary>
        public static TResult? TryExecute<TResult>(this object? obj, Func<object, TResult> action, TResult? fallback = default)
        {
            if (obj == null)
                return fallback;

            try
            {
                return action(obj);
            }
            catch
            {
                return fallback;
            }
        }
    }
}
