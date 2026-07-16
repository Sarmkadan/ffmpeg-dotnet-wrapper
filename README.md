// ... (rest of README.md content remains unchanged)

## IntegrationExample

The `IntegrationExample` class demonstrates how to integrate the FFmpeg .NET Wrapper into ASP.NET Core applications. It shows dependency injection setup, service registration, and usage patterns in controllers and services. This example serves as a starting point for building web applications that leverage FFmpeg capabilities.

Here is an example usage of the `IntegrationExample` class with its public members:

```csharp
using FFmpegDotnetWrapper.Configuration;
using FFmpegDotnetWrapper.Models;
using FFmpegDotnetWrapper.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Create and configure the web host
var builder = WebApplication.CreateBuilder(args);

// Configure services
ConfigureServices(builder.Services);

// Build the application
var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Map minimal API endpoints for video processing
MapEndpoints(app);

app.Run("http://localhost:5000");

// Configure services including FFmpeg wrapper in the DI container
private static void ConfigureServices(IServiceCollection services)
{
    // Add controllers (if using MVC)
    services.AddControllers();

    // Add logging
    services.AddLogging(builder =>
    {
        builder.AddConsole();
        builder.AddDebug();
    });

    // Configure FFmpeg wrapper with application-specific settings
    services.AddFFmpegWrapper(options =>
    {
        // Set appropriate timeout for web operations (10 minutes)
        options.DefaultTimeout = TimeSpan.FromMinutes(10);

        // Enable detailed logging for debugging
        options.EnableDetailedLogging = false;

        // Configure operation caching for better performance
        options.EnableOperationCaching = true;
        options.MaxCachedOperations = 1000;
    });

    // Register additional services
    services.AddSingleton<VideoProcessingService>();
    services.AddSingleton<MediaAnalysisService>();
}

// Video trimming example using the service
var videoProcessingService = new VideoProcessingService(ffmpegService, logger);
var result = await videoProcessingService.ConvertForWebOptimizationAsync(
    inputFile: @"/videos/input.mp4",
    outputFile: @"/videos/web-optimized.mp4"
);

if (result.IsSuccess)
{
    Console.WriteLine($"Web optimization completed: {result.OutputMedia?.FileSize} bytes");
}

// Create thumbnail example
var thumbnailResult = await videoProcessingService.CreateThumbnailAsync(
    inputFile: @"/videos/input.mp4",
    outputFile: @"/thumbnails/preview.jpg",
    timestamp: TimeSpan.FromSeconds(30)
);

if (thumbnailResult.IsSuccess)
{
    Console.WriteLine("Thumbnail created successfully");
}

// Media analysis example
var mediaAnalysisService = new MediaAnalysisService(ffmpegService, logger);
var mediaInfo = await mediaAnalysisService.GetMediaInfoAsync(@"/videos/input.mp4");

Console.WriteLine($"Duration: {mediaInfo.Duration.TotalSeconds}s");
Console.WriteLine($"Resolution: {mediaInfo.Width}x{mediaInfo.Height}");
Console.WriteLine($"Video codec: {mediaInfo.VideoCodec}");
```

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

## OperationStats

The `OperationStats` class provides detailed statistics and performance metrics for FFmpeg operations, tracking success rates, execution times, data throughput, and failure analysis. It supports aggregating statistics across multiple operations and generating comprehensive performance reports for monitoring and optimization purposes.

Here is an example usage of the `OperationStats` class with its public members:

```csharp
using FFmpegDotnetWrapper.Monitoring;
using FFmpegDotnetWrapper.Models;

// Create operation statistics tracker for a specific operation type
var stats = new OperationStats(OperationType.Transcode);

// Record successful operations
stats.RecordSuccess(TimeSpan.FromSeconds(45.2), 157286400); // 150MB processed
stats.RecordSuccess(TimeSpan.FromSeconds(38.7), 125829120); // 120MB processed
stats.RecordSuccess(TimeSpan.FromSeconds(52.1), 214958080); // 205MB processed

// Record failed operations
stats.RecordFailure(TimeSpan.FromSeconds(12.5));
stats.RecordFailure(TimeSpan.FromSeconds(8.3));

// Get current statistics
Console.WriteLine($"Total attempts: {stats.TotalAttempts}");
Console.WriteLine($"Successful operations: {stats.SuccessfulOperations}");
Console.WriteLine($"Failed operations: {stats.FailedOperations}");
Console.WriteLine($"Total bytes processed: {stats.TotalBytesProcessed} ({stats.TotalBytesProcessed / (1024.0 * 1024.0):F2} MB)");
Console.WriteLine($"Total execution time: {stats.TotalExecutionTime}");
Console.WriteLine($"Average execution time: {stats.TotalExecutionTime / stats.TotalAttempts:F2}s");
Console.WriteLine($"Minimum execution time: {stats.MinimumExecutionTime}");
Console.WriteLine($"Maximum execution time: {stats.MaximumExecutionTime}");
Console.WriteLine($"Last updated: {stats.LastUpdated}");

// Get aggregate statistics from multiple operation instances
var allStats = new List<OperationStats> { stats, new OperationStats(OperationType.Watermark) };
var aggregateStats = OperationStats.GetAggregateStatistics(allStats);
Console.WriteLine($"Aggregate successful operations: {aggregateStats.SuccessfulOperations}");

// Generate performance report
var report = stats.GetPerformanceReport();
Console.WriteLine(report);

// Export statistics as CSV
var csv = stats.ExportAsCSV();
File.WriteAllText("operation_stats.csv", csv);

// Reset statistics for a new batch of operations
stats.Reset();

// Get statistics for a specific operation type
var transcodeStats = OperationStats.GetStatistics(OperationType.Transcode);
var watermarkStats = OperationStats.GetStatistics(OperationType.Watermark);

// Get all stored statistics
var allOperationStats = OperationStats.GetAllStatistics();
foreach (var operationStat in allOperationStats)
{
    Console.WriteLine($"{operationStat.Type}: {operationStat.SuccessfulOperations} successes, " +
    $"{operationStat.FailedOperations} failures");
}
```

## ProgressReport

The `ProgressReport` class provides detailed progress tracking information for FFmpeg operations, including completion percentage, items processed, timing metrics, throughput rates, and status messages. It is used by the `ProgressTracker` to report operation status during video processing workflows.

Here is an example usage of the `ProgressReport` class with its public members:

```csharp
using FFmpegDotnetWrapper.Utilities;
using System;

// Simulate a video processing operation with 1000 frames
var progressTracker = new ProgressTracker(totalItems: 1000);

// Simulate processing 250 frames (25% complete)
for (int i = 0; i < 250; i++)
{
    progressTracker.ReportItemProgress($"Processing frame {i + 1}");
}

// Get the current progress report
var progressReport = progressTracker.GetProgressReport();

Console.WriteLine($"Progress: {progressReport.ProgressPercentage:F1}%");
Console.WriteLine($"Items completed: {progressReport.ItemsCompleted}/{progressReport.TotalItems}");
Console.WriteLine($"Status: {progressReport.StatusMessage}");
Console.WriteLine($"Elapsed time: {progressReport.ElapsedTime}");
Console.WriteLine($"ETA: {progressReport.EstimatedTimeRemaining}");
Console.WriteLine($"Throughput: {progressReport.ThroughputItemsPerSecond:F2} items/sec");
Console.WriteLine($"Throughput: {progressReport.ThroughputBytesPerSecond / 1024 / 1024:F2} MB/sec");

// Reset for a new operation
progressTracker.Reset(totalItems: 500);

// Alternative: track by bytes processed
var byteProgressTracker = new ProgressTracker(totalBytes: 1000000000); // 1GB
byteProgressTracker.ReportBytesProgress(250000000, "Processing 250MB"); // 250MB
var byteProgressReport = byteProgressTracker.GetProgressReport();
Console.WriteLine($"Bytes processed: {byteProgressReport.ThroughputBytesPerSecond / 1024 / 1024:F2} MB/sec");
```

## FileUtilities

The `FileUtilities` class provides a collection of static methods for safe file system operations, including path validation, file accessibility checks, temporary file management, and format compatibility verification. It includes comprehensive security checks to prevent directory traversal attacks and handles edge cases like locked files, invalid paths, and permission issues gracefully.

Here is an example usage of the `FileUtilities` class with its public members:

