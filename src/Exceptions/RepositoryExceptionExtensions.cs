using System;

namespace FFmpegDotnetWrapper.Exceptions;

public static class RepositoryExceptionExtensions
{
    /// <summary>
    /// Determines whether the exception indicates a repository not found error.
    /// </summary>
    /// <param name="exception">The repository exception to check.</param>
    /// <returns>True if the repository is not found; otherwise, false.</returns>
    public static bool IsRepositoryNotFound(this RepositoryException exception)
    {
        if (exception is null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        return exception.Message.Contains("not found", StringComparison.OrdinalIgnoreCase) ||
               exception.Message.Contains("does not exist", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether the exception indicates a repository already exists error.
    /// </summary>
    /// <param name="exception">The repository exception to check.</param>
    /// <returns>True if the repository already exists; otherwise, false.</returns>
    public static bool IsRepositoryAlreadyExists(this RepositoryException exception)
    {
        if (exception is null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        return exception.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase) ||
               exception.Message.Contains("already present", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether the exception indicates an access denied or permission error.
    /// </summary>
    /// <param name="exception">The repository exception to check.</param>
    /// <returns>True if the error is related to access denied; otherwise, false.</returns>
    public static bool IsAccessDenied(this RepositoryException exception)
    {
        if (exception is null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        return exception.Message.Contains("access denied", StringComparison.OrdinalIgnoreCase) ||
               exception.Message.Contains("permission denied", StringComparison.OrdinalIgnoreCase) ||
               exception.Message.Contains("insufficient permissions", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Creates a new RepositoryException with additional context while preserving the original exception.
    /// </summary>
    /// <param name="exception">The original repository exception.</param>
    /// <param name="additionalContext">Additional context to include in the new exception message.</param>
    /// <returns>A new RepositoryException with combined context.</returns>
    public static RepositoryException WithContext(this RepositoryException exception, string additionalContext)
    {
        if (exception is null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        if (string.IsNullOrWhiteSpace(additionalContext))
        {
            throw new ArgumentException("Additional context cannot be null or whitespace.", nameof(additionalContext));
        }

        return new RepositoryException(
            $"{exception.Message} | Context: {additionalContext}",
            exception.RepositoryName,
            exception);
    }
}
