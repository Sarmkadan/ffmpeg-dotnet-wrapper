// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace FFmpegDotnetWrapper.Exceptions;

/// <summary>
/// Base exception for all FFmpeg-related errors in the wrapper.
/// </summary>
public class FFmpegException : Exception
{
    public int? ExitCode { get; set; }
    public string? ErrorOutput { get; set; }

    public FFmpegException(string message) : base(message)
    {
    }

    public FFmpegException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public FFmpegException(string message, int exitCode, string? errorOutput = null)
        : base(message)
    {
        ExitCode = exitCode;
        ErrorOutput = errorOutput;
    }
}

/// <summary>
/// Thrown when an invalid media file is provided or detected.
/// </summary>
public class InvalidMediaFileException : FFmpegException
{
    public string? FilePath { get; set; }

    public InvalidMediaFileException(string message) : base(message)
    {
    }

    public InvalidMediaFileException(string message, string filePath)
        : base(message)
    {
        FilePath = filePath;
    }
}

/// <summary>
/// Thrown when FFmpeg process execution fails or times out.
/// </summary>
public class FFmpegProcessException : FFmpegException
{
    public TimeSpan? Timeout { get; set; }

    public FFmpegProcessException(string message) : base(message)
    {
    }

    public FFmpegProcessException(string message, TimeSpan timeout)
        : base(message)
    {
        Timeout = timeout;
    }
}

/// <summary>
/// Thrown when operation configuration is invalid or incomplete.
/// </summary>
public class InvalidOperationConfigurationException : FFmpegException
{
    public string? ConfigurationKey { get; set; }

    public InvalidOperationConfigurationException(string message) : base(message)
    {
    }

    public InvalidOperationConfigurationException(string message, string configKey)
        : base(message)
    {
        ConfigurationKey = configKey;
    }
}

/// <summary>
/// Thrown when an operation is not supported or not implemented.
/// </summary>
public class UnsupportedOperationException : FFmpegException
{
    public UnsupportedOperationException(string message) : base(message)
    {
    }
}
