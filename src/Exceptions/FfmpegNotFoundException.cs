// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace FFmpegDotnetWrapper.Exceptions;

/// <summary>
/// Thrown when the <c>ffmpeg</c> or <c>ffprobe</c> executable cannot be located, or when the
/// located <c>ffmpeg</c> binary cannot be launched and identified as FFmpeg.
/// </summary>
public class FfmpegNotFoundException : FFmpegException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FfmpegNotFoundException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains why the executable is unusable.</param>
    public FfmpegNotFoundException(string message)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FfmpegNotFoundException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The error message that explains why the executable is unusable.</param>
    /// <param name="innerException">The exception that caused the failure, if any.</param>
    public FfmpegNotFoundException(string message, Exception? innerException)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
    }
}
