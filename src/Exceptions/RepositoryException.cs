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
    public string? RepositoryName { get; set; }

    public RepositoryException(string message) : base(message)
    {
    }

    public RepositoryException(string message, string repositoryName) : base(message)
    {
        RepositoryName = repositoryName;
    }

    public RepositoryException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public RepositoryException(string message, string repositoryName, Exception innerException) : base(message, innerException)
    {
        RepositoryName = repositoryName;
    }
}
