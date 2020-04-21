## FFmpegExceptionExtensions

The `FFmpegExceptionExtensions` class provides a set of extension methods for `FFmpegException` instances, allowing for more detailed error handling and analysis.

```csharp
// Example usage:
var ffmpegException = new FFmpegException("Error message");
if (FFmpegExceptionExtensions.IsProcessFailure(ffmpegException))
{
    Console.WriteLine("Process failure occurred.");
}
else if (FFmpegExceptionExtensions.IsInvalidMediaFileError(ffmpegException))
{
    Console.WriteLine("Invalid media file error occurred.");
}
```

## BackgroundJobExtensions

The `BackgroundJobExtensions` class provides utility methods for monitoring and managing background job lifecycles, including progress tracking, status checks, and metadata retrieval. It enables developers to inspect job state, update progress, and enforce timeouts.

```csharp
// Example usage:
void MonitorJob(BackgroundJob job)
{
    if (job.IsActive)
    {
        var progress = job.GetMetadataValue<float>("ProgressKey");
        job.UpdateProgress(progress + 5.0f);
        
        if (job.IsTakingTooLong(TimeSpan.FromMinutes(5)))
        {
            Console.WriteLine($"Job {job.GetMetadataValue<string>("jobId")} is stalled: {job.GetSummary()}");
        }
    }
    else if (job.IsCompletedSuccessfully)
    {
        Console.WriteLine($"Job completed in {job.GetTimeInfo.TotalSeconds:F1}s");
    }
    else if (job.IsFailed)
    {
        Console.WriteLine($"Job failed: {job.GetSummary()}");
    }
}
```

## WatermarkSettingsExtensions

The `WatermarkSettingsExtensions` class provides a fluent API for configuring watermark settings, enabling developers to define positions, scaling, animation, time constraints, and opacity in a declarative way.

```csharp
// Example usage:
var watermarkSettings = WatermarkSettingsExtensions.WithTopLeftPosition()
    .WithScale(0.5f)
    .WithOpacity(0.7f)
    .WithAnimation(TimeSpan.FromSeconds(1), EasingType.EaseInOut)
    .WithTimeConstraints(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30));
```

## ConversionResultExtensions

The `ConversionResultExtensions` class provides methods to analyze and summarize conversion results, including performance metrics, file size, warnings, and resource usage statistics. It enables developers to extract detailed insights about the conversion process.

```csharp
// Example usage:
var result = GetConversionResult(); // Assume this is obtained from a conversion operation
double? fps = ConversionResultExtensions.GetProcessingSpeedFps(result);
double? fileSize = ConversionResultExtensions.GetOutputFileSizeMb(result);
bool hasWarnings = ConversionResultExtensions.HasWarnings(result);
string duration = ConversionResultExtensions.GetFormattedDuration(result);
ConversionResultExtensions.AddPerformanceMetrics(result, metrics);
double? cpuUsage = ConversionResultExtensions.GetCpuUsage(result);
double? memoryUsage = ConversionResultExtensions.GetMemoryUsageMb(result);
bool completedOnTime = ConversionResultExtensions.CompletedWithinThreshold(result);
string summary = ConversionResultExtensions.GetMetricsSummary(result);

Console.WriteLine($"Conversion completed with {fps} FPS, {fileSize} MB output, {duration} duration.");
if (hasWarnings) Console.WriteLine("Warnings detected in conversion.");
Console.WriteLine($"CPU: {cpuUsage}%, Memory: {memoryUsage} MB, Threshold: {completedOnTime}");
Console.WriteLine($"Summary: {summary}");
```

## MediaFileExtensions

The `MediaFileExtensions` class provides metadata analysis for media files, including resolution checks, aspect ratio, duration formatting, file size formatting, quality descriptions, frame count, HDR metadata, and localized creation date.

