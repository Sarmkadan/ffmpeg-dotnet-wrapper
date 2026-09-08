# RequestLoggingMiddleware

## Purpose

`RequestLoggingMiddleware` provides a consistent logging facade for API operation requests, responses, exceptions, and performance measurements. It formats timestamps, durations, byte sizes, request identifiers, and optional contextual values before sending entries to `ILogger<RequestLoggingMiddleware>`.

Logging is best-effort: each public logging method catches failures that occur while constructing or emitting an entry and reports the logging failure instead of propagating it to the operation being observed. Logged values are truncated according to the configured maximum length. See [`RequestLoggingOptions`](RequestLoggingOptions.md) for the available configuration settings.

## Public API

| Member | Signature | Description |
| --- | --- | --- |
| Constructor | `RequestLoggingMiddleware(ILogger<RequestLoggingMiddleware> logger, RequestLoggingOptions? options = null)` | Creates the logger. `logger` is required; omitting `options` uses a new `RequestLoggingOptions` instance with its defaults. |
| `LogRequest` | `void LogRequest<T>(T request, string operationName) where T : class` | Logs an incoming operation at `Information` level. The entry includes a UTC timestamp and request ID. When argument logging is enabled, public properties other than `RequestId` and `CreatedAt` are included. Requests derived from `ApiRequest` retain their request ID; other request types receive a generated ID for the entry. |
| `LogResponse` | `void LogResponse<T>(ApiResponse<T> response, string operationName, TimeSpan executionTime)` | Logs the response status, request ID, status code, and message. Successful responses use `Information`; failed responses use `Warning` and include their errors. Execution time and response data are controlled by the options. |
| `LogError` | `void LogError(Exception ex, string operationName, string? requestId = null, Dictionary<string, object>? context = null)` | Logs an exception at `Error` level, with optional request correlation and contextual key/value pairs. The stack trace is included when enabled and available. |
| `LogPerformanceMetrics` | `void LogPerformanceMetrics(string operationName, TimeSpan duration, long inputSize, long outputSize, Dictionary<string, object>? metrics = null)` | Logs duration, input/output byte sizes, compression ratio, throughput, and optional custom metrics at `Information` level. The call is a no-op when performance logging is disabled. |

Collection values that implement `IEnumerable<object>` are summarized by item count (and capped once more than ten items are observed); other values use `ToString()` and are truncated to `MaxLogValueLength`.

## Usage

```csharp
using System.Diagnostics;
using FFmpegDotnetWrapper.Api.DTOs;
using FFmpegDotnetWrapper.Middleware;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<RequestLoggingMiddleware>();
var requestLogger = new RequestLoggingMiddleware(
    logger,
    new RequestLoggingOptions
    {
        LogArguments = true,
        LogResponseData = false,
        LogPerformanceMetrics = true
    });

var request = new TranscodeRequest
{
    InputPath = "input.mov",
    OutputPath = "output.mp4"
};

var stopwatch = Stopwatch.StartNew();
requestLogger.LogRequest(request, "Transcode");

try
{
    var response = ApiResponse<string>.Ok("output.mp4");
    response.RequestId = request.RequestId;

    stopwatch.Stop();
    requestLogger.LogResponse(response, "Transcode", stopwatch.Elapsed);
    requestLogger.LogPerformanceMetrics(
        "Transcode",
        stopwatch.Elapsed,
        inputSize: 12_000_000,
        outputSize: 8_000_000);
}
catch (Exception ex)
{
    requestLogger.LogError(ex, "Transcode", request.RequestId);
    throw;
}
```

The middleware is a logging helper rather than an ASP.NET Core pipeline component: callers invoke its methods around an operation at the points where request, response, error, and size information are available.
