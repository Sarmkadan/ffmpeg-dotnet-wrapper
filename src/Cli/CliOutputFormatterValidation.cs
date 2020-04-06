// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Validation helpers for CliOutputFormatter to ensure configuration values
// are within acceptable ranges before use.
// =============================================================================

using System;
using System.Collections.Generic;

namespace FFmpegDotnetWrapper.Cli
{
    /// <summary>
    /// Provides validation helpers for CLI output formatting operations.
    /// Validates constructor parameters and ensures formatting methods receive valid inputs.
    /// </summary>
    public static class CliOutputFormatterValidation
    {
        /// <summary>
        /// Validates the specified console width value.
        /// </summary>
        /// <param name="consoleWidth">The console width to validate.</param>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        public static IReadOnlyList<string> ValidateConsoleWidth(int consoleWidth)
        {
            var problems = new List<string>();

            if (consoleWidth <= 0)
            {
                problems.Add($"Console width must be positive, but was {consoleWidth}.");
            }

            if (consoleWidth > 200)
            {
                problems.Add($"Console width {consoleWidth} exceeds reasonable maximum of 200.");
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates the specified use colors flag.
        /// </summary>
        /// <param name="useColors">The use colors flag to validate.</param>
        /// <returns>A list of validation problems; empty if valid.</returns>
        public static IReadOnlyList<string> ValidateUseColors(bool useColors)
        {
            // Boolean is always valid
            return Array.Empty<string>();
        }

        /// <summary>
        /// Validates a percentage value for progress bars.
        /// </summary>
        /// <param name="percentage">The percentage to validate (0-100).</param>
        /// <returns>A list of validation problems; empty if valid.</returns>
        public static IReadOnlyList<string> ValidatePercentage(double percentage)
        {
            var problems = new List<string>();

            if (double.IsNaN(percentage))
            {
                problems.Add("Percentage cannot be NaN.");
            }
            else if (double.IsInfinity(percentage))
            {
                problems.Add("Percentage cannot be infinite.");
            }
            else if (percentage < 0)
            {
                problems.Add($"Percentage cannot be negative, but was {percentage}.");
            }
            else if (percentage > 100)
            {
                problems.Add($"Percentage cannot exceed 100, but was {percentage}.");
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates a width parameter for formatting operations.
        /// </summary>
        /// <param name="width">The width to validate.</param>
        /// <returns>A list of validation problems; empty if valid.</returns>
        public static IReadOnlyList<string> ValidateWidth(int width)
        {
            var problems = new List<string>();

            if (width <= 0)
            {
                problems.Add($"Width must be positive, but was {width}.");
            }

            if (width > 200)
            {
                problems.Add($"Width {width} exceeds reasonable maximum of 200.");
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates a list of strings to ensure none are null or empty.
        /// </summary>
        /// <param name="values">The list of strings to validate.</param>
        /// <param name="paramName">The name of the parameter for error messages.</param>
        /// <returns>A list of validation problems; empty if valid.</returns>
        public static IReadOnlyList<string> ValidateStringList(IReadOnlyList<string> values, string paramName = "value")
        {
            ArgumentNullException.ThrowIfNull(values);

            var problems = new List<string>();

            for (int i = 0; i < values.Count; i++)
            {
                if (values[i] == null)
                {
                    problems.Add($"{paramName}[{i}] is null.");
                }
                else if (string.IsNullOrEmpty(values[i]))
                {
                    problems.Add($"{paramName}[{i}] is null or empty.");
                }
                else if (string.IsNullOrWhiteSpace(values[i]))
                {
                    problems.Add($"{paramName}[{i}] is null, empty, or whitespace.");
                }
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates a string message parameter.
        /// </summary>
        /// <param name="message">The message to validate.</param>
        /// <param name="paramName">The name of the parameter for error messages.</param>
        /// <returns>A list of validation problems; empty if valid.</returns>
        public static IReadOnlyList<string> ValidateMessage(string message, string paramName = "message")
        {
            ArgumentNullException.ThrowIfNull(message);

            var problems = new List<string>();

            if (string.IsNullOrEmpty(message))
            {
                problems.Add($"{paramName} is null or empty.");
            }
            else if (string.IsNullOrWhiteSpace(message))
            {
                problems.Add($"{paramName} is null, empty, or whitespace.");
            }

            return problems.AsReadOnly();
        }
    }
}
