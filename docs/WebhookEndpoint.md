# WebhookEndpoint

The `WebhookEndpoint` class provides a configuration model and management interface for receiving HTTP-based notifications regarding FFmpeg task lifecycle events. By defining endpoints, users can asynchronously subscribe to specific system events, allowing for real-time monitoring and integration of media processing workflows.

## API

| Member | Type | Description |
| :--- | :--- | :--- |
| `WebhookId` | `string` | A unique identifier for the webhook configuration. |
| `Url` | `string` | The destination URL where event notifications are sent via HTTP POST. |
| `EventTypes` | `List<string>` | A collection of event identifiers that trigger this webhook. |
| `AuthToken` | `string?` | An optional authentication token for secure communication with the receiver. |
| `IsActive` | `bool` | Indicates whether the webhook is currently enabled for event dispatching. |
| `MaxRetries` | `int` | The maximum number of delivery attempts for a failed notification. |
| `CreatedAt` | `DateTime` | The timestamp representing when the webhook was registered. |
| `Headers` | `Dictionary<string, string>` | A collection of custom HTTP headers sent with the webhook request. |
| `WebhookService` | `WebhookService` | A reference to the service managing the registration and lifecycle of this endpoint. |

### Methods

*   **`Task RegisterWebhookAsync()`**
    Registers the current endpoint configuration with the underlying service. Throws an exception if the URL is invalid or the connection to the service fails.
*   **`Task UnregisterWebhookAsync()`**
    Removes the endpoint from the active registrations in the service.
*   **`Task<WebhookEndpoint?> GetWebhookAsync()`**
    Retrieves the current state of this specific webhook configuration from the service. Returns `null` if the webhook no longer exists.
*   **`Task<IEnumerable<WebhookEndpoint>> GetActiveWebhooksAsync()`**
    Fetches a collection of all currently registered and active webhooks.

## Usage

### Registering a New Webhook

```csharp
var endpoint = new WebhookEndpoint
{
    Url = "https://api.example.com/hooks/ffmpeg-events",
    EventTypes = new List<string> { "job.completed", "job.failed" },
    MaxRetries = 3,
    Headers = new Dictionary<string, string> { { "X-Custom-Header", "Value" } }
};

// Register the endpoint with the service
await endpoint.RegisterWebhookAsync();
```

### Retrieving Active Webhooks

```csharp
// Assuming an existing instance of WebhookService is available
var activeWebhooks = await webhookService.GetActiveWebhooksAsync();

foreach (var hook in activeWebhooks)
{
    Console.WriteLine($"Webhook {hook.WebhookId} is active at {hook.Url}");
}
```

## Notes

*   **Thread Safety:** The `WebhookEndpoint` properties are not inherently thread-safe. Modifying properties (`Url`, `EventTypes`, `Headers`, etc.) while asynchronous operations (`RegisterWebhookAsync`) are in progress may lead to undefined behavior or inconsistent state.
*   **Persistence:** Changes made to instance properties (e.g., `IsActive = false`) do not automatically persist to the backend service. You must explicitly call `RegisterWebhookAsync()` to synchronize the updated configuration.
*   **Nullable Types:** The `AuthToken` property is nullable (`string?`). If set to `null`, no authentication token will be included in the outgoing HTTP requests.
*   **Timeouts:** All async methods are subject to the underlying `WebhookService` configuration regarding network timeouts and connectivity retries.
