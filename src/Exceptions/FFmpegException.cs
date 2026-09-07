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

    /// <summary>
    /// Initializes a new instance of the <see cref="FFmpegException"/> class.
    /// </summary>
    public FFmpegException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FFmpegException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public FFmpegException(string message)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FFmpegException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public FFmpegException(string message, Exception innerException)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FFmpegException"/> class with a specified error message, process exit code and error output.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="exitCode">The process exit code associated with the error.</param>
    /// <param name="errorOutput">The error output from the process execution, if any.</param>
    public FFmpegException(string message, int exitCode, string? errorOutput = null)
        : base(message)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(exitCode);
        ExitCode = exitCode;
        ErrorOutput = errorOutput;
    }

    public override string ToString()
    {
        return $"FFmpegException {{ ExitCode = {ExitCode}, ErrorOutput = {ErrorOutput} }}";
    }
}

/// <summary>
/// Thrown when an invalid media file is provided or detected.
/// </summary>
public class InvalidMediaFileException : FFmpegException
{
    /// <summary>
/// Gets or sets the path to the invalid media file.
/// </summary>
public string? FilePath { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidMediaFileException"/> class.
    /// </summary>
    public InvalidMediaFileException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidMediaFileException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public InvalidMediaFileException(string message) : base(message)
    {
        if (string.IsNullOrEmpty(message))
        {
            throw new ArgumentException("Message cannot be null or empty", nameof(message));
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidMediaFileException"/> class with a specified error message and file path.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="filePath">The path to the invalid media file.</param>
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
    /// <summary>
/// Gets or sets the timeout value for the FFmpeg process.
/// </summary>
public TimeSpan? Timeout { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FFmpegProcessException"/> class.
    /// </summary>
    public FFmpegProcessException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FFmpegProcessException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public FFmpegProcessException(string message) : base(message)
    {
        if (string.IsNullOrEmpty(message))
        {
            throw new ArgumentException("Message cannot be null or empty", nameof(message));
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FFmpegProcessException"/> class with a specified error message and timeout value.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="timeout">The timeout value for the FFmpeg process.</param>
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
    /// <summary>
/// Gets or sets the configuration key that is invalid or incomplete.
/// </summary>
public string? ConfigurationKey { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidOperationConfigurationException"/> class.
    /// </summary>
    public InvalidOperationConfigurationException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidOperationConfigurationException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public InvalidOperationConfigurationException(string message) : base(message)
    {
        if (string.IsNullOrEmpty(message))
        {
            throw new ArgumentException("Message cannot be null or empty", nameof(message));
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidOperationConfigurationException"/> class with a specified error message and configuration key.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="configKey">The configuration key that is invalid or incomplete.</param>
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
    /// <summary>
    /// Initializes a new instance of the <see cref="UnsupportedOperationException"/> class.
    /// </summary>
    public UnsupportedOperationException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnsupportedOperationException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public UnsupportedOperationException(string message) : base(message)
    {
        if (string.IsNullOrEmpty(message))
        {
            throw new ArgumentException("Message cannot be null or empty", nameof(message));
        }
    }
}
