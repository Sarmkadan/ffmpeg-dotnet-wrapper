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
    /// Gets the name of the service that caused this exception.
    /// </summary>
    public string? ServiceName { get; set; }

    public ServiceException(string message)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
    }

    public ServiceException(string message, string serviceName)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        ServiceName = serviceName;
        Context[nameof(ServiceName)] = serviceName ?? string.Empty;
    }

    public ServiceException(string message, Exception innerException)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
    }

    public ServiceException(string message, string serviceName, Exception innerException)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        ServiceName = serviceName;
        Context[nameof(ServiceName)] = serviceName ?? string.Empty;
    }

    public ServiceException(string message, int exitCode, string errorOutput)
        : base(message, exitCode, errorOutput)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        Context[nameof(ExitCode)] = exitCode.ToString();
    }
}
