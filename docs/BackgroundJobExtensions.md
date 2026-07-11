# BackgroundJobExtensions

The `BackgroundJobExtensions` static class provides a set of utility methods for inspecting and interacting with background jobs managed by the ffmpeg-dotnet-wrapper library. These extension methods operate on a `BackgroundJob` instance and offer a convenient way to query job state, retrieve timing and metadata information, report progress, and obtain a human-readable summary without accessing internal job properties directly.

## API

All methods in this class are extension methods on a `BackgroundJob` instance. Unless otherwise noted, they throw an `ArgumentNullException` if the job argument is `null`.

### `IsActive`

```csharp
public static bool IsActive(this BackgroundJob job)
```

Returns `true` if the job is currently running (i.e., it has been started and has not yet completed, failed, or been cancelled). Returns `false` otherwise.

### `IsCompletedSuccessfully`

```csharp
public static bool IsCompletedSuccessfully(this BackgroundJob job)
```

Returns `true` if the job has finished execution with a successful exit code. Returns `false` if the job is still running, has failed, or was cancelled.

### `IsFailed`

```csharp
public static bool IsFailed(this BackgroundJob job)
```

Returns `true` if the job has terminated with a non‑zero exit code or encountered an unrecoverable error. Returns `false` otherwise.

### `GetTimeInfo`

```csharp
public static TimeSpan GetTimeInfo(this BackgroundJob job)
```

Returns a `TimeSpan` representing the current elapsed time (if the job is active) or the total duration (if the job has completed). The exact meaning depends on the underlying job implementation; typically it reflects the processed media duration or wall‑clock time, whichever is appropriate for the job type.

### `GetMetadataValue<T>`

```csharp
public static T GetMetadataValue<T>(this BackgroundJob job)
```

Retrieves a metadata value of type `T` from the job’s output or state. The generic type parameter must match the expected metadata key’s value type. Throws `InvalidOperationException` if the requested metadata key is not present or cannot be converted to `T`. Throws `NotSupportedException` if the job type does not support metadata retrieval.

### `UpdateProgress`

```csharp
public static void UpdateProgress(this BackgroundJob job)
```

Signals the job to update its internal progress state (e.g., by reading the latest output from the underlying process). This method is typically called from a polling loop or a callback to refresh progress information before querying other properties like `GetTimeInfo` or `GetSummary`.

### `IsTakingTooLong`

```csharp
public static bool IsTakingTooLong(this BackgroundJob job)
```

Returns `true` if the job has been running longer than a predefined or configurable threshold (e.g., a timeout set on the job or a default limit). Returns `false` if the job is within acceptable duration or has already completed/failed.

### `GetSummary`

```csharp
public static string GetSummary(this BackgroundJob job)
```

Returns a human‑readable string summarizing the current state of the job. The summary typically includes the job status (active, completed, failed), elapsed time, and any relevant metadata. The exact format is implementation‑defined.

## Usage

### Example 1: Monitoring a conversion job

```csharp
using FFmpegDotNetWrapper;

var job = new BackgroundJob("input.mp4", "output.avi");
job.Start();

while (job.IsActive())
{
    job.UpdateProgress();
    var elapsed = job.GetTimeInfo();
    Console.WriteLine($"Elapsed: {elapsed}, Taking too long: {job.IsTakingTooLong()}");
    Thread.Sleep(1000);
}

if (job.IsCompletedSuccessfully())
{
    Console.WriteLine("Conversion succeeded.");
    Console.WriteLine(job.GetSummary());
}
else if (job.IsFailed())
{
    Console.WriteLine("Conversion failed.");
    Console.WriteLine(job.GetSummary());
}
```

### Example 2: Retrieving metadata after completion

```csharp
using FFmpegDotNetWrapper;

var job = new BackgroundJob("input.mkv");
job.Start();
job.WaitForCompletion(); // hypothetical blocking method

if (job.IsCompletedSuccessfully())
{
    double duration = job.GetMetadataValue<double>();
    int width = job.GetMetadataValue<int>();
    Console.WriteLine($"Duration: {duration}s, Width: {width}px");
}
else
{
    Console.WriteLine($"Job failed: {job.GetSummary()}");
}
```

## Notes

- All methods assume the `BackgroundJob` instance is not `null`; passing `null` will result in an `ArgumentNullException`.
- `GetMetadataValue<T>` may throw `InvalidOperationException` if the metadata key is absent or the type conversion fails. Ensure the generic type matches the expected metadata type.
- `UpdateProgress` should be called before querying time or progress‑related properties to obtain the most recent data. Without calling it, properties may return stale values.
- Thread safety is not guaranteed. If multiple threads access the same `BackgroundJob` instance concurrently, external synchronization (e.g., a lock) is required to avoid race conditions, especially when calling `UpdateProgress` alongside other query methods.
- `IsTakingTooLong` relies on an internal threshold that may be set during job creation or via configuration. If no threshold is defined, the method may always return `false`.
- The `GetTimeInfo` interpretation (elapsed vs. total duration) depends on the job state. For a completed job it returns the total duration; for an active job it returns the current elapsed time.
