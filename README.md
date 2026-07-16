// ... (rest of README.md content remains unchanged)

## TrimSettings

Represents settings for trimming media files while preserving audio/video streams and controlling keyframe behavior. The `TrimSettings` class provides methods to calculate trimmed durations, validate settings, and clone configurations.

```csharp
// Create trim settings for a 30-second video, preserving both audio and video streams
var trimSettings = new TrimSettings
{
    EndTime = TimeSpan.FromSeconds(30),
    PreserveAudio = true,
    PreserveVideo = true,
    Keyframe = false
};

// Validate the settings before use
trimSettings.Validate();

// Calculate the end time (handles null EndTime by using media duration)
var actualEndTime = trimSettings.CalculateEndTime(TimeSpan.FromSeconds(60));
Console.WriteLine($"Trimming from 00:00:00 to {actualEndTime.TotalSeconds} seconds");

// Get the duration of the trimmed segment
var trimmedDuration = trimSettings.GetTrimmedDuration(TimeSpan.FromSeconds(60));
Console.WriteLine($"Trimmed duration: {trimmedDuration.TotalSeconds} seconds");

// Clone settings for reuse
var clonedSettings = trimSettings.Clone();
clonedSettings.EndTime = TimeSpan.FromSeconds(45);
```

## QueuedJob

Represents a queued job with priority and execution metadata. It provides properties to access the job's ID, priority, enqueued time, due time, retry count, maximum retries, payload, tags, and more.

```csharp
var job = new QueuedJob
{
    JobId = Guid.NewGuid().ToString(),
    Priority = 5,
    EnqueuedAt = DateTime.UtcNow,
    DueAt = DateTime.UtcNow.AddMinutes(10),
    RetryCount = 0,
    MaxRetries = 3,
    Payload = "Process video file",
    Tags = new Dictionary<string, string> { { "video", "mp4" } }
};

Console.WriteLine($"Job ID: {job.JobId}");
Console.WriteLine($"Priority: {job.Priority}");
Console.WriteLine($"Enqueued at: {job.EnqueuedAt}");
Console.WriteLine($"Due at: {job.DueAt}");
Console.WriteLine($"Retry count: {job.RetryCount}/{job.MaxRetries}");
Console.WriteLine($"Payload: {job.Payload}");
Console.WriteLine($"Tags: {string.Join(", ", job.Tags.Select(x => $"{x.Key}={x.Value}"))}");
```

## ConversionResult

Represents the result of a media file conversion operation. This class captures comprehensive conversion metadata including success status, output file information, timing metrics, error details, and custom metrics collection. It provides methods to calculate performance metrics, mark operations as successful or failed, and generate detailed summary reports.

```csharp
// Create a conversion result for a successful conversion
var result = new ConversionResult
{
    Id = Guid.NewGuid().ToString(),
    Duration = TimeSpan.FromSeconds(45.2),
    CreatedAt = DateTime.UtcNow,
    OutputFilePath = "/output/processed-video.mp4",
    OutputMedia = new MediaFile { FileSize = 15728640 } // 15 MB
};

// Mark as successful and set metrics
result.MarkAsSuccess("/output/processed-video.mp4");
result.SetMetric("frames_per_second", 30.0);
result.SetMetric("bitrate_kbps", 5000);
result.SetMetric("codec_used", "h264");

// Calculate size reduction
var originalSize = 25165824L; // 24 MB
var reductionPercent = result.GetSizeReductionPercentage(originalSize);
Console.WriteLine($"Size reduction: {reductionPercent:F1}%");

// Get elapsed time
var elapsed = result.GetElapsedTime();
Console.WriteLine($"Conversion took: {elapsed.TotalSeconds:F2} seconds");

// Generate summary report
var summary = result.GenerateSummary();
Console.WriteLine(summary);

// Access metrics
var codec = result.GetMetric<string>("codec_used");
var fps = result.GetMetric<double>("frames_per_second");
```

## BackgroundJob

Represents an asynchronous background job with progress tracking and status monitoring capabilities. The `BackgroundJob` type provides comprehensive job lifecycle management through properties like `JobId`, `JobName`, state tracking via `State`, progress reporting with `ProgressPercentage`, and detailed status information including `StatusMessage`, timestamps (`CreatedAt`, `StartedAt`, `CompletedAt`), error handling via `ErrorMessage` and `StackTrace`, and extensible metadata storage in `Metadata`.

