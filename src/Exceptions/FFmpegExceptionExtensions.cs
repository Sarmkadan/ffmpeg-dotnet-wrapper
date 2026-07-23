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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static string ToDetailedErrorMessage(this FFmpegException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

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

        // Include CLI command from exception data if available
        if (exception.Data.Contains(nameof(Throw.WithCliContext)) && exception.Data[nameof(Throw.WithCliContext)] is string cliCommand)
        {
            sb.AppendLine($"Command: {cliCommand}");
        }

        return sb.ToString().Trim();
    }

    /// <summary>
    /// Determines whether the exception represents a process-related failure (FFmpegProcessException).
    /// </summary>
    /// <param name="exception">The FFmpeg exception to check.</param>
    /// <returns>True if the exception is a process-related failure; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static bool IsProcessFailure(this FFmpegException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception is FFmpegProcessException;
    }

    /// <summary>
    /// Determines whether the exception represents an invalid media file error (InvalidMediaFileException).
    /// </summary>
    /// <param name="exception">The FFmpeg exception to check.</param>
    /// <returns>True if the exception is an invalid media file error; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static bool IsInvalidMediaFileError(this FFmpegException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception is InvalidMediaFileException;
    }

    /// <summary>
    /// Determines whether the exception represents an invalid configuration error (InvalidOperationConfigurationException).
    /// </summary>
    /// <param name="exception">The FFmpeg exception to check.</param>
    /// <returns>True if the exception is an invalid configuration error; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static bool IsInvalidConfigurationError(this FFmpegException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception is InvalidOperationConfigurationException;
    }

    /// <summary>
    /// Determines whether the exception represents an unsupported operation error (UnsupportedOperationException).
    /// </summary>
    /// <param name="exception">The FFmpeg exception to check.</param>
    /// <returns>True if the exception is an unsupported operation error; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static bool IsUnsupportedOperationError(this FFmpegException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception is UnsupportedOperationException;
    }

    /// <summary>
    /// Gets the CLI command that was being executed when this exception occurred.
    /// Returns null if no CLI command context is available.
    /// </summary>
    /// <param name="exception">The FFmpeg exception to check.</param>
    /// <returns>The CLI command string if available; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static string? GetCliCommand(this FFmpegException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception.Data[nameof(Throw.WithCliContext)] as string;
    }

    /// <summary>
    /// Gets the exit code from the process execution that caused this exception.
    /// Returns null if no exit code is available.
    /// </summary>
    /// <param name="exception">The FFmpeg exception to check.</param>
    /// <returns>The exit code if available; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static int? GetExitCode(this FFmpegException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        if (exception.Data[nameof(Throw.WithCliContext)] is int exitCode)
        {
            return exitCode;
        }
        return exception.ExitCode;
    }

    /// <summary>
    /// Gets the error output from the process execution that caused this exception.
    /// Returns null if no error output is available.
    /// </summary>
    /// <param name="exception">The FFmpeg exception to check.</param>
    /// <returns>The error output if available; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static string? GetErrorOutput(this FFmpegException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception.ErrorOutput ?? exception.Data[nameof(Throw.WithCliContext)] as string;
    }

    /// <summary>
    /// Gets the configuration key that caused this configuration exception.
    /// Returns null if no configuration key is available.
    /// </summary>
    /// <param name="exception">The FFmpeg exception to check.</param>
    /// <returns>The configuration key if available; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static string? GetConfigurationKey(this FFmpegException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        if (exception is InvalidOperationConfigurationException configEx && !string.IsNullOrEmpty(configEx.ConfigurationKey))
        {
            return configEx.ConfigurationKey;
        }
        return exception.Data[nameof(Throw.WithConfigurationContext)] as string;
    }

    /// <summary>
    /// Gets the file path involved in this file operation exception.
    /// Returns null if no file path is available.
    /// </summary>
    /// <param name="exception">The FFmpeg exception to check.</param>
    /// <returns>The file path if available; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static string? GetFilePath(this FFmpegException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        if (exception is FileOperationException fileEx && !string.IsNullOrEmpty(fileEx.FilePath))
        {
            return fileEx.FilePath;
        }
        return exception.Data[nameof(Throw.WithFileContext)] as string;
    }

    /// <summary>
    /// Gets the repository name involved in this repository exception.
    /// Returns null if no repository name is available.
    /// </summary>
    /// <param name="exception">The FFmpeg exception to check.</param>
    /// <returns>The repository name if available; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static string? GetRepositoryName(this FFmpegException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        if (exception is RepositoryException repoEx && !string.IsNullOrEmpty(repoEx.RepositoryName))
        {
            return repoEx.RepositoryName;
        }
        return exception.Data[nameof(Throw.WithRepositoryContext)] as string;
    }

    /// <summary>
    /// Gets the service name involved in this service exception.
    /// Returns null if no service name is available.
    /// </summary>
    /// <param name="exception">The FFmpeg exception to check.</param>
    /// <returns>The service name if available; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static string? GetServiceName(this FFmpegException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        if (exception is ServiceException serviceEx && !string.IsNullOrEmpty(serviceEx.ServiceName))
        {
            return serviceEx.ServiceName;
        }
        return exception.Data[nameof(Throw.WithServiceContext)] as string;
    }

    /// <summary>
    /// Gets the media file path involved in this invalid media file exception.
    /// Returns null if no file path is available.
    /// </summary>
    /// <param name="exception">The FFmpeg exception to check.</param>
    /// <returns>The file path if available; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static string? GetMediaFilePath(this FFmpegException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        if (exception is InvalidMediaFileException mediaEx && !string.IsNullOrEmpty(mediaEx.FilePath))
        {
            return mediaEx.FilePath;
        }
        return exception.Data[nameof(Throw.WithMediaFileContext)] as string;
    }
}
