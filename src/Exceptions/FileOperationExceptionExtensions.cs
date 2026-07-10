using System;
using System.IO;

namespace FFmpegDotnetWrapper.Exceptions
{
    /// <summary>
    /// Extension methods for <see cref="FileOperationException"/>.
    /// </summary>
    public static class FileOperationExceptionExtensions
    {
        /// <summary>
        /// Returns the file name component of the <see cref="FileOperationException.FilePath"/>.
        /// If <see cref="FileOperationException.FilePath"/> is null or empty, returns an empty string.
        /// </summary>
        public static string GetFileName(this FileOperationException ex)
        {
            if (string.IsNullOrWhiteSpace(ex.FilePath))
                return string.Empty;

            return Path.GetFileName(ex.FilePath);
        }

        /// <summary>
        /// Indicates whether the exception contains a non‑empty file path.
        /// </summary>
        public static bool HasFilePath(this FileOperationException ex) =>
            !string.IsNullOrWhiteSpace(ex.FilePath);

        /// <summary>
        /// Formats the exception as a single loggable string, including the message and file path (if any).
        /// </summary>
        public static string ToLogString(this FileOperationException ex) =>
            $"Error: {ex.Message}{(ex.HasFilePath() ? $" (File: {ex.FilePath})" : string.Empty)}";

        /// <summary>
        /// Creates a new <see cref="FileOperationException"/> that appends additional context to the original message.
        /// The original exception is set as the inner exception.
        /// </summary>
        public static FileOperationException WithAdditionalInfo(this FileOperationException ex, string additionalInfo)
        {
            var combinedMessage = $"{ex.Message} - {additionalInfo}";
            // Preserve the original file path if present; otherwise pass an empty string.
            var filePath = ex.FilePath ?? string.Empty;
            return new FileOperationException(combinedMessage, filePath, ex);
        }
    }
}