```csharp
using FFmpegDotnetWrapper.Utilities;
using System;

// Validate file paths before processing
string inputPath = @"/home/user/videos/input.mp4";
string outputPath = @"/home/user/videos/output/processed.mp4";

if (FileUtilities.IsValidInputFile(inputPath))
{
    Console.WriteLine($"Input file exists and is accessible: {inputPath}");
    Console.WriteLine($"File size: {FileUtilities.GetHumanReadableFileSize(FileUtilities.GetFileSize(inputPath))}");
    Console.WriteLine($"File extension: {FileUtilities.GetFileExtension(inputPath)}");
}

// Validate output path and create directory if needed
if (FileUtilities.IsValidOutputPath(outputPath))
{
    Console.WriteLine($"Output path is valid and writable: {outputPath}");
}

// Generate a safe temporary file for intermediate processing
string tempFile = FileUtilities.GetTempFilePath(".tmp");
Console.WriteLine($"Temporary file created: {tempFile}");

// Sanitize filenames for user-provided input
string userFileName = "My Video #1!@#.mp4";
string safeFileName = FileUtilities.SanitizeFileName(userFileName);
Console.WriteLine($"Original: {userFileName}");
Console.WriteLine($"Sanitized: {safeFileName}"); // Output: My_Video__1____.mp4

// Check if two files have compatible formats for merging
string file1 = @"/videos/video1.mp4";
string file2 = @"/videos/video2.mp4";
bool formatsCompatible = FileUtilities.AreFormatsCompatible(file1, file2);
Console.WriteLine($"Formats compatible: {formatsCompatible}"); // Output: True

// Safely delete files that might be locked
bool deleted = FileUtilities.SafeDeleteFile(@"/tmp/old-temp-file.tmp");
Console.WriteLine($"File deleted successfully: {deleted}");

// Get human-readable file sizes for logging
long fileSize = 157286400; // 150MB
Console.WriteLine($"File size: {FileUtilities.GetHumanReadableFileSize(fileSize)}"); // Output: 150 MB
```

## ProcessUtilities

The `ProcessUtilities` class provides static methods for executing external processes with comprehensive output capture, timeout management, and error handling. It's designed for running command-line tools like FFmpeg and FFprobe safely, with support for both synchronous and asynchronous execution, progress tracking, and argument escaping to prevent command injection.

Here is an example usage of the `ProcessUtilities` class with its public members:

```csharp
using FFmpegDotnetWrapper.Utilities;
using System;
using System.Threading.Tasks;

// Check if FFmpeg is available in the system PATH
bool ffmpegAvailable = ProcessUtilities.IsExecutableAvailable("ffmpeg");
Console.WriteLine($"FFmpeg available: {ffmpegAvailable}");

// Execute FFmpeg synchronously to get media information
var mediaInfoResult = ProcessUtilities.ExecuteProcess(
    "ffmpeg",
    "-i input.mp4 -hide_banner -f null -",
    timeout: TimeSpan.FromSeconds(30)
);

Console.WriteLine($"Exit code: {mediaInfoResult.ExitCode}");
Console.WriteLine($"Execution time: {mediaInfoResult.ExecutionTime}");
Console.WriteLine($"Timed out: {mediaInfoResult.TimedOut}");

if (mediaInfoResult.Success)
{
    // Process succeeded
    Console.WriteLine("FFmpeg executed successfully");
    if (!string.IsNullOrEmpty(mediaInfoResult.StandardOutput))
    {
        // Parse output for media info
        Console.WriteLine(mediaInfoResult.StandardOutput);
    }
}
else
{
    // Process failed or timed out
    Console.WriteLine("FFmpeg failed:");
    Console.WriteLine(mediaInfoResult.StandardError);
}

// Execute FFmpeg asynchronously with cancellation support
var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
var asyncResult = await ProcessUtilities.ExecuteProcessAsync(
    "ffmpeg",
    "-i input.mp4 -c:v libx264 -preset fast -b:v 5000k output.mp4",
    workingDirectory: "/output",
    timeout: TimeSpan.FromMinutes(10),
    cancellationToken: cts.Token
);

Console.WriteLine($"Async execution completed in: {asyncResult.ExecutionTime}");
Console.WriteLine($"Frames processed: {asyncResult.StandardOutput.Split('\n').Count(line => line.Contains("frame="))}");

// Extract progress percentage from FFmpeg output (requires estimated total frames)
string ffmpegOutput = asyncResult.StandardOutput;
long estimatedFrames = 1000; // Would typically be calculated from input file
double progressPercentage = ProcessUtilities.ExtractProgressPercentage(ffmpegOutput, estimatedFrames);
Console.WriteLine($"Progress: {progressPercentage:F1}%");

// Safely escape command-line arguments to prevent injection
string inputFile = "/path with spaces/input.mp4";
string outputFile = "/output/result.mp4";
string escapedCommand = $"-i {ProcessUtilities.EscapeArgument(inputFile)} -o {ProcessUtilities.EscapeArgument(outputFile)}";
Console.WriteLine($"Safe command: {escapedCommand}");
```

## ExtensionMethods

The `ExtensionMethods` class provides a collection of extension methods that enhance standard .NET types with additional functionality. These methods improve code readability, reduce boilerplate, and provide convenient utilities for string manipulation, collection operations, time formatting, and file path handling throughout the FFmpeg wrapper library.

Here is an example usage of the `ExtensionMethods` class with its public members:

```csharp
using FFmpegDotnetWrapper.Utilities;
using System.Text;
using System.Linq;

// String manipulation extensions
var message = new StringBuilder();
message.AppendArgument("-i");
message.AppendArgument("/input/video.mp4");
message.AppendArguments("-c:v", "libx264", "-preset", "fast");
Console.WriteLine(message.ToString()); // Output: -i /input/video.mp4 -c:v libx264 -preset fast

var text = "Hello";
Console.WriteLine(text.Repeat(3)); // Output: HelloHelloHello

var items = new[] { "apple", "banana", "cherry" };
Console.WriteLine(items.Join(", ")); // Output: apple, banana, cherry
Console.WriteLine(items.Join(x => x.ToUpper(), " | ")); // Output: APPLE | BANANA | CHERRY

// String validation extensions
var emptyString = "";
Console.WriteLine(emptyString.IsNullOrWhiteSpace()); // Output: True
Console.WriteLine(emptyString.HasValue()); // Output: False

// Collection extensions
var numbers = new[] { 1, 2, 3, 4, 5, 6 };
var batches = numbers.Batch(2);
foreach (var batch in batches)
{
    Console.WriteLine(string.Join(", ", batch));
}
// Output:
// 1, 2
// 3, 4
// 5, 6

// Time and duration extensions
var duration = TimeSpan.FromSeconds(95.5);
Console.WriteLine(duration.FormatAsTime()); // Output: 01:35
Console.WriteLine(duration.ToSeconds()); // Output: 95.5
Console.WriteLine(duration.ToMilliseconds()); // Output: 95500

var timeString = "00:02:30";
var parsedSeconds = timeString.TryParseTime();
Console.WriteLine(parsedSeconds); // Output: 150

// File path extensions
var filePath = @"/home/user/videos/movie.mp4";
Console.WriteLine(filePath.GetFileName()); // Output: movie.mp4
Console.WriteLine(filePath.GetDirectoryPath()); // Output: /home/user/videos
Console.WriteLine(filePath.GetFileExtension()); // Output: mp4

// Size and bitrate formatting
Console.WriteLine(1024L.FormatAsSize()); // Output: 1.00 KB
Console.WriteLine(5000.FormatAsBitrate()); // Output: 5000 kbps
```

## ValidationUtilities

The `ValidationUtilities` class provides static methods for validating FFmpeg-related parameters such as bitrates, codecs, output formats, resolutions, frame rates, and various media settings. It includes validation for video files, time parsing/formatting, quality settings, watermark positioning, and aspect ratios, ensuring that all parameters passed to FFmpeg operations are valid and compatible.

Here is an example usage of the `ValidationUtilities` class with its public members:

