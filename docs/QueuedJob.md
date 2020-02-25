# QueuedJob

The `QueuedJob` class represents an individual unit of work within the `ffmpeg-dotnet-wrapper` job processing system. It encapsulates all necessary metadata, scheduling information, and payload content required to execute and track asynchronous FFmpeg operations, providing interfaces for managing the job lifecycle through an associated queue.

## API

### Properties

* `string JobId`: A unique identifier for the job.
* `int Priority`: The priority level of the job, where higher values typically indicate higher precedence.
* `DateTime EnqueuedAt`: The timestamp indicating when the job was added to the queue.
* `DateTime? DueAt`: The optional scheduled execution time for the job. If null, the job is eligible for immediate processing.
* `int RetryCount`: The number of times the job has been attempted.
* `int MaxRetries`: The maximum number of permitted retries before the job is considered failed.
* `string Payload`: The serialized data or configuration required to execute the job (e.g., FFmpeg arguments).
* `Dictionary<string, string> Tags`: A set of key-value pairs used for categorizing and filtering jobs.
* `JobQueue JobQueue`: A reference to the queue manager associated with this job.

### Methods

* `Task<string> EnqueueAsync()`: Submits the job to the queue. Returns the `JobId` upon successful enqueuing.
* `Task<QueuedJob?> DequeueAsync()`: Removes and returns the next available job from the queue based on priority and scheduling. Returns null if the queue is empty.
* `Task<QueuedJob?> GetJobAsync(string jobId)`: Retrieves a specific job by its unique identifier. Returns null if not found.
* `Task<List<QueuedJob>> GetPendingJobsAsync()`: Retrieves all jobs currently waiting in the queue.
* `Task<bool> RemoveJobAsync(string jobId)`: Removes a job from the queue. Returns true if the job was successfully removed, false otherwise.
* `Task<int> GetQueueCountAsync()`: Returns the total number of pending jobs in the queue.
* `Task RequeuJobAsync(string jobId)`: Resets the state of a job or re-adds a failed job back to the queue for reprocessing.
* `void Clear()`: Removes all jobs from the queue, effectively resetting it.

## Usage

### Creating and Enqueuing a Job

```csharp
var job = new QueuedJob
{
    JobId = Guid.NewGuid().ToString(),
    Priority = 1,
    Payload = "-i input.mp4 -c:v libx264 output.mp4",
    Tags = new Dictionary<string, string> { { "Type", "Conversion" } }
};

await job.EnqueueAsync();
```

### Monitoring and Dequeuing Jobs

```csharp
int pendingCount = await myJob.GetQueueCountAsync();
if (pendingCount > 0)
{
    var nextJob = await myJob.DequeueAsync();
    if (nextJob != null)
    {
        Console.WriteLine($"Processing job: {nextJob.JobId}");
        // Execute job logic here...
    }
}
```

## Notes

* **Thread Safety**: All asynchronous methods (`EnqueueAsync`, `DequeueAsync`, etc.) are designed to be thread-safe for use in concurrent environments, ensuring consistency across multiple worker threads accessing the same job queue.
* **Payload Integrity**: The `Payload` property expects valid, serialized content suitable for the target FFmpeg operation. Improperly formatted payloads may result in execution failures when the job is processed.
* **State Management**: `DueAt` is used for delayed job execution. If `DueAt` is in the future, the job will not be returned by `DequeueAsync` until the timestamp has elapsed.
