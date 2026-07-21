// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FFmpegDotnetWrapper.BackgroundJobs;
using FFmpegDotnetWrapper.Events;
using Microsoft.Extensions.Logging;

namespace FFmpegDotnetWrapper.BackgroundJobs
{
    /// <summary>
    /// Represents the state of a background job at any point in time.
    /// Used to track job progress and lifecycle.
    /// </summary>
    public enum JobState
    {
        /// <summary>Job is queued and waiting to be processed.</summary>
        Queued,
        /// <summary>Job is currently being processed.</summary>
        Processing,
        /// <summary>Job has completed successfully.</summary>
        Completed,
        /// <summary>Job failed with an error.</summary>
        Failed,
        /// <summary>Job was cancelled before completion.</summary>
        Cancelled
    }

    /// <summary>
    /// Represents a single background job that can be tracked and managed.
    /// Jobs are typically video processing operations that may take significant time.
    /// </summary>
    public class BackgroundJob
    {
        /// <summary>Unique identifier for this job.</summary>
        public string JobId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>Human-readable name describing what the job does.</summary>
        public string JobName { get; set; } = string.Empty;

        /// <summary>Current state of the job (Queued, Processing, Completed, Failed, Cancelled).</summary>
        public JobState State { get; set; }

        /// <summary>Progress from 0 to 100.</summary>
        public double ProgressPercentage { get; set; }

        /// <summary>User-friendly status message explaining current state.</summary>
        public string StatusMessage { get; set; } = string.Empty;

        /// <summary>When the job was created.</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>When the job started processing (null if not started yet).</summary>
        public DateTime? StartedAt { get; set; }

        /// <summary>When the job completed (null if still running).</summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>Error message if the job failed.</summary>
        public string? ErrorMessage { get; set; }

        /// <summary>Stack trace if the job failed with an exception.</summary>
        public string? StackTrace { get; set; }

        /// <summary>Any custom data associated with the job.</summary>
        public Dictionary<string, object> Metadata { get; set; } = new();

        /// <summary>Estimated time remaining until completion.</summary>
        public TimeSpan? EstimatedTimeRemaining { get; set; }

        /// <summary>Total execution time so far.</summary>
        public TimeSpan ExecutionTime
        {
            get
            {
                var end = CompletedAt ?? DateTime.UtcNow;
                var start = StartedAt ?? CreatedAt;
                return end - start;
            }
        }
    }

    /// <summary>
    /// Service for managing background jobs with job tracking and lifecycle management.
    /// Stores job state in memory with optional persistence integration.
    /// Supports job cancellation and progress monitoring.
    /// </summary>
    public interface IBackgroundJobService
    {
        string EnqueueJob(string jobName, Func<CancellationToken, Task> jobWork, Dictionary<string, object>? metadata = null, int priority = JobPriority.Normal);
        Task<BackgroundJob?> GetJobAsync(string jobId);
        Task<IEnumerable<BackgroundJob>> GetActiveJobsAsync();
        Task<IEnumerable<BackgroundJob>> GetJobsAsync(JobState state);
        Task<bool> CancelJobAsync(string jobId);
        Task UpdateJobProgressAsync(string jobId, double percentage, string? statusMessage = null);
    }

    public class BackgroundJobService : IBackgroundJobService
    {
        private readonly ILogger<BackgroundJobService> _logger;
        private readonly IEventPublisher _eventPublisher;
    private readonly IJobQueue _jobQueue;
        private readonly Dictionary<string, BackgroundJob> _jobs = new();
        private readonly Dictionary<string, CancellationTokenSource> _cancellationTokens = new();
        private readonly object _lockObject = new();

        public BackgroundJobService(ILogger<BackgroundJobService> logger, IEventPublisher eventPublisher, IJobQueue jobQueue)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _jobQueue = jobQueue ?? throw new ArgumentNullException(nameof(jobQueue));
        }

        /// <summary>
        /// Enqueues a new background job for processing.
        /// Jobs are executed on a thread pool and can be tracked via their job ID.
        /// </summary>