```csharp
using FFmpegDotnetWrapper.Utilities;
using System;

// Validate video file paths and formats
string inputFile = @"/home/user/videos/input.mp4";
string outputFile = @"/home/user/videos/output/processed.mp4";

if (ValidationUtilities.IsValidVideo(inputFile))
{
    Console.WriteLine($"Valid video file: {inputFile}");
    Console.WriteLine($"Video duration: {ValidationUtilities.ParseTimeToSeconds(ValidationUtilities.GetVideoDuration(inputFile))} seconds");
}

// Validate FFmpeg codecs
string videoCodec = "libx264";
string audioCodec = "aac";

if (ValidationUtilities.IsValidCodec(videoCodec))
{
    Console.WriteLine($"Valid video codec: {videoCodec}");
}

if (ValidationUtilities.IsValidCodec(audioCodec))
{
    Console.WriteLine($"Valid audio codec: {audioCodec}");
}

// Validate output format
string outputFormat = "mp4";
if (ValidationUtilities.IsValidOutputFormat(outputFormat))
{
    Console.WriteLine($"Valid output format: {outputFormat}");
}

// Validate resolution
int width = 1920;
int height = 1080;
if (ValidationUtilities.IsValidResolution(width, height))
{
    Console.WriteLine($"Valid resolution: {ValidationUtilities.FormatResolution(width, height)}");
}

// Validate frame rate
if (ValidationUtilities.IsValidFrameRate(30.0))
{
    Console.WriteLine("Valid frame rate: 30 fps");
}

// Validate bitrate
if (ValidationUtilities.IsValidBitrate(5000))
{
    Console.WriteLine("Valid bitrate: 5000 kbps");
}

// Validate quality setting
if (ValidationUtilities.IsValidQualitySetting(23))
{
    Console.WriteLine("Valid quality setting: 23");
}

// Validate aspect ratio
if (ValidationUtilities.IsValidAspectRatio(16, 9))
{
    Console.WriteLine("Valid aspect ratio: 16:9");
}

// Validate and parse time strings
string timeString = "00:02:30";
double? seconds = ValidationUtilities.ParseTimeToSeconds(timeString);
if (seconds.HasValue)
{
    Console.WriteLine($"Parsed time: {timeString} -> {seconds.Value} seconds");
    Console.WriteLine($"Formatted back: {ValidationUtilities.FormatSecondsToTime(seconds.Value)}");
}

// Validate trim times
string startTime = "00:00:10";
string endTime = "00:01:45";
if (ValidationUtilities.ValidateTrimTimes(startTime, endTime))
{
    Console.WriteLine($"Valid trim range: {startTime} to {endTime}");
}

// Get supported codecs and formats
Console.WriteLine("Supported video codecs:");
foreach (var codec in ValidationUtilities.GetSupportedCodecs())
{
    Console.WriteLine($"  - {codec}");
}

Console.WriteLine("\nSupported output formats:");
foreach (var format in ValidationUtilities.GetSupportedFormats())
{
    Console.WriteLine($"  - {format}");
}

// Validate watermark settings
if (ValidationUtilities.IsValidWatermarkPosition(10, 10))
{
    Console.WriteLine("Valid watermark position: (10, 10)");
}

if (ValidationUtilities.IsValidWatermarkScale(0.5))
{
    Console.WriteLine("Valid watermark scale: 0.5");
}

if (ValidationUtilities.IsValidOpacity(0.75))
{
    Console.WriteLine("Valid opacity: 0.75");
}
```

## ConcatenationBuilderTests

The `ConcatenationBuilderTests` class provides unit tests for the `ConcatenationBuilder` class, verifying that video concatenation operations work correctly with various configurations including segment management, transitions, trimming, and error handling scenarios.

Here is an example usage of the `ConcatenationBuilderTests` class with its public members:

```csharp
using FFmpegDotnetWrapper.Builders;
using FFmpegDotnetWrapper.Models;
using Xunit;

// Test basic segment addition and ordering
var builder = new ConcatenationBuilder();
builder.Add("video1.mp4");
builder.Add("video2.mp4");
builder.Add("video3.mp4");

// Verify segments were added in correct order
Assert.Equal(3, builder.Segments.Count);
Assert.Equal("video1.mp4", builder.Segments[0].Path);
Assert.Equal("video2.mp4", builder.Segments[1].Path);
Assert.Equal("video3.mp4", builder.Segments[2].Path);

// Test segment insertion at specific position
builder.Insert(1, "video1_5.mp4");
Assert.Equal(4, builder.Segments.Count);
Assert.Equal("video1_5.mp4", builder.Segments[1].Path);

// Test segment removal
builder.Remove("video2.mp4");
Assert.Equal(3, builder.Segments.Count);
Assert.DoesNotContain(s => s.Path == "video2.mp4", builder.Segments);

// Test adding segments with trim parameters
builder.Add("long_video.mp4", trimStart: TimeSpan.FromSeconds(10), trimEnd: TimeSpan.FromSeconds(60));
Assert.Single(builder.Segments.Where(s => s.TrimStart.HasValue && s.TrimEnd.HasValue));

// Test adding transition between segments
builder.WithTransition(TimeSpan.FromSeconds(2.5));
Assert.Equal(TimeSpan.FromSeconds(2.5), builder.Transition.Duration);

// Test fluent API chaining
var settings = new ConcatenationSettings
{
    OutputPath = "/output/merged.mp4",
    VideoCodec = "libx264",
    AudioCodec = "aac"
};

var result = builder
    .WithTransition(TimeSpan.FromSeconds(1.5))
    .WithReencode()
    .Build(settings);

Assert.Equal("/output/merged.mp4", result.OutputPath);
Assert.Equal("libx264", result.VideoCodec);
Assert.Equal("aac", result.AudioCodec);
Assert.True(result.TranscodeOnMerge);

// Test reset functionality
builder.Reset();
Assert.Empty(builder.Segments);
Assert.Null(builder.Transition);
```

## ValidationUtilitiesTests

The `ValidationUtilitiesTests` class provides unit tests for the `ValidationUtilities` class, verifying that validation methods work correctly with various FFmpeg parameters including bitrates, codecs, output formats, resolutions, time parsing, and trim validation scenarios.

Here is an example usage of the `ValidationUtilitiesTests` class with its public members:

```csharp
using FFmpegDotnetWrapper.Utilities;
using Xunit;

// Test bitrate validation
[Fact]
public void IsValidBitrate_WithinRange_ReturnsTrue()
{
    Assert.True(ValidationUtilities.IsValidBitrate(5000));
    Assert.True(ValidationUtilities.IsValidBitrate(1000));
    Assert.True(ValidationUtilities.IsValidBitrate(100000));
}

[Fact]
public void IsValidBitrate_OutsideRange_ReturnsFalse()
{
    Assert.False(ValidationUtilities.IsValidBitrate(50));
    Assert.False(ValidationUtilities.IsValidBitrate(100001));
    Assert.False(ValidationUtilities.IsValidBitrate(-1));
}

// Test codec validation
[Fact]
public void IsValidCodec_SupportedCodec_ReturnsTrue()
{
    Assert.True(ValidationUtilities.IsValidCodec("libx264"));
    Assert.True(ValidationUtilities.IsValidCodec("aac"));
    Assert.True(ValidationUtilities.IsValidCodec("h264"));
}

[Fact]
public void IsValidCodec_UnsupportedOrEmpty_ReturnsFalse()
{
    Assert.False(ValidationUtilities.IsValidCodec(""));
    Assert.False(ValidationUtilities.IsValidCodec("invalid-codec"));
    Assert.False(ValidationUtilities.IsValidCodec(null));
}

// Test output format validation
[Fact]
public void IsValidOutputFormat_SupportedFormat_ReturnsTrue()
{
    Assert.True(ValidationUtilities.IsValidOutputFormat("mp4"));
    Assert.True(ValidationUtilities.IsValidOutputFormat("mov"));
    Assert.True(ValidationUtilities.IsValidOutputFormat("mkv"));
}

[Fact]
public void IsValidOutputFormat_UnrecognizedFormat_ReturnsFalse()
{
    Assert.False(ValidationUtilities.IsValidOutputFormat("invalid-format"));
    Assert.False(ValidationUtilities.IsValidOutputFormat(""));
}

// Test time parsing and formatting
[Fact]
public void ParseTimeToSeconds_HhMmSsFormat_ReturnsCorrectSeconds()
{
    Assert.Equal(90, ValidationUtilities.ParseTimeToSeconds("00:01:30"));
    Assert.Equal(3661, ValidationUtilities.ParseTimeToSeconds("01:01:01"));
    Assert.Equal(0, ValidationUtilities.ParseTimeToSeconds("00:00:00"));
}

[Fact]
public void ParseTimeToSeconds_PureSecondsString_ReturnsValue()
{
    Assert.Equal(125, ValidationUtilities.ParseTimeToSeconds("125"));
    Assert.Equal(0, ValidationUtilities.ParseTimeToSeconds("0"));
}

[Fact]
public void ParseTimeToSeconds_InvalidOrEmpty_ReturnsNull()
{
    Assert.Null(ValidationUtilities.ParseTimeToSeconds(""));
    Assert.Null(ValidationUtilities.ParseTimeToSeconds("invalid"));
    Assert.Null(ValidationUtilities.ParseTimeToSeconds("1:2:3"));
}

[Fact]
public void FormatSecondsToTime_VariousValues_ReturnsHhMmSs()
{
    Assert.Equal("00:01:30", ValidationUtilities.FormatSecondsToTime(90));
    Assert.Equal("01:01:01", ValidationUtilities.FormatSecondsToTime(3661));
    Assert.Equal("00:00:00", ValidationUtilities.FormatSecondsToTime(0));
}

[Fact]
public void FormatSecondsToTime_NegativeSeconds_ClampsToZero()
{
    Assert.Equal("00:00:00", ValidationUtilities.FormatSecondsToTime(-1));
    Assert.Equal("00:00:00", ValidationUtilities.FormatSecondsToTime(-100));
}

// Test resolution validation
[Fact]
public void IsValidResolution_ValidFormat_ReturnsTrue()
{
    Assert.True(ValidationUtilities.IsValidResolution(1920, 1080));
    Assert.True(ValidationUtilities.IsValidResolution(1280, 720));
    Assert.True(ValidationUtilities.IsValidResolution(640, 480));
}

[Fact]
public void IsValidResolution_InvalidFormat_ReturnsFalse()
{
    Assert.False(ValidationUtilities.IsValidResolution(0, 1080));
    Assert.False(ValidationUtilities.IsValidResolution(1920, 0));
    Assert.False(ValidationUtilities.IsValidResolution(-1, -1));
}

// Test trim time validation
[Fact]
public void ValidateTrimTimes_StartBeforeEnd_ReturnsTrue()
{
    Assert.True(ValidationUtilities.ValidateTrimTimes("00:00:10", "00:01:45"));
    Assert.True(ValidationUtilities.ValidateTrimTimes("00:00:00", "00:00:01"));
}

[Fact]
public void ValidateTrimTimes_StartGreaterThanEnd_ReturnsFalse()
{
    Assert.False(ValidationUtilities.ValidateTrimTimes("00:01:45", "00:00:10"));
    Assert.False(ValidationUtilities.ValidateTrimTimes("00:00:10", "00:00:05"));
}

[Fact]
public void ValidateTrimTimes_NegativeStart_ReturnsFalse()
{
    Assert.False(ValidationUtilities.ValidateTrimTimes("-00:00:10", "00:01:45"));
    Assert.False(ValidationUtilities.ValidateTrimTimes("-10", "00:01:45"));
}

[Fact]
public void ValidateTrimTimes_WithDurationOnly_ReturnsTrue()
{
    Assert.True(ValidationUtilities.ValidateTrimTimes("00:00:10", null, "00:01:35"));
    Assert.True(ValidationUtilities.ValidateTrimTimes("00:00:00", null, "00:05:00"));
}

[Fact]
public void ValidateTrimTimes_NoEndOrDuration_ReturnsFalse()
{
    Assert.False(ValidationUtilities.ValidateTrimTimes("00:00:10", null, null));
    Assert.False(ValidationUtilities.ValidateTrimTimes("00:00:00", null, null));
}

// Test watermark scale validation
[Fact]
public void IsValidWatermarkScale_ValidRange_ReturnsTrue()
{
    Assert.True(ValidationUtilities.IsValidWatermarkScale(0.1));
    Assert.True(ValidationUtilities.IsValidWatermarkScale(0.5));
    Assert.True(ValidationUtilities.IsValidWatermarkScale(0.9));
}

[Fact]
public void IsValidWatermarkScale_OutsideRange_ReturnsFalse()
{
    Assert.False(ValidationUtilities.IsValidWatermarkScale(0.0));
    Assert.False(ValidationUtilities.IsValidWatermarkScale(1.0));
    Assert.False(ValidationUtilities.IsValidWatermarkScale(1.1));
    Assert.False(ValidationUtilities.IsValidWatermarkScale(-0.1));
}
```

