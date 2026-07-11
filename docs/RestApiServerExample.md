# RestApiServerExample

The `RestApiServerExample` class serves as the entry point and request contract definition for a standalone REST API server designed to expose FFmpeg functionality over HTTP. It encapsulates the data transfer objects required to initiate media analysis, transcoding, and trimming operations, providing a structured interface for external clients to interact with the underlying `ffmpeg-dotnet-wrapper` capabilities without direct process management.

## API

### `Main`
```csharp
public static async Task Main
```
The primary entry point for the application. This method initializes the HTTP listener, configures routing for the supported media operations, and begins processing incoming requests asynchronously. It does not accept parameters and returns a `Task` that completes only when the server shuts down. This method throws an exception if the configured port is already in use, if required environment variables are missing, or if the underlying FFmpeg binary cannot be located or executed.

### `AnalyzeRequest`
```csharp
public record AnalyzeRequest
```
A record type representing the payload required to request media file analysis. It typically contains the path or URI of the source media file. Instances of this record are immutable by default. No specific parameters are enforced at the type level beyond standard record construction, but logical validation (such as file existence) occurs during request processing. Exceptions related to invalid file paths or unreadable media streams are thrown during the handling of this request, not during instantiation.

### `TranscodeRequest`
```csharp
public record TranscodeRequest
```
A record type defining the parameters for a media transcoding operation. This includes properties for the input source, output format, codec selection, and quality settings (e.g., bitrate, resolution). As a record, it provides value-based equality. Instantiation does not throw; however, processing a request with incompatible codec combinations or insufficient disk space for the output will result in runtime exceptions during the execution phase.

### `TrimRequest`
```csharp
public record TrimRequest
```
A record type used to specify parameters for trimming a media file. It generally includes the source file path, start time, duration (or end time), and the desired output container. This type ensures that trim configurations are passed as a single immutable unit. Errors such as negative durations, start times exceeding the media length, or invalid time formats are detected and thrown as exceptions when the request is processed by the server logic.

## Usage

### Starting the Server
The following example demonstrates how to launch the REST API server. Since `Main` is the entry point, it is typically invoked by the runtime, but can be called programmatically if embedded within a larger hosting environment.

```csharp
using FfmpegDotNetWrapper.Examples;

// Launch the server asynchronously
// This will block until the server is stopped or an error occurs
await RestApiServerExample.Main();
```

### Constructing Request Objects
Clients or internal handlers instantiate the request records to define operations before serializing them to JSON or passing them to the processing logic.

```csharp
using FfmpegDotNetWrapper.Examples;

// Create a request to analyze a video file
var analyzePayload = new AnalyzeRequest("input_video.mp4");

// Create a request to transcode video to H.264 with specific bitrate
var transcodePayload = new TranscodeRequest(
    InputPath: "source.mov",
    OutputPath: "converted.mp4",
    VideoCodec: "libx264",
    Bitrate: "2M"
);

// Create a request to trim the first 10 seconds of a clip
var trimPayload = new TrimRequest(
    InputPath: "long_clip.avi",
    OutputPath: "short_clip.mp4",
    StartTime: "00:00:05",
    Duration: "00:00:10"
);
```

## Notes

*   **Immutability**: All request types (`AnalyzeRequest`, `TranscodeRequest`, `TrimRequest`) are defined as `record` types. Consequently, they are immutable after instantiation. To modify a request, a new instance must be created using the `with` expression or by constructing a fresh object.
*   **Thread Safety**: The `Main` method is designed to run as a long-lived asynchronous task. While the request records themselves are thread-safe due to their immutability, the internal state managed by the server instance during `Main` execution is not explicitly guaranteed to be thread-safe for external manipulation. Concurrent requests are handled via the asynchronous pipeline, but direct interaction with the server's internal listeners from multiple threads should be avoided.
*   **Exception Handling**: None of the public members catch exceptions internally. Callers must wrap invocations of `Main` and the processing logic associated with the request records in appropriate `try-catch` blocks to handle `IOException`, `ArgumentException`, or FFmpeg-specific execution errors.
*   **Validation**: The records do not perform constructor-time validation of file paths or time formats. Validation occurs lazily when the request is executed by the server logic, meaning invalid data may be instantiated successfully but will cause failures during the asynchronous operation phase.
