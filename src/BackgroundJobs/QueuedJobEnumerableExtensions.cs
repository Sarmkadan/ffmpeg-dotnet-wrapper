using System;
using System.Collections.Generic;
using System.Linq;

namespace FFmpegDotnetWrapper.BackgroundJobs
{
    /// <summary>
    /// Provides ordering operations for sequences of <see cref="QueuedJob"/> instances.
    /// </summary>
    public static class QueuedJobEnumerableExtensions
    {
        /// <summary>
        /// Orders queued jobs by ascending priority and then by their enqueue time.
        /// </summary>
        /// <param name="jobs">The queued jobs to order.</param>
        /// <returns>
        /// A sequence whose elements are ordered by <see cref="QueuedJob.Priority"/> and then by
        /// <see cref="QueuedJob.EnqueuedAt"/>, both in ascending order.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="jobs"/> is <see langword="null"/>.
        /// </exception>
        public static IEnumerable<QueuedJob> OrderByPriorityThenEnqueued(this IEnumerable<QueuedJob> jobs)
        {
            ArgumentNullException.ThrowIfNull(jobs);

            return jobs.OrderBy(job => job.Priority).ThenBy(job => job.EnqueuedAt);
        }
    }
}
