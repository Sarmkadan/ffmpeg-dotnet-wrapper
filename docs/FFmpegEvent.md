# FFmpegEvent

The `FFmpegEvent` class is a data transfer object that encapsulates the context and state of an FFmpeg operation lifecycle event within the `ffmpeg-dotnet-wrapper` library. It provides structured access to event-specific information, including execution metadata, progress metrics, and diagnostic details for error handling, enabling consistent logging and monitoring across various multimedia processing operations.

## API

| Member | Type | Description |
| :--- | :--- | :--- |
| `EventId` | `string` | A unique identifier for the specific event instance. |
| `OccurredAt` | `DateTime` | The timestamp indicating when the event was generated. |
| `CorrelationId` | `string?` | An optional identifier used to correlate this event with a larger operation or transaction context. |
| `Source` | `string?` | An optional string identifying the source component or module that triggered the event. |
| `InputFile` | `string` | The file path or URI of the input media being processed. |
| `OutputFile` | `string` | The file path or URI of the target output media. |
| `OperationType` | `string` | A string representing the type of FFmpeg operation being performed (e.g., "Transcode", "Trim", "Watermark"). |
| `Metadata` | `Dictionary<string, object>?` | An optional dictionary containing additional contextual metadata related to the operation. |
| `Duration` | `TimeSpan` | The total processed duration if applicable to the event context; defaults to `TimeSpan.Zero` if not applicable. |
| `OutputFileSize` | `long` | The size of the output file in bytes, if available; otherwise 0. |
| `ErrorMessage` | `string` | A description of the error if the event represents a failure; empty if the operation is successful. |
| `ErrorCode` | `string?` | An optional machine-readable error code associated with the failure. |
| `StackTrace` | `string?` | An optional stack trace capturing the location of the error for diagnostic purposes. |
| `ProgressPercentage` | `double` | The current completion percentage of the operation, ranging from 0.0 to 100.0. |

## Usage

### Logging Event Details
```csharp
public void HandleFFmpegEvent(FFmpegEvent ffmpegEvent)
{
    Console.WriteLine($"[{ffmpegEvent.OccurredAt}] Operation: {ffmpegEvent.OperationType}");
    Console.WriteLine($"Input: {ffmpegEvent.InputFile} -> Output: {ffmpegEvent.OutputFile}");
    
    if (!string.IsNullOrEmpty(ffmpegEvent.ErrorMessage))
    {
        Console.Error.WriteLine($"Error occurred: {ffmpegEvent.ErrorMessage} (Code: {ffmpegEvent.ErrorCode})");
    }
}
```

### Monitoring Progress
```csharp
public void OnProgressUpdated(FFmpegEvent progressEvent)
{
    // Ensure we are in a progress-related event
    if (progressEvent.OperationType == "Transcode")
    {
        double percentage = progressEvent.ProgressPercentage;
        Console.WriteLine($"Operation {progressEvent.EventId} is {percentage:F2}% complete.");
    }
}
```

## Notes

*   **Property Populations:** Properties such as `Duration`, `OutputFileSize`, `ErrorMessage`, `ErrorCode`, `StackTrace`, and `ProgressPercentage` are populated based on the specific context of the event. Consumers should verify the `OperationType` or the state of the event before relying on these properties, as they may contain default values (e.g., empty strings or `TimeSpan.Zero`) if the information is not applicable to the current event phase.
*   **Thread Safety:** `FFmpegEvent` is designed as an immutable DTO for event propagation. Once instantiated, its properties should not be modified. It is inherently thread-safe for reading across multiple threads; however, care should be taken if the `Metadata` dictionary is modified, as `Dictionary<TKey, TValue>` is not thread-safe for concurrent read/write operations.
