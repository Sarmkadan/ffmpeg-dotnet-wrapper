namespace FFmpegDotnetWrapper.Exceptions;

/// <summary>
/// Base exception for all FFmpeg-related errors in the wrapper.
/// </summary>
public class FFmpegException : Exception
{
    public int? ExitCode { get; set; }
    public string? ErrorOutput { get; set; }

    public FFmpegException()
    {
    }

    public FFmpegException(string message) : base(message)
    {
        if (string.IsNullOrEmpty(message))
        {
            throw new ArgumentException("Message cannot be null or empty", nameof(message));
        }
    }

    public FFmpegException(string message, Exception innerException)
        : base(message, innerException)
    {
        if (string.IsNullOrEmpty(message))
        {
            throw new ArgumentException("Message cannot be null or empty", nameof(message));
        }
    }

    public FFmpegException(string message, int exitCode, string? errorOutput = null)
        : base(message)
    {
        if (exitCode < 0)
        {
            throw new ArgumentException("Exit code cannot be less than 0", nameof(exitCode));
        }
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

    public InvalidMediaFileException()
    {
    }

    public InvalidMediaFileException(string message) : base(message)
    {
        if (string.IsNullOrEmpty(message))
        {
            throw new ArgumentException("Message cannot be null or empty", nameof(message));
        }
    }

    public InvalidMediaFileException(string message, string filePath)
        : base(message)
    {
        if (string.IsNullOrEmpty(message))
        {
            throw new ArgumentException("Message cannot be null or empty", nameof(message));
        }
        FilePath = filePath;
    }
}

/// <summary>
/// Thrown when FFmpeg process execution fails or times out.
/// </summary>
public class FFmpegProcessException : FFmpegException
{
    public TimeSpan? Timeout { get; set; }

    public FFmpegProcessException()
    {
    }

    public FFmpegProcessException(string message) : base(message)
    {
        if (string.IsNullOrEmpty(message))
        {
            throw new ArgumentException("Message cannot be null or empty", nameof(message));
        }
    }

    public FFmpegProcessException(string message, TimeSpan timeout)
        : base(message)
    {
        if (timeout < TimeSpan.Zero)
        {
            throw new ArgumentException("Timeout cannot be less than 0", nameof(timeout));
        }
        Timeout = timeout;
    }
}

/// <summary>
/// Thrown when operation configuration is invalid or incomplete.
/// </summary>
public class InvalidOperationConfigurationException : FFmpegException
{
    public string? ConfigurationKey { get; set; }

    public InvalidOperationConfigurationException()
    {
    }

    public InvalidOperationConfigurationException(string message) : base(message)
    {
        if (string.IsNullOrEmpty(message))
        {
            throw new ArgumentException("Message cannot be null or empty", nameof(message));
        }
    }

    public InvalidOperationConfigurationException(string message, string configKey)
        : base(message)
    {
        if (string.IsNullOrEmpty(message))
        {
            throw new ArgumentException("Message cannot be null or empty", nameof(message));
        }
        ConfigurationKey = configKey;
    }
}

/// <summary>
/// Thrown when an operation is not supported or not implemented.
/// </summary>
public class UnsupportedOperationException : FFmpegException
{
    public UnsupportedOperationException()
    {
    }

    public UnsupportedOperationException(string message) : base(message)
    {
        if (string.IsNullOrEmpty(message))
        {
            throw new ArgumentException("Message cannot be null or empty", nameof(message));
        }
    }
}
