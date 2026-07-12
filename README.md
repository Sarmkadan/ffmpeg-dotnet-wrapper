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
