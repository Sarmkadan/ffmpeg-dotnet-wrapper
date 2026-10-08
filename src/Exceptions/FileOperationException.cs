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

    /// <summary>
    /// Initializes a new instance of the <see cref="FileOperationException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public FileOperationException(string message)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileOperationException"/> class with a specified error message and the file path that caused the error.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="filePath">The file path that caused the error.</param>
    public FileOperationException(string message, string filePath)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        ArgumentNullException.ThrowIfNull(filePath);
        FilePath = filePath;
        Context[nameof(FilePath)] = filePath;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileOperationException"/> class with a specified error message, the file path that caused the error, and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="filePath">The file path that caused the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public FileOperationException(string message, string filePath, Exception innerException)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        ArgumentNullException.ThrowIfNull(filePath);
        ArgumentNullException.ThrowIfNull(innerException);
        FilePath = filePath;
        Context[nameof(FilePath)] = filePath;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileOperationException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public FileOperationException(string message, Exception innerException)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        ArgumentNullException.ThrowIfNull(innerException);
    }

    public override string ToString()
    {
        return !string.IsNullOrEmpty(FilePath)
            ? $"[FilePath: {FilePath}] {base.ToString()}"
            : base.ToString();
    }
}