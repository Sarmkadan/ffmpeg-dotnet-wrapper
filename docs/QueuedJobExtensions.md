# QueuedJobExtensions

The `QueuedJobExtensions` and `QueuedJobEnumerableExtensions` static classes provide utility methods for working with queued jobs in the ffmpeg-dotnet-wrapper library. These extension methods operate on `QueuedJob` instances and sequences of `QueuedJob` instances to offer convenient ways to query job status, retry information, and ordering capabilities.

## API

### QueuedJobExtensions

All methods in this class are extension methods on a `QueuedJob` instance. Unless otherwise noted, they throw an `ArgumentNullException` if the job argument is `null`.

#### `GetStatusString`

```csharp
public static string GetStatusString(this QueuedJob job)
```

Returns a string representation of the job's status. Returns "No due date" if `QueuedJob.DueAt` is null, otherwise returns the due date in ISO 8601 format.

#### `IsOverdue`

```csharp
public static bool IsOverdue(this QueuedJob job)
```

Determines whether the job is overdue. Returns `true` if the job is overdue; otherwise, false. A job is overdue when `QueuedJob.DueAt` is not null and the due date is in the past.

#### `GetRetryInfoString`

```csharp
public static string GetRetryInfoString(this QueuedJob job)
```

Gets a string representation of the job's retry information. Format: "Retried {RetryCount} times out of {MaxRetries}".

#### `HasMaxRetries`

```csharp
public static bool HasMaxRetries(this QueuedJob job)
```

Determines whether the job has reached its maximum number of retries. Returns `true` if the job has reached its maximum number of retries; otherwise, false. Throws `ArgumentOutOfRangeException` if `QueuedJob.MaxRetries` is negative.

### QueuedJobEnumerableExtensions (Collection helpers)

All methods in this class are extension methods on `IEnumerable<QueuedJob>`. Unless otherwise noted, they throw an `ArgumentNullException` if the source argument is `null`.

#### `OrderByPriorityThenEnqueued`

```csharp
public static IEnumerable<QueuedJob> OrderByPriorityThenEnqueued(this IEnumerable<QueuedJob> jobs)
```

Orders queued jobs by ascending priority and then by their enqueue time. Returns a sequence whose elements are ordered by `QueuedJob.Priority` and then by `QueuedJob.EnqueuedAt`, both in ascending order.

## Usage

### Example 1: Checking job status and retry information

```csharp
using FFmpegDotnetWrapper.BackgroundJobs;

var job = new QueuedJob { DueAt = DateTime.UtcNow.AddHours(-1), RetryCount = 3, MaxRetries = 5 };

Console.WriteLine(job.GetStatusString()); // Output: Due at 2026-09-14T10:00:00.0000000Z
Console.WriteLine(job.IsOverdue());       // Output: True
Console.WriteLine(job.GetRetryInfoString()); // Output: Retried 3 times out of 5
Console.WriteLine(job.HasMaxRetries());   // Output: False
```

### Example 2: Ordering a collection of queued jobs

```csharp
using FFmpegDotnetWrapper.BackgroundJobs;
using System.Collections.Generic;
using System.Linq;

var jobs = new List<QueuedJob>
{
    new QueuedJob { Priority = 2, EnqueuedAt = DateTime.UtcNow.AddMinutes(-10) },
    new QueuedJob { Priority = 1, EnqueuedAt = DateTime.UtcNow.AddMinutes(-5) },
    new QueuedJob { Priority = 1, EnqueuedAt = DateTime.UtcNow.AddMinutes(-15) }
};

var orderedJobs = jobs.OrderByPriorityThenEnqueued();

foreach (var job in orderedJobs)
{
    Console.WriteLine($"Priority: {job.Priority}, EnqueuedAt: {job.EnqueuedAt}");
}
// Output:
// Priority: 1, EnqueuedAt: 9/14/2026 9:45:00 AM
// Priority: 1, EnqueuedAt: 9/14/2026 9:55:00 AM
// Priority: 2, EnqueuedAt: 9/14/2026 9:50:00 AM
```

## Notes

- All methods assume the `QueuedJob` instance is not `null`; passing `null` will result in an `ArgumentNullException`.
- For `HasMaxRetries`, if `QueuedJob.MaxRetries` is negative, an `ArgumentOutOfRangeException` will be thrown.
- The `OrderByPriorityThenEnqueued` method uses stable sorting, so jobs with equal priority will maintain their relative order based on enqueue time.
- When working with large collections, consider that `OrderByPriorityThenEnqueued` creates a new ordered sequence and does not modify the original collection.