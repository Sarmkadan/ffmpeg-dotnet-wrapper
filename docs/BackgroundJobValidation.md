# BackgroundJobValidation

`BackgroundJobValidation` provides extension methods for validating a [`BackgroundJob`](BackgroundJob.md). Validation reports all detected problems rather than stopping at the first one.

## API

### `Validate`

```csharp
IReadOnlyList<string> problems = job.Validate();
```

Returns a read-only list of human-readable validation problems. An empty list means the job is valid. Passing `null` throws `ArgumentNullException`.

### `IsValid`

```csharp
bool valid = job.IsValid();
```

Returns `true` when `Validate` returns no problems. Passing `null` throws `ArgumentNullException` through `Validate`.

### `EnsureValid`

```csharp
job.EnsureValid();
```

Returns normally for a valid job. For an invalid job, it throws `ArgumentException` with all problems joined into the exception message. Passing `null` throws `ArgumentNullException`.

## Validation rules

| Property | Requirement |
| --- | --- |
| `JobId` | Must not be null, empty, or whitespace. |
| `JobName` | Must not be null, empty, or whitespace. |
| `State` | Must be a defined value in the `JobState` enum. |
| `ProgressPercentage` | Must be between 0 and 100 inclusive. |
| `StatusMessage` | Required for all states except `Queued` (before processing starts). For non-queued jobs, it must not be null or whitespace. |
| `CreatedAt` | Must not be the default `DateTime` value. |
| `StartedAt` | If set, must not be earlier than `CreatedAt` and must not be the default `DateTime` value. |
| `CompletedAt` | If set, must not be earlier than `CreatedAt`, must not be the default `DateTime` value, and must not be earlier than `StartedAt` (if `StartedAt` is set). |
| `ErrorMessage` and `StackTrace` | When `State` is `Failed`: `ErrorMessage` must be set (non-null and non-whitespace). Additionally, if `ErrorMessage` is not "Operation cancelled by user", then `StackTrace` must be set. When `State` is not `Failed`: both `ErrorMessage` and `StackTrace` must be null or whitespace. |
| `EstimatedTimeRemaining` | If set, must not be negative. |
| `Metadata` | Must not be null. |

## Example

This example shows a valid background job in the `Queued` state.

```csharp
using FFmpegDotnetWrapper.BackgroundJobs;
using System;

var job = new BackgroundJob
{
    JobId = Guid.NewGuid().ToString(),
    JobName = "Test Job",
    State = JobState.Queued,
    ProgressPercentage = 0,
    StatusMessage = string.Empty, // Allowed for Queued state
    CreatedAt = DateTime.UtcNow,
    StartedAt = null, // Not started yet
    CompletedAt = null, // Not completed yet
    ErrorMessage = null,
    StackTrace = null,
    EstimatedTimeRemaining = null,
    Metadata = new Dictionary<string, string>()
};

IReadOnlyList<string> problems = job.Validate();
if (problems.Count == 0)
{
    job.EnsureValid(); // Will not throw
}
```