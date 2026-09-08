# BackgroundJobService

`IBackgroundJobService` defines the API for submitting, tracking, cancelling, and pruning asynchronous work. `BackgroundJobService` is its in-memory implementation: it creates a [`BackgroundJob`](BackgroundJob.md) record for each submission, dispatches work through `IJobQueue`, supports cooperative cancellation, and publishes operation lifecycle events.

Job records are retained in memory until `PruneCompletedJobs` removes terminal jobs. Consequently, records do not survive application restarts. Access to the service's job and cancellation-token collections is synchronized, although callers should still treat the mutable `BackgroundJob` values and their `Metadata` dictionaries with care.

## Job states

`JobState` describes the lifecycle of a job:

| Value | Meaning |
|---|---|
| `Queued` | The job is waiting to be dispatched. |
| `Processing` | The delegate is currently running. |
| `Completed` | The delegate finished successfully. |
| `Failed` | Dispatch or execution failed. Error details are stored on the job. |
| `Cancelled` | The delegate observed cancellation and threw `OperationCanceledException`. |

Cancellation is cooperative: `CancelJobAsync` signals the job's `CancellationTokenSource`; the supplied delegate must observe its token for execution to stop and the state to become `Cancelled`.

## API

| Method | Purpose | Parameters | Returns / behavior |
|---|---|---|---|
| `EnqueueJob` | Creates a queued job and schedules its delegate according to queue priority. | `string jobName`, `Func<CancellationToken, Task> jobWork`, optional `Dictionary<string, object>? metadata`, optional `int priority` (defaults to `JobPriority.Normal`). | The new job ID. Throws `ArgumentException` for a null or empty name and `ArgumentNullException` for a null delegate. |
| `GetJobAsync` | Looks up a job by ID. | `string jobId` | `Task<BackgroundJob?>`; returns `null` when the ID is unknown or has been pruned. |
| `GetActiveJobsAsync` | Gets jobs that have not reached a terminal state. | None | Jobs whose state is `Queued` or `Processing`. |
| `GetJobsAsync` | Filters retained jobs by one state. | `JobState state` | All currently retained jobs whose state equals `state`. |
| `CancelJobAsync` | Requests cooperative cancellation. | `string jobId` | `true` when cancellation is signalled for the first time; `false` when the ID has no cancellation token or cancellation was already requested. |
| `UpdateJobProgressAsync` | Updates a retained job's progress and, optionally, its status text. | `string jobId`, `double percentage`, optional `string? statusMessage` | A completed `Task`. Percentage is clamped to `0`–`100`; a null or empty message leaves the existing message unchanged. Unknown IDs are ignored. |
| `PruneCompletedJobs` | Removes sufficiently old terminal jobs and disposes their cancellation tokens. | `TimeSpan olderThan` | Number of removed jobs. Only `Completed`, `Failed`, or `Cancelled` jobs with `CompletedAt` earlier than the cutoff qualify. Throws `ArgumentOutOfRangeException` for a negative age. |

`BackgroundJobService` is registered as the singleton implementation of `IBackgroundJobService` by the library's application-startup registration. Its constructor requires `ILogger<BackgroundJobService>`, `IEventPublisher`, and `IJobQueue`.

## Example

```csharp
using FFmpegDotnetWrapper.BackgroundJobs;
using Microsoft.Extensions.DependencyInjection;

var jobs = serviceProvider.GetRequiredService<IBackgroundJobService>();

string jobId = jobs.EnqueueJob(
    "Create preview",
    async cancellationToken =>
    {
        // Pass cancellationToken to all cancellable asynchronous work.
        await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    },
    metadata: new Dictionary<string, object>
    {
        ["Input"] = "video.mp4"
    });

BackgroundJob? job = await jobs.GetJobAsync(jobId);
Console.WriteLine($"{job?.State}: {job?.StatusMessage}");

// Elsewhere, request cancellation if the work is no longer needed.
bool cancellationRequested = await jobs.CancelJobAsync(jobId);
```

The delegate should let `OperationCanceledException` propagate when cancellation is observed so that the service records the job as `Cancelled`. Other exceptions are captured on the job and change its state to `Failed`; successful completion sets progress to `100` and state to `Completed`.
