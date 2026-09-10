using System;
using FFmpegDotnetWrapper.Exceptions;

namespace FFmpegDotnetWrapper.Abstraction
{
    /// <summary>
    /// Extension methods for <see cref="ProcessResult"/>.
    /// </summary>
    public static class ProcessResultExtensions
    {
        /// <summary>
        /// Determines whether the process result indicates success.
        /// </summary>
        /// <param name="result">The process result.</param>
        /// <returns>
        /// <see langword="true"/> if the exit code is 0, the process did not time out, and was not cancelled; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsSuccess(this ProcessResult result)
        {
            ArgumentNullException.ThrowIfNull(result);
            return result.ExitCode == 0 && !result.TimedOut && !result.WasCancelled;
        }

        /// <summary>
        /// Determines whether the process result contains standard error output.
        /// </summary>
        /// <param name="result">The process result.</param>
        /// <returns>
        /// <see langword="true"/> if the standard error tail is not empty; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool HasStdErr(this ProcessResult result)
        {
            ArgumentNullException.ThrowIfNull(result);
            return !string.IsNullOrEmpty(result.StdErrTail);
        }

        /// <summary>
        /// Throws a <see cref="ProcessExecutionException"/> if the process result indicates failure.
        /// </summary>
        /// <param name="result">The process result.</param>
        /// <param name="context">
        /// Optional context to include in the exception message.
        /// </param>
        /// <exception cref="ProcessExecutionException">
        /// Thrown when the process result indicates failure (non-zero exit code, timeout, or cancellation).
        /// </exception>
        public static void ThrowIfFailed(this ProcessResult result, string? context = null)
        {
            ArgumentNullException.ThrowIfNull(result);

            if (result.IsSuccess())
            {
                return;
            }

            string failureReason = result.GetFailureReason() ?? "unknown failure";
            string message = string.IsNullOrEmpty(context)
                ? $"Process execution failed: {failureReason}"
                : $"Process execution failed ({context}): {failureReason}";

            // Use the constructor that takes message and exitCode if we have an exit code, otherwise just message.
            if (!result.TimedOut && !result.WasCancelled)
            {
                throw new ProcessExecutionException(message, result.ExitCode, result.StdErrTail);
            }
            else
            {
                // For timeout or cancellation, we don't have an exit code that indicates failure in the same way.
                // We'll use the constructor that takes only a message.
                throw new ProcessExecutionException(message);
            }
        }

        /// <summary>
        /// Gets a human-readable string describing the failure reason, or <see langword="null"/> if the process succeeded.
        /// </summary>
        /// <param name="result">The process result.</param>
        /// <returns>
        /// A string describing the failure reason (e.g., "timed out", "cancelled", "exit code N"), or <see langword="null"/> if successful.
        /// </returns>
        public static string? GetFailureReason(this ProcessResult result)
        {
            ArgumentNullException.ThrowIfNull(result);

            if (result.IsSuccess())
            {
                return null;
            }

            if (result.TimedOut)
            {
                return "timed out";
            }

            if (result.WasCancelled)
            {
                return "cancelled";
            }

            return $"exit code {result.ExitCode}";
        }
    }
}