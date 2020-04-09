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
		/// <exception cref="ArgumentNullException"><paramref name="ex"/> is <see langword="null"/></exception>
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
		/// <exception cref="ArgumentNullException"><paramref name="ex"/> is <see langword="null"/></exception>
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
		/// <exception cref="ArgumentNullException"><paramref name="ex"/> is <see langword="null"/></exception>
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
		/// <exception cref="ArgumentNullException"><paramref name="ex"/> is <see langword="null"/></exception>
		public static string GetFullExceptionDetails(this ProcessExecutionException ex)
		{
			ArgumentNullException.ThrowIfNull(ex);
			var detailedErrorMessage = ex.GetDetailedErrorMessage();
			var innerExceptionMessage = ex.InnerException != null ? $"Inner exception: {ex.InnerException.Message}" : "No inner exception";
			return $"{detailedErrorMessage}{innerExceptionMessage}";
		}
	}
}