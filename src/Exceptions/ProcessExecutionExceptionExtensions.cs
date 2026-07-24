using System;

namespace FFmpegDotnetWrapper.Exceptions
{
    /// <summary>
    /// Provides extension methods for <see cref="ProcessExecutionException"/> to facilitate error handling and diagnostics.
    /// </summary>
    public static class ProcessExecutionExceptionExtensions
    {
        /// <summary>
        /// Determines whether the process execution was successful based on the exit code.
        /// </summary>
        /// <param name="ex">The exception containing process execution information.</param>
        /// <returns><see langword="true"/> if the exit code is 0; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ex"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method checks the <see cref="ProcessExecutionException.ExitCode"/> property to determine success.
        /// A null exit code will be treated as unsuccessful.
        /// </remarks>
        public static bool IsSuccessful(this ProcessExecutionException ex)
        {
            ArgumentNullException.ThrowIfNull(ex);
            return ex.ExitCode == 0;
        }

        /// <summary>
        /// Gets the primary error message from the exception, preferring <see cref="ProcessExecutionException.ErrorOutput"/> over <see cref="Exception.Message"/>.
        /// </summary>
        /// <param name="ex">The exception containing process execution information.</param>
        /// <returns>The error output if available; otherwise, the exception message.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ex"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method prioritizes the <see cref="ProcessExecutionException.ErrorOutput"/> property as the primary error source.
        /// </remarks>
        public static string GetErrorMessage(this ProcessExecutionException ex)
        {
            ArgumentNullException.ThrowIfNull(ex);
            return ex.ErrorOutput ?? ex.Message;
        }

        /// <summary>
        /// Gets a detailed error message that includes both the error output and the process exit code.
        /// </summary>
        /// <param name="ex">The exception containing process execution information.</param>
        /// <returns>A formatted string containing the error message and exit code information.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ex"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// The exit code is formatted as "Exit code: {value}" if available, or "No exit code available" otherwise.
        /// </remarks>
        public static string GetDetailedErrorMessage(this ProcessExecutionException ex)
        {
            ArgumentNullException.ThrowIfNull(ex);
            var errorMessage = ex.GetErrorMessage();
            var exitCode = ex.ExitCode.HasValue ? $"Exit code: {ex.ExitCode}" : "No exit code available";
            return $"{errorMessage} ({exitCode})\n";
        }

        /// <summary>
        /// Gets comprehensive exception details including the detailed error message and inner exception information.
        /// </summary>
        /// <param name="ex">The exception containing process execution information.</param>
        /// <returns>A formatted string containing the detailed error message and inner exception details.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ex"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method combines the output of <see cref="GetDetailedErrorMessage(ProcessExecutionException)"/> with inner exception information.
        /// If no inner exception exists, "No inner exception" is appended to the result.
        /// </remarks>
        public static string GetFullExceptionDetails(this ProcessExecutionException ex)
        {
            ArgumentNullException.ThrowIfNull(ex);
            var detailedErrorMessage = ex.GetDetailedErrorMessage();
            var innerExceptionMessage = ex.InnerException != null ? $"Inner exception: {ex.InnerException.Message}" : "No inner exception";
            return $"{detailedErrorMessage}{innerExceptionMessage}";
        }

        /// <summary>
        /// Adds additional context to the exception's Context dictionary.
        /// </summary>
        /// <param name="ex">The process execution exception to update.</param>
        /// <param name="key">The context key to add.</param>
        /// <param name="value">The context value to add.</param>
        /// <returns>The same exception instance for fluent chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="ex"/> is null.</exception>
        public static ProcessExecutionException WithContext(this ProcessExecutionException ex, string key, string value)
        {
            ArgumentNullException.ThrowIfNull(ex);
            ArgumentException.ThrowIfNullOrEmpty(key, nameof(key));
            ArgumentException.ThrowIfNullOrEmpty(value, nameof(value));

            ex.Context[key] = value;
            return ex;
        }

        /// <summary>
        /// Gets the exit code from the exception's Context dictionary.
        /// </summary>
        /// <param name="ex">The process execution exception to check.</param>
        /// <returns>The exit code if available; otherwise, null.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="ex"/> is null.</exception>
        public static int? GetExitCode(this ProcessExecutionException ex)
        {
            ArgumentNullException.ThrowIfNull(ex);
            return ex.ExitCode;
        }

        /// <summary>
        /// Gets the error output from the exception's Context dictionary.
        /// </summary>
        /// <param name="ex">The process execution exception to check.</param>
        /// <returns>The error output if available; otherwise, null.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="ex"/> is null.</exception>
        public static string? GetErrorOutput(this ProcessExecutionException ex)
        {
            ArgumentNullException.ThrowIfNull(ex);
            return ex.ErrorOutput;
        }
    }
}