The `BackgroundJobService` class offers job management operations including enqueuing new jobs, retrieving jobs by ID or status, canceling active jobs, and updating job progress in real-time.

```csharp
// Create background job service
var jobService = new BackgroundJobService();

// Enqueue a new background job
var jobId = jobService.EnqueueJob("Video Processing Job", new Dictionary<string, object>
{
    { "inputFile", "/videos/input.mp4" },
    { "outputFile", "/videos/output.mp4" },
    { "preset", "medium" }
});

Console.WriteLine($"Enqueued job with ID: {jobId}");

// Retrieve and monitor the job
var job = await jobService.GetJobAsync(jobId);
if (job != null)
{
    Console.WriteLine($"Job Name: {job.JobName}");
    Console.WriteLine($"State: {job.State}");
    Console.WriteLine($"Progress: {job.ProgressPercentage}%");
    Console.WriteLine($"Status: {job.StatusMessage}");
    Console.WriteLine($"Created: {job.CreatedAt}");
    Console.WriteLine($"Estimated time remaining: {job.EstimatedTimeRemaining?.ToString("g") ?? "N/A"}");
    
    // Update progress periodically
    await jobService.UpdateJobProgressAsync(jobId, 25, "Processing video...");
    
    // Check active jobs
    var activeJobs = await jobService.GetActiveJobsAsync();
    Console.WriteLine($"Active jobs: {activeJobs.Count()}");
}

// Complete the job
await jobService.UpdateJobProgressAsync(jobId, 100, "Job completed successfully");
```

## SubtitleSettings

Configuration settings for embedding subtitles into video files. Supports both soft embedding (as a subtitle stream) and hard embedding (burning subtitles directly into video frames). Provides font selection, size control, and stream selection capabilities.

```csharp
// Create subtitle settings for soft-embedding English subtitles
var subtitleSettings = new SubtitleSettings
{
  SubtitlePath = "/subtitles/english.srt",
  HardEmbed = false,
  FontName = "Arial",
  FontSize = 24,
  SubtitleStreamIndex = 0,
  Language = "en",
  CharEncoding = "UTF-8"
};

// Validate the settings before use
subtitleSettings.Validate();

// Clone settings for reuse with different parameters
var clonedSettings = subtitleSettings.Clone();
clonedSettings.FontSize = 28;
clonedSettings.Language = "fr";

// Create settings for hard-embedding subtitles (burned into frames)
var burnedSubtitles = new SubtitleSettings
{
  SubtitlePath = "/subtitles/forced.srt",
  HardEmbed = true,
  FontName = "Helvetica",
  FontSize = 32,
  CharEncoding = "latin1"
};
```

## ConcatenationSegment

Represents a segment of media files to be concatenated into a single output. The `ConcatenationSegment` class provides properties for file paths, time trimming options, labels, and builder methods for constructing concatenation pipelines with transitions, re-encoding, and transcode settings.

```csharp
// Create a concatenation builder
var builder = new ConcatenationBuilder();

// Add segments with optional trimming and labels
builder.Add(new ConcatenationSegment
{
    FilePath = "/videos/intro.mp4",
    TrimStart = TimeSpan.FromSeconds(2.5),
    TrimEnd = TimeSpan.FromSeconds(15),
    Label = "Intro"
});

builder.Add(new ConcatenationSegment
{
    FilePath = "/videos/main-content.mp4",
    TrimStart = TimeSpan.FromSeconds(1),
    Label = "Main Content"
});

builder.Add(new ConcatenationSegment
{
    FilePath = "/videos/outro.mp4",
    TrimDuration = TimeSpan.FromSeconds(8),
    Label = "Outro"
});

// Configure transitions between segments
builder.WithTransition(TimeSpan.FromSeconds(1.5));

// Configure re-encoding settings
builder.WithReencode(new ReencodeSettings
{
    VideoCodec = "libx264",
    AudioCodec = "aac",
    Bitrate = "5000k"
});

// Build the concatenation pipeline
var mergeSettings = builder.Build();

// Execute the concatenation
var result = await mergeSettings.ExecuteAsync("/output/final-video.mp4");

Console.WriteLine($"Concatenation completed: {result.IsSuccess}");
Console.WriteLine($"Output file: {result.OutputFilePath}");
```

## ... (rest of README.md content remains unchanged)
