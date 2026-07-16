// ... (rest of README.md content remains unchanged)

## WatermarkSettings

Configuration settings for applying watermarks to videos. This class allows you to specify the position, size, and animation of the watermark, as well as its duration and start time.

```csharp
// Create watermark settings for a video
var watermarkSettings = new WatermarkSettings
{
    Position = WatermarkPosition.TopRight,
    XOffset = 10,
    YOffset = 10,
    Scale = 0.2,
    PreserveAspectRatio = true,
    StartTime = TimeSpan.FromSeconds(5),
    Duration = TimeSpan.FromSeconds(10),
    AnimateIn = true,
    AnimateInDuration = TimeSpan.FromSeconds(2)
};

// Validate the watermark settings against a media file
var inputMedia = new MediaFile { Duration = TimeSpan.FromSeconds(10) };
watermarkSettings.Validate(inputMedia);

// Calculate the watermark position coordinates
var (x, y) = watermarkSettings.CalculatePosition(1920, 1080);

// Clone the watermark settings for reuse with different parameters
var clonedSettings = watermarkSettings.Clone();
clonedSettings.XOffset = 20;
clonedSettings.YOffset = 20;
```

## MediaFile

Represents a media file with metadata and validation capabilities. This class encapsulates essential properties for video and audio files including duration, resolution, codec information, and timestamps.




```csharp
// Create a media file representing a video file
var videoFile = new MediaFile
{
    Id = Guid.NewGuid().ToString(),
    Name = "sample_video.mp4",
    Duration = TimeSpan.FromSeconds(183.45),
    Width = 1920,
    Height = 1080,
    FrameRate = 30.0,
    Bitrate = 8500000,
    VideoCodec = "h264",
    AudioCodec = "aac",
    AudioSampleRate = 48000,
    AudioChannels = 2,
    CreatedAt = DateTime.UtcNow.AddDays(-7),
    ModifiedAt = DateTime.UtcNow,
    Description = "Sample video for testing",
    Metadata = new Dictionary<string, string>
    {
        { "title", "Sample Video" },
        { "author", "Test User" },
        { "copyright", "© 2026" }
    }
};

// Calculate file size in megabytes
double fileSizeMB = videoFile.GetFileSizeInMegabytes();
Console.WriteLine($"File size: {fileSizeMB:F2} MB");

// Validate as video file
videoFile.ValidateAsVideo();

// Create a media file representing an audio file
var audioFile = new MediaFile
{
    Id = Guid.NewGuid().ToString(),
    Name = "background_music.mp3",
    Duration = TimeSpan.FromSeconds(245.5),
    Bitrate = 320000,
    AudioCodec = "mp3",
    AudioSampleRate = 44100,
    AudioChannels = 2,
    CreatedAt = DateTime.UtcNow.AddDays(-3)
};

// Validate as audio file
audioFile.ValidateAsAudio();
```

## FFmpegOperation

Represents a single FFmpeg command operation with configurable inputs, outputs, arguments, and execution parameters. This class provides a fluent interface for building FFmpeg commands programmatically with validation and cloning capabilities.

```csharp
// Create a video transcoding operation
var operation = new FFmpegOperation
{
    Name = "Transcode to H.264",
    Type = FFmpegOperationType.Transcode,
    Priority = 1,
    IsParallel = false
};

// Add input files
operation.AddInputFile("input.mp4");
operation.AddInputFile("logo.png");

// Add FFmpeg arguments for H.264 encoding
operation.AddArguments(
    "-c:v", "libx264",
    "-preset", "fast",
    "-crf", "23",
    "-c:a", "aac",
    "-b:a", "192k"
);

// Set output file
operation.OutputFile = "output_h264.mp4";

// Validate the operation configuration
operation.Validate();

// Build the FFmpeg command line
string command = operation.BuildCommandLine();
Console.WriteLine(command);
// Output: ffmpeg -i "input.mp4" -i "logo.png" -c:v libx264 -preset fast -crf 23 -c:a aac -b:a 192k "output_h264.mp4"

// Clone the operation for reuse with different parameters
var clonedOperation = operation.Clone();
clonedOperation.OutputFile = "output_h264_720p.mp4";
clonedOperation.AddArgument("-vf", "scale=-2:720");
```

## TranscodeSettings

TranscodeSettings defines the configuration options used when transcoding a media file. It lets you specify codecs, container format, resolution, quality presets, scaling behavior, audio normalization and hardware acceleration. After configuring the instance you can call `Validate()` to ensure the settings are consistent and `Clone()` to create a copy for further modifications.

