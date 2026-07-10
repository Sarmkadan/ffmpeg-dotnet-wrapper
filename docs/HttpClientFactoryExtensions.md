# HttpClientFactoryExtensions

The `HttpClientFactoryExtensions` class provides a suite of static extension methods and configuration properties designed to streamline the integration of `HttpClient` instances within the `IServiceCollection` dependency injection container. Specifically tailored for `ffmpeg-dotnet-wrapper`, these utilities manage complex HTTP scenarios including media file transfers, webhook notifications, and probe requests. It simplifies the implementation of resilient communication patterns by offering configurable timeout settings and integrated retry logic for transient network failures.

## API

### Static Methods

*   **`AddFFmpegHttpClients(IServiceCollection services)`**: Registers the default set of `HttpClient` configurations required for FFmpeg operations within the `IServiceCollection`.
*   **`AddCustomHttpClient(IServiceCollection services, ...)`**: Registers a custom-configured `HttpClient` with specialized policies to the `IServiceCollection`.
*   **`IsTransientError(HttpStatusCode statusCode)`**: Determines if a given HTTP status code represents a transient error that warrants a retry attempt.
*   **`IsPermanentError(HttpStatusCode statusCode)`**: Evaluates whether an HTTP status code indicates a permanent failure where retrying is unlikely to succeed.
*   **`ShouldRetryOnException(Exception ex)`**: Checks if the provided exception is of a type that suggests a retryable network issue.
*   **`GetErrorMessage(HttpResponseMessage response)`**: Generates a standardized error message string based on the content and status of an `HttpResponseMessage`.

### Configuration Properties

*   **`WebhookTimeoutSeconds`**: Configures the timeout duration in seconds for HTTP requests directed at webhook endpoints.
*   **`ProbeTimeoutSeconds`**: Sets the timeout duration in seconds for HTTP requests used for probing media services.
*   **`MediaTransferTimeoutMinutes`**: Specifies the timeout duration in minutes for long-running media transfer operations.
*   **`EnableRetries`**: A boolean flag indicating whether automatic retry logic is enabled for configured `HttpClient` instances.
*   **`MaxRetryAttempts`**: Defines the maximum number of retry attempts permitted for failed requests.
*   **`InitialBackoffMs`**: The initial delay in milliseconds before the first retry attempt in an exponential backoff strategy.

### Retry Logic Members

*   **`ExponentialBackoffRetryPolicy`**: Represents the strategy implementation used to calculate delays between successive retry attempts.
*   **`GetRetryDelay(int attemptCount)`**: Calculates the required delay duration for a specific retry attempt based on the exponential backoff policy.
*   **`ShouldRetry(HttpResponseMessage response, Exception ex)`**: Evaluates whether a request should be retried based on the resulting response or exception encountered.

## Usage

### Registering FFmpeg HTTP Clients

This example demonstrates how to configure the default FFmpeg HTTP clients within the service container configuration during application startup.

```csharp
using Microsoft.Extensions.DependencyInjection;
using FfmpegDotNetWrapper.Extensions;

var services = new ServiceCollection();

// Register the default FFmpeg HTTP clients
services.AddFFmpegHttpClients();

var provider = services.BuildServiceProvider();
```

### Implementing Custom Retry Logic

This example illustrates how to utilize the static helper methods to evaluate if a failed request should be retried based on its status code.

```csharp
using System.Net;
using FfmpegDotNetWrapper.Extensions;

// Assuming a failed response is received
var response = await httpClient.GetAsync("https://api.example.com/media/probe");

if (HttpClientFactoryExtensions.IsTransientError(response.StatusCode))
{
    // Logic to initiate a retry attempt
    Console.WriteLine("Transient error detected. Retrying...");
}
else
{
    var errorMessage = HttpClientFactoryExtensions.GetErrorMessage(response);
    Console.WriteLine($"Permanent failure: {errorMessage}");
}
```

## Notes

*   **Thread Safety**: The static methods `IsTransientError`, `IsPermanentError`, `ShouldRetryOnException`, and `GetErrorMessage` are stateless and thread-safe. They may be called concurrently from multiple threads without external synchronization.
*   **Configuration State**: Properties such as `WebhookTimeoutSeconds`, `EnableRetries`, and `MaxRetryAttempts` are intended to be set during the application's initial configuration phase. Modifying these properties while `HttpClient` instances are actively using them may result in undefined behavior.
*   **Exponential Backoff**: The `ExponentialBackoffRetryPolicy` assumes a standard exponential progression. When implementing custom retry handling, ensure that `InitialBackoffMs` is set to a reasonable value to avoid overwhelming downstream services.
