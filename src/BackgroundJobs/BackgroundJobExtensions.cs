// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace FFmpegDotnetWrapper.BackgroundJobs
{
    /// <summary>
    /// Extension methods for <see cref="BackgroundJob"/> providing additional functionality
    /// for job management, state checking, and metadata operations.
    /// </summary>
    public static class BackgroundJobExtensions
    {
        /// <summary>
        /// Checks if the job is currently active (queued or processing).
        /// </summary>
        /// <param name="job">The background job to check</param>
        /// <returns>True if job is active, false otherwise</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="job"/> is null</exception>
        public static bool IsActive(this BackgroundJob job)
        {
            ArgumentNullException.ThrowIfNull(job);

            return job.State == JobState.Queued || job.State == JobState.Processing;
        }

        /// <summary>
        /// Checks if the job has completed successfully.
        /// </summary>
        /// <param name="job">The background job to check</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="job"/> is null</exception>
        /// <returns>True if job completed successfully, false otherwise</returns>
        public static bool IsCompletedSuccessfully(this BackgroundJob job)
        {
            ArgumentNullException.ThrowIfNull(job);

            return job.State == JobState.Completed && job.CompletedAt.HasValue;
        }

        /// <summary>
        /// Checks if the job has failed.
        /// </summary>
        /// <param name="job">The background job to check</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="job"/> is null</exception>
        /// <returns>True if job failed, false otherwise</returns>
        public static bool IsFailed(this BackgroundJob job)
        {
            ArgumentNullException.ThrowIfNull(job);

            return job.State == JobState.Failed && !string.IsNullOrEmpty(job.ErrorMessage);
        }

        /// <summary>
        /// Gets the job duration or estimated remaining time.
        /// Returns the execution time if completed, or estimated time remaining if still running.
        /// </summary>
        /// <param name="job">The background job</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="job"/> is null</exception>
        /// <returns>TimeSpan representing duration or estimated remaining time</returns>
        public static TimeSpan GetTimeInfo(this BackgroundJob job)
        {
            ArgumentNullException.ThrowIfNull(job);

            if (job.State == JobState.Completed && job.CompletedAt.HasValue)
            {
                return job.ExecutionTime;
            }

            return job.EstimatedTimeRemaining ?? TimeSpan.Zero;
        }

        /// <summary>
        /// Safely gets a metadata value by key with type conversion.
        /// </summary>
        /// <typeparam name="T">The expected type of the metadata value</typeparam>
        /// <param name="job">The background job</param>
        /// <param name="key">The metadata key</param>
        /// <param name="defaultValue">Default value if key doesn't exist or conversion fails</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="job"/> is null</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="key"/> is null or empty</exception>
        /// <returns>The converted metadata value or default</returns>
        public static T GetMetadataValue<T>(this BackgroundJob job, string key, T defaultValue = default)
        {
            ArgumentNullException.ThrowIfNull(job);

            ArgumentException.ThrowIfNullOrEmpty(key);

            if (job.Metadata is null || !job.Metadata.TryGetValue(key, out var value))
                return defaultValue;

            try
            {
                if (value is T typedValue)
                    return typedValue;

                // Handle common type conversions
                if (typeof(T) == typeof(int) && value is long longValue)
                    return (T)(object)Convert.ToInt32(longValue);

                if (typeof(T) == typeof(double))
                {
                    if (value is float floatValue)
                        return (T)(object)floatValue;

                    if (value is double doubleValue)
                        return (T)(object)doubleValue;

                    if (value is int intValue)
                        return (T)(object)intValue;

                    if (value is string stringValue &&
                        double.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedDouble))
                        return (T)(object)parsedDouble;
                }

                if (typeof(T) == typeof(string))
                    return (T)(object)(Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty);

                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Updates job progress with a formatted status message.
        /// Combines percentage update and status message in one call.
        /// </summary>
        /// <param name="job">The background job</param>
        /// <param name="percentage">Progress percentage (0-100)</param>
        /// <param name="format">Status message format string</param>
        /// <param name="args">Format arguments</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="job"/> is null</exception>
        public static void UpdateProgress(this BackgroundJob job, double percentage, string format, params object[] args)
        {
            ArgumentNullException.ThrowIfNull(job);

            job.ProgressPercentage = Math.Clamp(percentage, 0, 100);
            job.StatusMessage = string.Format(format, args);
        }

        /// <summary>
        /// Checks if the job is taking longer than expected based on creation time.
        /// </summary>
        /// <param name="job">The background job</param>
        /// <param name="threshold">Time threshold to consider "too long"</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="job"/> is null</exception>
        /// <returns>True if job has exceeded the threshold, false otherwise</returns>
        public static bool IsTakingTooLong(this BackgroundJob job, TimeSpan threshold)
        {
            ArgumentNullException.ThrowIfNull(job);

            if (job.State != JobState.Processing || !job.StartedAt.HasValue)
                return false;

            var elapsed = DateTime.UtcNow - job.StartedAt.Value;
            return elapsed > threshold;
        }

        /// <summary>
        /// Gets a formatted job summary including ID, name, state, and progress.
        /// </summary>
        /// <param name="job">The background job</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="job"/> is null</exception>
        /// <returns>Formatted job summary string</returns>
        public static string GetSummary(this BackgroundJob job)
        {
            ArgumentNullException.ThrowIfNull(job);

            return $"Job {job.JobId} - {job.JobName}: {job.State} ({job.ProgressPercentage:F1}%)";
        }
    }
}