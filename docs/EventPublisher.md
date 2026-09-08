# FFmpeg Event Publisher Documentation

## FFmpegEvent Hierarchy

### FFmpegEvent (Base Class)

**Purpose**: Base class for all events in the FFmpeg wrapper system. Provides timestamps and correlation IDs for tracking events across distributed systems.

**Public API**:

| Member | Signature | Description |
|--------|-----------|-------------|
| `EventId` | `string EventId { get; set; }` | Unique identifier for this event instance. Useful for deduplication and idempotent event processing. Defaults to a new GUID. |
| `OccurredAt` | `DateTime OccurredAt { get; set; }` | Timestamp when the event occurred (server time). All times are in UTC for consistency across time zones. Defaults to current UTC time. |
| `CorrelationId` | `string? CorrelationId { get; set; }` | Correlation ID linking related events in a workflow. Enables tracking of composite operations like "transcode → watermark → upload". |
| `Source` | `string? Source { get; set; }` | Optional source identifier indicating which operation triggered this event. Examples: "TranscodeService", "WatermarkService", "BatchProcessor". |

### OperationStartedEvent

**Purpose**: Event raised when a video processing operation starts.

**Public API**:

| Member | Signature | Description |
|--------|-----------|-------------|
| `InputFile` | `string InputFile { get; set; }` | Path to the input file being processed. Defaults to empty string. |
| `OutputFile` | `string OutputFile { get; set; }` | Path to the output file being generated. Defaults to empty string. |
| `OperationType` | `string OperationType { get; set; }` | Type of operation being performed (e.g., "Transcode", "Watermark"). Defaults to empty string. |
| `Metadata` | `Dictionary<string, object>? Metadata { get; set; }` | Additional metadata associated with the operation. Can be null. |

### OperationCompletedEvent

**Purpose**: Event raised when a video processing operation completes successfully.

**Public API**:

| Member | Signature | Description |
|--------|-----------|-------------|
| `InputFile` | `string InputFile { get; set; }` | Path to the input file that was processed. Defaults to empty string. |
| `OutputFile` | `string OutputFile { get; set; }` | Path to the output file that was generated. Defaults to empty string. |
| `OperationType` | `string OperationType { get; set; }` | Type of operation that was performed. Defaults to empty string. |
| `Duration` | `TimeSpan Duration { get; set; }` | Time taken to complete the operation. |
| `OutputFileSize` | `long OutputFileSize { get; set; }` | Size of the output file in bytes. |

### OperationFailedEvent

**Purpose**: Event raised when a video processing operation fails.

**Public API**:

| Member | Signature | Description |
|--------|-----------|-------------|
| `InputFile` | `string InputFile { get; set; }` | Path to the input file that failed processing. Defaults to empty string. |
| `OperationType` | `string OperationType { get; set; }` | Type of operation that failed. Defaults to empty string. |
| `ErrorMessage` | `string ErrorMessage { get; set; }` | Description of the error that occurred. Defaults to empty string. |
| `ErrorCode` | `string? ErrorCode { get; set; }` | Optional error code associated with the failure. |
| `StackTrace` | `string? StackTrace { get; set; }` | Optional stack trace for debugging purposes. |

### ProgressReportedEvent

**Purpose**: Event raised to report progress during long-running operations.

**Public API**:

| Member | Signature | Description |
|--------|-----------|-------------|
| `OperationType` | `string OperationType { get; set; }` | Type of operation being reported. Defaults to empty string. |
| `ProgressPercentage` | `double ProgressPercentage { get; set; }` | Progress percentage (0-100). |
| `ElapsedTime` | `TimeSpan ElapsedTime { get; set; }` | Time elapsed since the operation started. |
| `StatusMessage` | `string? StatusMessage { get; set; }` | Optional status message providing additional context. |

## IEventHandler<TEvent>

**Purpose**: Interface for event handlers that listen to specific event types. Implementations handle events asynchronously and can perform side effects.

**Public API**:

| Member | Signature | Description |
|--------|-----------|-------------|
| `HandleAsync` | `Task HandleAsync(TEvent @event)` | Asynchronously handles the specified event. The handler is contravariant in TEvent (using `in` keyword). |

