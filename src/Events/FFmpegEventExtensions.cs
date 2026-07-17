// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace FFmpegDotnetWrapper.Events
{
    /// <summary>
    /// Extension methods for <see cref="FFmpegEvent"/> and its derived types.
    /// Provides utility methods for event filtering, formatting, and analysis.
    /// </summary>
    public static class FFmpegEventExtensions
    {
        /// <summary>
        /// Determines whether this event represents a successful operation.
        /// </summary>
        /// <param name="ffmpegEvent">The event to check</param>
        /// <returns>True if the event represents success; otherwise false</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ffmpegEvent"/> is null</exception>
        public static bool IsSuccess(this FFmpegEvent ffmpegEvent)
        {
            ArgumentNullException.ThrowIfNull(ffmpegEvent);

            return ffmpegEvent switch
            {
                OperationCompletedEvent => true,
                OperationStartedEvent => true,
                ProgressReportedEvent => true,
                _ => false
            };
        }

        /// <summary>
        /// Determines whether this event represents a failed operation.
        /// </summary>
        /// <param name="ffmpegEvent">The event to check</param>
        /// <returns>True if the event represents failure; otherwise false</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ffmpegEvent"/> is null</exception>
        public static bool IsFailure(this FFmpegEvent ffmpegEvent)
        {
            ArgumentNullException.ThrowIfNull(ffmpegEvent);

            return ffmpegEvent is OperationFailedEvent;
        }

        /// <summary>
        /// Gets the operation type from the event, or an empty string if not available.
        /// </summary>
        /// <param name="ffmpegEvent">The event to extract operation type from</param>
        /// <returns>Operation type string, or empty string if not available</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ffmpegEvent"/> is null</exception>
        public static string GetOperationType(this FFmpegEvent ffmpegEvent)
        {
            ArgumentNullException.ThrowIfNull(ffmpegEvent);

            return ffmpegEvent switch
            {
                OperationStartedEvent e => e.OperationType,
                OperationCompletedEvent e => e.OperationType,
                OperationFailedEvent e => e.OperationType,
                ProgressReportedEvent e => e.OperationType,
                _ => string.Empty
            };
        }

        /// <summary>
        /// Gets the input file path from the event, or null if not available.
        /// </summary>
        /// <param name="ffmpegEvent">The event to extract input file from</param>
        /// <returns>Input file path, or null if not available</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ffmpegEvent"/> is null</exception>
        public static string? GetInputFile(this FFmpegEvent ffmpegEvent)
        {
            ArgumentNullException.ThrowIfNull(ffmpegEvent);

            return ffmpegEvent switch
            {
                OperationStartedEvent e => e.InputFile.NullIfEmpty(),
                OperationCompletedEvent e => e.InputFile.NullIfEmpty(),
                OperationFailedEvent e => e.InputFile.NullIfEmpty(),
                _ => null
            };
        }

        /// <summary>
        /// Gets the output file path from the event, or null if not available.
        /// </summary>
        /// <param name="ffmpegEvent">The event to extract output file from</param>
        /// <returns>Output file path, or null if not available</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ffmpegEvent"/> is null</exception>
        public static string? GetOutputFile(this FFmpegEvent ffmpegEvent)
        {
            ArgumentNullException.ThrowIfNull(ffmpegEvent);

            return ffmpegEvent switch
            {
                OperationStartedEvent e => e.OutputFile.NullIfEmpty(),
                OperationCompletedEvent e => e.OutputFile.NullIfEmpty(),
                _ => null
            };
        }

        /// <summary>
        /// Gets the error message from the event if it represents a failure.
        /// </summary>
        /// <param name="ffmpegEvent">The event to extract error message from</param>
        /// <returns>Error message if available; otherwise null</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ffmpegEvent"/> is null</exception>
        public static string? GetErrorMessage(this FFmpegEvent ffmpegEvent)
        {
            ArgumentNullException.ThrowIfNull(ffmpegEvent);

            return ffmpegEvent switch
            {
                OperationFailedEvent e => e.ErrorMessage.NullIfEmpty(),
                _ => null
            };
        }

        /// <summary>
        /// Gets the progress percentage from the event if available.
        /// </summary>
        /// <param name="ffmpegEvent">The event to extract progress from</param>
        /// <returns>Progress percentage (0-100), or null if not available</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ffmpegEvent"/> is null</exception>
        public static double? GetProgressPercentage(this FFmpegEvent ffmpegEvent)
        {
            ArgumentNullException.ThrowIfNull(ffmpegEvent);

            return ffmpegEvent switch
            {
                ProgressReportedEvent e => e.ProgressPercentage,
                _ => null
            };
        }

        /// <summary>
        /// Gets the duration from the event if available.
        /// </summary>
        /// <param name="ffmpegEvent">The event to extract duration from</param>
        /// <returns>Duration if available; otherwise null</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ffmpegEvent"/> is null</exception>
        public static TimeSpan? GetDuration(this FFmpegEvent ffmpegEvent)
        {
            ArgumentNullException.ThrowIfNull(ffmpegEvent);

            return ffmpegEvent switch
            {
                OperationCompletedEvent e => e.Duration,
                ProgressReportedEvent e => e.ElapsedTime,
                _ => null
            };
        }

        /// <summary>
        /// Gets the output file size in bytes from the event if available.
        /// </summary>
        /// <param name="ffmpegEvent">The event to extract file size from</param>
        /// <returns>File size in bytes, or null if not available</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ffmpegEvent"/> is null</exception>
        public static long? GetOutputFileSize(this FFmpegEvent ffmpegEvent)
        {
            ArgumentNullException.ThrowIfNull(ffmpegEvent);

            return ffmpegEvent switch
            {
                OperationCompletedEvent e => e.OutputFileSize > 0 ? e.OutputFileSize : null,
                _ => null
            };
        }

        /// <summary>
        /// Gets the error code from the event if it represents a failure.
        /// </summary>
        /// <param name="ffmpegEvent">The event to extract error code from</param>
        /// <returns>Error code if available; otherwise null</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ffmpegEvent"/> is null</exception>
        public static string? GetErrorCode(this FFmpegEvent ffmpegEvent)
        {
            ArgumentNullException.ThrowIfNull(ffmpegEvent);

            return ffmpegEvent switch
            {
                OperationFailedEvent e => e.ErrorCode.NullIfEmpty(),
                _ => null
            };
        }

        /// <summary>
        /// Formats the event as a human-readable string for logging or display.
        /// </summary>
        /// <param name="ffmpegEvent">The event to format</param>
        /// <param name="includeTimestamp">Whether to include timestamp in output</param>
        /// <returns>Formatted event information</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ffmpegEvent"/> is null</exception>
        public static string ToLogString(this FFmpegEvent ffmpegEvent, bool includeTimestamp = true)
        {
            ArgumentNullException.ThrowIfNull(ffmpegEvent);

            var builder = new StringBuilder();

            if (includeTimestamp)
            {
                builder.Append('[').Append(ffmpegEvent.OccurredAt.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)).Append(']');
            }

            builder.Append("Event: ").Append(ffmpegEvent.GetType().Name);

            if (!string.IsNullOrEmpty(ffmpegEvent.EventId))
            {
                builder.Append(" | ID: ").Append(ffmpegEvent.EventId);
            }

            if (!string.IsNullOrEmpty(ffmpegEvent.CorrelationId))
            {
                builder.Append(" | Correlation: ").Append(ffmpegEvent.CorrelationId);
            }

            if (!string.IsNullOrEmpty(ffmpegEvent.Source))
            {
                builder.Append(" | Source: ").Append(ffmpegEvent.Source);
            }

            var operationType = ffmpegEvent.GetOperationType();
            if (!string.IsNullOrEmpty(operationType))
            {
                builder.Append(" | Operation: ").Append(operationType);
            }

            var inputFile = ffmpegEvent.GetInputFile();
            var outputFile = ffmpegEvent.GetOutputFile();

            if (!string.IsNullOrEmpty(inputFile))
            {
                builder.Append(" | Input: ").Append(inputFile);
            }

            if (!string.IsNullOrEmpty(outputFile))
            {
                builder.Append(" | Output: ").Append(outputFile);
            }

            var progress = ffmpegEvent.GetProgressPercentage();
            if (progress.HasValue)
            {
                builder.Append(" | Progress: ").Append(progress.Value.ToString("0.00", CultureInfo.InvariantCulture)).Append('%');
            }

            var duration = ffmpegEvent.GetDuration();
            if (duration.HasValue)
            {
                builder.Append(" | Duration: ").Append(duration.Value.TotalSeconds.ToString("0.00", CultureInfo.InvariantCulture)).Append('s');
            }

            var fileSize = ffmpegEvent.GetOutputFileSize();
            if (fileSize.HasValue)
            {
                builder.Append(" | Size: ").Append(fileSize.Value.ToString("N0", CultureInfo.InvariantCulture)).Append(" bytes");
            }

            var errorMessage = ffmpegEvent.GetErrorMessage();
            if (!string.IsNullOrEmpty(errorMessage))
            {
                builder.Append(" | Error: ").Append(errorMessage);
            }

            var errorCode = ffmpegEvent.GetErrorCode();
            if (!string.IsNullOrEmpty(errorCode))
            {
                builder.Append(" | Code: ").Append(errorCode);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Determines whether this event is related to a specific correlation ID.
        /// </summary>
        /// <param name="ffmpegEvent">The event to check</param>
        /// <param name="correlationId">The correlation ID to match</param>
        /// <returns>True if the event's correlation ID matches; otherwise false</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ffmpegEvent"/> is null</exception>
        public static bool HasCorrelationId(this FFmpegEvent ffmpegEvent, string correlationId)
        {
            ArgumentNullException.ThrowIfNull(ffmpegEvent);
            ArgumentException.ThrowIfNullOrEmpty(correlationId);

            return string.Equals(ffmpegEvent.CorrelationId, correlationId, StringComparison.Ordinal);
        }

        /// <summary>
        /// Gets all metadata values as a formatted dictionary string.
        /// </summary>
        /// <param name="ffmpegEvent">The event to extract metadata from</param>
        /// <returns>Formatted metadata string, or empty string if no metadata</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ffmpegEvent"/> is null</exception>
        public static string GetMetadataString(this FFmpegEvent ffmpegEvent)
        {
            ArgumentNullException.ThrowIfNull(ffmpegEvent);

            if (ffmpegEvent is OperationStartedEvent { Metadata: { } metadata } && metadata.Count > 0)
            {
                var metadataParts = metadata
                    .Select(kvp => $"{kvp.Key}={FormatMetadataValue(kvp.Value)}")
                    .ToList();
                return string.Join(", ", metadataParts);
            }

            return string.Empty;
        }

        private static string? NullIfEmpty(this string? value)
        {
            return string.IsNullOrEmpty(value) ? null : value;
        }

        private static string FormatMetadataValue(object? value)
        {
            return value switch
            {
                null => "null",
                string s => s,
                IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
                _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? "null"
            };
        }
    }
}