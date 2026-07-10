# BackgroundJob

A `BackgroundJob` encapsulates a unit of work executed asynchronously by the ffmpeg‑dotnet‑wrapper’s job service. It tracks the job’s identity, lifecycle state, progress, and result metadata, allowing callers to monitor, query, and control the job from application code.

## API

| Member | Description | Parameters | Return Value | Exceptions |
|--------|-------------|------------|--------------|------------|
| **JobId** | Unique identifier assigned when the job is created. Immutable for the lifetime of the object. | – | `string` | None |
| **JobName** | Human‑readable name describing the job’s purpose. Set at creation and does not change. | – | `string` | None |
| **State** | Current execution state of the job (`Queued`, `Running`, `Completed`, `Failed`, `Canceled`). | – | `JobState` | None |
| **ProgressPercentage** | Completion progress expressed as a value from 0.0 to 100.0. Updated only by the job service. | – | `double` | None |
| **StatusMessage** | Optional descriptive message reflecting the current step or status of the job. | – | `string` | None |
| **CreatedAt** | UTC timestamp indicating when the `BackgroundJob` instance was instantiated. | – | `DateTime` | None |
| **StartedAt** | UTC timestamp when the job began execution; `null` while the job is queued or has not started. | – | `DateTime?` | None |
| **CompletedAt** | UTC timestamp when the job finished (successfully, failed, or canceled); `null` until the job reaches a terminal state. | – | `DateTime?` | None |
| **ErrorMessage** | If `State` is `Failed`, contains the error message; otherwise `null`. | – | `string?` | None |
| **StackTrace** | If `State` is `Failed`, contains the stack trace of the exception; otherwise `null`. | – | `string?` | None |
| **Metadata** | Arbitrary key‑value store for attaching custom data to the job. The dictionary is initialized empty and can be read or updated by the service or caller. | – | `Dictionary<string, object>` | None |
| **EstimatedTimeRemaining** | Approximate time left until job completion; `null` when the estimate is unavailable (e.g., before start or after completion). | – | `TimeSpan?` | None |
| **BackgroundJobService** | Reference to the service that manages this job’s execution lifecycle. Read‑only after job creation. | – | `BackgroundJobService` | None |
| **EnqueueJob** | Submits the job to its associated service for execution. Returns the job’s identifier (typically the same as `JobId`). | – | `string` | * `InvalidOperationException` if the job is already enqueued or in a terminal state.<br>* `ObjectDisposedException` if the owning service has been disposed. |
| **GetJobAsync** | Asynchronously retrieves the current `BackgroundJob` instance from the service (useful for obtaining an updated snapshot). | – | `Task<BackgroundJob?>` | * `ObjectDisposedException` if the service is disposed.<br>* `TaskCanceledException` if the operation is cancelled via a cancellation token (if one is supplied internally). |
| **GetActiveJobsAsync** | Asynchronously returns all jobs that are currently queued or running. | – | `Task<IEnumerable<BackgroundJob>>` | Same as `GetJobAsync`. |
| **GetJobsAsync** | Asynchronously returns all jobs known to the service, regardless of state (including completed, failed, and canceled). | – | `Task<IEnumerable<BackgroundJob>>` | Same as `GetJobAsync`. |
| **CancelJobAsync** | Requests cancellation of the job. Returns `true` if the cancellation request was accepted; `false` if the job cannot be cancelled (e.g., already finished). | – | `Task<bool>` | * `ObjectDisposedException` if the service is disposed.<br>* `InvalidOperationException` if the job is not in a cancellable state. |
| **UpdateJobProgressAsync** | Notifies the service of a progress update. The caller supplies the new progress percentage and an optional status message; the service updates the corresponding properties. | – | `Task` | * `ObjectDisposedException` if the service is disposed.<br>* `ArgumentOutOfRangeException` if the supplied progress is outside the range 0‑100. |

## Usage

### Example 1: Enqueue a job and monitor its progress

```csharp
using System;
using System.Threading.Tasks;

// Assume `jobService` is an initialized BackgroundJobService.
var job = new BackgroundJob
{
    JobName = "FFmpeg thumbnail generation",
    // Optional metadata can be prepopulated.
    Metadata = { ["InputFile"] = "video.mp4", ["OutputFile"] = "thumb.jpg" }
};

// Enqueue the job and obtain its identifier.
string jobId = job.EnqueueJob();
Console.WriteLine($"Job enqueued with ID: {jobId}");

// Poll until the job reaches a terminal state.
while (job.State != JobState.Completed && job.State != JobState.Failed && job.State != JobState.Canceled)
{
    // Refresh the job state from the service.
    job = await job.GetJobAsync() ?? throw new InvalidOperationException("Job disappeared.");
    
    Console.Write(
        $"[{job.State}] Progress: {job.ProgressPercentage:F1}% - {job.StatusMessage}\r");
    
    await Task.Delay(500); // Avoid tight looping.
}

Console.WriteLine(); // New line after polling loop.

if (job.State == JobState.Completed)
{
    Console.WriteLine("Job completed successfully.");
}
else if (job.State == JobState.Failed)
{
    Console.WriteLine($"Job failed: {job.ErrorMessage}");
}
else
{
    Console.WriteLine("Job was canceled.");
}
```

### Example 2: Cancel a long‑running job

```csharp
using System;
using System.Threading.Tasks;

// Obtain a reference to an existing job (e.g., from a UI list).
BackgroundJob job = await jobService.GetJobAsync(); // Replace with actual lookup logic.

if (job.State == JobState.Running || job.State == JobState.Queued)
{
    bool cancelRequested = await job.CancelJobAsync();
    if (cancelRequested)
    {
        Console.WriteLine("Cancellation request submitted.");
    }
    else
    {
        Console.WriteLine("Unable to cancel the job; it may already be finishing.");
    }
}
else
{
    Console.WriteLine($"Job is in state {job.State} and cannot be cancelled.");
}
```

## Notes

- **Thread safety**: All read‑only properties (`JobId`, `JobName`, `State`, `ProgressPercentage`, `StatusMessage`, timestamps, `Metadata`, etc.) are safe to access concurrently from multiple threads after the job has been created. Mutations to `ProgressPercentage`, `StatusMessage`, `State`, and the timestamp fields are performed exclusively by the `BackgroundJobService`; external callers should not modify these properties directly.
- **Metadata concurrency**: The `Metadata` dictionary is not internally synchronized. If multiple threads update the same key simultaneously, the final value is undefined. External synchronization (e.g., locking) is required when concurrent updates are expected.
- **Service lifetime**: The `BackgroundJobService` reference remains valid for the lifetime of the job. If the service is disposed, any further interaction with the job (including property reads that trigger internal refreshes) will throw `ObjectDisposedException`.
- **Progress values**: The service clamps progress updates to the range `[0, 100]`. Supplying a value outside this range via `UpdateJobProgressAsync` results in an `ArgumentOutOfRangeException`.
- **Terminal states**: Once a job reaches `Completed`, `Failed`, or `Canceled`, further calls to `EnqueueJob`, `CancelJobAsync`, or `UpdateJobProgressAsync` will throw `InvalidOperationException`. The `State` property will not change after this point.
- **Estimated time remaining**: This value is a best‑effort estimate based on current processing speed and may fluctuate or become `null` if the service cannot produce a reliable prediction.
