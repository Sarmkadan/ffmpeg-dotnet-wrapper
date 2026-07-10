namespace FFmpegDotnetWrapper.Exceptions
{
    public static class ServiceExceptionExtensions
    {
        /// <summary>
        /// Creates a new ServiceException with the same message but a different service name
        /// </summary>
        public static ServiceException WithServiceName(this ServiceException exception, string newServiceName)
        {
            return new ServiceException(exception.Message, newServiceName, exception.InnerException);
        }

        /// <summary>
        /// Returns a formatted string containing both service name (if present) and message
        /// </summary>
        public static string GetMessageWithService(this ServiceException exception)
        {
            return string.IsNullOrEmpty(exception.ServiceName)
                ? exception.Message
                : $"{exception.ServiceName}: {exception.Message}";
        }

        /// <summary>
        /// Checks if the exception has service context (service name is set)
        /// </summary>
        public static bool HasServiceContext(this ServiceException exception)
        {
            return !string.IsNullOrEmpty(exception.ServiceName);
        }
    }
}
