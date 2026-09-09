using System;
using System.Globalization;

namespace FFmpegDotnetWrapper.Abstraction
{
    /// <summary>
    /// Represents the result of a process execution.
    /// </summary>
    public class ProcessResult
    {
        /// <summary>
        /// Gets or sets the exit code of the process.
        /// </summary>
        public int ExitCode { get; set; }

        /// <summary>
        /// Gets or sets the tail of the standard error output.
        /// </summary>
        public string StdErrTail { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the process timed out.
        /// </summary>
        public bool TimedOut { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the process was cancelled.
        /// </summary>
        public bool WasCancelled { get; set; }

        /// <summary>
        /// Returns a culture-invariant string representation of the process result.
        /// </summary>
        /// <returns>A string containing ExitCode, TimedOut, WasCancelled, and a truncated StdErrTail.</returns>
        public override string ToString()
        {
            const int maxStdErrLength = 100;
            var truncatedStdErr = StdErrTail.Length > maxStdErrLength
                ? StdErrTail.Substring(0, maxStdErrLength) + "..."
                : StdErrTail;

            return string.Format(CultureInfo.InvariantCulture,
                "ExitCode={0}, TimedOut={1}, WasCancelled={2}, StdErrTail=\"{3}\"",
                ExitCode, TimedOut, WasCancelled, truncatedStdErr);
        }
    }
}