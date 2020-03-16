# ApplicationStartup
The `ApplicationStartup` type provides a set of static methods for initializing and configuring the ffmpeg-dotnet-wrapper application. It serves as a central hub for setting up the application's services, logging, and event handling, allowing developers to easily integrate the wrapper into their .NET applications.

## API
The `ApplicationStartup` type exposes the following public members:
* `AddFFmpegWrapper`: Adds the ffmpeg wrapper services to the specified `IServiceCollection`. Parameters: `IServiceCollection` services. Return value: `IServiceCollection`. Throws: None.
* `AddFFmpegWrapperWithConfiguration`: Adds the ffmpeg wrapper services to the specified `IServiceCollection` with the provided configuration. Parameters: `IServiceCollection` services, `Action<FFmpegOptions>` configureOptions. Return value: `IServiceCollection`. Throws: None.
* `InitializeApplicationAsync`: Initializes the application asynchronously. Parameters: None. Return value: `Task`. Throws: Exceptions may be thrown by the underlying initialization logic.
* `RegisterEventHandler<TEvent, THandler>`: Registers an event handler for the specified event type. Parameters: `TEvent` eventType, `THandler` handler. Return value: None. Throws: None.
* `ConfigureFFmpegLogging`: Configures the ffmpeg logging. Parameters: `ILoggingBuilder` logging. Return value: `ILoggingBuilder`. Throws: None.
* `GetFFmpegOptions`: Retrieves the ffmpeg options. Parameters: None. Return value: `FFmpegOptions`. Throws: None.
* `GetCacheService`: Retrieves the cache service. Parameters: None. Return value: `ICacheService`. Throws: None.
* `GetEventPublisher`: Retrieves the event publisher. Parameters: None. Return value: `IEventPublisher`. Throws: None.
* `GetBackgroundJobService`: Retrieves the background job service. Parameters: None. Return value: `IBackgroundJobService`. Throws: None.
* `GetRateLimiter`: Retrieves the rate limiter. Parameters: None. Return value: `IRateLimiter`. Throws: None.

## Usage
The following examples demonstrate how to use the `ApplicationStartup` type:
```csharp
// Example 1: Adding ffmpeg wrapper services to the service collection
var services = new ServiceCollection();
ApplicationStartup.AddFFmpegWrapper(services);

// Example 2: Initializing the application and registering an event handler
await ApplicationStartup.InitializeApplicationAsync();
ApplicationStartup.RegisterEventHandler<MyEvent, MyEventHandler>();
```

## Notes
When using the `ApplicationStartup` type, consider the following edge cases and thread-safety remarks:
* The `InitializeApplicationAsync` method should be called only once during the application's lifetime, as it performs initialization tasks that should not be repeated.
* The `RegisterEventHandler` method can be called multiple times to register different event handlers for the same event type.
* The `GetFFmpegOptions`, `GetCacheService`, `GetEventPublisher`, `GetBackgroundJobService`, and `GetRateLimiter` methods return shared instances, which should be used carefully in multi-threaded environments to avoid concurrency issues.
* The `ConfigureFFmpegLogging` method allows for customization of the ffmpeg logging behavior, which may impact the application's performance and logging output.
