# API Reference

Complete reference for all public types and methods in FFmpeg .NET Wrapper.

## IFFmpegService

Main service interface for all video operations.

```csharp
public interface IFFmpegService
{
    // Availability checks
    Task<bool> IsFFmpegAvailableAsync();
    Task<string> GetFFmpegVersionAsync();
    
    // Core operations
    Task<ConversionResult> TranscodeAsync(
        string inputPath,
        string outputPath,
        TranscodeSettings settings,
        IProgress<OperationStatistics>? progress = null,
        CancellationToken cancellationToken = default);
    
    Task<ConversionResult> TrimAsync(
        string inputPath,
        string outputPath,
        TrimSettings settings,
        IProgress<OperationStatistics>? progress = null,
        CancellationToken cancellationToken = default);
    
    Task<ConversionResult> MergeAsync(
        string[] inputPaths,
        string outputPath,
        MergeSettings? settings = null,
        IProgress<OperationStatistics>? progress = null,
        CancellationToken cancellationToken = default);
    
    Task<ConversionResult> WatermarkAsync(
        string inputPath,
        string watermarkPath,
        string outputPath,
        WatermarkSettings settings,
        IProgress<OperationStatistics>? progress = null,
        CancellationToken cancellationToken = default);
    
    // Media analysis
    Task<MediaFile> AnalyzeMediaAsync(
        MediaFile media,
        CancellationToken cancellationToken = default);
}
```

### IsFFmpegAvailableAsync()

Checks if FFmpeg is installed and accessible.

**Returns**: `Task<bool>` – True if FFmpeg is available

**Example**:
```csharp
var available = await ffmpeg.IsFFmpegAvailableAsync();
if (!available)
    throw new InvalidOperationException("FFmpeg not installed");
```

### GetFFmpegVersionAsync()

Retrieves FFmpeg version string.

**Returns**: `Task<string>` – Version (e.g., "ffmpeg version 7.0.1")

**Example**:
```csharp
var version = await ffmpeg.GetFFmpegVersionAsync();
Console.WriteLine(version);  // ffmpeg version 7.0.1 Copyright ...
```

### TranscodeAsync()

Converts video to different format/codec.

**Parameters**:
- `inputPath` (string) – Path to input file
- `outputPath` (string) – Path to output file
- `settings` (TranscodeSettings) – Encoding configuration
- `progress` (IProgress<OperationStatistics>?) – Optional progress reporter
- `cancellationToken` (CancellationToken) – Cancellation token

**Returns**: `Task<ConversionResult>` – Operation result with duration/error

**Throws**:
- `FFmpegException` – FFmpeg error
- `FileNotFoundException` – Input file not found
- `OperationCanceledException` – Operation cancelled

**Example**:
```csharp
var result = await ffmpeg.TranscodeAsync(
    "input.mkv",
    "output.mp4",
    new TranscodeSettings
    {
        VideoCodec = VideoCodec.H264,
        AudioCodec = AudioCodec.AAC,
        Container = ContainerFormat.MP4,
        VideoBitrate = 2500
    });

if (result.Success)
    Console.WriteLine($"Completed in {result.ElapsedTime.TotalSeconds}s");
```

### TrimAsync()

Extracts a segment from a video file.

**Parameters**:
- `inputPath` (string) – Path to input file
- `outputPath` (string) – Path to output file
- `settings` (TrimSettings) – Trimming configuration
- `progress` (IProgress<OperationStatistics>?) – Optional progress reporter
- `cancellationToken` (CancellationToken) – Cancellation token

**Returns**: `Task<ConversionResult>` – Operation result

**Example**:
```csharp
var result = await ffmpeg.TrimAsync(
    "video.mp4",
    "clip.mp4",
    new TrimSettings
    {
        StartTime = TimeSpan.FromMinutes(1),
        Duration = TimeSpan.FromMinutes(5),
        Keyframe = true
    });
```

### MergeAsync()

Concatenates multiple video files.

