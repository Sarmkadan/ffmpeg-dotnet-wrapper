// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace FFmpegDotnetWrapper.BackgroundJobs
{
    /// <summary>
    /// Provides validation helpers for <see cref="BackgroundJob"/> instances.
    /// Validates job state, progress, timestamps, and other invariants.
    /// </summary>
    public static class BackgroundJobValidation
    {
        /// <summary>
        /// Validates a background job and returns a list of human-readable problems.
        /// </summary>
        /// <param name="value">The job to validate.</param>
        /// <returns>An empty list if valid; otherwise, a list of validation errors.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this BackgroundJob value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Validate JobId
            if (string.IsNullOrWhiteSpace(value.JobId))
            {
                errors.Add("JobId cannot be null or whitespace.");
            }

            // Validate JobName
            if (string.IsNullOrWhiteSpace(value.JobName))
            {
                errors.Add("JobName cannot be null or whitespace.");
            }

            // Validate State
            if (!Enum.IsDefined(typeof(JobState), value.State))
            {
                errors.Add("State has an invalid value.");
            }

            // Validate ProgressPercentage
            if (value.ProgressPercentage < 0 || value.ProgressPercentage > 100)
            {
                errors.Add("ProgressPercentage must be between 0 and 100 inclusive.");
            }

            // Validate StatusMessage
            if (string.IsNullOrWhiteSpace(value.StatusMessage))
            {
                errors.Add("StatusMessage cannot be null or whitespace.");
            }

            // Validate CreatedAt
            if (value.CreatedAt == default)
            {
                errors.Add("CreatedAt cannot be the default DateTime value.");
            }

            // Validate StartedAt vs CreatedAt
            if (value.StartedAt.HasValue)
            {
                if (value.StartedAt.Value < value.CreatedAt)
                {
                    errors.Add("StartedAt cannot be earlier than CreatedAt.");
                }

                if (value.StartedAt.Value == default)
                {
                    errors.Add("StartedAt cannot be the default DateTime value.");
                }
            }

            // Validate CompletedAt vs CreatedAt
            if (value.CompletedAt.HasValue)
            {
                if (value.CompletedAt.Value < value.CreatedAt)
                {
                    errors.Add("CompletedAt cannot be earlier than CreatedAt.");
                }

                if (value.CompletedAt.Value == default)
                {
                    errors.Add("CompletedAt cannot be the default DateTime value.");
                }
            }

            // Validate CompletedAt vs StartedAt
            if (value.StartedAt.HasValue && value.CompletedAt.HasValue)
            {
                if (value.CompletedAt.Value < value.StartedAt.Value)
                {
                    errors.Add("CompletedAt cannot be earlier than StartedAt.");
                }
            }

            // Validate ErrorMessage and StackTrace consistency
            if (value.State == JobState.Failed && string.IsNullOrWhiteSpace(value.ErrorMessage))
            {
                errors.Add("ErrorMessage must be set when State is Failed.");
            }

            if (value.State != JobState.Failed && !string.IsNullOrWhiteSpace(value.ErrorMessage))
            {
                errors.Add("ErrorMessage should only be set when State is Failed.");
            }

            if (value.State == JobState.Failed && string.IsNullOrWhiteSpace(value.StackTrace) && value.ErrorMessage != "Operation cancelled by user")
            {
                errors.Add("StackTrace should be set when State is Failed and error is not a cancellation.");
            }

            if (value.State != JobState.Failed && !string.IsNullOrWhiteSpace(value.StackTrace))
            {
                errors.Add("StackTrace should only be set when State is Failed.");
            }

            // Validate EstimatedTimeRemaining
            if (value.EstimatedTimeRemaining.HasValue && value.EstimatedTimeRemaining.Value < TimeSpan.Zero)
            {
                errors.Add("EstimatedTimeRemaining cannot be negative.");
            }

            // Validate Metadata
            if (value.Metadata == null)
            {
                errors.Add("Metadata cannot be null.");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether a background job is valid.
        /// </summary>
        /// <param name="value">The job to check.</param>
        /// <returns>True if the job is valid; otherwise, false.</returns>
        public static bool IsValid(this BackgroundJob value)
        {
            return value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that a background job is valid, throwing an exception if it is not.
        /// </summary>
        /// <param name="value">The job to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the job is invalid, containing a list of validation errors.</exception>
        public static void EnsureValid(this BackgroundJob value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = value.Validate();
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"BackgroundJob is invalid:{Environment.NewLine}- {
                        string.Join(
                            $"{Environment.NewLine}- ",
                            errors
                        )
                    }",
                    nameof(value));
            }
        }
    }
}