## TranscodeServiceTests

The `TranscodeServiceTests` class provides unit tests for the `TranscodeService` class, verifying that video transcoding operations work correctly with various configurations including web transcoding presets, bitrate adjustments, resolution changes, and error handling scenarios.

Here is an example usage of the `TranscodeServiceTests` class with its public members:

```csharp
using FFmpegDotnetWrapper.Services;
using FFmpegDotnetWrapper.Models;
using Xunit;

// Test web transcoding with standard HLS settings
var transcodeService = new TranscodeService();
var webResult = await transcodeService.TranscodeToWebAsync(
    inputPath: "/input/video.mp4",
    outputDirectory: "/output/hls",
    preset: WebPreset.Hls_720p
);
Assert.True(webResult.Success);

// Test bitrate adjustment with validation
var bitrateResult = await transcodeService.TranscodeWithBitrateAsync(
    inputPath: "/input/video.mp4",
    outputPath: "/output/bitrate.mp4",
    bitrateKbps: 3000,
    videoCodec: "libx264"
);
Assert.True(bitrateResult.Success);

// Test video resizing with validation
var resizeResult = await transcodeService.ResizeVideoAsync(
    inputPath: "/input/video.mp4",
    outputPath: "/output/resized.mp4",
    width: 1280,
    height: 720,
    keepAspectRatio: true
);
Assert.True(resizeResult.Success);

// Test audio extraction
var audioResult = await transcodeService.ExtractAudioAsync(
    inputPath: "/input/video.mp4",
    outputPath: "/output/audio.mp3"
);
Assert.True(audioResult.Success);

// Test error handling for invalid bitrate
await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
    transcodeService.TranscodeWithBitrateAsync(
        inputPath: "/input/video.mp4",
        outputPath: "/output/invalid.mp4",
        bitrateKbps: 50,
        videoCodec: "libx264"
    )
);

// Test error handling for zero dimensions
await Assert.ThrowsAsync<ArgumentException>(() =>
    transcodeService.ResizeVideoAsync(
        inputPath: "/input/video.mp4",
        outputPath: "/output/invalid.mp4",
        width: 0,
        height: 0
    )
);

// Test error handling for non-video input in audio extraction
await Assert.ThrowsAsync<ArgumentException>(() =>
    transcodeService.ExtractAudioAsync(
        inputPath: "/input/audio.mp3",
        outputPath: "/output/audio.mp3"
    )
);
```

## ThumbnailSettingsTests

The `ThumbnailSettingsTests` class provides unit tests for the `ThumbnailSettings` class, verifying that thumbnail extraction configurations work correctly with various settings including count, quality, dimensions, timestamps, and validation scenarios.

Here is an example usage of the `ThumbnailSettingsTests` class with its public members:

```csharp
using FFmpegDotnetWrapper.Models;
using FluentAssertions;
using Xunit;

// Create a new ThumbnailSettings instance with default values
var settings = new ThumbnailSettings();

// Verify default values
settings.Count.Should().Be(1);
settings.Format.Should().Be(ThumbnailFormat.Jpeg);
settings.Times.Should().BeEmpty();
settings.Width.Should().BeNull();
settings.Height.Should().BeNull();
settings.JpegQuality.Should().Be(2);

// Set valid count value
var settingsWithCount = new ThumbnailSettings { Count = 10 };
settingsWithCount.Count.Should().Be(10);

// Set valid JPEG quality value
var settingsWithQuality = new ThumbnailSettings { JpegQuality = 15 };
settingsWithQuality.JpegQuality.Should().Be(15);

// Add explicit timestamps for thumbnail extraction
var settingsWithTimestamps = new ThumbnailSettings();
settingsWithTimestamps.Times.Add(TimeSpan.FromSeconds(10));
settingsWithTimestamps.Times.Add(TimeSpan.FromSeconds(30));
settingsWithTimestamps.Times.Add(TimeSpan.FromSeconds(60));

// Set specific dimensions for thumbnails
var settingsWithDimensions = new ThumbnailSettings
{
    Width = 640,
    Height = 480
};
settingsWithDimensions.Width.Should().Be(640);
settingsWithDimensions.Height.Should().Be(480);

// Set auto width (negative value) with specific height
var settingsWithAutoWidth = new ThumbnailSettings
{
    Width = -1,
    Height = 720
};

// Clone settings to create an independent copy
var originalSettings = new ThumbnailSettings
{
    Count = 5,
    Format = ThumbnailFormat.Png,
    Width = 640,
    Height = 360
};
originalSettings.Times.Add(TimeSpan.FromSeconds(10));

var clonedSettings = originalSettings.Clone();

// Verify clone has same values
clonedSettings.Count.Should().Be(5);
clonedSettings.Format.Should().Be(ThumbnailFormat.Png);
clonedSettings.Width.Should().Be(640);
clonedSettings.Times.Should().HaveCount(1);

// Mutations on clone should not affect original
clonedSettings.Times.Add(TimeSpan.FromSeconds(20));
originalSettings.Times.Should().HaveCount(1);
```

## FileUtilitiesTests

The `FileUtilitiesTests` class provides unit tests for the `FileUtilities` class, verifying that file path validation, file operations, and utility methods work correctly. It includes tests for validating absolute and relative paths, handling edge cases like null/empty strings, directory traversal attempts, and environment variable expansion, as well as testing file existence checks and extension extraction.