```csharp
using FFmpegDotnetWrapper.Models;
using FFmpegDotnetWrapper.Constants;

// Create a transcode settings instance
var transcodeSettings = new TranscodeSettings
{
    VideoCodec = VideoCodec.H264,
    AudioCodec = AudioCodec.AAC,
    Container = ContainerFormat.MP4,
    Width = 1280,
    Height = 720,
    Quality = QualityPreset.High,
    EnableAutoScale = true,
    MaxWidth = 1920,
    MaxHeight = 1080,
    ScalingMode = ScalingMode.Lanczos,
    PreserveAspectRatio = true,
    EnableAudioNormalization = true,
    TargetLoudness = -23.0,
    TwoPass = false,
    CustomFFmpegArgs = "-movflags +faststart",
    HardwareAcceleration = HwAccel.Auto
};

// Validate the configuration
transcodeSettings.Validate();

// Clone for a slightly different output
var clonedSettings = transcodeSettings.Clone();
clonedSettings.Width = 640;
clonedSettings.Height = 360;
```

## StreamingProfile

`StreamingProfile` describes a single quality rendition in an adaptive‑bitrate ladder. It is immutable, thread‑safe, and provides helper properties such as the resolution string and total bitrate.

```csharp
using FFmpegDotnetWrapper.Models;

// Create a custom profile
var customProfile = new StreamingProfile(
    Name: "720p-custom",
    Width: 1280,
    Height: 720,
    VideoBitrateKbps: 3000,
    AudioBitrateKbps: 128,
    FrameRate: 30);

// Use one of the predefined profiles
var hdProfile = StreamingProfile.HD;

// Access helper properties
Console.WriteLine($"HD resolution: {hdProfile.Resolution}");
Console.WriteLine($"HD total bitrate: {hdProfile.TotalBitrateKbps} kbps");

// Enumerate the default ladder
foreach (var profile in StreamingProfile.DefaultLadder)
{
    Console.WriteLine($"{profile.Name}: {profile.Resolution} @ {profile.VideoBitrateKbps} kbps video");
}
```

The `StreamingProfile` record is used in `StreamingPipelineSettings.Profiles` to define which renditions the pipeline should generate.

```csharp
var pipelineSettings = new StreamingPipelineSettings
{
    InputFilePath = "/videos/source.mp4",
    OutputDirectory = "/videos/output",
    Profiles = new List<StreamingProfile> { StreamingProfile.FullHD, StreamingProfile.HD, StreamingProfile.SD },
    EnableHardwareAcceleration = true,
    EncodeProfilesConcurrently = false
};

pipelineSettings.Validate();
```

## OperationRepository

In-memory repository implementation for managing FFmpeg operations. Provides CRUD operations and query capabilities for FFmpeg operations with built-in memory management to prevent unbounded growth. The repository automatically evicts the oldest operations when the configured memory limit is reached.


```csharp
using FFmpegDotnetWrapper.Models;
using FFmpegDotnetWrapper.Repository;

// Create repository instance
var repository = new OperationRepository();

// Add a new FFmpeg operation
var operation = new FFmpegOperation
{
    Name = "Transcode video",
    Type = FFmpegOperationType.Transcode,
    Priority = 1,
    IsParallel = false
};
operation.AddInputFile("input.mp4");
operation.AddArguments("-c:v", "libx264", "-crf", "23");
operation.OutputFile = "output.mp4";

var addedOperation = await repository.AddAsync(operation);
Console.WriteLine($"Added operation with ID: {addedOperation.Id}");

// Get operation by ID
var retrievedOperation = await repository.GetByIdAsync(addedOperation.Id);
Console.WriteLine(retrievedOperation?.Name);

// Get all operations
var allOperations = await repository.GetAllAsync();
Console.WriteLine($"Total operations: {allOperations.Count()}");

// Update an operation
addedOperation.Priority = 2;
var updatedOperation = await repository.UpdateAsync(addedOperation);

// Get operations by type
var transcodeOps = await repository.GetByTypeAsync(FFmpegOperationType.Transcode);
Console.WriteLine($"Transcode operations: {transcodeOps.Count()}");

// Get recent operations
var recentOps = await repository.GetRecentAsync(5);
Console.WriteLine($"Most recent operations: {recentOps.Count()}");

// Get operations by date range
var fromDate = DateTime.UtcNow.AddDays(-7);
var toDate = DateTime.UtcNow;
var dateRangeOps = await repository.GetByDateRangeAsync(fromDate, toDate);
Console.WriteLine($"Operations from last 7 days: {dateRangeOps.Count()}");

// Get total count
var totalCount = await repository.GetCountAsync();
Console.WriteLine($"Total operations in repository: {totalCount}");

// Delete an operation
await repository.DeleteAsync(addedOperation.Id);

// Clear old operations (older than 30 days)
var clearedCount = await repository.ClearOldAsync(30);
Console.WriteLine($"Cleared {clearedCount} old operations");
```

// ... (rest of README.md content remains unchanged)