**Parameters**:
- `inputPaths` (string[]) – Array of input file paths
- `outputPath` (string) – Path to output file
- `settings` (MergeSettings?) – Optional merge configuration
- `progress` (IProgress<OperationStatistics>?) – Optional progress reporter
- `cancellationToken` (CancellationToken) – Cancellation token

**Returns**: `Task<ConversionResult>` – Operation result

**Example**:
```csharp
var result = await ffmpeg.MergeAsync(
    new[] { "intro.mp4", "main.mp4", "outro.mp4" },
    "complete.mp4",
    new MergeSettings { PreserveAudio = true });
```

### WatermarkAsync()

Overlays an image on a video.

**Parameters**:
- `inputPath` (string) – Video input file
- `watermarkPath` (string) – Watermark image file (PNG/JPG)
- `outputPath` (string) – Path to output file
- `settings` (WatermarkSettings) – Watermark configuration
- `progress` (IProgress<OperationStatistics>?) – Optional progress reporter
- `cancellationToken` (CancellationToken) – Cancellation token

**Returns**: `Task<ConversionResult>` – Operation result

**Example**:
```csharp
var result = await ffmpeg.WatermarkAsync(
    "video.mp4",
    "logo.png",
    "watermarked.mp4",
    new WatermarkSettings
    {
        Position = WatermarkPosition.TopRight,
        Scale = 0.15,
        Opacity = 0.8
    });
```

### AnalyzeMediaAsync()

Extracts metadata from a media file.

**Parameters**:
- `media` (MediaFile) – Media file to analyze
- `cancellationToken` (CancellationToken) – Cancellation token

**Returns**: `Task<MediaFile>` – Populated with metadata

**Example**:
```csharp
var media = new MediaFile { Path = "video.mp4" };
var analyzed = await ffmpeg.AnalyzeMediaAsync(media);

Console.WriteLine($"Duration: {analyzed.Duration}");
Console.WriteLine($"Resolution: {analyzed.Width}x{analyzed.Height}");
Console.WriteLine($"Codec: {analyzed.VideoCodec}");
Console.WriteLine($"Bitrate: {analyzed.Bitrate} bps");
```

---

## TranscodeSettings

Configuration for video transcoding operation.

```csharp
public class TranscodeSettings
{
    public VideoCodec VideoCodec { get; set; }
    public AudioCodec AudioCodec { get; set; }
    public ContainerFormat Container { get; set; }
    public int VideoBitrate { get; set; }          // kbps
    public int AudioBitrate { get; set; }          // kbps
    public int FrameRate { get; set; }             // fps
    public QualityPreset Quality { get; set; }
    public bool EnableAutoScale { get; set; }
    public int MaxWidth { get; set; }              // pixels
    public int MaxHeight { get; set; }             // pixels
    public bool PreserveAspectRatio { get; set; }
}
```

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| VideoCodec | VideoCodec | H264 | Video encoder |
| AudioCodec | AudioCodec | AAC | Audio encoder |
| Container | ContainerFormat | MP4 | Output container |
| VideoBitrate | int | 2500 | Bits per second (kbps) |
| AudioBitrate | int | 128 | Audio bits per second (kbps) |
| FrameRate | int | 30 | Frames per second |
| Quality | QualityPreset | Medium | Quality profile |
| EnableAutoScale | bool | false | Auto-scale to max dimensions |
| MaxWidth | int | 1920 | Maximum width when scaling |
| MaxHeight | int | 1080 | Maximum height when scaling |
| PreserveAspectRatio | bool | true | Maintain aspect ratio when scaling |

### Example

```csharp
var settings = new TranscodeSettings
{
    VideoCodec = VideoCodec.VP9,
    AudioCodec = AudioCodec.Opus,
    Container = ContainerFormat.WebM,
    VideoBitrate = 1500,
    AudioBitrate = 96,
    FrameRate = 30,
    Quality = QualityPreset.High,
    EnableAutoScale = true,
    MaxWidth = 1280,
    MaxHeight = 720,
    PreserveAspectRatio = true
};

await ffmpeg.TranscodeAsync("input.mp4", "output.webm", settings);
```

---