Here is an example usage of the `FileUtilitiesTests` class with its public members:

```csharp
using FFmpegDotnetWrapper.Utilities;
using FluentAssertions;
using Xunit;

// Test that absolute file paths are considered valid
var absolutePath = Path.GetFullPath(@"/home/user/videos/input.mp4");
Assert.True(FileUtilities.IsValidFilePath(absolutePath));

// Test that relative paths are rejected for security
Assert.False(FileUtilities.IsValidFilePath("relative/path/file.mp4"));
Assert.False(FileUtilities.IsValidFilePath("../file.mp4"));

// Test that null and empty strings are handled gracefully
Assert.False(FileUtilities.IsValidFilePath(null));
Assert.False(FileUtilities.IsValidFilePath(string.Empty));
Assert.False(FileUtilities.IsValidFilePath(" "));

// Test that environment variables and tilde expansion are rejected
Assert.False(FileUtilities.IsValidFilePath("$HOME/file.mp4"));
Assert.False(FileUtilities.IsValidFilePath("~/file.mp4"));

// Test input file validation - must exist and be accessible
var testFile = @"/home/user/videos/input.mp4";
Assert.True(FileUtilities.IsValidInputFile(testFile));
Assert.False(FileUtilities.IsValidInputFile("/nonexistent/file.mp4"));
Assert.False(FileUtilities.IsValidInputFile("relative/path/file.mp4"));

// Test output path validation - must be absolute and in writable directory
var outputPath = @"/home/user/videos/output/processed.mp4";
Assert.True(FileUtilities.IsValidOutputPath(outputPath));

// Test output path with directory creation
var newOutputPath = @"/home/user/videos/newdir/output.mp4";
Assert.True(FileUtilities.IsValidOutputPath(newOutputPath, createDirectoryIfNeeded: true));
Assert.False(FileUtilities.IsValidOutputPath(newOutputPath, createDirectoryIfNeeded: false));

// Test file extension extraction
var extension = FileUtilities.GetFileExtension(@"/home/user/videos/input.mp4");
Assert.Equal("mp4", extension);

var mkvExtension = FileUtilities.GetFileExtension(@"/home/user/videos/input.mkv");
Assert.Equal("mkv", mkvExtension);

// Test file size utilities
var fileSize = FileUtilities.GetFileSize(@"/home/user/videos/input.mp4");
Assert.Greater(fileSize, 0);

var humanReadable = FileUtilities.GetHumanReadableFileSize(1572864); // 1.5 MB
Assert.Equal("1.5 MB", humanReadable);

// Test file operations
var tempFile = FileUtilities.GetTempFilePath(".tmp");
Assert.True(File.Exists(Path.GetDirectoryName(tempFile)));
Assert.Equal(".tmp", Path.GetExtension(tempFile));

// Test file sanitization
var unsafeFileName = "video\0copy.mp4";
var safeFileName = FileUtilities.SanitizeFileName(unsafeFileName);
Assert.DoesNotContain("\0", safeFileName);
Assert.Equal(".mp4", Path.GetExtension(safeFileName));

// Test format compatibility
Assert.True(FileUtilities.AreFormatsCompatible(@"/video1.mp4", @"/video2.mp4"));
Assert.False(FileUtilities.AreFormatsCompatible(@"/video.mp4", @"/video.mkv"));
```

## SubtitleSettingsTests

The `SubtitleSettingsTests` class provides unit tests for the `SubtitleSettings` class, verifying that subtitle configuration validation works correctly with various settings including file paths, character encoding, font properties, language specification, and validation scenarios.

Here is an example usage of the `SubtitleSettingsTests` class with its public members:

```csharp
using FFmpegDotnetWrapper.Models;
using FFmpegDotnetWrapper.Exceptions;
using FluentAssertions;

// Create a new SubtitleSettings instance with default values
var settings = new SubtitleSettings();

// Verify default values
settings.HardEmbed.Should().BeFalse();
settings.CharEncoding.Should().Be("UTF-8");
settings.FontName.Should().Be("Arial");
settings.FontSize.Should().Be(24);
settings.SubtitleStreamIndex.Should().Be(0);
settings.Language.Should().BeNull();

// Set subtitle file path (must exist and have supported extension like .srt or .ass)
var subtitlePath = @"/subtitles/english.srt";
settings.SubtitlePath = subtitlePath;
settings.SubtitlePath.Should().Be(Path.GetFullPath(subtitlePath));

// Configure subtitle embedding settings
var settingsWithEmbedding = new SubtitleSettings
{
    SubtitlePath = subtitlePath,
    HardEmbed = true,           // Embed subtitles directly into video stream
    CharEncoding = "UTF-8",      // Character encoding for subtitle file
    FontName = "Arial",          // Font family to use
    FontSize = 24,              // Font size in pixels
    Language = "en",             // Language code
    SubtitleStreamIndex = 0      // Stream index for embedded subtitles
};

// Validate settings before use (throws if invalid)
settingsWithEmbedding.Validate(); // No exception thrown for valid settings

// Test validation with invalid font size (too small)
var invalidSettings = new SubtitleSettings
{
    SubtitlePath = subtitlePath,
    FontSize = 5  // Below minimum of 8
};

var act = () => invalidSettings.Validate();
act.Should().Throw<InvalidOperationConfigurationException>()
    .WithMessage("*FontSize*");

// Test validation with non-existent file
var nonexistentSettings = new SubtitleSettings();
var fileAct = () => nonexistentSettings.SubtitlePath = @"/nonexistent/subtitles.srt";
fileAct.Should().Throw<InvalidOperationConfigurationException>()
    .WithMessage("*does not exist*");

// Clone settings to create an independent copy
var originalSettings = new SubtitleSettings
{
    SubtitlePath = subtitlePath,
    HardEmbed = true,
    FontSize = 30,
    Language = "fr"
};

var clonedSettings = originalSettings.Clone();

// Verify clone has same values
clonedSettings.SubtitlePath.Should().Be(originalSettings.SubtitlePath);
clonedSettings.HardEmbed.Should().Be(originalSettings.HardEmbed);
clonedSettings.FontSize.Should().Be(originalSettings.FontSize);
clonedSettings.Language.Should().Be(originalSettings.Language);

// Mutations on clone should not affect original
clonedSettings.FontSize = 20;
originalSettings.FontSize.Should().Be(30);
```

## TranscodeSettingsTests

The `TranscodeSettingsTests` class provides unit tests for the `TranscodeSettings` class, verifying that transcoding configuration validation works correctly with various settings including video/audio bitrates, frame rates, dimensions, codecs, containers, and validation scenarios.

Here is an example usage of the `TranscodeSettingsTests` class with its public members:

```csharp
using FFmpegDotnetWrapper.Models;
using FFmpegDotnetWrapper.Constants;
using FFmpegDotnetWrapper.Exceptions;
using FluentAssertions;

// Create a new TranscodeSettings instance with default values
var settings = new TranscodeSettings();

// Verify default values
settings.VideoCodec.Should().Be(VideoCodec.H264);
settings.AudioCodec.Should().Be(AudioCodec.AAC);
settings.Container.Should().Be(ContainerFormat.MP4);
settings.VideoBitrate.Should().Be(FFmpegConstants.DefaultBitrate);
settings.AudioBitrate.Should().Be(FFmpegConstants.DefaultAudioBitrate);
settings.FrameRate.Should().Be(FFmpegConstants.DefaultFrameRate);
settings.Quality.Should().Be(QualityPreset.Medium);
settings.EnableAutoScale.Should().BeTrue();
settings.PreserveAspectRatio.Should().BeTrue();
settings.TwoPass.Should().BeFalse();
settings.HardwareAcceleration.Should().Be(HwAccel.None);

// Configure transcoding settings for H.264 to MP4
var h264Settings = new TranscodeSettings
{
    VideoCodec = VideoCodec.H264,
    AudioCodec = AudioCodec.AAC,
    Container = ContainerFormat.MP4,
    VideoBitrate = 5000, // 5000 kbps
    AudioBitrate = 192, // 192 kbps
    FrameRate = 30,
    Width = 1920,
    Height = 1080,
    Quality = QualityPreset.High,
    EnableAutoScale = true,
    PreserveAspectRatio = true,
    TwoPass = false,
    HardwareAcceleration = HwAccel.NVENC,
    CustomFFmpegArgs = "-movflags +faststart"
};

// Validate settings before use (throws if invalid)
h264Settings.Validate(); // No exception thrown for valid settings

// Test validation with invalid video bitrate (too low)
var invalidBitrateSettings = new TranscodeSettings { VideoBitrate = 50 }; // Below minimum
var bitrateAct = () => invalidBitrateSettings.Validate();
bitrateAct.Should().Throw<InvalidOperationConfigurationException>()
    .WithMessage("*bitrate*");

// Test validation with incompatible codec/container combination
var invalidCodecSettings = new TranscodeSettings
{
    VideoCodec = VideoCodec.H264,
    Container = ContainerFormat.WebM // H.264 not supported in WebM
};
var codecAct = () => invalidCodecSettings.Validate();
codecAct.Should().Throw<InvalidOperationConfigurationException>()
    .WithMessage("*not supported*");

// Test validation with invalid dimensions (zero width)
var invalidDimensionsSettings = new TranscodeSettings { Width = 0 };
var dimensionsAct = () => invalidDimensionsSettings.Validate();
dimensionsAct.Should().Throw<InvalidOperationConfigurationException>()
    .WithMessage("*too small*");

// Clone settings to create an independent copy
var originalSettings = new TranscodeSettings
{
    VideoCodec = VideoCodec.VP9,
    VideoBitrate = 8000,
    Width = 1280,
    TwoPass = true
};

var clonedSettings = originalSettings.Clone();

// Verify clone has same values
clonedSettings.VideoCodec.Should().Be(VideoCodec.VP9);
clonedSettings.VideoBitrate.Should().Be(8000);
clonedSettings.Width.Should().Be(1280);
clonedSettings.TwoPass.Should().BeTrue();

// Mutations on clone should not affect original
clonedSettings.VideoBitrate = 6000;
originalSettings.VideoBitrate.Should().Be(8000);
```

