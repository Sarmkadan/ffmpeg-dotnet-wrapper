# ServiceCollectionExtensions

Provides extension methods for `IServiceCollection` to register FFmpeg wrapper services and configure their runtime behavior. This type centralizes dependency injection setup, allowing consumers to specify executable paths, logging verbosity, operation caching, and default timeouts in a fluent manner.

## API

### public static IServiceCollection AddFFmpegWrapper(this IServiceCollection services, Action<ServiceCollectionExtensions> configure)

Registers the core FFmpeg wrapper services into the service collection and applies the configuration provided by the delegate.

- **Parameters:**
  - `services` — The `IServiceCollection` to add the wrapper services to. Must not be null.
  - `configure` — A delegate that receives a `ServiceCollectionExtensions` instance for property-based configuration.
- **Returns:** The same `IServiceCollection` instance, enabling fluent chaining.
- **Throws:** `ArgumentNullException` if `services` or `configure` is null.

### public static IServiceCollection AddFFmpegWrapper(this IServiceCollection services)

Registers the core FFmpeg wrapper services with default configuration. Equivalent to calling the overload with an empty configure action.

- **Parameters:**
  - `services` — The `IServiceCollection` to add the wrapper services to. Must not be null.
- **Returns:** The same `IServiceCollection` instance.
- **Throws:** `ArgumentNullException` if `services` is null.

### public TimeSpan DefaultTimeout

Gets or sets the default maximum duration for FFmpeg operations initiated through the wrapper. When an operation exceeds this duration, it is cancelled.

- **Default:** 30 seconds.
- **Remarks:** Individual operations may override this value. Setting this to `TimeSpan.Zero` or a negative value disables the default timeout.

### public string? FFmpegPath

Gets or sets the file system path to the FFmpeg executable. When null, the wrapper attempts to locate `ffmpeg` via the system PATH.

- **Default:** null.
- **Remarks:** Assigning a path that does not point to a valid executable will cause runtime failures when operations are executed, not during service registration.

### public string? FFprobePath

Gets or sets the file system path to the FFprobe executable. When null, the wrapper attempts to locate `ffprobe` via the system PATH.

- **Default:** null.
- **Remarks:** Same resolution behavior as `FFmpegPath`.

### public LogLevel LogLevel

Gets or sets the minimum log level for messages emitted by the wrapper’s internal logger.

- **Default:** `LogLevel.Information`.
- **Remarks:** Setting this to `LogLevel.None` suppresses all wrapper-generated log output.

### public bool EnableOperationCaching

Gets or sets whether completed operation results (e.g., probe metadata) are cached to avoid redundant executions.

- **Default:** false.
- **Remarks:** Caching is keyed by operation arguments. Changing this value after services are built has no effect.

### public int MaxCachedOperations

Gets or sets the maximum number of cached operation results when `EnableOperationCaching` is true. When the limit is exceeded, the least recently used entry is evicted.

- **Default:** 100.
- **Remarks:** Values less than 1 are clamped to 1 internally.

### public bool EnableDetailedLogging

Gets or sets whether the wrapper includes FFmpeg standard error output and internal diagnostic messages in its log stream.

- **Default:** false.
- **Remarks:** When enabled, log volume can increase significantly for long-running or complex operations.

## Usage

### Example 1: Basic registration with custom paths and timeout

```csharp
services.AddFFmpegWrapper(cfg =>
{
    cfg.FFmpegPath = "/usr/local/bin/ffmpeg";
    cfg.FFprobePath = "/usr/local/bin/ffprobe";
    cfg.DefaultTimeout = TimeSpan.FromMinutes(2);
});
```

### Example 2: Enabling caching and detailed logging for diagnostics

```csharp
services.AddFFmpegWrapper(cfg =>
{
    cfg.EnableOperationCaching = true;
    cfg.MaxCachedOperations = 50;
    cfg.EnableDetailedLogging = true;
    cfg.LogLevel = LogLevel.Debug;
});
```

## Notes

- **Thread safety:** Configuration properties are not thread-safe during the `configure` delegate execution. Once `AddFFmpegWrapper` returns, the registered services are safe for concurrent resolution and use, assuming the underlying DI container supports concurrent service access.
- **Path validation:** The `FFmpegPath` and `FFprobePath` properties are not validated at configuration time. If an invalid path is supplied, exceptions will surface when the first operation attempts to start the corresponding process.
- **Timeout behavior:** `DefaultTimeout` applies to operations that do not specify their own timeout. Setting it to `TimeSpan.Zero` removes the default timeout, meaning operations without an explicit timeout will run indefinitely.
- **Caching scope:** Caching is scoped to the service lifetime. If services are registered as transient or scoped, cache entries may not persist across resolutions unless the underlying cache store is a singleton.
- **Configuration immutability:** Changes to `ServiceCollectionExtensions` properties after the `AddFFmpegWrapper` method returns do not affect already-registered services.
