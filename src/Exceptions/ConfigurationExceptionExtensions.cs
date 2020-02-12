namespace FFmpegDotnetWrapper.Exceptions
{
    public static class ConfigurationExceptionExtensions
    {
        /// <summary>
        /// Checks if the exception was caused by a specific configuration key
        /// </summary>
        public static bool HasConfigurationKey(this ConfigurationException exception, string key)
        {
            return string.Equals(exception.ConfigurationKey, key, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Creates a new exception with additional context while preserving original state
        /// </summary>
        public static ConfigurationException WithAdditionalContext(this ConfigurationException exception, string context)
        {
            var baseMessage = exception.Message;
            var newMessage = $"{baseMessage} - Context: {context}";
            
            if (exception.InnerException != null)
            {
                return new ConfigurationException(newMessage, exception.ConfigurationKey, exception.InnerException);
            }
            return new ConfigurationException(newMessage, exception.ConfigurationKey);
        }

        /// <summary>
        /// Gets a formatted message including configuration key if present
        /// </summary>
        public static string GetMessageWithKey(this ConfigurationException exception)
        {
            return exception.ConfigurationKey != null
                ? $"{exception.Message} (Configuration Key: {exception.ConfigurationKey})"
                : exception.Message;
        }
    }
}
