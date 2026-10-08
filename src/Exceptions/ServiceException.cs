// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace FFmpegDotnetWrapper.Exceptions;

/// <summary>
/// Thrown when a service-level error occurs during media processing operations.
/// This includes failures in FFmpeg execution, media analysis, and transcoding operations.
/// </summary>
public class ServiceException : FFmpegException
{
    /// <summary>
    /// Gets or sets the name of the service that caused this exception.
    /// </summary>
    public string? ServiceName { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public ServiceException(string message)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceException"/> class with a specified error message and service name.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="serviceName">The name of the service that caused the exception.</param>
    public ServiceException(string message, string serviceName)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        ServiceName = serviceName;
        Context[nameof(ServiceName)] = serviceName ?? string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public ServiceException(string message, Exception innerException)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceException"/> class with a specified error message, service name, and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="serviceName">The name of the service that caused the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public ServiceException(string message, string serviceName, Exception innerException)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        ServiceName = serviceName;
        Context[nameof(ServiceName)] = serviceName ?? string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceException"/> class with a specified error message, exit code, and error output.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="exitCode">The exit code of the FFmpeg process.</param>
    /// <param name="errorOutput">The error output from the FFmpeg process.</param>
    public ServiceException(string message, int exitCode, string errorOutput)
        : base(message, exitCode, errorOutput)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        Context[nameof(ExitCode)] = exitCode.ToString();
    }

    public override string ToString()
    {
        return !string.IsNullOrEmpty(ServiceName)
            ? $"[ServiceName: {ServiceName}] {base.ToString()}"
            : base.ToString();
    }
}
