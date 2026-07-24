using System;

namespace FFmpegDotnetWrapper.Exceptions
{
    public static class RepositoryExceptionExtensions
    {
        /// <summary>
        /// Determines whether the exception indicates a repository not found error.
        /// </summary>
        /// <param name="exception">The repository exception to check.</param>
        /// <returns>True if the repository is not found; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/>.</exception>
        public static bool IsRepositoryNotFound(this RepositoryException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);

            return exception.Message.Contains("not found", StringComparison.OrdinalIgnoreCase) ||
                   exception.Message.Contains("does not exist", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determines whether the exception indicates a repository already exists error.
        /// </summary>
        /// <param name="exception">The repository exception to check.</param>
        /// <returns>True if the repository already exists; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/>.</exception>
        public static bool IsRepositoryAlreadyExists(this RepositoryException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);

            return exception.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase) ||
                   exception.Message.Contains("already present", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determines whether the exception indicates an access denied or permission error.
        /// </summary>
        /// <param name="exception">The repository exception to check.</param>
        /// <returns>True if the error is related to access denied; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/>.</exception>
        public static bool IsAccessDenied(this RepositoryException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);

            return exception.Message.Contains("access denied", StringComparison.OrdinalIgnoreCase) ||
                   exception.Message.Contains("permission denied", StringComparison.OrdinalIgnoreCase) ||
                   exception.Message.Contains("insufficient permissions", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Adds additional context to the exception's Context dictionary.
        /// </summary>
        /// <param name="exception">The repository exception to update.</param>
        /// <param name="key">The context key to add.</param>
        /// <param name="value">The context value to add.</param>
        /// <returns>The same exception instance for fluent chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
        public static RepositoryException WithContext(this RepositoryException exception, string key, string value)
        {
            ArgumentNullException.ThrowIfNull(exception);
            ArgumentException.ThrowIfNullOrEmpty(key, nameof(key));
            ArgumentException.ThrowIfNullOrEmpty(value, nameof(value));

            exception.Context[key] = value;
            return exception;
        }

        /// <summary>
        /// Creates a new RepositoryException with additional context while preserving the original exception.
        /// </summary>
        /// <param name="exception">The original repository exception.</param>
        /// <param name="additionalContext">Additional context to include in the new exception message.</param>
        /// <returns>A new RepositoryException with combined context.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="exception"/> is <see langword="null"/>.
        /// <paramref name="additionalContext"/> is <see langword="null"/>.
        /// </exception>
        public static RepositoryException WithContext(this RepositoryException exception, string additionalContext)
        {
            ArgumentNullException.ThrowIfNull(exception);
            ArgumentNullException.ThrowIfNull(additionalContext);

            return new RepositoryException(
                $"{exception.Message} | Context: {additionalContext}",
                exception.RepositoryName,
                exception);
        }

        /// <summary>
        /// Gets the repository name from the exception's Context dictionary.
        /// </summary>
        /// <param name="exception">The repository exception to check.</param>
        /// <returns>The repository name if present; otherwise, null.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
        public static string? GetRepositoryName(this RepositoryException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);
            return exception.Context.TryGetValue(nameof(RepositoryException.RepositoryName), out var value) ? value : null;
        }
    }
}
