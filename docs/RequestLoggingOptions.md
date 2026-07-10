# RequestLoggingOptions

`RequestLoggingOptions` configures the behavior of the `RequestLoggingMiddleware`, controlling which aspects of HTTP requests and responses are captured, the verbosity of logged data, and whether performance metrics are emitted. It exposes typed logging methods that delegate to the middleware’s internal logger, ensuring consistent formatting and filtering across all logged events.

## API

### Properties

#### `LogArguments`
`public bool LogArguments`

When `true`, the middleware includes query-string and route arguments in request log entries. Defaults to `false`.

#### `LogResponseData`
`public bool LogResponseData`

When `true`, the response body (or a truncated representation) is appended to response log entries. Defaults to `false`.

#### `LogStackTrace`
`public bool LogStackTrace`

When `true`, exception stack traces are written to error log entries. Defaults to `false`.

#### `MaxLogValueLength`
`public int MaxLogValueLength`

The maximum number of characters retained for any single logged value (headers, arguments, response data). Values exceeding this limit are truncated. Must be greater than zero. Defaults to `1024`.

#### `LogPerformanceMetrics`
`public bool LogPerformanceMetrics`

When `true`, the middleware measures and logs request duration, response size, and status-code distribution. Defaults to `false`.

#### `RequestLoggingMiddleware`
`public RequestLoggingMiddleware RequestLoggingMiddleware`

A reference to the middleware instance that consumes these options. This property is set during middleware initialization and must not be null when logging methods are invoked.

### Methods

#### `LogRequest<T>`
`public void LogRequest<T>(HttpContext context)`

Logs the incoming request. The generic parameter `T` is a marker type (commonly the controller or handler class) used to enrich the log entry with a category name. The method extracts the HTTP method, path, arguments (if `LogArguments` is `true`), and selected headers from `context.Request`. Throws `ArgumentNullException` if `context` is `null`. Throws `InvalidOperationException` if `RequestLoggingMiddleware` has not been assigned.

#### `LogResponse<T>`
`public void LogResponse<T>(HttpContext context)`

Logs the outgoing response. The generic parameter `T` serves the same category-marker role as in `LogRequest<T>`. The method records the status code and, when `LogResponseData` is `true`, captures the response body up to `MaxLogValueLength` characters. Throws `ArgumentNullException` if `context` is `null`. Throws `InvalidOperationException` if `RequestLoggingMiddleware` has not been assigned.

#### `LogError`
`public void LogError(HttpContext context, Exception exception)`

Logs an unhandled exception that occurred during request processing. Records the exception message and, if `LogStackTrace` is `true`, the full stack trace. Throws `ArgumentNullException` if either `context` or `exception` is `null`. Throws `InvalidOperationException` if `RequestLoggingMiddleware` has not been assigned.

#### `LogPerformanceMetrics`
`public void LogPerformanceMetrics(HttpContext context, long elapsedMilliseconds)`

Logs request-performance data when `LogPerformanceMetrics` is `true`. The method records the elapsed time in milliseconds and the response status code. If `LogPerformanceMetrics` is `false`, the call is a no-op. Throws `ArgumentNullException` if `context` is `null`. Throws `ArgumentOutOfRangeException` if `elapsedMilliseconds` is negative. Throws `InvalidOperationException` if `RequestLoggingMiddleware` has not been assigned.

## Usage

### Example 1: Minimal setup with request and error logging

```csharp
var options = new RequestLoggingOptions
{
    LogArguments = true,
    LogStackTrace = true,
    MaxLogValueLength = 512
};

// Assume middleware is already instantiated and assigned.
options.RequestLoggingMiddleware = middleware;

// Inside a controller or minimal API handler:
app.MapGet("/items/{id}", async (HttpContext context, int id) =>
{
    options.LogRequest<ItemsHandler>(context);

    try
    {
        // ... process request ...
        context.Response.StatusCode = 200;
        options.LogResponse<ItemsHandler>(context);
    }
    catch (Exception ex)
    {
        options.LogError(context, ex);
        throw;
    }
});
```

### Example 2: Full telemetry with performance metrics

```csharp
var options = new RequestLoggingOptions
{
    LogArguments = true,
    LogResponseData = true,
    LogStackTrace = true,
    LogPerformanceMetrics = true,
    MaxLogValueLength = 2048
};

options.RequestLoggingMiddleware = middleware;

app.Use(async (HttpContext context, Func<Task> next) =>
{
    var sw = Stopwatch.StartNew();
    options.LogRequest<GlobalMiddleware>(context);

    try
    {
        await next();
        options.LogResponse<GlobalMiddleware>(context);
    }
    catch (Exception ex)
    {
        options.LogError(context, ex);
        throw;
    }
    finally
    {
        sw.Stop();
        options.LogPerformanceMetrics(context, sw.ElapsedMilliseconds);
    }
});
```

## Notes

- All logging methods require `RequestLoggingMiddleware` to be assigned before invocation; otherwise they throw `InvalidOperationException`. This is typically done once during application startup.
- `LogPerformanceMetrics` silently returns when `LogPerformanceMetrics` is `false`, making it safe to call unconditionally in a pipeline without checking the flag.
- The `MaxLogValueLength` truncation applies per value, not to the aggregate log entry. Setting it to a very small value may render arguments or response bodies unreadable.
- The generic type parameter `T` on `LogRequest<T>` and `LogResponse<T>` is used only for log-category derivation; it does not affect runtime behavior beyond the category string.
- None of the logging methods are thread-safe by themselves. If multiple threads share the same `RequestLoggingOptions` instance (e.g., in a singleton middleware), the caller must ensure that `HttpContext` instances are not accessed concurrently. The options properties themselves are simple value types and booleans; reads and writes to them from multiple threads without synchronization may observe stale values but will not cause tearing or crashes.
