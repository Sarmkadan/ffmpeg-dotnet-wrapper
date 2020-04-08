using System;
using System.IO;

namespace FFmpegDotnetWrapper.Exceptions
{
    /// <summary>
    /// Provides extension methods for <see cref="FileOperationException"/> to facilitate common file operation error handling scenarios.
    /// </summary>
    public static class FileOperationExceptionExtensions
    {
        /// <summary>
        /// Gets the file name component from the <see cref="FileOperationException.FilePath"/> property.
        /// </summary>
        /// <param name="ex">The exception instance.</param>
        /// <returns>The file name extracted from the path, or an empty string if the path is null, empty, or whitespace.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ex"/> is <see langword="null"/>.</exception>
        public static string GetFileName(this FileOperationException ex)
        {
            ArgumentNullException.ThrowIfNull(ex);

            return string.IsNullOrWhiteSpace(ex.FilePath)
                ? string.Empty
                : Path.GetFileName(ex.FilePath);
        }

        /// <summary>
        /// Determines whether the exception contains a non-empty file path.
        /// </summary>
        /// <param name="ex">The exception instance.</param>
        /// <returns><see langword="true"/> if the exception has a non-empty file path; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ex"/> is <see langword="null"/>.</exception>
        public static bool HasFilePath(this FileOperationException ex)
        {
            ArgumentNullException.ThrowIfNull(ex);
            return !string.IsNullOrWhiteSpace(ex.FilePath);
        }

        /// <summary>
        /// Formats the exception as a single loggable string, including the message and file path (if present).
        /// </summary>
        /// <param name="ex">The exception instance.</param>
        /// <returns>A formatted string suitable for logging purposes.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ex"/> is <see langword="null"/>.</exception>
        public static string ToLogString(this FileOperationException ex)
        {
            ArgumentNullException.ThrowIfNull(ex);

            return ex.HasFilePath()
                ? $"Error: {ex.Message} (File: {ex.FilePath})"
                : $"Error: {ex.Message}";
        }

        /// <summary>
        /// Creates a new <see cref="FileOperationException"/> with additional context appended to the original message.
        /// The original exception is preserved as the inner exception.
        /// </summary>
        /// <param name="ex">The original exception instance.</param>
        /// <param name="additionalInfo">Additional context information to append to the error message.</param>
        /// <returns>A new <see cref="FileOperationException"/> with combined message and preserved file path.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="ex"/> is <see langword="null"/>.
        /// <paramref name="additionalInfo"/> is <see langword="null"/>.
        /// </exception>
        public static FileOperationException WithAdditionalInfo(this FileOperationException ex, string additionalInfo)
        {
            ArgumentNullException.ThrowIfNull(ex);
            ArgumentNullException.ThrowIfNull(additionalInfo);

            var combinedMessage = $"{ex.Message} - {additionalInfo}";
            var filePath = ex.FilePath ?? string.Empty;
            return new FileOperationException(combinedMessage, filePath, ex);
        }
    }
}
