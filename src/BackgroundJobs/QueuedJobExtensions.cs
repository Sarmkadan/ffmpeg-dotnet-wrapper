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
        /// <returns>A string representation of the job's status.
        /// Returns "No due date" if <see cref="QueuedJob.DueAt"/> is null,
        /// otherwise returns the due date in ISO 8601 format.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="job"/> is null.</exception>
        public static string GetStatusString(this QueuedJob job)
        {
            ArgumentNullException.ThrowIfNull(job);

            return job.DueAt is null
                ? "No due date"
                : $"Due at {job.DueAt.Value.ToString("o", CultureInfo.InvariantCulture)}";
        }

        /// <summary>
        /// Determines whether the job is overdue.
        /// </summary>
        /// <param name="job">The job to check.</param>
        /// <returns>True if the job is overdue; otherwise, false.
        /// A job is overdue when <see cref="QueuedJob.DueAt"/> is not null and the due date is in the past.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="job"/> is null.</exception>
        public static bool IsOverdue(this QueuedJob job)
        {
            ArgumentNullException.ThrowIfNull(job);

            return job.DueAt is { } dueAt && dueAt < DateTime.UtcNow;
        }

        /// <summary>
        /// Gets a string representation of the job's retry information.
        /// </summary>
        /// <param name="job">The job to get the retry information for.</param>
        /// <returns>A string representation of the job's retry information.
        /// Format: "Retried {RetryCount} times out of {MaxRetries}".</returns>
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
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <see cref="QueuedJob.MaxRetries"/> is negative.</exception>
        public static bool HasMaxRetries(this QueuedJob job)
        {
            ArgumentNullException.ThrowIfNull(job);

            if (job.MaxRetries < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(job.MaxRetries), "MaxRetries cannot be negative.");
            }

            return job.RetryCount >= job.MaxRetries;
        }
    }
}
