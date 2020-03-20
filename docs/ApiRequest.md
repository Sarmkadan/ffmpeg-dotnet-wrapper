# ApiRequest
The `ApiRequest` type represents a request for processing media files using the FFmpeg library. It encapsulates various parameters that define the input and output settings for the media processing operation, such as file paths, formats, codecs, and quality settings. This type is used to configure and initiate media processing tasks in the `ffmpeg-dotnet-wrapper` project.

## API
The `ApiRequest` type has the following public members:
* `RequestId`: A unique identifier for the request.
* `CreatedAt`: The date and time when the request was created.
* `CorrelationId`: An optional identifier for correlating related requests.
* `TenantId`: An optional identifier for the tenant associated with the request.
* `InputPath`: The path to the input media file.
* `OutputPath`: The path where the processed media file will be saved.
* `OutputFormat`: The format of the output media file.
* `Codec`: The optional codec to use for encoding the output media file.
* `Bitrate`: The optional bitrate to use for encoding the output media file.
* `Quality`: The optional quality setting to use for encoding the output media file.
* `StartTime`: The optional start time for processing a portion of the input media file.
* `EndTime`: The optional end time for processing a portion of the input media file.
* `Duration`: The optional duration for processing a portion of the input media file.
* `InputPaths`: A list of paths to input media files (for multi-file processing).
* `MaintainAspectRatio`: A flag indicating whether to maintain the aspect ratio of the input media file during processing.

## Usage
Here are two examples of using the `ApiRequest` type:
```csharp
// Example 1: Simple video conversion
var request = new ApiRequest
{
    InputPath = "input.mp4",
    OutputPath = "output.avi",
    OutputFormat = "avi"
};
// Process the request using the FFmpeg library

// Example 2: Advanced video processing with multiple input files
var request2 = new ApiRequest
{
    InputPaths = new List<string> { "input1.mp4", "input2.mp4" },
    OutputPath = "output.mp4",
    OutputFormat = "mp4",
    Codec = "h264",
    Bitrate = 1000000,
    Quality = 5,
    MaintainAspectRatio = true
};
// Process the request using the FFmpeg library
```

## Notes
When using the `ApiRequest` type, note that:
* The `InputPath` and `OutputPath` properties must be valid file paths.
* The `OutputFormat` property must be a supported format by the FFmpeg library.
* The `Codec`, `Bitrate`, and `Quality` properties are optional, but must be valid settings for the chosen output format.
* The `StartTime`, `EndTime`, and `Duration` properties are optional, but must be valid time settings for the input media file.
* The `MaintainAspectRatio` property only applies when the output format supports aspect ratio preservation.
* The `ApiRequest` type is not thread-safe, and concurrent access to its properties may result in unexpected behavior. It is recommended to create a new instance of `ApiRequest` for each media processing task to avoid conflicts.
