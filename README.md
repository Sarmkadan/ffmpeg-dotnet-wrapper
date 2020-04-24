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

## HttpClientFactoryExtensions

The `HttpClientFactoryExtensions` class provides extension methods for configuring HTTP clients used by the FFmpeg wrapper for external integrations. It includes methods for registering named HTTP clients with custom timeouts, configuring retry policies, and utility methods for error handling and status code classification.

```csharp
using FFmpegDotnetWrapper.Integration;
using Microsoft.Extensions.DependencyInjection;

// Configure HTTP clients with default timeouts
var services = new ServiceCollection();
services.AddFFmpegHttpClients();

// Configure HTTP clients with custom timeouts
services.AddFFmpegHttpClients(config =>
{
    config.WebhookTimeoutSeconds = 60;
    config.ProbeTimeoutSeconds = 120;
    config.MediaTransferTimeoutMinutes = 60;
    config.EnableRetries = true;
    config.MaxRetryAttempts = 5;
    config.InitialBackoffMs = 200;
});

// Add a custom HTTP client with specific configuration
services.AddCustomHttpClient(
    "custom-api",
    baseAddress: "https://api.example.com/v1",
    timeout: TimeSpan.FromSeconds(45),
    defaultHeaders: new Dictionary<string, string>
    {
        ["X-API-Key"] = "your-api-key-here",
        ["Accept"] = "application/json"
    }
);

// Use the registered HTTP clients
var serviceProvider = services.BuildServiceProvider();
var webhookClient = serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("webhook");
var probeClient = serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("probe");
var mediaClient = serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("media");
var customClient = serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("custom-api");

// Check if an HTTP status code is transient (should be retried)
bool isTransient = HttpClientUtilities.IsTransientError(429);

// Check if an HTTP status code is permanent (should not be retried)
bool isPermanent = HttpClientUtilities.IsPermanentError(404);

// Determine if an exception should trigger a retry
bool shouldRetry = HttpClientUtilities.ShouldRetryOnException(new HttpRequestException("Network error"));

// Get a formatted error message from an HTTP response
var errorMessage = HttpClientUtilities.GetErrorMessage(response);

// Create and use an exponential backoff retry policy
var retryPolicy = new ExponentialBackoffRetryPolicy(maxRetries: 3, initialDelayMs: 100);
TimeSpan delay = retryPolicy.GetRetryDelay(2); // 200ms delay
bool shouldRetryAgain = retryPolicy.ShouldRetry(2); // true
```
