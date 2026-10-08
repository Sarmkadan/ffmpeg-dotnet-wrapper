// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace FFmpegDotnetWrapper.Exceptions;

/// <summary>
/// Thrown when repository operations fail, such as database access, file storage, or cache operations.
/// </summary>
public class RepositoryException : FFmpegException
{
    /// <summary>
    /// Gets the name of the repository that caused this exception.
    /// </summary>
    public string? RepositoryName { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public RepositoryException(string message)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryException"/> class with a specified error message and repository name.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="repositoryName">The name of the repository that caused this exception.</param>
    public RepositoryException(string message, string repositoryName)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        RepositoryName = repositoryName;
        Context[nameof(RepositoryName)] = repositoryName ?? string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public RepositoryException(string message, Exception innerException)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryException"/> class with a specified error message, repository name, and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="repositoryName">The name of the repository that caused this exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public RepositoryException(string message, string repositoryName, Exception innerException)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        RepositoryName = repositoryName;
        Context[nameof(RepositoryName)] = repositoryName ?? string.Empty;
    }
    public override string ToString()
    {
        return !string.IsNullOrEmpty(RepositoryName)
            ? $"[Repository: {RepositoryName}] {base.ToString()}"
            : base.ToString();
    }
}
