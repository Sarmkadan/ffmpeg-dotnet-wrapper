// ... (rest of README.md content remains unchanged)

## TranscodeService

The `TranscodeService` class provides a set of methods for transcoding media files to various formats. It supports transcoding to web, H.265, mobile, and high-quality formats, as well as extracting audio and resizing video. 

```csharp
using FFmpegDotnetWrapper.Services;
using FFmpegDotnetWrapper.Models;

// Create TranscodeService instance
var transcodeService = new TranscodeService(new FFmpegService(), new Logger<TranscodeService>(new LoggerFactory()));

// Transcode to web format
var webResult = await transcodeService.TranscodeToWebAsync(new MediaFile { Name = "sample_video.mp4", FilePath = "/path/to/sample_video.mp4" }, "/path/to/output/web.mp4");

// Extract audio from video
var audioResult = await transcodeService.ExtractAudioAsync(new MediaFile { Name = "sample_video.mp4", FilePath = "/path/to/sample_video.mp4" }, "/path/to/output/audio.mp3");

// Resize video to specific resolution
var resizeResult = await transcodeService.ResizeVideoAsync(new MediaFile { Name = "sample_video.mp4", FilePath = "/path/to/sample_video.mp4" }, "/path/to/output/resized.mp4", 1280, 720);
```

// ... (rest of README.md content remains unchanged)
```