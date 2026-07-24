// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace FFmpegDotnetWrapper.Exceptions;

/// <summary>
/// Thrown when a process execution fails, including FFmpeg and FFprobe operations.
/// Contains information about the process exit code and error output.
/// </summary>
public class ProcessExecutionException : FFmpegException
{
    public ProcessExecutionException(string message)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
    }

    public ProcessExecutionException(string message, int exitCode)
        : base(message, exitCode)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        Context[nameof(ExitCode)] = exitCode.ToString();
    }

    public ProcessExecutionException(string message, int exitCode, string errorOutput)
        : base(message, exitCode, errorOutput)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        Context[nameof(ExitCode)] = exitCode.ToString();
        Context[nameof(ErrorOutput)] = errorOutput ?? string.Empty;
    }

    public ProcessExecutionException(string message, Exception innerException)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
    }

    public ProcessExecutionException(string message, int exitCode, string errorOutput, Exception innerException)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        ExitCode = exitCode;
        ErrorOutput = errorOutput;
        Context[nameof(ExitCode)] = exitCode.ToString();
        Context[nameof(ErrorOutput)] = errorOutput ?? string.Empty;
    }
}
