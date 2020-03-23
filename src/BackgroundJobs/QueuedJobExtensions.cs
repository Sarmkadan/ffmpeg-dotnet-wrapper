using System;
using System.Collections.Generic;
using System.Globalization;

namespace FFmpegDotnetWrapper.BackgroundJobs
{
    /// <summary>
    /// Provides extension methods for the <see cref="QueuedJob"/> class.
    /// </summary>
    public static class QueuedJobExtensions
    {
        /// <summary>
        /// Gets a string representation of the job's status.
        /// </summary>
        /// <param name="job">The job to get the status for.</param>
        /// <returns>A string representation of the job's status.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="job"/> is null.</exception>
        public static string GetStatusString(this QueuedJob job)
        {
            ArgumentNullException.ThrowIfNull(job);

            return job.DueAt is null ? "No due date" : $"Due at {job.DueAt.Value.ToString("o", CultureInfo.InvariantCulture)}";
        }

        /// <summary>
        /// Determines whether the job is overdue.
        /// </summary>
        /// <param name="job">The job to check.</param>
        /// <returns>True if the job is overdue; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="job"/> is null.</exception>
        public static bool IsOverdue(this QueuedJob job)
        {
            ArgumentNullException.ThrowIfNull(job);

            return job.DueAt is not null && job.DueAt.Value < DateTime.UtcNow;
        }

        /// <summary>
        /// Gets a string representation of the job's retry information.
        /// </summary>
        /// <param name="job">The job to get the retry information for.</param>
        /// <returns>A string representation of the job's retry information.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="job"/> is null.</exception>
        public static string GetRetryInfoString(this QueuedJob job)
        {
            ArgumentNullException.ThrowIfNull(job);

            return $"Retried {job.RetryCount} times out of {job.MaxRetries}";
        }

        /// <summary>
        /// Determines whether the job has reached its maximum number of retries.
        /// </summary>
        /// <param name="job">The job to check.</param>
        /// <returns>True if the job has reached its maximum number of retries; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="job"/> is null.</exception>
        public static bool HasMaxRetries(this QueuedJob job)
        {
            ArgumentNullException.ThrowIfNull(job);

            return job.RetryCount >= job.MaxRetries;
        }
    }
}