## IEventPublisher

**Purpose**: Event publisher using pub-sub pattern for decoupled event handling. Subscribers register handlers for specific event types. Publisher notifies all registered handlers when events occur.

**Public API**:

| Member | Signature | Description |
|--------|-----------|-------------|
| `Subscribe<TEvent>` | `void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : FFmpegEvent` | Registers an event handler for a specific event type. The same handler can be registered multiple times (will be called multiple times). |
| `Unsubscribe<TEvent>` | `void Unsubscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : FFmpegEvent` | Unregisters an event handler from a specific event type. Removes only the first occurrence if registered multiple times. |
| `PublishAsync<TEvent>` | `Task PublishAsync<TEvent>(TEvent @event) where TEvent : FFmpegEvent` | Publishes an event to all registered subscribers. Handles both sync and async operations, ensuring all handlers are called. Catches and logs exceptions from individual handlers to prevent cascade failures. |

## EventPublisher

**Purpose**: Concrete implementation of `IEventPublisher` using a thread-safe dictionary to manage subscribers. Provides logging and diagnostic capabilities.

**Public API**:

| Member | Signature | Description |
|--------|-----------|-------------|
| Constructor | `EventPublisher(ILogger<EventPublisher> logger)` | Creates a new event publisher instance. Throws `ArgumentNullException` if logger is null. |
| `Subscribe<TEvent>` | `void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : FFmpegEvent` | Registers an event handler for a specific event type. Thread-safe. |
| `Unsubscribe<TEvent>` | `void Unsubscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : FFmpegEvent` | Unregisters an event handler from a specific event type. Thread-safe. |
| `PublishAsync<TEvent>` | `Task PublishAsync<TEvent>(TEvent @event) where TEvent : FFmpegEvent` | Publishes an event to all registered subscribers. Thread-safe. |
| `GetSubscriberCount<TEvent>` | `int GetSubscriberCount<TEvent>() where TEvent : FFmpegEvent` | Gets the number of registered subscribers for a specific event type. Useful for testing and debugging. Thread-safe. |
| `ClearSubscriptions` | `void ClearSubscriptions()` | Clears all event subscriptions. Used during shutdown or testing to reset event system state. Thread-safe. |

### Thread-Safety Notes

All public methods of `EventPublisher` are thread-safe and use locking (`_lockObject`) to protect access to the internal `_subscribers` dictionary. The `PublishAsync` method creates a copy of the subscriber list before invoking handlers to prevent modification during iteration.

### Exceptions Notes

- The constructor throws `ArgumentNullException` if the `logger` parameter is null.
- `Subscribe` throws `ArgumentNullException` if the `handler` parameter is null.
- `PublishAsync` throws `ArgumentNullException` if the `@event` parameter is null.
- Exceptions thrown by individual event handlers are caught and logged (as errors) but do not prevent other handlers from being invoked. The publisher continues to invoke all registered handlers even if some fail.
- Exceptions during the `Task.WhenAll` wait operation are caught and logged.

### C# Usage Example

```csharp
// Setup
var loggerFactory = LoggerFactory.Create(builder => 
    builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
var logger = loggerFactory.CreateLogger<EventPublisher>();
var publisher = new EventPublisher(logger);

// Define a custom event handler
public class ConsoleLogHandler : IEventHandler<OperationCompletedEvent>
{
    public Task HandleAsync(OperationCompletedEvent @event)
    {
        Console.WriteLine($"Operation { @event.OperationType } completed in { @event.Duration }");
        return Task.CompletedTask;
    }
}

// Subscribe to events
var handler = new ConsoleLogHandler();
publisher.Subscribe(handler);

// Publish an event
var completedEvent = new OperationCompletedEvent
{
    OperationType = "Transcode",
    Duration = TimeSpan.FromSeconds(30),
    InputFile = "input.mp4",
    OutputFile = "output.mp4",
    OutputFileSize = 1024000
};

await publisher.PublishAsync(completedEvent);

// Unsubscribe when done
publisher.Unsubscribe(handler);
```
