// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Validation helpers for JsonOutputFormatter to ensure proper configuration
// before use in production scenarios.
// =====================================================================

using System;
using System.Collections.Generic;

namespace FFmpegDotnetWrapper.Serialization
{
    /// <summary>
    /// Provides validation helpers for <see cref="JsonOutputFormatter"/> instances.
    /// </summary>
    public static class JsonOutputFormatterValidation
    {
        /// <summary>
        /// Validates that a JsonOutputFormatter instance is properly configured.
        /// </summary>
        /// <param name="value">The JsonOutputFormatter instance to validate.</param>
        /// <returns>A list of validation errors; empty if the instance is valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this object? value)
        {
            ArgumentNullException.ThrowIfNull(value);

            // JsonOutputFormatter has no configurable properties to validate
            // The constructor parameter 'indent' is private and has no public accessors
            // All validation is handled by the constructor itself
            return Array.Empty<string>();
        }

        /// <summary>
        /// Determines whether a JsonOutputFormatter instance is valid.
        /// </summary>
        /// <param name="value">The JsonOutputFormatter instance to check.</param>
        /// <returns>True if the instance is valid; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static bool IsValid(this object? value)
        {
            ArgumentNullException.ThrowIfNull(value);
            return value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that a JsonOutputFormatter instance is valid.
        /// </summary>
        /// <param name="value">The JsonOutputFormatter instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the instance is invalid, containing validation errors.</exception>
        public static void EnsureValid(this object? value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = value.Validate();
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"JsonOutputFormatter is not valid. Validation errors: {string.Join("; ", errors)}");
            }
        }
    }
}
