// ... (rest of README.md content remains unchanged)

## ICacheService

The `ICacheService` interface provides an in-memory caching mechanism for storing frequently accessed data like media metadata and operation results. It supports time-based expiration, automatic cleanup of expired entries, and size-based eviction using an LRU (Least Recently Used) policy. This helps reduce unnecessary file system access and expensive FFmpeg probing operations.


Here is an example usage of the `ICacheService` interface with its public members:

```csharp
using FFmpegDotnetWrapper.Caching;
using Microsoft.Extensions.Logging;

// Create a cache service instance with default settings (max 1000 entries, 1 hour expiration)
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var cacheService = new CacheService(loggerFactory.CreateLogger<CacheService>());

// Store a value in the cache with default expiration (1 hour)
cacheService.Set("media-info-123", mediaMetadata);

// Store a value with custom expiration (5 minutes)
cacheService.Set("thumbnail-urls", thumbnailUrls, TimeSpan.FromMinutes(5));

// Retrieve a cached value
var cachedMediaInfo = cacheService.Get<MediaMetadata>("media-info-123");
if (cachedMediaInfo != null)
{
    Console.WriteLine($"Retrieved cached media info: {cachedMediaInfo.Duration}");
}

// Check if a key exists by attempting to retrieve it
var thumbnailUrls = cacheService.Get<List<string>>("thumbnail-urls");
if (thumbnailUrls != null)
{
    Console.WriteLine($"Found {thumbnailUrls.Count} cached thumbnails");
}

// Remove a specific cache entry
var wasRemoved = cacheService.Remove("media-info-123");
Console.WriteLine($"Entry removed: {wasRemoved}");

// Get current cache statistics
var stats = cacheService.GetStats();
Console.WriteLine($"Cache stats - Count: {stats.Count}, MaxSize: {stats.MaxSize}, Utilization: {stats.Utilization:F2}%");

// Clear the entire cache (useful during application shutdown or cache invalidation)
cacheService.Clear();

// The Count property allows checking cache size without retrieving statistics
var currentCount = cacheService.Count;
Console.WriteLine($"Current cache entries: {currentCount}");
```

## StreamingPipelineMetrics

The `StreamingPipelineMetrics` class provides real-time monitoring and analytics for streaming pipeline performance, tracking segment production, bitrate switches, pipeline completion/failure states, and comprehensive profile-based metrics breakdown. It enables detailed performance analysis and debugging of FFmpeg transcoding workflows by collecting and exporting operational data.

Here is an example usage of the `StreamingPipelineMetrics` class with its public members:

```csharp
using FFmpegDotnetWrapper.Monitoring;

// Create a metrics tracker for a specific profile
var metrics = new StreamingPipelineMetrics("hls-transcode-1080p");

// Record segment production events
metrics.RecordSegmentProduced("segment_001.ts", TimeSpan.FromSeconds(2.45));
metrics.RecordSegmentProduced("segment_002.ts", TimeSpan.FromSeconds(2.51));

// Record bitrate switches during encoding
metrics.RecordBitrateSwitch("1080p", 5000);  // kbps
metrics.RecordBitrateSwitch("720p", 3000);   // kbps
metrics.RecordBitrateSwitch("480p", 1500);   // kbps

// Record pipeline completion
metrics.RecordPipelineCompleted(TimeSpan.FromSeconds(125.3));

// Get detailed profile breakdown
var profileMetrics = metrics.GetProfileBreakdown();
foreach (var profile in profileMetrics)
{
    Console.WriteLine($"Profile {profile.Key}: {profile.Value.SegmentsProduced} segments, " +
                     $"Avg duration: {profile.Value.AverageSegmentDuration.TotalSeconds:F2}s");
}

// Generate summary report
var summary = metrics.GetSummaryReport();
Console.WriteLine(summary);

// Export metrics to CSV for external analysis
var csvData = metrics.ExportProfilesAsCsv();
File.WriteAllText("pipeline_metrics.csv", csvData);

// Reset metrics for a new encoding session
metrics.Reset();

// Create a new metrics instance with a different profile
var backupMetrics = new StreamingPipelineMetrics("backup-stream-720p");
```

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