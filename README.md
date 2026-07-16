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
