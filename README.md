// ... (rest of README.md content remains unchanged)

## ProcessExecutionException

The `ProcessExecutionException` class represents an exception that occurs when a process execution fails, including FFmpeg and FFprobe operations. It provides information about the process exit code and error output.

```csharp
try
{
    // Process execution code
}
catch (ProcessExecutionException ex)
{
    Console.WriteLine($"Process Error: {ex.Message}");
    if (ex.ExitCode.HasValue)
    {
        Console.WriteLine($"Exit Code: {ex.ExitCode}");
    }
    if (ex.ErrorOutput != null)
    {
        Console.WriteLine($"Error Output: {ex.ErrorOutput}");
    }
}
```

## WebhookEndpoint

The `WebhookEndpoint` type represents a configurable webhook endpoint used for receiving event notifications from the FFmpeg processing pipeline. It encapsulates webhook registration details including the target URL, event types to subscribe to, authentication settings, and retry behavior.



```csharp
using FFmpegDotnetWrapper.Integration;

// Create a new webhook endpoint
var webhook = new WebhookEndpoint
{
    WebhookId = Guid.NewGuid().ToString(),
    Url = "https://api.example.com/webhooks/ffmpeg-events",
    EventTypes = new List<string> { "OperationStarted", "OperationCompleted", "OperationFailed", "ProgressReported" },
    AuthToken = "your-secret-token-here",
    IsActive = true,
    MaxRetries = 3,
    Headers = new Dictionary<string, string>
    {
        ["X-Webhook-Secret"] = "webhook-secret-value",
        ["User-Agent"] = "FFmpegDotnetWrapper/1.0"
    }
};

// Register the webhook with the service
var service = new WebhookService();
await service.RegisterWebhookAsync(webhook);

// Retrieve an active webhook
var activeWebhook = await service.GetWebhookAsync(webhook.WebhookId);

// Get all active webhooks
var activeWebhooks = await service.GetActiveWebhooksAsync();

// Unregister the webhook when no longer needed
await service.UnregisterWebhookAsync(webhook.WebhookId);
```

## FFmpegEvent

The `FFmpegEvent` hierarchy provides a structured way to emit and consume events during FFmpeg operations. Each event carries a unique identifier, a UTC timestamp, and optional correlation and source information, enabling robust tracking and debugging across distributed systems.

```csharp
using FFmpegDotnetWrapper.Events;

// Create a started event
var started = new OperationStartedEvent
{
    InputFile = "input.mp4",
    OutputFile = "output.mp4",
    OperationType = "Transcode",
    Metadata = new Dictionary<string, object>
    {
        ["Resolution"] = "1920x1080",
        ["Bitrate"] = 4000
    },
    CorrelationId = Guid.NewGuid().ToString(),
    Source = "TranscodeService"
};

// Create a completed event
var completed = new OperationCompletedEvent
{
    InputFile = "input.mp4",
    OutputFile = "output.mp4",
    OperationType = "Transcode",
    Duration = TimeSpan.FromSeconds(12.5),
    OutputFileSize = 10485760
};

// Create a failed event
var failed = new OperationFailedEvent
{
    InputFile = "input.mp4",
    OperationType = "Transcode",
    ErrorMessage = "Unsupported codec",
    ErrorCode = "E_UNSUPPORTED_CODEC",
    StackTrace = Environment.StackTrace
};

// Create a progress event
var progress = new ProgressReportedEvent
{
    OperationType = "Transcode",
    ProgressPercentage = 45.3,
    ElapsedTime = TimeSpan.FromSeconds(5),
    StatusMessage = "Encoding..."
};
```
