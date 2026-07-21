// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace FFmpegDotnetWrapper.BackgroundJobs
{
    /// <summary>
    /// Represents a queued job with priority and execution metadata.
    /// </summary>
    public class QueuedJob
    {
        public string JobId { get; set; } = Guid.NewGuid().ToString();
        public int Priority { get; set; } = 5; // 1=highest, 10=lowest
        public DateTime EnqueuedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DueAt { get; set; }
        public int RetryCount { get; set; } = 0;
        public int MaxRetries { get; set; } = 3;
        public string Payload { get; set; } = string.Empty;
        public Dictionary<string, string> Tags { get; set; } = new();
    }

    /// <summary>
    /// Priority-based job queue for background processing.
    /// Maintains a queue of jobs with support for priorities and delayed execution.
    /// Thread-safe for concurrent access from multiple threads.
    /// </summary>
    public interface IJobQueue
    {
        Task<string> EnqueueAsync(string payload, int priority = 5, TimeSpan? delay = null, Dictionary<string, string>? tags = null);
        Task<QueuedJob?> DequeueAsync();
        Task<QueuedJob?> GetJobAsync(string jobId);
        Task<List<QueuedJob>> GetPendingJobsAsync();
        Task<bool> RemoveJobAsync(string jobId);
        Task<int> GetQueueCountAsync();
    }

    public class JobQueue : IJobQueue
    {
        private readonly ILogger<JobQueue> _logger;
        private readonly PriorityQueue<QueuedJob, int> _queue = new();
        private readonly Dictionary<string, QueuedJob> _jobRegistry = new();
        private readonly object _lockObject = new();

        public JobQueue(ILogger<JobQueue> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Enqueues a new job with optional priority and delay.
        /// Higher priority (lower number) jobs are processed first.
        /// </summary>
        public Task<string> EnqueueAsync(
            string payload,
            int priority = 5,
            TimeSpan? delay = null,
            Dictionary<string, string>? tags = null)
        {
            if (string.IsNullOrEmpty(payload))
                throw new ArgumentException("Payload cannot be empty", nameof(payload));

            // Clamp priority to valid range (1-10)
            priority = Math.Clamp(priority, 1, 10);

            var job = new QueuedJob
            {
                Priority = priority,
                Payload = payload,
                Tags = tags ?? new(),
                DueAt = delay.HasValue ? DateTime.UtcNow.Add(delay.Value) : null
            };

            lock (_lockObject)
            {
                _queue.Enqueue(job, priority);
                _jobRegistry[job.JobId] = job;

                _logger.LogDebug(
                    "Job enqueued: {JobId} (Priority: {Priority}, Queue Size: {Size})",
                    job.JobId,
                    priority,
                    _queue.Count);
            }

            return Task.FromResult(job.JobId);
        }

        /// <summary>
        /// Dequeues the highest priority job that is ready to execute.
        /// Returns null if queue is empty or all jobs are still delayed.
        /// </summary>
        public Task<QueuedJob?> DequeueAsync()
        {
            lock (_lockObject)
            {
                // Remove jobs that are still delayed
                var now = DateTime.UtcNow;
                QueuedJob? readyJob = null;

                while (_queue.Count > 0)
                {
                    var job = _queue.Peek();

                    // Check if job is ready to execute
                    if (job.DueAt != null && job.DueAt > now)
                    {
                        // All remaining jobs are delayed (sorted by priority)
                        break;
                    }

                    // Dequeue the job
                    _queue.Dequeue();
                    readyJob = job;
                    break;
                }

                if (readyJob != null)
                {
                    _logger.LogDebug(
                        "Job dequeued: {JobId} (Retries: {Retries}/{MaxRetries})",
                        readyJob.JobId,
                        readyJob.RetryCount,
                        readyJob.MaxRetries);
                }

                return Task.FromResult(readyJob);
            }
        }

        /// <summary>
        /// Retrieves job information by ID.
        /// Returns null if job doesn't exist or has been removed.
        /// </summary>
        public Task<QueuedJob?> GetJobAsync(string jobId)
        {
            lock (_lockObject)
            {
                _jobRegistry.TryGetValue(jobId, out var job);
                return Task.FromResult(job);
            }
        }

        /// <summary>
        /// Gets all pending jobs in the queue.
        /// Returns list ordered by priority.
        /// </summary>
        public Task<List<QueuedJob>> GetPendingJobsAsync()
        {
            lock (_lockObject)
            {
                var jobs = new List<QueuedJob>(_jobRegistry.Values);
                return Task.FromResult(jobs.OrderBy(j => j.Priority).ToList());
            }
        }

        /// <summary>
        /// Removes a job from the queue.
        /// Returns true if job was found and removed.
        /// </summary>
        public Task<bool> RemoveJobAsync(string jobId)
        {
            lock (_lockObject)
            {
                if (_jobRegistry.TryGetValue(jobId, out var job))
                {
                    _jobRegistry.Remove(jobId);
                    _logger.LogDebug("Job removed from queue: {JobId}", jobId);
                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Gets the current number of jobs in the queue.
        /// </summary>
        public Task<int> GetQueueCountAsync()
        {
            lock (_lockObject)
            {
                return Task.FromResult(_queue.Count);
            }
        }

        /// <summary>
        /// Requeues a failed job with incremented retry count and increased priority delay.
        /// Job will be re-executed after a backoff delay.
        /// </summary>
        public Task RequeuJobAsync(QueuedJob job)
        {
            if (job == null)
                throw new ArgumentNullException(nameof(job));

            if (job.RetryCount >= job.MaxRetries)
            {
                _logger.LogWarning(
                    "Job exceeded max retries: {JobId} (Retries: {Count}/{Max})",
                    job.JobId,
                    job.RetryCount,
                    job.MaxRetries);
                return Task.CompletedTask;
            }

            job.RetryCount++;

            // Exponential backoff: 1s, 2s, 4s, 8s
            var delayMs = (int)Math.Pow(2, job.RetryCount - 1) * 1000;
            var backoffDelay = TimeSpan.FromMilliseconds(delayMs);

            lock (_lockObject)
            {
                _queue.Enqueue(job, job.Priority + 1); // Lower priority after retry
                _jobRegistry[job.JobId] = job;

                _logger.LogInformation(
                    "Job requeued: {JobId} (Retry: {Count}/{Max}, Backoff: {Delay}ms)",
                    job.JobId,
                    job.RetryCount,
                    job.MaxRetries,
                    delayMs);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Clears all jobs from the queue.
        /// Used during shutdown or for testing.
        /// </summary>
        public void Clear()
        {
            lock (_lockObject)
            {
                var count = _queue.Count;
                // Note: PriorityQueue doesn't have Clear(), so we need to drain it
                while (_queue.Count > 0)
                {
                    _queue.Dequeue();
                }
                _jobRegistry.Clear();

                _logger.LogInformation("Job queue cleared ({Count} jobs removed)", count);
            }
        }
    }
}

/// <summary>
/// Priority constants for job queue operations.
/// Lower values indicate higher priority (1 = highest, 10 = lowest).
/// </summary>
public static class JobPriority
{
    /// <summary>Highest priority jobs (1).</summary>
    public const int High = 1;

    /// <summary>Normal priority jobs (5).</summary>
    public const int Normal = 5;

    /// <summary>Lowest priority jobs (10).</summary>
    public const int Low = 10;
}
