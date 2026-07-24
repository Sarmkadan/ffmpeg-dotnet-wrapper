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

    public RepositoryException(string message)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
    }

    public RepositoryException(string message, string repositoryName)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        RepositoryName = repositoryName;
        Context[nameof(RepositoryName)] = repositoryName ?? string.Empty;
    }

    public RepositoryException(string message, Exception innerException)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
    }

    public RepositoryException(string message, string repositoryName, Exception innerException)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        RepositoryName = repositoryName;
        Context[nameof(RepositoryName)] = repositoryName ?? string.Empty;
    }
}