## MediaFileTests

The `MediaFileTests` class provides unit tests for the `MediaFile` class, verifying that media file properties, constructors, and validation methods work correctly. It includes tests for file path validation, file properties extraction, video validation, metadata storage, and unique identifier generation.

Here is an example usage of the `MediaFileTests` class with its public members:

```csharp
using FFmpegDotnetWrapper.Models;
using FluentAssertions;
using Xunit;

// Create a new MediaFile instance using the default constructor
var mediaFile = new MediaFile();

// Verify default values
mediaFile.Id.Should().NotBeEmpty();
mediaFile.Name.Should().BeEmpty();
mediaFile.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
mediaFile.Metadata.Should().BeEmpty();

// Create a MediaFile instance from a real file path
var mediaFileFromPath = new MediaFile(@"/home/user/videos/sample.mp4");

// Verify file properties are correctly extracted
mediaFileFromPath.FilePath.Should().NotBeEmpty();
mediaFileFromPath.Name.Should().Be("sample");
mediaFileFromPath.Extension.Should().Be(".mp4");
mediaFileFromPath.FileSize.Should().BeGreaterThan(0);

// Set and validate video metadata
mediaFileFromPath.VideoCodec = "h264";
mediaFileFromPath.AudioCodec = "aac";
mediaFileFromPath.FrameRate = 30.0;
mediaFileFromPath.Bitrate = 5000000;
mediaFileFromPath.Duration = TimeSpan.FromSeconds(125);
mediaFileFromPath.Width = 1920;
mediaFileFromPath.Height = 1080;

// Validate the media file as a valid video
var act = () => mediaFileFromPath.ValidateAsVideo();
act.Should().NotThrow();

// Store arbitrary metadata for additional properties
mediaFileFromPath.Metadata["encoder"] = "libx264";
mediaFileFromPath.Metadata["profile"] = "High";
mediaFileFromPath.Metadata["created-by"] = "FFmpeg .NET Wrapper";

// Set descriptive properties
mediaFileFromPath.Description = "Sample video for testing transcoding operations";
mediaFileFromPath.ModifiedAt = DateTime.UtcNow;

// Verify unique ID generation
var anotherMediaFile = new MediaFile(@"/home/user/videos/another.mp4");
anotherMediaFile.Id.Should().NotBe(mediaFileFromPath.Id);

// Verify FilePath normalizes to absolute path
mediaFileFromPath.FilePath.Should().Be(Path.GetFullPath(@"/home/user/videos/sample.mp4"));
```

## FFmpegOperationTests

The `FFmpegOperationTests` class provides unit tests for FFmpeg operations including command line building, conversion results, and service mocking. It verifies that FFmpeg operations can be constructed with input files and arguments, cloned independently, and that conversion results can be marked as successful or failed with appropriate metrics and error messages.

Here is an example usage of the `FFmpegOperationTests` class with its public members:

```csharp
using FFmpegDotnetWrapper.Models;
using FFmpegDotnetWrapper.Services;
using FluentAssertions;

// Create an FFmpeg operation for transcoding
var operation = new FFmpegOperation
{
    Name = "Video transcoding",
    OutputFile = "/output/transcoded.mp4",
    Type = FFmpegOperationType.Transcode
};

// Add input files to the operation
operation.AddInputFile("/input/video1.mp4");
operation.AddInputFile("/input/video2.mp4");

// Add additional FFmpeg arguments
operation.AddArguments("-c:v", "libx264");
operation.AddArguments("-crf", "23");
operation.AddArguments("-preset", "fast");

// Build the complete FFmpeg command line
var commandLine = operation.BuildCommandLine();
Console.WriteLine(commandLine);
/* Output:
ffmpeg -i "/input/video1.mp4" -i "/input/video2.mp4" -c:v libx264 -crf 23 -preset fast "/output/transcoded.mp4"
*/

// Clone the operation to create an independent copy
var clonedOperation = operation.Clone();
clonedOperation.AddInputFile("/input/video3.mp4");

// Verify original operation is unchanged
Console.WriteLine($"Original inputs: {operation.InputFiles.Count}"); // Output: Original inputs: 2
Console.WriteLine($"Cloned inputs: {clonedOperation.InputFiles.Count}"); // Output: Cloned inputs: 3

// Create a conversion result and mark it as successful
var result = new ConversionResult();
result.MarkAsSuccess("/output/result.mp4");

// Set metrics on the result
result.SetMetric("bitrate", 5000);
result.SetMetric("fps", 30);

// Retrieve metrics
var bitrate = result.GetMetric<int>("bitrate");
var fps = result.GetMetric<int>("fps");
Console.WriteLine($"Bitrate: {bitrate} kbps, FPS: {fps}"); // Output: Bitrate: 5000 kbps, FPS: 30

// Mark a result as failed with an error message
var failedResult = new ConversionResult();
failedResult.MarkAsFailed("FFmpeg exited with code 1: invalid codec");

// Generate a summary for logging
var summary = failedResult.GenerateSummary();
Console.WriteLine(summary);
/* Output:
[Failed] FFmpeg exited with code 1: invalid codec
*/

// Calculate size reduction percentage (returns null if not successful)
var sizeReduction = result.GetSizeReductionPercentage(10_000_000); // 10MB input
Console.WriteLine($"Size reduction: {sizeReduction}%"); // Output depends on actual file sizes
```

## FormattingUtilities

The `FormattingUtilities` class provides a collection of static formatting methods for consistent string representation of FFmpeg-related data types. It handles time formatting, byte size formatting, bitrate formatting, resolution formatting, and various string sanitization utilities used throughout the library for logging, CLI output, and API responses.

Here is an example usage of the `FormattingUtilities` class with its public members:

