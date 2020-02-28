using System;

namespace FFmpegDotnetWrapper.Exceptions
{
    public static class ProcessExecutionExceptionExtensions
    {
        public static bool IsSuccessful(this ProcessExecutionException ex)
        {
            return ex.ExitCode == 0;
        }

        public static string GetErrorMessage(this ProcessExecutionException ex)
        {
            return ex.ErrorOutput ?? ex.Message;
        }

        public static string GetDetailedErrorMessage(this ProcessExecutionException ex)
        {
            var errorMessage = ex.GetErrorMessage();
            var exitCode = ex.ExitCode.HasValue ? $"Exit code: {ex.ExitCode}" : "No exit code available";
            return $"{errorMessage} ({exitCode})";
        }

        public static string GetFullExceptionDetails(this ProcessExecutionException ex)
        {
            var detailedErrorMessage = ex.GetDetailedErrorMessage();
            var innerExceptionMessage = ex.InnerException != null ? $"Inner exception: {ex.InnerException.Message}" : "No inner exception";
            return $"{detailedErrorMessage}\n{innerExceptionMessage}";
        }
    }
}
