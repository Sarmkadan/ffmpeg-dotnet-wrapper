namespace FFmpegDotnetWrapper.Exceptions
{
    /// <summary>
    /// Provides extension methods for <see cref="ConfigurationException"/> to enhance error handling and diagnostics.
    /// </summary>
    public static class ConfigurationExceptionExtensions
    {
        /// <summary>
        /// Checks if the exception was caused by a specific configuration key.
        /// </summary>
        /// <param name="exception">The configuration exception to check.</param>
        /// <param name="key">The configuration key to compare against.</param>
        /// <returns>True if the exception's configuration key matches the specified key (case-insensitive); otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
        public static bool HasConfigurationKey(this ConfigurationException exception, string key)
        {
            ArgumentNullException.ThrowIfNull(exception);
            ArgumentException.ThrowIfNullOrEmpty(key, nameof(key));

            return string.Equals(exception.ConfigurationKey, key, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Creates a new exception with additional context while preserving original state.
        /// </summary>
        /// <param name="exception">The original configuration exception.</param>
        /// <param name="context">Additional context information to include in the error message.</param>
        /// <returns>A new <see cref="ConfigurationException"/> with the additional context appended to the message.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
        public static ConfigurationException WithAdditionalContext(this ConfigurationException exception, string context)
        {
            ArgumentNullException.ThrowIfNull(exception);
            ArgumentNullException.ThrowIfNull(context);

            var baseMessage = exception.Message;
            var newMessage = $"{baseMessage} - Context: {context}";

            return exception.InnerException != null
                ? new ConfigurationException(newMessage, exception.ConfigurationKey, exception.InnerException)
                : new ConfigurationException(newMessage, exception.ConfigurationKey);
        }

        /// <summary>
        /// Gets a formatted message including configuration key if present.
        /// </summary>
        /// <param name="exception">The configuration exception to format.</param>
        /// <returns>A formatted message string that includes the configuration key if it exists; otherwise, the original message.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
        public static string GetMessageWithKey(this ConfigurationException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);
            return exception.ConfigurationKey != null
                ? $"{exception.Message} (Configuration Key: {exception.ConfigurationKey})"
                : exception.Message;
        }
    }
}