```csharp
using FFmpegDotnetWrapper.Utilities;
using System;

// Format durations for logging and progress reporting
var duration = TimeSpan.FromSeconds(3725); // 1 hour, 2 minutes, 5 seconds
Console.WriteLine(FormattingUtilities.FormatDuration(duration)); // Output: 01:02:05

var shortDuration = TimeSpan.FromSeconds(95); // Less than 1 hour
Console.WriteLine(FormattingUtilities.FormatDuration(shortDuration)); // Output: 00:01:35

// Format byte sizes for file size display
Console.WriteLine(FormattingUtilities.FormatBytes(1024)); // Output: 1 KB
Console.WriteLine(FormattingUtilities.FormatBytes(1572864)); // Output: 1.5 MB
Console.WriteLine(FormattingUtilities.FormatBytes(2147483648)); // Output: 2 GB

// Format bitrates for encoding settings
Console.WriteLine(FormattingUtilities.FormatBitrate(5000)); // Output: 5000 Kbps
Console.WriteLine(FormattingUtilities.FormatBitrate(3000000)); // Output: 3 Mbps
Console.WriteLine(FormattingUtilities.FormatBitrate(2500000000)); // Output: 2.5 Gbps

// Format FFmpeg commands for logging (automatically masks file paths)
var ffmpegCommand = FormattingUtilities.FormatFFmpegCommand(
    "ffmpeg",
    "-i /home/user/input.mp4 -c:v libx264 -preset fast -b:v 5000k /output/output.mp4"
);
Console.WriteLine(ffmpegCommand);
/* Output:
ffmpeg \
 -i <input.mp4> \
 -c:v libx264 \
 -preset fast \
 -b:v 5000k \
 <output.mp4>
*/

// Parse FFmpeg progress output for display
var progressOutput = @"frame=  123: fps= 29.98 q=28.0 size=    123kB time=00:00:04.12 bitrate= 243kbits/s speed=1.21x"';
Console.WriteLine(FormattingUtilities.ExtractProgressSummary(progressOutput));
// Output: Frame: 123 | Speed: 1.21x | FPS: 29.98 | Bitrate: 243kbits/s

// Format progress time display (elapsed / estimated)
var elapsed = TimeSpan.FromSeconds(125);
var estimated = TimeSpan.FromSeconds(500);
Console.WriteLine(FormattingUtilities.FormatProgressTime(elapsed, estimated));
// Output: 00:02:05 / 00:08:20

// Calculate and format ETA
var progressPercentage = 25.0; // 25% complete
Console.WriteLine(FormattingUtilities.FormatETA(elapsed, progressPercentage));
// Output: ~00:06:15 remaining

// Format timestamps for logging
Console.WriteLine(FormattingUtilities.FormatTimestamp(DateTime.Now));
// Output: 2026-07-16 14:30:45.123

// Format resolution for video metadata
Console.WriteLine(FormattingUtilities.FormatResolution(1920, 1080)); // Output: 1920x1080
Console.WriteLine(FormattingUtilities.FormatResolution(1280, 720)); // Output: 1280x720

// Format percentages for progress display
Console.WriteLine(FormattingUtilities.FormatPercentage(25.5)); // Output: 25.5%
Console.WriteLine(FormattingUtilities.FormatPercentage(99.99)); // Output: 100.0%

// Truncate long strings for display
var longPath = @"/home/user/videos/very/long/path/with/many/nested/directories/file-name-that-is-quite-long.mp4";
Console.WriteLine(FormattingUtilities.TruncateString(longPath, 60));
// Output: /home/user/videos/very/long/path/with/many/n...mp4

// Sanitize strings for safe display
var unsafeString = "Hello World\tLine1\nLine2";
Console.WriteLine(FormattingUtilities.SanitizeForDisplay(unsafeString));
// Output: HelloWorld
Line1
Line2

// Convert kebab-case to Title Case
Console.WriteLine(FormattingUtilities.TitleCase("output-format")); // Output: Output Format
Console.WriteLine(FormattingUtilities.TitleCase("input_file-path")); // Output: Input File Path
```

## MediaFileTests

The `MediaFileTests` class provides unit tests for the `MediaFile` class, verifying that media file properties, constructors, and validation methods work correctly. It includes tests for file path validation, file properties extraction, video validation, metadata storage, and unique identifier generation.

Here is an example usage of the `MediaFileTests` class with its public members:

```csharp
using FFmpegDotnetWrapper.Models;
using FluentAssertions;
using Xunit;

// Create a new MediaFile instance using the default constructor
var mediaFile = new MediaFile();

// Verify default values
mediaFile.Id.Should().NotBeEmpty();
mediaFile.Name.Should().BeEmpty();
mediaFile.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
mediaFile.Metadata.Should().BeEmpty();

// Create a MediaFile instance from a real file path
var mediaFileFromPath = new MediaFile(@"/home/user/videos/sample.mp4");

// Verify file properties are correctly extracted
mediaFileFromPath.FilePath.Should().NotBeEmpty();
mediaFileFromPath.Name.Should().Be("sample");
mediaFileFromPath.Extension.Should().Be(".mp4");
mediaFileFromPath.FileSize.Should().BeGreaterThan(0);

// Set and validate video metadata
mediaFileFromPath.VideoCodec = "h264";
mediaFileFromPath.AudioCodec = "aac";
mediaFileFromPath.FrameRate = 30.0;
mediaFileFromPath.Bitrate = 5000000;
mediaFileFromPath.Duration = TimeSpan.FromSeconds(125);
mediaFileFromPath.Width = 1920;
mediaFileFromPath.Height = 1080;

// Validate the media file as a valid video
var act = () => mediaFileFromPath.ValidateAsVideo();
act.Should().NotThrow();

// Store arbitrary metadata for additional properties
mediaFileFromPath.Metadata["encoder"] = "libx264";
mediaFileFromPath.Metadata["profile"] = "High";
mediaFileFromPath.Metadata["created-by"] = "FFmpeg .NET Wrapper";

// Set descriptive properties
mediaFileFromPath.Description = "Sample video for testing transcoding operations";
mediaFileFromPath.ModifiedAt = DateTime.UtcNow;

// Verify unique ID generation
var anotherMediaFile = new MediaFile(@"/home/user/videos/another.mp4");
anotherMediaFile.Id.Should().NotBe(mediaFileFromPath.Id);

// Verify FilePath normalizes to absolute path
mediaFileFromPath.FilePath.Should().Be(Path.GetFullPath(@"/home/user/videos/sample.mp4"));
```

## FFmpegServiceIntegrationTests

The `FFmpegServiceIntegrationTests` class provides integration tests for the `FFmpegService` class, verifying end-to-end video processing workflows including transcoding, trimming, merging, watermarking, and batch operations. These tests ensure that the FFmpeg wrapper integrates correctly with the actual FFmpeg binary and produces expected results with various configurations and edge cases.

Here is an example usage of the `FFmpegServiceIntegrationTests` class with its public members:

```csharp
using FFmpegDotnetWrapper.Services;
using FFmpegDotnetWrapper.Models;

// Create an FFmpeg service instance
var ffmpegService = new FFmpegService();

// Test basic transcoding workflow
var transcodeResult = await ffmpegService.TranscodeAsync(
    inputPath: "/input/source.mp4",
    outputPath: "/output/transcoded.webm",
    settings: new TranscodeSettings
    {
        VideoCodec = VideoCodec.VP9,
        AudioCodec = AudioCodec.Vorbis,
        Container = ContainerFormat.WebM,
        VideoBitrate = 2500,
        Quality = QualityPreset.Medium
    }
);
Assert.True(transcodeResult.Success);

// Test hardware acceleration with NVENC
var hwResult = await ffmpegService.TranscodeAsync(
    inputPath: "/input/source.mp4",
    outputPath: "/output/hw-accelerated.mp4",
    settings: new TranscodeSettings
    {
        VideoCodec = VideoCodec.H264,
        HardwareAcceleration = HwAccel.NVENC,
        VideoBitrate = 5000,
        Quality = QualityPreset.High
    }
);
Assert.True(hwResult.Success);

// Test audio normalization workflow
var normalizeResult = await ffmpegService.TranscodeAsync(
    inputPath: "/input/source.mp4",
    outputPath: "/output/normalized.mp4",
    settings: new TranscodeSettings
    {
        VideoCodec = VideoCodec.H264,
        AudioNormalization = true,
        AudioBitrate = 192,
        VideoBitrate = 3000
    }
);
Assert.True(normalizeResult.Success);

// Test trimming workflow
var trimResult = await ffmpegService.TrimAsync(
    inputPath: "/input/source.mp4",
    outputPath: "/output/trimmed.mp4",
    startTime: TimeSpan.FromSeconds(10),
    duration: TimeSpan.FromSeconds(30)
);
Assert.True(trimResult.Success);

// Test trimming to preserve only audio
var audioOnlyResult = await ffmpegService.TrimAsync(
    inputPath: "/input/source.mp4",
    outputPath: "/output/audio-only.mp3",
    startTime: TimeSpan.FromSeconds(0),
    duration: TimeSpan.FromSeconds(60),
    videoStreamIndex: -1 // Exclude video stream
);
Assert.True(audioOnlyResult.Success);

// Test merging multiple videos
var mergeResult = await ffmpegService.MergeAsync(
    inputPaths: new List<string> { "/input/video1.mp4", "/input/video2.mp4" },
    outputPath: "/output/merged.mp4",
    transitionDuration: TimeSpan.FromSeconds(2)
);
Assert.True(mergeResult.Success);

// Test watermarking workflow
var watermarkResult = await ffmpegService.WatermarkAsync(
    inputPath: "/input/source.mp4",
    outputPath: "/output/watermarked.mp4",
    watermarkPath: "/watermark.png",
    positionX: 10,
    positionY: 10,
    opacity: 0.3,
    scale: 0.2
);
Assert.True(watermarkResult.Success);

// Test batch processing of multiple files
var batchResult = await ffmpegService.ProcessBatchAsync(
    operations: new List<BatchOperation>
    {
        new BatchOperation
        {
            InputPath = "/input/video1.mp4",
            OutputPath = "/output/processed1.mp4",
            OperationType = BatchOperationType.Transcode,
            Settings = new TranscodeSettings { VideoCodec = VideoCodec.H264 }
        },
        new BatchOperation
        {
            InputPath = "/input/video2.mp4",
            OutputPath = "/output/processed2.mp4",
            OperationType = BatchOperationType.Transcode,
            Settings = new TranscodeSettings { VideoCodec = VideoCodec.H264 }
        }
    },
    parallel: true
);
Assert.True(batchResult.All(r => r.Success));

// Test error handling with invalid input
var invalidResult = await ffmpegService.TranscodeAsync(
    inputPath: "/input/nonexistent.mp4",
    outputPath: "/output/output.mp4",
    settings: new TranscodeSettings { VideoCodec = VideoCodec.H264 }
);
Assert.False(invalidResult.Success);
Assert.Contains("not found", invalidResult.ErrorMessage);

// Test cancellation support
var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
var cancelResult = await ffmpegService.TranscodeAsync(
    inputPath: "/input/large-video.mp4",
    outputPath: "/output/cancelled.mp4",
    settings: new TranscodeSettings { VideoCodec = VideoCodec.H264 },
    cancellationToken: cts.Token
);
Assert.True(cancelResult.TimedOut || !cancelResult.Success);

// Test various codec and container combinations
var combinations = new List<(VideoCodec, AudioCodec, ContainerFormat)>
{
    (VideoCodec.H264, AudioCodec.AAC, ContainerFormat.MP4),
    (VideoCodec.VP9, AudioCodec.Vorbis, ContainerFormat.WebM),
    (VideoCodec.H265, AudioCodec.AAC, ContainerFormat.MP4),
    (VideoCodec.MPEG4, AudioCodec.MP3, ContainerFormat.AVI)
};

foreach (var (videoCodec, audioCodec, container) in combinations)
{
    var comboResult = await ffmpegService.TranscodeAsync(
        inputPath: "/input/source.mp4",
        outputPath: $@"/output/combo-{videoCodec}-{audioCodec}-{container}",
        settings: new TranscodeSettings
        {
            VideoCodec = videoCodec,
            AudioCodec = audioCodec,
            Container = container,
            VideoBitrate = 3000
        }
    );
    Assert.True(comboResult.Success);
}

// Test different quality presets
var presets = new[] { QualityPreset.Low, QualityPreset.Medium, QualityPreset.High, QualityPreset.VeryHigh };
foreach (var preset in presets)
{
    var presetResult = await ffmpegService.TranscodeAsync(
        inputPath: "/input/source.mp4",
        outputPath: $@"/output/preset-{preset}",
        settings: new TranscodeSettings
        {
            VideoCodec = VideoCodec.H264,
            Quality = preset,
            VideoBitrate = 4000
        }
    );
    Assert.True(presetResult.Success);
}

// Verify FFmpeg availability
var availability = ffmpegService.IsFFmpegAvailable();
Assert.True(availability);
```

