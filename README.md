// ... (rest of README.md content remains unchanged)

## ApiRequest

The `ApiRequest` class serves as the base class for all API request DTOs (Data Transfer Objects) in the FFmpeg wrapper. It provides common properties for tracking and correlation across distributed systems, including a unique request identifier, creation timestamp, and optional correlation and tenant identifiers.

Here is an example usage of the `ApiRequest` class with its public members:

```csharp
using FFmpegDotnetWrapper.Api.DTOs;

// Create a TranscodeRequest, which inherits from ApiRequest
var transcodeRequest = new TranscodeRequest
{
    RequestId = Guid.NewGuid().ToString(),
    CreatedAt = DateTime.UtcNow,
    CorrelationId = "workflow-123",
    TenantId = "customer-1",
    InputPath = "/input/video.mp4",
    OutputPath = "/output/video.mp4",
    OutputFormat = "mp4",
    Codec = "libx264",
    Bitrate = 5000,
    Quality = 20
};

// Similarly, you can create other request types like TrimRequest, MergeRequest, etc.
// For example, a TrimRequest:
var trimRequest = new TrimRequest
{
    RequestId = Guid.NewGuid().ToString(),
    CreatedAt = DateTime.UtcNow,
    InputPath = "/input/video.mp4",
    OutputPath = "/output/trimmed-video.mp4",
    StartTime = "00:00:10",
    Duration = "00:01:00"
};

// Or a MergeRequest:
var mergeRequest = new MergeRequest
{
    RequestId = Guid.NewGuid().ToString(),
    CreatedAt = DateTime.UtcNow,
    InputPaths = new List<string> { "/input/video1.mp4", "/input/video2.mp4" },
    OutputPath = "/output/merged-video.mp4",
    MaintainAspectRatio = true
};
```

