// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace FFmpegDotnetWrapper.Middleware
{
    /// <summary>
    /// Provides validation helpers for <see cref="RateLimitPolicy"/> instances.
    /// Validates policy configuration to ensure rate limits are properly defined.
    /// </summary>
    public static class RateLimitPolicyValidation
    {
        /// <summary>
        /// Validates a <see cref="RateLimitPolicy"/> instance and returns a list of validation problems.
        /// </summary>
        /// <param name="value">The policy to validate.</param>
        /// <returns>An empty list if valid, otherwise a list of human-readable problem descriptions.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this RateLimitPolicy value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // Validate MaxRequests
            if (value.MaxRequests <= 0)
            {
                problems.Add($"MaxRequests must be greater than 0, but was {value.MaxRequests}.");
            }

            // Validate WindowSeconds
            if (value.WindowSeconds <= 0)
            {
                problems.Add($"WindowSeconds must be greater than 0, but was {value.WindowSeconds}.");
            }

            // Validate PolicyName
            if (string.IsNullOrWhiteSpace(value.PolicyName))
            {
                problems.Add("PolicyName cannot be null or whitespace.");
            }
            else if (value.PolicyName.Length > 100)
            {
                problems.Add($"PolicyName cannot exceed 100 characters, but was {value.PolicyName.Length}.");
            }

            // Validate PerUserLimit (no validation needed, it's a boolean)

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether a <see cref="RateLimitPolicy"/> instance is valid.
        /// </summary>
        /// <param name="value">The policy to validate.</param>
        /// <returns>True if the policy is valid; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static bool IsValid(this RateLimitPolicy value)
        {
            return value is not null && value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that a <see cref="RateLimitPolicy"/> instance is valid, throwing an exception if it is not.
        /// </summary>
        /// <param name="value">The policy to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the policy is invalid, containing a list of validation problems.</exception>
        public static void EnsureValid(this RateLimitPolicy value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = value.Validate();
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"RateLimitPolicy '{value.PolicyName}' is invalid. Problems:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}",
                    nameof(value));
            }
        }
    }
}