## FormattingUtilitiesTests

The `FormattingUtilitiesTests` class provides unit tests for the `FormattingUtilities` class, verifying that formatting methods work correctly with various FFmpeg-related data types. It includes tests for duration formatting, byte size formatting, bitrate formatting, resolution formatting, string truncation, title casing, percentage formatting, ETA calculation, and string sanitization scenarios.

Here is an example usage of the `FormattingUtilitiesTests` class with its public members:

```csharp
using FFmpegDotnetWrapper.Utilities;
using Xunit;

// Test duration formatting for various time spans
[Fact]
public void FormatDuration_LessThanOneMinute_ReturnsZeroHoursAndMinutes()
{
    var duration = TimeSpan.FromSeconds(45);
    var result = FormattingUtilities.FormatDuration(duration);
    Assert.Equal("00:00:45", result);
}

[Fact]
public void FormatDuration_BetweenOneAndSixtyMinutes_ReturnsZeroHours()
{
    var duration = TimeSpan.FromMinutes(35);
    var result = FormattingUtilities.FormatDuration(duration);
    Assert.Equal("00:35:00", result);
}

[Fact]
public void FormatDuration_MoreThanOneHour_IncludesHours()
{
    var duration = TimeSpan.FromHours(2) + TimeSpan.FromMinutes(15) + TimeSpan.FromSeconds(30);
    var result = FormattingUtilities.FormatDuration(duration);
    Assert.Equal("02:15:30", result);
}

// Test byte size formatting
[Fact]
public void FormatBytes_LessThanOneKilobyte_ReturnsByteSuffix()
{
    var result = FormattingUtilities.FormatBytes(512);
    Assert.Equal("512 B", result);
}

[Fact]
public void FormatBytes_ExactMegabyte_ReturnsMbSuffix()
{
    var result = FormattingUtilities.FormatBytes(1048576);
    Assert.Equal("1.00 MB", result);
}

[Fact]
public void FormatBytes_LargeGigabyteValue_ReturnsGbSuffix()
{
    var result = FormattingUtilities.FormatBytes(5368709120); // 5 GB
    Assert.Equal("5.00 GB", result);
}

// Test bitrate formatting
[Fact]
public void FormatBitrate_BelowOneThousand_ReturnsKbps()
{
    var result = FormattingUtilities.FormatBitrate(500);
    Assert.Equal("500 Kbps", result);
}

[Fact]
public void FormatBitrate_Thousands_ReturnsMbps()
{
    var result = FormattingUtilities.FormatBitrate(3000);
    Assert.Equal("3 Mbps", result);
}

[Fact]
public void FormatBitrate_Millions_ReturnsGbps()
{
    var result = FormattingUtilities.FormatBitrate(2500000);
    Assert.Equal("2.5 Gbps", result);
}

// Test string truncation
[Fact]
public void TruncateString_BelowMaxLength_ReturnsUnchanged()
{
    var result = FormattingUtilities.TruncateString("short text", 20);
    Assert.Equal("short text", result);
}

[Fact]
public void TruncateString_ExceedsMaxLength_AppendsEllipsis()
{
    var longText = "This is a very long string that definitely exceeds the maximum length";
    var result = FormattingUtilities.TruncateString(longText, 30);
    Assert.Equal("This is a very long string th...", result);
}

[Fact]
public void TruncateString_NullOrEmpty_ReturnsEmptyString()
{
    var result1 = FormattingUtilities.TruncateString(null, 20);
    var result2 = FormattingUtilities.TruncateString(string.Empty, 20);
    Assert.Equal(string.Empty, result1);
    Assert.Equal(string.Empty, result2);
}

// Test title case conversion
[Fact]
public void TitleCase_KebabOrSnakeCase_ReturnsTitleCase()
{
    var result1 = FormattingUtilities.TitleCase("output-format");
    var result2 = FormattingUtilities.TitleCase("input_file-path");
    Assert.Equal("Output Format", result1);
    Assert.Equal("Input File Path", result2);
}

// Test percentage formatting
[Fact]
public void FormatPercentage_VariousValues_ReturnsOneDecimalPlace()
{
    var result1 = FormattingUtilities.FormatPercentage(25.5);
    var result2 = FormattingUtilities.FormatPercentage(99.99);
    var result3 = FormattingUtilities.FormatPercentage(0);
    Assert.Equal("25.5%", result1);
    Assert.Equal("100.0%", result2);
    Assert.Equal("0.0%", result3);
}

// Test ETA formatting
[Fact]
public void FormatETA_ZeroProgress_ReturnsCalculatingMessage()
{
    var result = FormattingUtilities.FormatETA(TimeSpan.Zero, 0);
    Assert.Equal("Calculating...", result);
}

[Fact]
public void FormatETA_HalfwayThrough_ReturnsRemainingTimeEstimate()
{
    var elapsed = TimeSpan.FromSeconds(125);
    var result = FormattingUtilities.FormatETA(elapsed, 50.0);
    Assert.Contains("remaining", result);
}

// Test string sanitization
[Fact]
public void SanitizeForDisplay_StringWithControlChars_RemovesThem()
{
    var unsafeString = "Hello World\tLine1\nLine2";
    var result = FormattingUtilities.SanitizeForDisplay(unsafeString);
    Assert.DoesNotContain(" ", result);
    Assert.Contains("Line1", result);
    Assert.Contains("Line2", result);
}

[Fact]
public void SanitizeForDisplay_StringWithNewline_PreservesNewline()
{
    var result = FormattingUtilities.SanitizeForDisplay("Line1\nLine2");
    Assert.Contains("Line1", result);
    Assert.Contains("Line2", result);
}

// Test resolution formatting
[Fact]
public void FormatResolution_StandardHd_ReturnsWidthXHeight()
{
    var result = FormattingUtilities.FormatResolution(1920, 1080);
    Assert.Equal("1920x1080", result);
}
```
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