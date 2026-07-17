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
        private const int MaxConsoleWidth = 200;
        private const int MaxWidth = 200;
        private static readonly IReadOnlyList<string> EmptyProblems = Array.Empty<string>();

        /// <summary>
        /// Validates the specified console width value.
        /// </summary>
        /// <param name="consoleWidth">The console width to validate.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if consoleWidth is less than or equal to 0.</exception>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        public static IReadOnlyList<string> ValidateConsoleWidth(int consoleWidth)
        {
            if (consoleWidth <= 0)
            {
                return new[] { $"Console width must be positive, but was {consoleWidth}." };
            }

            if (consoleWidth > MaxConsoleWidth)
            {
                return new[] { $"Console width {consoleWidth} exceeds reasonable maximum of {MaxConsoleWidth}." };
            }

            return EmptyProblems;
        }

        /// <summary>
        /// Validates the specified use colors flag.
        /// </summary>
        /// <param name="useColors">The use colors flag to validate.</param>
        /// <returns>A list of validation problems; empty if valid.</returns>
        public static IReadOnlyList<string> ValidateUseColors(bool useColors)
            => EmptyProblems;

        /// <summary>
        /// Validates a percentage value for progress bars.
        /// </summary>
        /// <param name="percentage">The percentage to validate (0-100).</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if percentage is NaN, infinite, negative, or greater than 100.</exception>
        /// <returns>A list of validation problems; empty if valid.</returns>
        public static IReadOnlyList<string> ValidatePercentage(double percentage)
        {
            if (double.IsNaN(percentage))
            {
                return new[] { "Percentage cannot be NaN." };
            }

            if (double.IsInfinity(percentage))
            {
                return new[] { "Percentage cannot be infinite." };
            }

            if (percentage < 0)
            {
                return new[] { $"Percentage cannot be negative, but was {percentage}." };
            }

            if (percentage > 100)
            {
                return new[] { $"Percentage cannot exceed 100, but was {percentage}." };
            }

            return EmptyProblems;
        }

        /// <summary>
        /// Validates a width parameter for formatting operations.
        /// </summary>
        /// <param name="width">The width to validate.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if width is less than or equal to 0.</exception>
        /// <returns>A list of validation problems; empty if valid.</returns>
        public static IReadOnlyList<string> ValidateWidth(int width)
        {
            if (width <= 0)
            {
                return new[] { $"Width must be positive, but was {width}." };
            }

            if (width > MaxWidth)
            {
                return new[] { $"Width {width} exceeds reasonable maximum of {MaxWidth}." };
            }

            return EmptyProblems;
        }

        /// <summary>
        /// Validates a list of strings to ensure none are null or empty.
        /// </summary>
        /// <param name="values">The list of strings to validate.</param>
        /// <param name="paramName">The name of the parameter for error messages.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="values"/> is null.</exception>
        /// <returns>A list of validation problems; empty if valid.</returns>
        public static IReadOnlyList<string> ValidateStringList(IReadOnlyList<string> values, string paramName = "value")
        {
            ArgumentNullException.ThrowIfNull(values);

            if (values.Count == 0)
            {
                return EmptyProblems;
            }

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
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates a string message parameter.
        /// </summary>
        /// <param name="message">The message to validate.</param>
        /// <param name="paramName">The name of the parameter for error messages.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="message"/> is empty or whitespace.</exception>
        /// <returns>A list of validation problems; empty if valid.</returns>
        public static IReadOnlyList<string> ValidateMessage(string message, string paramName = "message")
        {
            ArgumentNullException.ThrowIfNull(message);

            if (string.IsNullOrEmpty(message))
            {
                return new[] { $"{paramName} is null or empty." };
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                return new[] { $"{paramName} is null, empty, or whitespace." };
            }

            return EmptyProblems;
        }
    }
}
