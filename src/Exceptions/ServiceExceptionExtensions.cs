using System;

namespace FFmpegDotnetWrapper.Exceptions
{
    /// <summary>
    /// Provides extension methods for <see cref="ServiceException"/> to facilitate error handling and diagnostics.
    /// </summary>
    public static class ServiceExceptionExtensions
    {
        /// <summary>
        /// Creates a new ServiceException with the same message but a different service name.
        /// </summary>
        /// <param name="exception">The original exception containing the message and inner exception.</param>
        /// <param name="newServiceName">The new service name to use for the exception.</param>
        /// <returns>A new ServiceException instance with the specified service name.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="newServiceName"/> is <see langword="null"/></exception>
        public static ServiceException WithServiceName(this ServiceException exception, string newServiceName)
        {
            ArgumentNullException.ThrowIfNull(exception);
            ArgumentNullException.ThrowIfNull(newServiceName);

            return new ServiceException(exception.Message, newServiceName, exception.InnerException);
        }

        /// <summary>
        /// Adds additional context to the exception's Context dictionary.
        /// </summary>
        /// <param name="exception">The service exception to update.</param>
        /// <param name="key">The context key to add.</param>
        /// <param name="value">The context value to add.</param>
        /// <returns>The same exception instance for fluent chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
        public static ServiceException WithContext(this ServiceException exception, string key, string value)
        {
            ArgumentNullException.ThrowIfNull(exception);
            ArgumentException.ThrowIfNullOrEmpty(key, nameof(key));
            ArgumentException.ThrowIfNullOrEmpty(value, nameof(value));

            exception.Context[key] = value;
            return exception;
        }

        /// <summary>
        /// Returns a formatted string containing both service name (if present) and message.
        /// </summary>
        /// <param name="exception">The exception to format.</param>
        /// <returns>A formatted string containing service name and message, or just the message if no service name is set.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/></exception>
        public static string GetMessageWithService(this ServiceException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);

            return string.IsNullOrEmpty(exception.ServiceName)
                ? exception.Message
                : $"{exception.ServiceName}: {exception.Message}";
        }

        /// <summary>
        /// Checks if the exception has service context (service name is set).
        /// </summary>
        /// <param name="exception">The exception to check.</param>
        /// <returns><see langword="true"/> if the exception has a service name; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/></exception>
        public static bool HasServiceContext(this ServiceException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);
            return !string.IsNullOrEmpty(exception.ServiceName);
        }

        /// <summary>
        /// Gets the service name from the exception's Context dictionary.
        /// </summary>
        /// <param name="exception">The service exception to check.</param>
        /// <returns>The service name if present; otherwise, null.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
        public static string? GetServiceName(this ServiceException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);
            return exception.Context.TryGetValue(nameof(ServiceException.ServiceName), out var value) ? value : null;
        }
    }
}
