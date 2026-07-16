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