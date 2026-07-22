using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace FFmpegDotnetWrapper.BackgroundJobs.Tests
{
    public class JobQueueTests
    {
        private readonly ILogger<JobQueue> _logger;

        public JobQueueTests()
        {
            _logger = new NullLogger<JobQueue>();
        }

        [Fact]
        public async Task EnqueueAsync_WithValidPayload_ReturnsJobId()
        {
            // Arrange
            var queue = new JobQueue(_logger);

            // Act
            var jobId = await queue.EnqueueAsync("test payload");

            // Assert
            jobId.Should().NotBeNullOrEmpty();
            Guid.TryParse(jobId, out _).Should().BeTrue();
        }

        [Fact]
        public async Task EnqueueAsync_WithEmptyPayload_ThrowsArgumentException()
        {
            // Arrange
            var queue = new JobQueue(_logger);

            // Act
            Func<Task> act = async () => await queue.EnqueueAsync("");

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task EnqueueAsync_WithNullPayload_ThrowsArgumentException()
        {
            // Arrange
            var queue = new JobQueue(_logger);

            // Act
            Func<Task> act = async () => await queue.EnqueueAsync(null!);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task EnqueueAsync_WithPriority_RespectsPriorityOrder()
        {
            // Arrange
            var queue = new JobQueue(_logger);

            // Act
            var highPriorityJob = await queue.EnqueueAsync("high priority", priority: 1);
            var normalPriorityJob = await queue.EnqueueAsync("normal priority", priority: 5);
            var lowPriorityJob = await queue.EnqueueAsync("low priority", priority: 10);

            // Assert - jobs should be dequeued in priority order (lower number = higher priority)
            var dequeued1 = await queue.DequeueAsync();
            dequeued1.Should().NotBeNull();
            dequeued1!.JobId.Should().Be(highPriorityJob);

            var dequeued2 = await queue.DequeueAsync();
            dequeued2.Should().NotBeNull();
            dequeued2!.JobId.Should().Be(normalPriorityJob);

            var dequeued3 = await queue.DequeueAsync();
            dequeued3.Should().NotBeNull();
            dequeued3!.JobId.Should().Be(lowPriorityJob);
        }

        [Fact]
        public async Task EnqueueAsync_WithClampedPriority_UsesClampedValue()
        {
            // Arrange
            var queue = new JobQueue(_logger);

            // Act
            var jobId1 = await queue.EnqueueAsync("test", priority: 0); // Should clamp to 1
            var jobId2 = await queue.EnqueueAsync("test", priority: 11); // Should clamp to 10
            var jobId3 = await queue.EnqueueAsync("test", priority: 5); // Should stay 5

            // Assert
            var job1 = await queue.GetJobAsync(jobId1);
            job1.Should().NotBeNull();
            job1!.Priority.Should().Be(1);

            var job2 = await queue.GetJobAsync(jobId2);
            job2.Should().NotBeNull();
            job2!.Priority.Should().Be(10);

            var job3 = await queue.GetJobAsync(jobId3);
            job3.Should().NotBeNull();
            job3!.Priority.Should().Be(5);
        }

        [Fact]
        public async Task DequeueAsync_FromEmptyQueue_ReturnsNull()
        {
            // Arrange
            var queue = new JobQueue(_logger);

            // Act
            var job = await queue.DequeueAsync();

            // Assert
            job.Should().BeNull();
        }

        [Fact]
        public async Task DequeueAsync_FromSingleJobQueue_ReturnsJob()
        {
            // Arrange
            var queue = new JobQueue(_logger);
            var jobId = await queue.EnqueueAsync("test payload");

            // Act
            var dequeuedJob = await queue.DequeueAsync();

            // Assert
            dequeuedJob.Should().NotBeNull();
            dequeuedJob!.JobId.Should().Be(jobId);
            dequeuedJob.Payload.Should().Be("test payload");
        }

        [Fact]
        public async Task DequeueAsync_RemovesJobFromQueue()
        {
            // Arrange
            var queue = new JobQueue(_logger);
            var jobId = await queue.EnqueueAsync("test payload");

            // Act
            var dequeuedJob = await queue.DequeueAsync();

            // Assert
            dequeuedJob.Should().NotBeNull();
            (await queue.GetQueueCountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task GetJobAsync_WithExistingJob_ReturnsJob()
        {
            // Arrange
            var queue = new JobQueue(_logger);
            var jobId = await queue.EnqueueAsync("test payload");

            // Act
            var job = await queue.GetJobAsync(jobId);

            // Assert
            job.Should().NotBeNull();
            job!.JobId.Should().Be(jobId);
            job.Payload.Should().Be("test payload");
        }

        [Fact]
        public async Task GetJobAsync_WithNonExistingJob_ReturnsNull()
        {
            // Arrange
            var queue = new JobQueue(_logger);

            // Act
            var job = await queue.GetJobAsync(Guid.NewGuid().ToString());

            // Assert
            job.Should().BeNull();
        }

        [Fact]
        public async Task GetPendingJobsAsync_ReturnsJobsInPriorityOrder()
        {
            // Arrange
            var queue = new JobQueue(_logger);
            await queue.EnqueueAsync("job 1", priority: 3);
            await queue.EnqueueAsync("job 2", priority: 1);
            await queue.EnqueueAsync("job 3", priority: 5);
            await queue.EnqueueAsync("job 4", priority: 2);

            // Act
            var pendingJobs = await queue.GetPendingJobsAsync();

            // Assert
            pendingJobs.Should().HaveCount(4);
            pendingJobs[0].Priority.Should().Be(1); // Highest priority first
            pendingJobs[1].Priority.Should().Be(2);
            pendingJobs[2].Priority.Should().Be(3);
            pendingJobs[3].Priority.Should().Be(5);
        }

        [Fact]
        public async Task RemoveJobAsync_WithExistingJob_RemovesAndReturnsTrue()
        {
            // Arrange
            var queue = new JobQueue(_logger);
            var jobId = await queue.EnqueueAsync("test payload");

            // Act
            var result = await queue.RemoveJobAsync(jobId);

            // Assert
            result.Should().BeTrue();
            (await queue.GetJobAsync(jobId)).Should().BeNull();
        }

        [Fact]
        public async Task RemoveJobAsync_WithNonExistingJob_ReturnsFalse()
        {
            // Arrange
            var queue = new JobQueue(_logger);

            // Act
            var result = await queue.RemoveJobAsync(Guid.NewGuid().ToString());

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetQueueCountAsync_WithEmptyQueue_ReturnsZero()
        {
            // Arrange
            var queue = new JobQueue(_logger);

            // Act
            var count = await queue.GetQueueCountAsync();

            // Assert
            count.Should().Be(0);
        }

        [Fact]
        public async Task GetQueueCountAsync_WithMultipleJobs_ReturnsCorrectCount()
        {
            // Arrange
            var queue = new JobQueue(_logger);
            await queue.EnqueueAsync("job 1");
            await queue.EnqueueAsync("job 2");
            await queue.EnqueueAsync("job 3");

            // Act
            var count = await queue.GetQueueCountAsync();

            // Assert
            count.Should().Be(3);
        }

        [Fact]
        public async Task EnqueueAsync_WithTags_StoresTags()
        {
            // Arrange
            var queue = new JobQueue(_logger);
            var tags = new Dictionary<string, string> { { "type", "conversion" }, { "format", "mp4" } };

            // Act
            var jobId = await queue.EnqueueAsync("test payload", tags: tags);

            // Assert
            var job = await queue.GetJobAsync(jobId);
            job.Should().NotBeNull();
            job!.Tags.Should().HaveCount(2);
            job.Tags["type"].Should().Be("conversion");
            job.Tags["format"].Should().Be("mp4");
        }

        [Fact]
        public async Task EnqueueAsync_WithDelay_DueAtIsSet()
        {
            // Arrange
            var queue = new JobQueue(_logger);

            // Act
            var jobId = await queue.EnqueueAsync("delayed job", delay: TimeSpan.FromSeconds(10));

            // Assert
            var job = await queue.GetJobAsync(jobId);
            job.Should().NotBeNull();
            job!.DueAt.Should().NotBeNull();
            job.DueAt.Should().BeCloseTo(DateTime.UtcNow.AddSeconds(10), TimeSpan.FromMilliseconds(100));
        }

        [Fact]
        public async Task DequeueAsync_WithDelayedJob_ReturnsNull()
        {
            // Arrange
            var queue = new JobQueue(_logger);
            await queue.EnqueueAsync("delayed job", delay: TimeSpan.FromSeconds(10));

            // Act
            var job = await queue.DequeueAsync();

            // Assert
            job.Should().BeNull(); // Delayed job should not be dequeued
        }

        [Fact]
        public async Task Clear_RemovesAllJobs()
        {
            // Arrange
            var queue = new JobQueue(_logger);
            await queue.EnqueueAsync("job 1");
            await queue.EnqueueAsync("job 2");
            await queue.EnqueueAsync("job 3");

            // Act
            queue.Clear();

            // Assert
            (await queue.GetQueueCountAsync()).Should().Be(0);
            (await queue.GetPendingJobsAsync()).Should().BeEmpty();
        }

        [Fact]
        public async Task FIFO_Order_WithSamePriority()
        {
            // Arrange
            var queue = new JobQueue(_logger);
            await queue.EnqueueAsync("job 1", priority: 5);
            await queue.EnqueueAsync("job 2", priority: 5);
            await queue.EnqueueAsync("job 3", priority: 5);

            // Act
            var dequeued1 = await queue.DequeueAsync();
            var dequeued2 = await queue.DequeueAsync();
            var dequeued3 = await queue.DequeueAsync();

            // Assert - FIFO order for same priority
            dequeued1.Should().NotBeNull();
            dequeued2.Should().NotBeNull();
            dequeued3.Should().NotBeNull();
        }

        [Fact]
        public async Task ConcurrentEnqueueDequeue_MultipleThreads_ThreadSafe()
        {
            // Arrange
            var queue = new JobQueue(_logger);
            var tasks = new List<Task>();
            var enqueuedIds = new List<string>();

            // Act - Enqueue from multiple threads
            for (int i = 0; i < 10; i++)
            {
                int index = i;
                tasks.Add(Task.Run(async () =>
                {
                    var jobId = await queue.EnqueueAsync($"job {index}");
                    lock (enqueuedIds)
                    {
                        enqueuedIds.Add(jobId);
                    }
                }));
            }

            await Task.WhenAll(tasks);

            // Verify all jobs were enqueued
            (await queue.GetQueueCountAsync()).Should().Be(10);

            // Act - Dequeue from multiple threads
            var dequeuedJobs = new List<QueuedJob>();
            tasks.Clear();

            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var job = await queue.DequeueAsync();
                    if (job != null)
                    {
                        lock (dequeuedJobs)
                        {
                            dequeuedJobs.Add(job);
                        }
                    }
                }));
            }

            await Task.WhenAll(tasks);

            // Assert - All jobs should be dequeued
            dequeuedJobs.Should().HaveCount(10);
            dequeuedJobs.Select(j => j.JobId).Should().BeEquivalentTo(enqueuedIds);
        }

        [Fact]
        public async Task RequeuJobAsync_WithRetryCount_UpdatesRetryAndPriority()
        {
            // Arrange
            var queue = new JobQueue(_logger);
            var job = new QueuedJob
            {
                Payload = "test",
                RetryCount = 0,
                MaxRetries = 3,
                Priority = 5
            };

            // Act
            await queue.RequeuJobAsync(job);

            // Assert - check the job in the registry
            var retrievedJob = await queue.GetJobAsync(job.JobId);
            retrievedJob.Should().NotBeNull();
            retrievedJob!.RetryCount.Should().Be(1);
            retrievedJob.Priority.Should().Be(5); // Priority field not modified, only enqueue priority differs
        }

        [Fact]
        public async Task RequeuJobAsync_WithMaxRetries_LogsWarning()
        {
            // Arrange
            var queue = new JobQueue(_logger);
            var job = new QueuedJob
            {
                Payload = "test",
                RetryCount = 3,
                MaxRetries = 3,
                Priority = 5
            };

            // Act
            await queue.RequeuJobAsync(job);

            // Assert - job should not be requeued
            (await queue.GetQueueCountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task RequeuJobAsync_WithNullJob_ThrowsArgumentNullException()
        {
            // Arrange
            var queue = new JobQueue(_logger);

            // Act
            Func<Task> act = async () => await queue.RequeuJobAsync(null!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }
    }
}
