# HttpClientFactory

The `HttpClientFactory` file (`src/Integration/HttpClientFactory.cs`) contains components for creating and configuring `HttpClient` instances, managing retry policies, and providing HTTP utility functions for external integrations in the FFmpeg wrapper.

## Components

- **`HttpClientFactoryExtensions`**: Extension methods for `IServiceCollection` to register named HTTP clients (webhook, probe, media) and custom clients with configurable timeouts and headers.
- **`HttpClientConfig`**: Configuration class for HTTP client timeouts and retry settings (timeouts, retry attempts, backoff).
- **`HttpClientUtilities`**: Static utility methods for HTTP operations (transient error detection, error message extraction, retry determination).
- **`ExponentialBackoffRetryPolicy`**: Retry policy implementation with exponential backoff and jitter for transient failure handling.

## Related Documentation

For detailed API reference and usage examples, see the [HttpClientFactoryExtensions documentation](HttpClientFactoryExtensions.md).

## Usage

The components in this file are typically used together during application startup to configure resilient HTTP clients:

```csharp
using Microsoft.Extensions.DependencyInjection;
using FFmpegDotnetWrapper.Integration;

var services = new ServiceCollection();

// Register default FFmpeg HTTP clients with custom configuration
services.AddFFmpegHttpClients(config =>
{
    config.WebhookTimeoutSeconds = 15;
    config.MaxRetryAttempts = 5;
});

// Add a custom HTTP client for a specific API
services.AddCustomHttpClient("myapi", "https://api.example.com", 
    timeout: TimeSpan.FromSeconds(10),
    defaultHeaders: new Dictionary<string, string> { ["Accept"] = "application/json" });
```

The registered clients can then be resolved via `IHttpClientFactory` for typed or named client usage throughout the application.