## TrimSettings

Configuration for video trimming operation.

```csharp
public class TrimSettings
{
    public TimeSpan StartTime { get; set; }
    public TimeSpan? Duration { get; set; }
    public bool PreserveAudio { get; set; }
    public bool PreserveVideo { get; set; }
    public bool Keyframe { get; set; }
}
```

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| StartTime | TimeSpan | 0:00:00 | Start position in input |
| Duration | TimeSpan? | null | Length of segment (null = to end) |
| PreserveAudio | bool | true | Include audio track |
| PreserveVideo | bool | true | Include video track |
| Keyframe | bool | false | Align cut to nearest keyframe |

### Example

```csharp
var settings = new TrimSettings
{
    StartTime = TimeSpan.FromMinutes(5),
    Duration = TimeSpan.FromMinutes(2),
    PreserveAudio = true,
    PreserveVideo = true,
    Keyframe = true
};

await ffmpeg.TrimAsync("full.mp4", "segment.mp4", settings);
```

---

## MergeSettings

Configuration for video merge operation.

```csharp
public class MergeSettings
{
    public bool PreserveAudio { get; set; }
    public bool PreserveVideo { get; set; }
    public bool Crossfade { get; set; }
    
    public void AddInputFile(string path);
    public int GetInputFileCount();
    public IEnumerable<string> GetInputFiles();
}
```

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| PreserveAudio | bool | true | Include audio tracks |
| PreserveVideo | bool | true | Include video tracks |
| Crossfade | bool | false | Fade between clips |

### Methods

**AddInputFile(string path)** – Add file to merge list

**GetInputFileCount()** – Get number of input files

**GetInputFiles()** – Get enumerable of input paths

### Example

```csharp
var settings = new MergeSettings { PreserveAudio = true };
settings.AddInputFile("clip1.mp4");
settings.AddInputFile("clip2.mp4");
settings.AddInputFile("clip3.mp4");

await ffmpeg.MergeAsync(settings.GetInputFiles().ToArray(), "merged.mp4", settings);
```

---

## WatermarkSettings

Configuration for watermark overlay operation.

```csharp
public class WatermarkSettings
{
    public WatermarkPosition Position { get; set; }
    public int XOffset { get; set; }
    public int YOffset { get; set; }
    public double Scale { get; set; }              // 0.0 to 1.0
    public double Opacity { get; set; }            // 0.0 to 1.0
    public bool PreserveAspectRatio { get; set; }
}
```

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Position | WatermarkPosition | BottomRight | Anchor position |
| XOffset | int | 0 | X offset in pixels |
| YOffset | int | 0 | Y offset in pixels |
| Scale | double | 0.1 | Size as fraction of video width |
| Opacity | double | 0.75 | Transparency (0=invisible, 1=opaque) |
| PreserveAspectRatio | bool | true | Maintain watermark proportions |

### WatermarkPosition Enum

```csharp
public enum WatermarkPosition
{
    TopLeft,
    TopRight,
    TopCenter,
    BottomLeft,
    BottomRight,
    BottomCenter,
    MiddleLeft,
    MiddleRight,
    Center
}
```

### Example

```csharp
var settings = new WatermarkSettings
{
    Position = WatermarkPosition.BottomRight,
    XOffset = 20,
    YOffset = 20,
    Scale = 0.15,
    Opacity = 0.8,
    PreserveAspectRatio = true
};

await ffmpeg.WatermarkAsync("video.mp4", "logo.png", "branded.mp4", settings);
```

---

## ConversionResult

Result of a transcoding operation.

```csharp
public class ConversionResult
{
    public bool Success { get; set; }
    public TimeSpan ElapsedTime { get; set; }
    public string? OutputPath { get; set; }
    public string? ErrorMessage { get; set; }
    public string? RawOutput { get; set; }
    public int ExitCode { get; set; }
}
```

### Properties

| Property | Type | Description |
|----------|------|-------------|
| Success | bool | Whether operation succeeded |
| ElapsedTime | TimeSpan | Duration of operation |
| OutputPath | string? | Path to output file |
| ErrorMessage | string? | Human-readable error |
| RawOutput | string? | FFmpeg stderr output |
| ExitCode | int | FFmpeg process exit code |

