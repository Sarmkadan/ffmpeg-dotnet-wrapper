namespace FFmpegDotnetWrapper.Exceptions;

/// <summary>
/// Thrown when a process execution fails, including FFmpeg and FFprobe operations.
/// Contains information about the process exit code and error output.
/// </summary>
/// <remarks>
/// This exception is typically thrown when an external process (like ffmpeg or ffprobe) returns a non-zero exit code.
/// </remarks>
public class ProcessExecutionException : FFmpegException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProcessExecutionException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="message"/> is empty.</exception>
    public ProcessExecutionException(string message)
    : base(message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProcessExecutionException"/> class with a specified error message and process exit code.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="exitCode">The exit code of the failed process.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="message"/> is empty.</exception>
    public ProcessExecutionException(string message, int exitCode)
    : base(message, exitCode)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        Context[nameof(ExitCode)] = exitCode.ToString();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProcessExecutionException"/> class with a specified error message, process exit code, and error output.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="exitCode">The exit code of the failed process.</param>
    /// <param name="errorOutput">The error output of the failed process.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="message"/> is empty.</exception>
    public ProcessExecutionException(string message, int exitCode, string errorOutput)
    : base(message, exitCode, errorOutput)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        Context[nameof(ExitCode)] = exitCode.ToString();
        Context[nameof(ErrorOutput)] = errorOutput ?? string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProcessExecutionException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="message"/> is empty.</exception>
    public ProcessExecutionException(string message, Exception innerException)
    : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProcessExecutionException"/> class with a specified error message, process exit code, error output, and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="exitCode">The exit code of the failed process.</param>
    /// <param name="errorOutput">The error output of the failed process.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="message"/> is empty.</exception>
    public ProcessExecutionException(string message, int exitCode, string errorOutput, Exception innerException)
    : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        ExitCode = exitCode;
        ErrorOutput = errorOutput;
        Context[nameof(ExitCode)] = exitCode.ToString();
        Context[nameof(ErrorOutput)] = errorOutput ?? string.Empty;
    }

    public override string ToString() => $"ProcessExecutionException {{ ExitCode = {ExitCode}, ErrorOutput = {ErrorOutput} }}";
}
