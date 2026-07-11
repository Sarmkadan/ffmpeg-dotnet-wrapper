using System;
using System.Collections.Generic;
using System.Linq;

namespace FFmpegDotnetWrapper.Tests
{
    /// <summary>
    /// Provides validation extension methods for <see cref="SubtitleSettingsTests"/> instances.
    /// </summary>
    internal static class SubtitleSettingsTestsValidation
    {
        /// <summary>
        /// Validates the <see cref="SubtitleSettingsTests"/> instance and returns a list of human-readable problems.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <returns>A read-only list of validation error messages. Empty if the instance is considered valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        public static IReadOnlyList<string> Validate(this SubtitleSettingsTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // The test class does not expose state; validation is limited to ensuring the instance exists.
            // Additional reflection-based checks could be added here if the test class gains properties to validate.

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the <see cref="SubtitleSettingsTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to check.</param>
        /// <returns><c>true</c> if no validation problems are found; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        public static bool IsValid(this SubtitleSettingsTests value)
        {
            return value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that the <see cref="SubtitleSettingsTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when validation problems are found.</exception>
        public static void EnsureValid(this SubtitleSettingsTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = value.Validate();
            if (problems.Count > 0)
            {
                throw new ArgumentException($"SubtitleSettingsTests validation failed: {string.Join("; ", problems)}");
            }
        }
    }
}