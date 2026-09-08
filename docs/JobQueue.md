# Background job queue

`IJobQueue` defines an in-memory API for enqueueing, inspecting, dequeuing, and removing background jobs. `JobQueue` implements that API with a priority queue and a job registry. `QueuedJob` stores the job's execution metadata, and `JobPriority` provides the predefined priority values.

Lower numeric priority values are processed before higher values. The implementation clamps priorities supplied to `EnqueueAsync` to the range 1 through 10.

## `IJobQueue`

| Member | Signature | Description |
| --- | --- | --- |
| Enqueue | `Task<string> EnqueueAsync(string payload, int priority = 5, TimeSpan? delay = null, Dictionary<string, string>? tags = null)` | Creates and queues a job, optionally with a priority, delay, and tags, and returns its generated ID. |
| Dequeue | `Task<QueuedJob?> DequeueAsync()` | Removes and returns the highest-priority job at the head of the queue when it is due. Returns `null` when the queue is empty or its head job is delayed. |
| Get job | `Task<QueuedJob?> GetJobAsync(string jobId)` | Looks up a job in the registry by ID, returning `null` when it is not registered. |
| Get pending jobs | `Task<List<QueuedJob>> GetPendingJobsAsync()` | Returns the registered jobs ordered by ascending numeric priority. |
| Remove job | `Task<bool> RemoveJobAsync(string jobId)` | Removes a job from the registry and reports whether it was found. See the queue/registry note below. |
| Get count | `Task<int> GetQueueCountAsync()` | Returns the number of entries in the priority queue. |

## `JobQueue`

`JobQueue` implements `IJobQueue` and adds retry and clearing operations.

| Member | Signature | Description |
| --- | --- | --- |
| Constructor | `JobQueue(ILogger<JobQueue> logger)` | Creates a queue using the supplied logger. |
| Enqueue | `Task<string> EnqueueAsync(string payload, int priority = 5, TimeSpan? delay = null, Dictionary<string, string>? tags = null)` | Implements `IJobQueue.EnqueueAsync`. A delay sets `DueAt` relative to the current UTC time. |
| Dequeue | `Task<QueuedJob?> DequeueAsync()` | Implements `IJobQueue.DequeueAsync`. The dequeued job remains in the registry. |
| Get job | `Task<QueuedJob?> GetJobAsync(string jobId)` | Implements `IJobQueue.GetJobAsync`. |
| Get pending jobs | `Task<List<QueuedJob>> GetPendingJobsAsync()` | Implements `IJobQueue.GetPendingJobsAsync`. |
| Remove job | `Task<bool> RemoveJobAsync(string jobId)` | Implements `IJobQueue.RemoveJobAsync`. It removes only the registry entry, not the priority-queue entry. |
| Get count | `Task<int> GetQueueCountAsync()` | Implements `IJobQueue.GetQueueCountAsync`. |
| Requeue | `Task RequeuJobAsync(QueuedJob job)` | Requeues a failed job unless it has reached `MaxRetries`. It increments `RetryCount` and enqueues the job using `job.Priority + 1` as the queue priority. The public method name is spelled `RequeuJobAsync`. |
| Clear | `void Clear()` | Drains the priority queue and clears the registry. |

## `QueuedJob`

| Member | Signature | Description |
| --- | --- | --- |
| Job ID | `string JobId { get; set; }` | Job identifier. Defaults to a new GUID formatted as a string. |
| Priority | `int Priority { get; set; }` | Stored job priority. Defaults to `5`; lower values represent higher priority. |
| Enqueued time | `DateTime EnqueuedAt { get; set; }` | UTC creation time, initialized when the object is constructed. |
| Due time | `DateTime? DueAt { get; set; }` | Earliest intended execution time, or `null` for no delay. |
| Retry count | `int RetryCount { get; set; }` | Number of retries performed. Defaults to `0`. |
| Maximum retries | `int MaxRetries { get; set; }` | Retry limit. Defaults to `3`. |
| Payload | `string Payload { get; set; }` | Job data. Defaults to an empty string. |
| Tags | `Dictionary<string, string> Tags { get; set; }` | Mutable job metadata. Defaults to an empty dictionary. |

## `JobPriority`

`JobPriority` is declared in the global namespace, while the queue types above are in `FFmpegDotnetWrapper.BackgroundJobs`.

| Member | Signature | Description |
| --- | --- | --- |
| High | `const int High = 1` | Highest predefined priority. |
| Normal | `const int Normal = 5` | Normal predefined priority. |
| Low | `const int Low = 10` | Lowest predefined priority. |

## Example

```csharp
using System;
using System.Collections.Generic;
using FFmpegDotnetWrapper.BackgroundJobs;
using Microsoft.Extensions.Logging;

ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
IJobQueue queue = new JobQueue(loggerFactory.CreateLogger<JobQueue>());

string jobId = await queue.EnqueueAsync(
    payload: "encode input.mp4",
    priority: JobPriority.High,
    tags: new Dictionary<string, string> { ["format"] = "mp4" });

QueuedJob? job = await queue.DequeueAsync();
if (job is not null)
{
    Console.WriteLine($"Processing {job.JobId}: {job.Payload}");
    await queue.RemoveJobAsync(jobId);
}
```

## Thread safety and exceptions

- `JobQueue` serializes access to its internal priority queue and registry with a private lock. Its methods complete synchronously and return already-completed tasks.
- Returned `QueuedJob` instances and their mutable `Tags` dictionaries are not copied or protected by the queue's lock. Callers that share or mutate them concurrently must provide their own synchronization.
- `GetPendingJobsAsync` is based on registry contents, whereas `GetQueueCountAsync` is based on priority-queue contents. Dequeueing does not remove a registry entry, and `RemoveJobAsync` does not remove a priority-queue entry, so these views can differ.
- A delayed job at the head of the priority queue causes `DequeueAsync` to return `null`; the method does not scan lower-priority entries for a ready job.
- `RequeuJobAsync` calculates and logs an exponential backoff duration but does not update `DueAt`. It also changes the priority used by the internal queue without changing the job's `Priority` property.
- The constructor throws `ArgumentNullException` when `logger` is `null`. `EnqueueAsync` throws `ArgumentException` when `payload` is `null` or empty. `RequeuJobAsync` throws `ArgumentNullException` when `job` is `null`.
