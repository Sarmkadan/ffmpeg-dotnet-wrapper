// ... (rest of README.md content remains unchanged)

## FFmpegController

The `FFmpegController` class provides a REST API for FFmpeg transcoding, trimming, merging, and watermarking operations. It offers a fluent API for video transformation workflows with request validation and error handling.

Here is an example usage of the `FFmpegController` class with its public members:

```csharp
using FFmpegDotnetWrapper.Api.Controllers;
using FFmpegDotnetWrapper.Api.DTOs;
using FFmpegDotnetWrapper.Models;

// Create a new instance of the FFmpegController
var ffmpegController = new FFmpegController(new FFmpegService(), new Logger<FFmpegController>());

// Transcode a video file to a different format or codec
var transcodeRequest = new TranscodeRequest
{
    InputPath = "/input/video.mp4",
    OutputPath = "/output/video.mp4",
    OutputFormat = "mp4",
    Codec = "libx264",
    Bitrate = 5000,
    Quality = 20
};

var transcodeResponse = await ffmpegController.TranscodeAsync(transcodeRequest);
Console.WriteLine($"Transcode result: {transcodeResponse.Success}, StatusCode: {transcodeResponse.StatusCode}, Message: {transcodeResponse.Message}");

// Trim a video file to a specified duration or timeframe
var trimRequest = new TrimRequest
{
    InputPath = "/input/video.mp4",
    OutputPath = "/output/trimmed-video.mp4",
    StartTime = "00:00:10",
    Duration = "00:01:00"
};

var trimResponse = await ffmpegController.TrimAsync(trimRequest);
Console.WriteLine($"Trim result: {trimResponse.Success}, StatusCode: {trimResponse.StatusCode}, Message: {trimResponse.Message}");

// Merge multiple video files into a single output file
var mergeRequest = new MergeRequest
{
    InputPaths = new List<string> { "/input/video1.mp4", "/input/video2.mp4" },
    OutputPath = "/output/merged-video.mp4",
    MaintainAspectRatio = true
};

var mergeResponse = await ffmpegController.MergeAsync(mergeRequest);
Console.WriteLine($"Merge result: {mergeResponse.Success}, StatusCode: {mergeResponse.StatusCode}, Message: {mergeResponse.Message}");

// Add a watermark overlay to a video file
var watermarkRequest = new WatermarkRequest
{
    InputPath = "/input/video.mp4",
    OutputPath = "/output/watermarked-video.mp4",
    WatermarkPath = "/watermark.png",
    PositionX = 10,
    PositionY = 10,
    Opacity = 0.5,
    Scale = 0.5
};

var watermarkResponse = await ffmpegController.WatermarkAsync(watermarkRequest);
Console.WriteLine($"Watermark result: {watermarkResponse.Success}, StatusCode: {watermarkResponse.StatusCode}, Message: {watermarkResponse.Message}");

// Get media info for a file
var mediaInfoResponse = ffmpegController.GetMediaInfoAsync("/input/video.mp4");
Console.WriteLine($"Media info: {mediaInfoResponse.Success}, StatusCode: {mediaInfoResponse.StatusCode}, Message: {mediaInfoResponse.Message}");

// Embed subtitles into a video file
var subtitleRequest = new SubtitleRequest
{
    InputPath = "/input/video.mp4",
    OutputPath = "/output/subtitled-video.mp4",
    SubtitlePath = "/subtitle.srt",
    HardEmbed = true,
    Language = "en",
    FontName = "Arial",
    FontSize = 24
};

var subtitleResponse = await ffmpegController.EmbedSubtitlesAsync(subtitleRequest);
Console.WriteLine($"Subtitle embedding result: {subtitleResponse.Success}, StatusCode: {subtitleResponse.StatusCode}, Message: {subtitleResponse.Message}");

// Extract thumbnails from a video file
var thumbnailRequest = new ThumbnailRequest
{
    InputPath = "/input/video.mp4",
    OutputPattern = "/thumbnails/{index}.jpg",
    Count = 10,
    Width = 640,
    Height = 480,
    Format = "jpg"
};

var thumbnailResponse = await ffmpegController.ExtractThumbnailsAsync(thumbnailRequest);
Console.WriteLine($"Thumbnail extraction result: {thumbnailResponse.Success}, StatusCode: {thumbnailResponse.StatusCode}, Message: {thumbnailResponse.Message}");
```

```