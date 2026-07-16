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

// ... (rest of README.md content remains unchanged)
