# WebhookService

## Purpose
The `WebhookService` is responsible for delivering events to external systems via HTTP webhooks. It handles retries, backoff, and failure tracking, and integrates with the event system to automatically deliver events.

## Interface: IWebhookService
The `IWebhookService` interface defines the contract for webhook management:

```csharp
public interface IWebhookService
{
    Task RegisterWebhookAsync(WebhookEndpoint endpoint);
    Task UnregisterWebhookAsync(string webhookId);
    Task<WebhookEndpoint?> GetWebhookAsync(string webhookId);
    Task<IEnumerable<WebhookEndpoint>> GetActiveWebhooksAsync();
}
```

### Methods
- **RegisterWebhookAsync**: Registers a new webhook endpoint for event delivery.
- **UnregisterWebhookAsync**: Unregisters and removes a webhook endpoint by its ID.
- **GetWebhookAsync**: Retrieves webhook configuration by ID (returns null if not found).
- **GetActiveWebhooksAsync**: Gets all active webhooks that are registered.

## Class: WebhookService
The `WebhookService` implements `IWebhookService` and handles event delivery for `OperationCompletedEvent`, `OperationFailedEvent`, and `OperationStartedEvent`.

### Key Features
- **Retry Logic**: Uses a configurable retry policy (default: exponential backoff) for handling transient failures.
- **Event Filtering**: Webhooks can be configured to receive specific event types (empty list = all events).
- **Authentication & Headers**: Supports Bearer token authentication and custom headers.
- **Thread Safety**: Uses locking to ensure safe concurrent access to the webhook collection.

### Dependencies
- `ILogger<WebhookService>`: For logging.
- `IHttpClientFactory`: To create HTTP clients for webhook delivery.
- `IRetryPolicy`: Policy for retrying failed deliveries (defaults to `ExponentialBackoffRetryPolicy`).

## Retry Policy
The service uses an `ExponentialBackoffRetryPolicy` by default with:
- **Max attempts**: 3
- **Initial delay**: 1000 milliseconds (1 second)
- **Backoff factor**: Exponential (2^attempt * initialDelay)

A custom retry policy can be injected via the constructor.

## Usage Example
```csharp
// Setup (e.g., in Startup.cs or Program.cs)
services.AddHttpClient("webhook"); // Named client used by WebhookService
services.AddSingleton<IWebhookService, WebhookService>();

// Usage
public class MyEventHandler
{
    private readonly IWebhookService _webhookService;

    public MyEventHandler(IWebhookService webhookService)
    {
        _webhookService = webhookService;
    }

    public async Task HandleSomeEventAsync()
    {
        // Register a webhook
        var endpoint = new WebhookEndpoint
        {
            Url = "https://example.com/webhook",
            EventTypes = new List<string> { "OperationCompletedEvent" },
            AuthToken = "your-secret-token",
            MaxRetries = 5
        };
        endpoint.Headers.Add("X-Custom-Header", "value");

        await _webhookService.RegisterWebhookAsync(endpoint);

        // Later, to unregister:
        // await _webhookService.UnregisterWebhookAsync(endpoint.WebhookId);
    }
}
```

## Related Documentation
- See [WebhookEndpoint](./WebhookEndpoint.md) for details on the webhook endpoint configuration model.