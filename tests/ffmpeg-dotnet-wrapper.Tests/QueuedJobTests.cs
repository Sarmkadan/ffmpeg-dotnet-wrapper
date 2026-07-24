using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FFmpegDotnetWrapper.BackgroundJobs;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace FFmpegDotnetWrapper.Tests;

public class QueuedJobTests
{
    private readonly JobQueue _queue;

    public QueuedJobTests()
    {
        _queue = new JobQueue(NullLogger<JobQueue>.Instance);
    }

    [Fact]
    public async Task EnqueueAsync_HappyPath_ShouldEnqueueJob()
    {
        var tags = new Dictionary<string, string> { { "key", "value" } };
        var jobId = await _queue.EnqueueAsync("payload", priority: 3, delay: TimeSpan.FromSeconds(1), tags: tags);

        Assert.False(string.IsNullOrWhiteSpace(jobId));

        var job = await _queue.GetJobAsync(jobId);
        Assert.NotNull(job);
        Assert.Equal(jobId, job!.JobId);
        Assert.Equal(3, job.Priority);
        Assert.Equal("payload", job.Payload);
        Assert.Equal(tags, job.Tags);
        Assert.NotNull(job.DueAt);
        Assert.True(job.DueAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task EnqueueAsync_EmptyPayload_ShouldThrow()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _queue.EnqueueAsync(string.Empty));
    }

    [Fact]
    public async Task EnqueueAsync_PriorityClamping_ShouldClampValues()
    {
        var idLow = await _queue.EnqueueAsync("p1", priority: 0);
        var jobLow = await _queue.GetJobAsync(idLow);
        Assert.Equal(1, jobLow!.Priority);

        var idHigh = await _queue.EnqueueAsync("p2", priority: 15);
        var jobHigh = await _queue.GetJobAsync(idHigh);
        Assert.Equal(10, jobHigh!.Priority);
    }

    [Fact]
    public async Task DequeueAsync_NoJobs_ShouldReturnNull()
    {
        var job = await _queue.DequeueAsync();
        Assert.Null(job);
    }

    [Fact]
    public async Task DequeueAsync_DelayedJob_ShouldNotReturn()
    {
        await _queue.EnqueueAsync("delayed", delay: TimeSpan.FromSeconds(5));
        var job = await _queue.DequeueAsync();
        Assert.Null(job);
    }

    [Fact]
    public async Task DequeueAsync_ReadyJob_ShouldReturnJob()
    {
        var id = await _queue.EnqueueAsync("ready");
        var job = await _queue.DequeueAsync();
        Assert.NotNull(job);
        Assert.Equal(id, job!.JobId);
    }

    [Fact]
    public async Task PriorityOrdering_ShouldReturnHighestPriorityFirst()
    {
        await _queue.EnqueueAsync("low", priority: 10);
        await _queue.EnqueueAsync("high", priority: 1);

        var first = await _queue.DequeueAsync();
        var second = await _queue.DequeueAsync();

        Assert.Equal("high", first!.Payload);
        Assert.Equal("low", second!.Payload);
    }

    [Fact]
    public async Task GetPendingJobsAsync_ShouldReturnSortedByPriority()
    {
        await _queue.EnqueueAsync("mid", priority: 5);
        await _queue.EnqueueAsync("high", priority: 1);
        await _queue.EnqueueAsync("low", priority: 10);

        var pending = await _queue.GetPendingJobsAsync();

        Assert.Equal(3, pending.Count);
        Assert.Equal("high", pending[0].Payload);
        Assert.Equal("mid", pending[1].Payload);
        Assert.Equal("low", pending[2].Payload);
    }

    [Fact]
    public async Task RemoveJobAsync_Existing_ShouldReturnTrue()
    {
        var id = await _queue.EnqueueAsync("toRemove");
        var removed = await _queue.RemoveJobAsync(id);
        Assert.True(removed);

        var job = await _queue.GetJobAsync(id);
        Assert.Null(job);
    }

    [Fact]
    public async Task RemoveJobAsync_NonExisting_ShouldReturnFalse()
    {
        var removed = await _queue.RemoveJobAsync("nonexistent");
        Assert.False(removed);
    }

    [Fact]
    public async Task RequeuJobAsync_ShouldIncrementRetryAndRequeue()
    {
        var id = await _queue.EnqueueAsync("retry");
        var job = await _queue.GetJobAsync(id);
        Assert.NotNull(job);
        Assert.Equal(0, job!.RetryCount);

        await _queue.RequeuJobAsync(job);

        var requeued = await _queue.GetJobAsync(id);
        Assert.NotNull(requeued);
        Assert.Equal(1, requeued!.RetryCount);
        Assert.Equal(job.Priority + 1, requeued.Priority);
    }

    [Fact]
    public async Task RequeuJobAsync_ExceedsMaxRetries_ShouldNotRequeue()
    {
        var id = await _queue.EnqueueAsync("maxRetry");
        var job = await _queue.GetJobAsync(id);
        Assert.NotNull(job);
        job!.MaxRetries = 1;

        // First retry
        await _queue.RequeuJobAsync(job);
        Assert.Equal(1, job.RetryCount);

        // Second retry should be ignored
        await _queue.RequeuJobAsync(job);
        Assert.Equal(1, job.RetryCount); // still 1

        // Job should still be in registry
        var existing = await _queue.GetJobAsync(id);
        Assert.NotNull(existing);
    }
}
