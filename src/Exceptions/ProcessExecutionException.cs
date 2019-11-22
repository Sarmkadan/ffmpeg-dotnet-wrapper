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
    public new int? ExitCode { get; set; }
    public new string? ErrorOutput { get; set; }

    public ProcessExecutionException(string message) : base(message)
    {
    }

    public ProcessExecutionException(string message, int exitCode) : base(message)
    {
        ExitCode = exitCode;
    }

    public ProcessExecutionException(string message, int exitCode, string errorOutput) : base(message, exitCode, errorOutput)
    {
        ExitCode = exitCode;
        ErrorOutput = errorOutput;
    }

    public ProcessExecutionException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public ProcessExecutionException(string message, int exitCode, string errorOutput, Exception innerException) : base(message, innerException)
    {
        ExitCode = exitCode;
        ErrorOutput = errorOutput;
    }
}
