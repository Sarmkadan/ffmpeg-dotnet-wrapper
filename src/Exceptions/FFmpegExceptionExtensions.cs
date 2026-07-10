using System;
using System.Text;

namespace FFmpegDotnetWrapper.Exceptions;

/// <summary>
/// Provides extension methods for FFmpeg-related exceptions to enhance error handling and diagnostics.
/// </summary>
public static class FFmpegExceptionExtensions
{
    /// <summary>
    /// Creates a detailed error message that includes the exception type, message, exit code (if available),
    /// error output (if available), and file path (if available) for better debugging.
    /// </summary>
    /// <param name="exception">The FFmpeg exception to format.</param>
    /// <returns>A formatted error message string.</returns>
    public static string ToDetailedErrorMessage(this FFmpegException exception)
    {
        if (exception == null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        var sb = new StringBuilder();
        sb.AppendLine($"FFmpeg Error: {exception.GetType().Name}");
        sb.AppendLine($"Message: {exception.Message}");

        if (exception.ExitCode.HasValue)
        {
            sb.AppendLine($"Exit Code: {exception.ExitCode.Value}");
        }

        if (!string.IsNullOrEmpty(exception.ErrorOutput))
        {
            sb.AppendLine($"Error Output: {exception.ErrorOutput}");
        }

        if (exception is InvalidMediaFileException mediaEx && !string.IsNullOrEmpty(mediaEx.FilePath))
        {
            sb.AppendLine($"File Path: {mediaEx.FilePath}");
        }

        if (exception is InvalidOperationConfigurationException configEx && !string.IsNullOrEmpty(configEx.ConfigurationKey))
        {
            sb.AppendLine($"Configuration Key: {configEx.ConfigurationKey}");
        }

        if (exception is FFmpegProcessException processEx && processEx.Timeout.HasValue)
        {
            sb.AppendLine($"Timeout: {processEx.Timeout.Value.TotalSeconds} seconds");
        }

        return sb.ToString().Trim();
    }

    /// <summary>
    /// Determines whether the exception represents a process-related failure (FFmpegProcessException).
    /// </summary>
    /// <param name="exception">The FFmpeg exception to check.</param>
    /// <returns>True if the exception is a process-related failure; otherwise, false.</returns>
    public static bool IsProcessFailure(this FFmpegException exception)
    {
        return exception is FFmpegProcessException;
    }

    /// <summary>
    /// Determines whether the exception represents an invalid media file error (InvalidMediaFileException).
    /// </summary>
    /// <param name="exception">The FFmpeg exception to check.</param>
    /// <returns>True if the exception is an invalid media file error; otherwise, false.</returns>
    public static bool IsInvalidMediaFileError(this FFmpegException exception)
    {
        return exception is InvalidMediaFileException;
    }

    /// <summary>
    /// Determines whether the exception represents an invalid configuration error (InvalidOperationConfigurationException).
    /// </summary>
    /// <param name="exception">The FFmpeg exception to check.</param>
    /// <returns>True if the exception is an invalid configuration error; otherwise, false.</returns>
    public static bool IsInvalidConfigurationError(this FFmpegException exception)
    {
        return exception is InvalidOperationConfigurationException;
    }

    /// <summary>
    /// Determines whether the exception represents an unsupported operation error (UnsupportedOperationException).
    /// </summary>
    /// <param name="exception">The FFmpeg exception to check.</param>
    /// <returns>True if the exception is an unsupported operation error; otherwise, false.</returns>
    public static bool IsUnsupportedOperationError(this FFmpegException exception)
    {
        return exception is UnsupportedOperationException;
    }
}