```csharp
// Example usage:
string mediaFilePath = "example.mp4";

var isHD = MediaFileExtensions.IsHighDefinition(mediaFilePath);
var is4K = MediaFileExtensions.Is4K(mediaFilePath);
var aspectRatio = MediaFileExtensions.GetAspectRatio(mediaFilePath);
var duration = MediaFileExtensions.GetFormattedDuration(mediaFilePath);
var fileSize = MediaFileExtensions.GetFormattedFileSize(mediaFilePath);
var videoQuality = MediaFileExtensions.GetVideoQualityDescription(mediaFilePath);
var audioQuality = MediaFileExtensions.GetAudioQualityDescription(mediaFilePath);
var frameCount = MediaFileExtensions.GetFrameCount(mediaFilePath);
var estimatedFileSize = MediaFileExtensions.GetEstimatedFileSize(mediaFilePath);
var hasHDR = MediaFileExtensions.HasHDRMetadata(mediaFilePath);
var creationDate = MediaFileExtensions.GetLocalizedCreationDate(mediaFilePath);

Console.WriteLine($"Media Info:\n" +
                  $"  HD: {isHD}, 4K: {is4K}\n" +
                  $"  Aspect Ratio: {aspectRatio}\n" +
                  $"  Duration: {duration}\n" +
                  $"  File Size: {fileSize}\n" +
                  $"  Video Quality: {videoQuality}\n" +
                  $"  Audio Quality: {audioQuality}\n" +
                  $"  Frame Count: {frameCount}\n" +
                  $"  Estimated File Size: {estimatedFileSize}\n" +
                  $"  HDR: {hasHDR}\n" +
                  $"  Created: {creationDate}");
```

## TrimSettingsExtensions

The `TrimSettingsExtensions` class provides a set of extension methods for configuring trim settings. It enables developers to define start time offsets, duration adjustments, and other trim-related settings.

```csharp
// Example usage:
var trimSettings = TrimSettingsExtensions.WithStartTimeOffset(TimeSpan.FromSeconds(10))
    .WithDurationAdjustment(TimeSpan.FromSeconds(20));

var endTime = TrimSettingsExtensions.GetEndTime(trimSettings);
var trimmedDuration = TrimSettingsExtensions.GetTrimmedDurationOrZero(trimSettings);

Console.WriteLine($"Trimmed duration: {trimmedDuration}");
Console.WriteLine($"End time: {endTime}");

var trimmedSettings = TrimSettingsExtensions.TrimToEnd(trimSettings);
Console.WriteLine($"Preserves both streams: {TrimSettingsExtensions.PreservesBothStreams(trimmedSettings)}");
Console.WriteLine($"Preserves only audio: {TrimSettingsExtensions.PreservesOnlyAudio(trimmedSettings)}");
Console.WriteLine($"Preserves only video: {TrimSettingsExtensions.PreservesOnlyVideo(trimmedSettings)}");
Console.WriteLine($"Requires keyframes: {TrimSettingsExtensions.RequiresKeyframes(trimmedSettings)}");
```

## FFmpegControllerExtensions

The `FFmpegControllerExtensions` class provides a collection of extension methods for the `FFmpegController` that simplify common video processing operations such as transcoding, trimming, merging, adding watermarks, extracting thumbnails, and embedding subtitles. These methods handle error checking, provide sensible defaults, and return strongly-typed `ApiResponse<T>` objects for easy integration into applications.

```csharp
// Example usage:
var controller = new FFmpegController();

// Extract media information
var mediaInfo = controller.ExtractMediaInfo("input.mp4");
if (mediaInfo.Success)
{
    Console.WriteLine($"Duration: {mediaInfo.Data?.Duration}");
    Console.WriteLine($"Resolution: {mediaInfo.Data?.Width}x{mediaInfo.Data?.Height}");
}

// Transcode video with custom bitrate and quality
var transcodeResult = await controller.TranscodeAsync(
    "input.mp4", 
    "output_h265.mp4",
    bitrate: 3000,
    quality: 90
);

// Trim from start (first 30 seconds)
var trimResult = await controller.TrimFromStartAsync(
    "input.mp4",
    "trimmed.mp4",
    duration: 30.0
);

// Trim between specific times (10s to 45s)
var trimRangeResult = await controller.TrimAsync(
    "input.mp4",
    "trimmed_range.mp4",
    startTime: 10.0,
    endTime: 45.0
);

// Merge multiple videos
var mergeResult = await controller.MergeAsync(
    new[] { "part1.mp4", "part2.mp4", "part3.mp4" },
    "merged_output.mp4"
);

// Add watermark to video
var watermarkResult = await controller.AddWatermarkAsync(
    "input.mp4",
    "watermarked.mp4",
    "watermark.png",
    opacity: 0.6,
    scale: 0.25
);

// Extract thumbnails at regular intervals
var thumbnailsResult = await controller.ExtractThumbnailsAsync(
    "input.mp4",
    "thumbnails/thumb_{0}.jpg",
    count: 8,
    width: 480,
    height: 360
);

// Embed subtitles into video (hard-burned)
var subtitleResult = await controller.EmbedSubtitlesAsync(
    "input.mp4",
    "output_with_subs.mp4",
    "subtitles.srt",
    language: "eng",
    fontName: "Arial",
    fontSize: 28
);

// Chain multiple operations: trim → watermark → transcode
var chainResult = await controller.TrimWatermarkTranscodeAsync(
    "input.mp4",
    "final_output.mp4",
    trimDuration: 60.0,
    watermarkPath: "logo.png",
    targetBitrate: 2500
);
```

