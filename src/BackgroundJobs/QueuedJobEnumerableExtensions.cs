using System;
using System.Collections.Generic;
using System.Linq;

namespace FFmpegDotnetWrapper.BackgroundJobs
{
    /// <summary>
    /// Provides ordering and filtering operations for sequences of <see cref="QueuedJob"/> instances.
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

        /// <summary>
        /// Filters queued jobs that are ready for execution (no due date or due date has passed).
        /// </summary>
        /// <param name="jobs">The queued jobs to filter.</param>
        /// <returns>
        /// A sequence containing jobs where <see cref="QueuedJob.DueAt"/> is null or in the past.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="jobs"/> is <see langword="null"/>.
        /// </exception>
        public static IEnumerable<QueuedJob> FilterPending(this IEnumerable<QueuedJob> jobs)
        {
            ArgumentNullException.ThrowIfNull(jobs);

            var now = DateTime.UtcNow;
            return jobs.Where(job => job.DueAt == null || job.DueAt <= now);
        }

        /// <summary>
        /// Groups queued jobs by their priority level.
        /// </summary>
        /// <param name="jobs">The queued jobs to group.</param>
        /// <returns>
        /// A sequence of groups, each containing jobs with the same priority level,
        /// ordered by priority (ascending, so priority 1 comes first).
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="jobs"/> is <see langword="null"/>.
        /// </exception>
        public static IEnumerable<IGrouping<int, QueuedJob>> GroupByPriority(this IEnumerable<QueuedJob> jobs)
        {
            ArgumentNullException.ThrowIfNull(jobs);

            return jobs.GroupBy(job => job.Priority).OrderBy(group => group.Key);
        }
    }
}
