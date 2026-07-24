// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace FFmpegDotnetWrapper.Exceptions;

/// <summary>
/// Thrown when file system operations fail, such as reading, writing, or accessing files.
/// Includes information about the file path that caused the error.
/// </summary>
public class FileOperationException : FFmpegException
{
    /// <summary>
    /// Gets the file path that caused this exception.
    /// </summary>
    public string? FilePath { get; set; }

    public FileOperationException(string message)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
    }

    public FileOperationException(string message, string filePath)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        FilePath = filePath;
        Context[nameof(FilePath)] = filePath ?? string.Empty;
    }

    public FileOperationException(string message, string filePath, Exception innerException)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        FilePath = filePath;
        Context[nameof(FilePath)] = filePath ?? string.Empty;
    }

    public FileOperationException(string message, Exception innerException)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
    }
}