        /// <summary>
        /// Retrieves job information by ID.
        /// Returns null if job doesn't exist or has been cleaned up.
        /// </summary>
    /// <summary>
    /// Enqueues a new background job for processing.
    /// Jobs are executed on a thread pool and can be tracked via their job ID.
    /// </summary>
    public string EnqueueJob(string jobName, Func<CancellationToken, Task> jobWork, Dictionary<string, object>? metadata = null, int priority = JobPriority.Normal)
    {
        if (string.IsNullOrEmpty(jobName))
            throw new ArgumentException("Job name cannot be empty", nameof(jobName));
        if (jobWork == null)
            throw new ArgumentNullException(nameof(jobWork));

        var job = new BackgroundJob
        {
            JobName = jobName,
            State = JobState.Queued,
            StatusMessage = "Waiting to be processed",
            Metadata = metadata ?? new()
        };

        var cts = new CancellationTokenSource();

        lock (_lockObject)
        {
            _jobs[job.JobId] = job;
            _cancellationTokens[job.JobId] = cts;
        }

        _logger.LogInformation("Job enqueued: {JobId} ({JobName}, Priority: {Priority})", job.JobId, jobName, priority);

        // Fire off the job on thread pool with priority-based scheduling
        _ = Task.Run(async () =>
        {
            try
            {
                // Wait for job to be dequeued based on priority
                var queuedJob = await _jobQueue.DequeueAsync();
                if (queuedJob?.JobId == job.JobId)
                {
                    await ProcessJobAsync(job, jobWork, cts.Token);
                }
                else
                {
                    _logger.LogWarning("Job {JobId} was not dequeued, marking as failed", job.JobId);
                    lock (_lockObject)
                    {
                        job.State = JobState.Failed;
                        job.ErrorMessage = "Job was not dequeued from priority queue";
                        job.StatusMessage = "Failed - not dequeued";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing job {JobId}", job.JobId);
                lock (_lockObject)
                {
                    job.State = JobState.Failed;
                    job.ErrorMessage = ex.Message;
                    job.StatusMessage = "Failed during processing";
                }
            }
        }, cts.Token);

        return job.JobId;
    }

        public Task<BackgroundJob?> GetJobAsync(string jobId)
        {
            lock (_lockObject)
            {
                _jobs.TryGetValue(jobId, out var job);
                return Task.FromResult(job);
            }
        }

        /// <summary>
        /// Gets all jobs that are currently running (not completed or failed).
        /// </summary>
        public Task<IEnumerable<BackgroundJob>> GetActiveJobsAsync()
        {
            lock (_lockObject)
            {
                var activeJobs = _jobs.Values
                    .Where(j => j.State == JobState.Queued || j.State == JobState.Processing)
                    .ToList();
                return Task.FromResult<IEnumerable<BackgroundJob>>(activeJobs);
            }
        }

        /// <summary>
        /// Gets all jobs in a specific state.
        /// Useful for querying completed jobs or failed jobs.
        /// </summary>
        public Task<IEnumerable<BackgroundJob>> GetJobsAsync(JobState state)
        {
            lock (_lockObject)
            {
                var jobs = _jobs.Values
                    .Where(j => j.State == state)
                    .ToList();
                return Task.FromResult<IEnumerable<BackgroundJob>>(jobs);
            }
        }

        /// <summary>
        /// Attempts to cancel a running job.
        /// Returns true if cancellation was requested, false if job already completed.
        /// </summary>
        public Task<bool> CancelJobAsync(string jobId)
        {
            lock (_lockObject)
            {
                if (!_cancellationTokens.TryGetValue(jobId, out var cts))
                    return Task.FromResult(false);

                if (!cts.IsCancellationRequested)
                {
                    cts.Cancel();
                    _logger.LogInformation("Job cancellation requested: {JobId}", jobId);
                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Updates a job's progress and status message.
        /// Typically called by the job itself to report progress.
        /// </summary>
        public Task UpdateJobProgressAsync(string jobId, double percentage, string? statusMessage = null)
        {
            lock (_lockObject)
            {
                if (_jobs.TryGetValue(jobId, out var job))
                {
                    job.ProgressPercentage = Math.Clamp(percentage, 0, 100);
                    if (!string.IsNullOrEmpty(statusMessage))
                        job.StatusMessage = statusMessage;
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Processes a job, handling its lifecycle and publishing events.
        /// Catches exceptions and marks job as failed if it throws.
        /// </summary>
        private async Task ProcessJobAsync(BackgroundJob job, Func<CancellationToken, Task> jobWork, CancellationToken cancellationToken)
        {
            try
            {
                // Update job state to Processing
                lock (_lockObject)
                {
                    job.State = JobState.Processing;
                    job.StartedAt = DateTime.UtcNow;
                    job.StatusMessage = "Processing...";
                }

                // Publish operation started event
                await _eventPublisher.PublishAsync(new OperationStartedEvent
                {
                    OperationType = job.JobName,
                    Source = "BackgroundJobService",
                    CorrelationId = job.JobId
                });

                // Execute the job
                await jobWork(cancellationToken);

                // Mark as completed
                lock (_lockObject)
                {
                    job.State = JobState.Completed;
                    job.CompletedAt = DateTime.UtcNow;
                    job.StatusMessage = "Completed successfully";
                    job.ProgressPercentage = 100;
                }

                _logger.LogInformation("Job completed: {JobId} ({JobName})", job.JobId, job.JobName);

                // Publish completion event
                await _eventPublisher.PublishAsync(new OperationCompletedEvent
                {
                    OperationType = job.JobName,
                    Duration = job.ExecutionTime,
                    Source = "BackgroundJobService",
                    CorrelationId = job.JobId
                });
            }
            catch (OperationCanceledException)
            {
                lock (_lockObject)
                {
                    job.State = JobState.Cancelled;
                    job.CompletedAt = DateTime.UtcNow;
                    job.StatusMessage = "Cancelled by user";
                }

                _logger.LogWarning("Job cancelled: {JobId} ({JobName})", job.JobId, job.JobName);
            }
            catch (Exception ex)
            {
                lock (_lockObject)
                {
                    job.State = JobState.Failed;
                    job.CompletedAt = DateTime.UtcNow;
                    job.ErrorMessage = ex.Message;
                    job.StackTrace = ex.StackTrace;
                    job.StatusMessage = "Failed with error";
                }

                _logger.LogError(ex, "Job failed: {JobId} ({JobName})", job.JobId, job.JobName);

                // Publish failure event
                await _eventPublisher.PublishAsync(new OperationFailedEvent
                {
                    OperationType = job.JobName,
                    ErrorMessage = ex.Message,
                    Source = "BackgroundJobService",
                    CorrelationId = job.JobId
                });
            }
        }
    }
}
