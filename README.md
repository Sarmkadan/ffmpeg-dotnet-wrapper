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

## BackgroundJob

Represents an asynchronous background job with progress tracking and status monitoring capabilities. The `BackgroundJob` type provides comprehensive job lifecycle management through properties like `JobId`, `JobName`, state tracking via `State`, progress reporting with `ProgressPercentage`, and detailed status information including `StatusMessage`, timestamps (`CreatedAt`, `StartedAt`, `CompletedAt`), error handling via `ErrorMessage` and `StackTrace`, and extensible metadata storage in `Metadata`.

The `BackgroundJobService` class offers job management operations including enqueuing new jobs, retrieving jobs by ID or status, canceling active jobs, and updating job progress in real-time.

```csharp
// Create background job service
var jobService = new BackgroundJobService();

// Enqueue a new background job
var jobId = jobService.EnqueueJob("Video Processing Job", new Dictionary<string, object>
{
    { "inputFile", "/videos/input.mp4" },
    { "outputFile", "/videos/output.mp4" },
    { "preset", "medium" }
});

Console.WriteLine($"Enqueued job with ID: {jobId}");

// Retrieve and monitor the job
var job = await jobService.GetJobAsync(jobId);
if (job != null)
{
    Console.WriteLine($"Job Name: {job.JobName}");
    Console.WriteLine($"State: {job.State}");
    Console.WriteLine($"Progress: {job.ProgressPercentage}%");
    Console.WriteLine($"Status: {job.StatusMessage}");
    Console.WriteLine($"Created: {job.CreatedAt}");
    Console.WriteLine($"Estimated time remaining: {job.EstimatedTimeRemaining?.ToString("g") ?? "N/A"}");
    
    // Update progress periodically
    await jobService.UpdateJobProgressAsync(jobId, 25, "Processing video...");
    
    // Check active jobs
    var activeJobs = await jobService.GetActiveJobsAsync();
    Console.WriteLine($"Active jobs: {activeJobs.Count()}");
}

// Complete the job
await jobService.UpdateJobProgressAsync(jobId, 100, "Job completed successfully");
```

## ... (rest of README.md content remains unchanged)