### Example

```csharp
var result = await ffmpeg.TranscodeAsync(input, output, settings);

if (result.Success)
{
    Console.WriteLine($"✓ Completed in {result.ElapsedTime.TotalSeconds:F2}s");
    Console.WriteLine($"Output: {result.OutputPath}");
}
else
{
    Console.WriteLine($"✗ Error: {result.ErrorMessage}");
    Console.WriteLine($"Exit code: {result.ExitCode}");
}
```

---

## OperationStatistics

Statistics from batch or concurrent operations.

```csharp
public class OperationStatistics
{
    public int TotalOperations { get; set; }
    public int CompletedOperations { get; set; }
    public int SuccessfulOperations { get; set; }
    public int FailedOperations { get; set; }
    public double SuccessRate { get; set; }        // 0.0 to 1.0
    public double Percentage { get; set; }         // 0.0 to 100.0
    public TimeSpan ElapsedTime { get; set; }
    public TimeSpan? EstimatedTimeRemaining { get; set; }
}
```

### Properties

| Property | Type | Description |
|----------|------|-------------|
| TotalOperations | int | Total number of operations |
| CompletedOperations | int | Finished operations |
| SuccessfulOperations | int | Successful operations |
| FailedOperations | int | Failed operations |
| SuccessRate | double | Success ratio (0.0-1.0) |
| Percentage | double | Completion percentage (0.0-100.0) |
| ElapsedTime | TimeSpan | Time elapsed |
| EstimatedTimeRemaining | TimeSpan? | Estimated remaining time |

### Example

```csharp
var progress = new Progress<OperationStatistics>(stat =>
{
    Console.WriteLine($"Progress: {stat.Percentage:F1}%");
    Console.WriteLine($"Completed: {stat.CompletedOperations}/{stat.TotalOperations}");
    Console.WriteLine($"Success rate: {stat.SuccessRate:P2}");
    if (stat.EstimatedTimeRemaining.HasValue)
        Console.WriteLine($"ETA: {stat.EstimatedTimeRemaining.Value.TotalSeconds:F0}s");
});

await batchService.ProcessFilesAsync(files, outputDir, settings, progress);
```

---

## Enums

### VideoCodec

```csharp
public enum VideoCodec
{
    H264,      // x264 - Fast, widely supported
    H265,      // x265 - Slower, better quality
    VP8,       // libvpx-vp8 - WebM format
    VP9,       // libvpx-vp9 - Better VP8
    AV1        // libaom-av1 - Best compression, very slow
}
```

### AudioCodec

```csharp
public enum AudioCodec
{
    AAC,       // libfdk-aac - Streaming standard
    MP3,       // libmp3lame - Legacy, wide support
    Opus,      // libopus - Modern, efficient
    FLAC,      // flac - Lossless compression
    VORBIS     // libvorbis - OGG Vorbis
}
```

### ContainerFormat

```csharp
public enum ContainerFormat
{
    MP4,       // MPEG-4 Part 14
    WebM,      // WebM container
    MKV,       // Matroska
    Ogg,       // OGG/Theora
    AVI        // Audio Video Interleave
}
```

### QualityPreset

```csharp
public enum QualityPreset
{
    Low,       // Faster encoding, larger file
    Medium,    // Balanced
    High,      // Slower encoding, smaller file
    Lossless   // Highest quality, biggest file
}
```

---

## Exception Types

### FFmpegException

Thrown when FFmpeg operation fails.

```csharp
public class FFmpegException : Exception
{
    public int ExitCode { get; set; }
    public string RawOutput { get; set; }
}
```

**Example**:
```csharp
try
{
    await ffmpeg.TranscodeAsync(input, output, settings);
}
catch (FFmpegException ex)
{
    Console.WriteLine($"Exit code: {ex.ExitCode}");
    Console.WriteLine($"Output: {ex.RawOutput}");
}
```