## FFmpegOptionsExtensions

The `FFmpegOptionsExtensions` class provides a collection of helper methods for inspecting and deriving configuration values from an `FFmpegOptions` instance. It simplifies retrieving effective paths, timeout settings, concurrency limits, supported formats, and default encoding parameters.

```csharp
// Example usage:
var options = new FFmpegOptions
{
    // assume properties are set as needed
};

string? ffmpegPath = FFmpegOptionsExtensions.GetEffectiveFFmpegPath(options);
string? ffprobePath = FFmpegOptionsExtensions.GetEffectiveFFprobePath(options);
bool hwAccel = FFmpegOptionsExtensions.IsHardwareAccelerationEnabled(options);
string preset = FFmpegOptionsExtensions.GetEffectiveEncodingPreset(options);
int timeoutMs = FFmpegOptionsExtensions.GetTimeoutMilliseconds(options);
bool canRunConcurrently = FFmpegOptionsExtensions.CanRunConcurrently(options);
int maxConcurrent = FFmpegOptionsExtensions.GetMaxConcurrentOperations(options);
bool formatSupported = FFmpegOptionsExtensions.IsFormatSupported(options, "mp4");
string supportedFormats = FFmpegOptionsExtensions.GetSupportedFormatsString(options);
string tempDir = FFmpegOptionsExtensions.GetEffectiveTemporaryDirectory(options);
bool keepTemp = FFmpegOptionsExtensions.ShouldKeepTemporaryFiles(options);
var (attempts, delayMs) = FFmpegOptionsExtensions.GetRetryConfiguration(options);
bool verbose = FFmpegOptionsExtensions.IsVerboseLoggingEnabled(options);
int defaultAudioBitrate = FFmpegOptionsExtensions.GetDefaultAudioBitrate(options);
int defaultVideoBitrate = FFmpegOptionsExtensions.GetDefaultVideoBitrate(options);
int? defaultQuality = FFmpegOptionsExtensions.GetDefaultQuality(options);
bool pathValidation = FFmpegOptionsExtensions.IsPathValidationEnabled(options);
bool outputPathValidation = FFmpegOptionsExtensions.IsOutputPathValidationEnabled(options);

Console.WriteLine($"FFmpeg: {ffmpegPath}, FFprobe: {ffprobePath}");
Console.WriteLine($"Hardware Acceleration: {hwAccel}, Preset: {preset}");
Console.WriteLine($"Timeout: {timeoutMs}ms, Max Concurrent: {maxConcurrent}");
Console.WriteLine($"Supported Formats: {supportedFormats}");
Console.WriteLine($"Temp Dir: {tempDir}, Keep Temp Files: {keepTemp}");
Console.WriteLine($"Retry: {attempts} attempts, {delayMs}ms delay");
Console.WriteLine($"Verbose Logging: {verbose}");
Console.WriteLine($"Default Audio Bitrate: {defaultAudioBitrate} kbps, Video Bitrate: {defaultVideoBitrate} kbps, Quality: {defaultQuality}");
Console.WriteLine($"Path Validation: {pathValidation}, Output Path Validation: {outputPathValidation}");
```
