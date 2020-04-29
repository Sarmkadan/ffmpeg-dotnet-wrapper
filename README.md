// ... (rest of README.md content remains unchanged)

## QueuedJob

Represents a queued job with priority and execution metadata. It provides properties to access the job's ID, priority, enqueued time, due time, retry count, maximum retries, payload, tags, and more.

```csharp
var job = new QueuedJob
{
    JobId = Guid.NewGuid().ToString(),
    Priority = 5,
    EnqueuedAt = DateTime.UtcNow,
    DueAt = DateTime.UtcNow.AddMinutes(10),
    RetryCount = 0,
    MaxRetries = 3,
    Payload = "Process video file",
    Tags = new Dictionary<string, string> { { "video", "mp4" } }
};

Console.WriteLine($"Job ID: {job.JobId}");
Console.WriteLine($"Priority: {job.Priority}");
Console.WriteLine($"Enqueued at: {job.EnqueuedAt}");
Console.WriteLine($"Due at: {job.DueAt}");
Console.WriteLine($"Retry count: {job.RetryCount}/{job.MaxRetries}");
Console.WriteLine($"Payload: {job.Payload}");
Console.WriteLine($"Tags: {string.Join(", ", job.Tags.Select(x => $"{x.Key}={x.Value}"))}");
```

## ... (rest of README.md content remains unchanged)
