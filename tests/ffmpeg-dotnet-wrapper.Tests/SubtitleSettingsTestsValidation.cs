using System;
using System.Collections.Generic;
using System.Linq;

namespace FFmpegDotnetWrapper.Tests
{
    /// <summary>
    /// Validation helpers for <see cref="SubtitleSettingsTests"/>.
    /// </summary>
    public static class SubtitleSettingsTestsValidation
    {
        /// <summary>
        /// Validates the <see cref="SubtitleSettingsTests"/> instance and returns a list of human‑readable problems.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <returns>A read‑only list of validation error messages. Empty if the instance is considered valid.</returns>
        public static IReadOnlyList<string> Validate(this SubtitleSettingsTests value)
        {
            var problems = new List<string>();

            if (value == null)
            {
                problems.Add("SubtitleSettingsTests instance is null.");
                return problems;
            }

            // The test class does not expose state; validation is limited to ensuring the instance exists.
            // Additional reflection‑based checks could be added here if needed.

            return problems;
        }

        /// <summary>
        /// Determines whether the <see cref="SubtitleSettingsTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to check.</param>
        /// <returns><c>true</c> if no validation problems are found; otherwise <c>false</c>.</returns>
        public static bool IsValid(this SubtitleSettingsTests value)
        {
            return !value.Validate().Any();
        }

        /// <summary>
        /// Ensures that the <see cref="SubtitleSettingsTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <exception cref="ArgumentException">Thrown when validation problems are found.</exception>
        public static void EnsureValid(this SubtitleSettingsTests value)
        {
            var problems = value.Validate();
            if (problems.Any())
            {
                throw new ArgumentException($"SubtitleSettingsTests validation failed: {string.Join("; ", problems)}");
            }
        }
    }
}
