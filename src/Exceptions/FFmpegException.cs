namespace FFmpegDotnetWrapper.Exceptions;

/// <summary>
/// Base exception for all FFmpeg-related errors in the wrapper.
/// </summary>
public class FFmpegException : Exception
{
    /// <summary>
    /// Gets the process exit code associated with this exception, if applicable.
    /// </summary>
    public int? ExitCode { get; set; }

    /// <summary>
    /// Gets the error output from the process execution, if applicable.
    /// </summary>
    public string? ErrorOutput { get; set; }

    /// <summary>
    /// Gets additional context information about the error.
    /// </summary>
    public Dictionary<string, string> Context { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public FFmpegException()
    {
    }

    public FFmpegException(string message)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
    }

    public FFmpegException(string message, Exception innerException)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
    }

    public FFmpegException(string message, int exitCode, string? errorOutput = null)
        : base(message)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(exitCode);
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
