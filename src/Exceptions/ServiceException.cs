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
    public string? ServiceName { get; set; }

    public ServiceException(string message) : base(message)
    {
    }

    public ServiceException(string message, string serviceName) : base(message)
    {
        ServiceName = serviceName;
    }

    public ServiceException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public ServiceException(string message, string serviceName, Exception innerException) : base(message, innerException)
    {
        ServiceName = serviceName;
    }

    public ServiceException(string message, int exitCode, string errorOutput) : base(message, exitCode, errorOutput)
    {
    }
}
