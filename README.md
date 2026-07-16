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

## ApplicationStartup

The `ApplicationStartup` class provides static extension methods for configuring and initializing the FFmpeg wrapper application in ASP.NET Core or custom applications. It registers all services, middleware, event handlers, and provides access to configured services through the service provider.

```csharp
using FFmpegDotnetWrapper.Configuration;
using FFmpegDotnetWrapper.Events;
using FFmpegDotnetWrapper.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Create service collection
var services = new ServiceCollection();

// Add FFmpeg wrapper services with configuration
services.AddFFmpegWrapperWithConfiguration(
    ffmpegOptions: options =>
    {
        options.FFMpegPath = "/usr/bin/ffmpeg";
        options.FFProbePath = "/usr/bin/ffprobe";
        options.TempDirectory = "/tmp/ffmpeg";
    },
    cachingOptions: options =>
    {
        options.CacheDuration = TimeSpan.FromHours(1);
        options.MaxCacheSize = 1024 * 1024 * 1024; // 1GB
    },
    rateLimitingOptions: options =>
    {
        options.MaxRequestsPerMinute = 100;
        options.MaxConcurrentJobs = 10;
    }
);

// Configure logging
services.AddLogging(builder => builder.ConfigureFFmpegLogging(LogLevel.Debug));

// Build service provider
var serviceProvider = services.BuildServiceProvider();

// Initialize application (subscribes event handlers and validates FFmpeg)
await serviceProvider.InitializeApplicationAsync();

// Access configured services
var ffmpegOptions = serviceProvider.GetFFmpegOptions();
var cacheService = serviceProvider.GetCacheService();
var eventPublisher = serviceProvider.GetEventPublisher();
var backgroundJobService = serviceProvider.GetBackgroundJobService();
var rateLimiter = serviceProvider.GetRateLimiter();

// Register custom event handler
serviceProvider.RegisterEventHandler<OperationCompletedEvent, CustomOperationHandler>();
```

## StreamingPipelineOptions

`StreamingPipelineOptions` provides application-level configuration for the adaptive bitrate streaming pipeline. It controls global settings such as segment duration, concurrent pipeline limits, quality thresholds, and default output directories. These options can be bound from the `FFmpeg:Streaming` section of `appsettings.json` or configured programmatically.

```csharp
using FFmpegDotnetWrapper.Configuration;
using FFmpegDotnetWrapper.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

// Configure streaming pipeline options
var services = new ServiceCollection();

services.Configure<StreamingPipelineOptions>(options =>
{
    options.Enabled = true;
    options.DefaultSegmentDurationSeconds = 6;
    options.DefaultPlaylistWindowSize = 5;
    options.DefaultFormat = StreamingFormat.Hls;
    options.DefaultEncodeProfilesConcurrently = true;
    options.MaxConcurrentPipelines = 3;
    options.MaxConcurrentRenditionsPerPipeline = 2;
    options.DowngradeSpeedThreshold = 0.9;
    options.UpgradeSpeedThreshold = 1.5;
    options.BitrateDecisionWindowSegments = 3;
    options.DefaultOutputBaseDirectory = "/var/www/streaming";
    options.DefaultEnableHardwareAcceleration = true;
    
    options.DefaultProfiles = new List<StreamingProfileOptions>
    {
        new StreamingProfileOptions
        {
            Name = "720p",
            Width = 1280,
            Height = 720,
            VideoBitrateKbps = 2500,
            AudioBitrateKbps = 128,
            FrameRate = 30
        },
        new StreamingProfileOptions
        {
            Name = "480p",
            Width = 854,
            Height = 480,
            VideoBitrateKbps = 1000,
            AudioBitrateKbps = 96,
            FrameRate = 30
        }
    };
});

var serviceProvider = services.BuildServiceProvider();
var streamingOptions = serviceProvider.GetRequiredService<IOptions<StreamingPipelineOptions>>().Value;
```

## JsonOutputFormatter

`JsonOutputFormatter` centralises JSON serialization and deserialization for API responses, offering pretty‑printed output and custom converters for `TimeSpan` and `DateTime`. It also bundles CSV and plain‑text formatters for batch operation results, giving a consistent way to produce machine‑readable and human‑readable output.

```csharp
using System;
using System.Collections.Generic;
using FFmpegDotnetWrapper.Api.DTOs;
using FFmpegDotnetWrapper.Models;
using FFmpegDotnetWrapper.Serialization;

public class FormatterDemo
{
    public void Run()
    {
        // Initialise the JSON formatter (indented output)
        var jsonFormatter = new JsonOutputFormatter(indent: true);

        // Example API response containing a MediaFile
        var apiResponse = new ApiResponse<MediaFile>
        {
            Success = true,
            StatusCode = 200,
            Message = "File retrieved",
            Data = new MediaFile { Name = "sample.mp4", FilePath = "/videos/sample.mp4" }
        };

        // Serialize the response to JSON
        string json = jsonFormatter.Format(apiResponse);
        Console.WriteLine("JSON output:");
        Console.WriteLine(json);

        // Deserialize the JSON back to an ApiResponse<MediaFile>
        var deserialized = jsonFormatter.DeserializeApiResponse<MediaFile>(json);
        Console.WriteLine($"Deserialized success: {deserialized?.Success}");

        // Serialize an arbitrary object
        var anon = new { Greeting = "Hello", Timestamp = DateTime.UtcNow };
        string anonJson = jsonFormatter.Format(anon);
        Console.WriteLine("Anonymous object JSON:");
        Console.WriteLine(anonJson);

        // CSV formatter usage for batch conversion results
        var csvFormatter = new CsvOutputFormatter();
        var conversionResults = new List<ConversionResult>
        {
            new ConversionResult
            {
                InputFile = "video1.mp4",
                OutputFile = "video1.webm",
                Success = true,
                Duration = 12.5,
                ExecutionTime = TimeSpan.FromSeconds(13)
            },
            new ConversionResult
            {
                InputFile = "video2.mp4",
                OutputFile = "video2.webm",
                Success = false,
                ErrorMessage = "Unsupported codec"
            }
        };
        string csv = csvFormatter.FormatResults(conversionResults);
        Console.WriteLine("CSV output:");
        Console.WriteLine(csv);

        // Plain‑text formatter for a readable summary
        var textFormatter = new PlainTextFormatter();
        string plain = textFormatter.Format(apiResponse);
        Console.WriteLine("Plain‑text output:");
        Console.WriteLine(plain);
    }
}
```
