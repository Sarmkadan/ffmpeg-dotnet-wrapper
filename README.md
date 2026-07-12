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
