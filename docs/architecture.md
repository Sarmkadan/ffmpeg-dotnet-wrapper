# Architecture Guide

This document describes the architecture of FFmpeg .NET Wrapper.

## High-Level Design

FFmpeg .NET Wrapper is built on a layered architecture with clean separation of concerns:

```
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                         │
│  REST API (Controllers) | CLI | Background Jobs             │
└────────────────────┬────────────────────────────────────────┘
                     ▼
┌─────────────────────────────────────────────────────────────┐
│                  Abstraction Layer                           │
│  IFFmpegService | TranscodeService | BatchOperationService  │
└────────────────────┬────────────────────────────────────────┘
                     ▼
┌─────────────────────────────────────────────────────────────┐
│                   Models & Settings                         │
│  TranscodeSettings | TrimSettings | MergeSettings |         │
│  WatermarkSettings | MediaFile | ConversionResult          │
└────────────────────┬────────────────────────────────────────┘
                     ▼
┌─────────────────────────────────────────────────────────────┐
│                  Utility Layer                               │
│  ProcessUtilities | FileUtilities | ValidationUtilities     │
└────────────────────┬────────────────────────────────────────┘
                     ▼
┌─────────────────────────────────────────────────────────────┐
│              FFmpeg Process (External)                       │
│  System.Diagnostics.Process spawned subprocess              │
└─────────────────────────────────────────────────────────────┘
```

## Layer Details

### 1. Application Layer

Entry points for using the library:

**REST API** (`src/Api/Controllers/`)
- `FFmpegController.cs` – HTTP endpoints for transcode, trim, merge, watermark
- Returns `ApiResponse<T>` with result or error details
- Handles request validation and error mapping

**CLI** (`src/Cli/`)
- `CliCommandParser.cs` – Parses command-line arguments
- `OutputFormatter.cs` – Formats console output
- Supports commands: `transcode`, `trim`, `merge`, `watermark`

**Background Jobs** (`src/BackgroundJobs/`)
- `BackgroundJobService.cs` – Queues operations for async processing
- `JobQueue.cs` – In-memory or persistent queue
- Supports webhooks on completion

### 2. Service Layer

Core business logic:

**IFFmpegService** (`src/Services/`)
```csharp
public interface IFFmpegService
{
    Task<bool> IsFFmpegAvailableAsync();
    Task<string> GetFFmpegVersionAsync();
    Task<ConversionResult> TranscodeAsync(...);
    Task<ConversionResult> TrimAsync(...);
    Task<ConversionResult> MergeAsync(...);
    Task<ConversionResult> WatermarkAsync(...);
    Task<MediaFile> AnalyzeMediaAsync(...);
}
```

**FFmpegService** – Main implementation
- Coordinates all operations
- Manages logging and progress tracking
- Handles error recovery

**TranscodeService** – Video codec operations
- H.264, H.265, VP9, AV1 encoding
- Bitrate and quality management
- Audio codec selection

**BatchOperationService** – Concurrent processing
- Process multiple files in parallel
- Configurable concurrency limits
- Aggregates progress statistics

### 3. Models & Settings Layer

Data structures for configuration and results:

**Settings Classes**
- `TranscodeSettings` – Encoding parameters (codec, bitrate, resolution)
- `TrimSettings` – Trimming parameters (start, duration, keyframe)
- `MergeSettings` – Merge parameters (preserve audio/video, crossfade)
- `WatermarkSettings` – Overlay parameters (position, scale, opacity)

**Domain Models**
- `MediaFile` – Represents a video file with metadata
- `ConversionResult` – Result of an operation (success, duration, error)
- `OperationStatistics` – Aggregate statistics from batch operations

**Enums**
- `VideoCodec` – H264, H265, VP8, VP9, AV1
- `AudioCodec` – AAC, MP3, Opus, FLAC, VORBIS
- `ContainerFormat` – MP4, WebM, MKV, Ogg, AVI
- `QualityPreset` – Low, Medium, High, Lossless
- `WatermarkPosition` – TopLeft, TopRight, BottomLeft, BottomRight, Center

### 4. Utility Layer

Helper functions:

**ProcessUtilities** – FFmpeg subprocess management
- Spawns FFmpeg process safely
- Parses command output
- Handles exit codes and signals

**FileUtilities** – File operations
- Path validation and normalization
- Temporary file cleanup
- Directory creation/verification

**ValidationUtilities** – Input validation
- File existence checks
- Codec support verification
- Setting range validation

**ProgressTracker** – Operation monitoring
- Parses FFmpeg progress output
- Calculates completion percentage
- Reports via `IProgress<T>`

## Key Design Patterns

### Dependency Injection

All services registered in `ServiceCollectionExtensions`:

```csharp
public static IServiceCollection AddFFmpegWrapper(
    this IServiceCollection services,
    Action<FFmpegOptions> configure)
{
    services.Configure(configure);
    services.AddSingleton<IFFmpegService, FFmpegService>();
    services.AddScoped<TranscodeService>();
    services.AddScoped<BatchOperationService>();
    // ...
}
```

Benefits:
- Testable (mock `IFFmpegService`)
- Loosely coupled
- Configurable at startup

### Repository Pattern

Data persistence abstraction:

```csharp
public interface IMediaRepository
{
    Task SaveMediaAsync(MediaFile media);
    Task<MediaFile> GetMediaAsync(string id);
    Task DeleteMediaAsync(string id);
}
```

Implementations:
- `MediaRepository` – In-memory storage
- Can be swapped for database-backed version

### Strategy Pattern

Codec and format selection:

```csharp
var codec = settings.VideoCodec switch
{
    VideoCodec.H264 => "libx264",
    VideoCodec.VP9 => "libvpx-vp9",
    VideoCodec.AV1 => "libaom-av1",
    _ => throw new InvalidOperationException()
};
```

### Observer Pattern

Progress tracking:

```csharp
var progress = new Progress<OperationStatistics>(stat =>
{
    Console.WriteLine($"Progress: {stat.Percentage:P}");
});

await ffmpeg.TranscodeAsync(input, output, settings, progress);
```

### Builder Pattern

Fluent configuration:

```csharp
var settings = new TranscodeSettings
{
    VideoCodec = VideoCodec.VP9
};
settings.AddAudioTrack(AudioCodec.Opus, 128);
```

## Data Flow

### Transcode Operation

```
User Request
    ↓
TranscodeAsync() validates input
    ↓
Builds FFmpeg command: ffmpeg -i input.mp4 -c:v libvpx-vp9 -c:a libopus output.webm
    ↓
ProcessUtilities spawns subprocess
    ↓
FFmpeg encodes video (writes to stdout/stderr)
    ↓
ProgressTracker parses output: "frame=1234 fps=30 time=00:10:30"
    ↓
IProgress<T> notifies subscriber of progress
    ↓
Process completes, ConversionResult returned
    ↓
User receives result with output path and duration
```

### Batch Operation

```
User provides file list + settings
    ↓
BatchOperationService validates all inputs
    ↓
Creates work queue: [file1.mp4, file2.mp4, file3.mp4]
    ↓
Spawns worker tasks (limited by MaxConcurrentOperations)
    ↓
Each worker: TranscodeAsync() → output file
    ↓
OperationStatistics aggregated: 2/3 complete, 66% success rate
    ↓
IProgress<T> notified
    ↓
All complete, results aggregated
    ↓
User receives final statistics
```

## Configuration Management

**FFmpegOptions** – Root configuration

```csharp
public class FFmpegOptions
{
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(600);
    public bool EnableDetailedLogging { get; set; } = false;
    public int MaxConcurrentOperations { get; set; } = 4;
    public string FFmpegPath { get; set; } = "ffmpeg";
    public string WorkingDirectory { get; set; } = Path.GetTempPath();
    public bool EnableWebhooks { get; set; } = false;
    public bool EnableBackgroundJobs { get; set; } = false;
}
```

Applied via:
1. `appsettings.json` configuration
2. Environment variables
3. Programmatic override in `AddFFmpegWrapper()`

Priority (highest to lowest):
1. Code configuration
2. Environment variables
3. appsettings.json
4. Defaults in code

## Error Handling Strategy

**FFmpegException** – Custom exception

```csharp
public class FFmpegException : Exception
{
    public int ExitCode { get; set; }
    public string RawOutput { get; set; }
}
```

Error sources:
1. **FFmpeg not installed** – IsFFmpegAvailableAsync() returns false
2. **Invalid settings** – ValidationUtilities throws ArgumentException
3. **Codec not supported** – FFmpeg returns non-zero exit code
4. **File permission** – FileUtilities throws UnauthorizedAccessException
5. **Timeout** – ProcessUtilities cancels after DefaultTimeout

Handling:
```csharp
try
{
    var result = await ffmpeg.TranscodeAsync(...);
    if (!result.Success)
        Log.Error(result.ErrorMessage);
}
catch (FFmpegException ex)
{
    Log.Error($"FFmpeg error: {ex.Message} (exit code: {ex.ExitCode})");
}
catch (OperationCanceledException)
{
    Log.Error("Operation timeout");
}
```

## Logging Architecture

Structured logging via `ILogger<T>`:

```csharp
_logger.LogInformation("Transcode started: {InputFile} -> {OutputFile}",
    inputFile, outputFile);

_logger.LogDebug("FFmpeg command: {Command}", command);

_logger.LogWarning("Codec not optimal: {Codec} may be slow", codec);

_logger.LogError(ex, "Transcode failed: {ErrorMessage}", ex.Message);
```

Log levels:
- **Information** – Operation start/complete
- **Debug** – Command construction, progress updates
- **Warning** – Suboptimal settings, recovery
- **Error** – Failures and exceptions

## Extension Points

### Custom Codec Support

Add new codec by extending `VideoCodec` enum and `ProcessUtilities`:

```csharp
// In VideoCodec enum
public enum VideoCodec
{
    H264,
    H265,
    VP9,
    AV1,
    MyCustomCodec  // New
}

// In ProcessUtilities
case VideoCodec.MyCustomCodec:
    return "my-custom-encoder-name";
```

### Custom Repository

Implement `IMediaRepository` for database storage:

```csharp
public class SqlServerMediaRepository : IMediaRepository
{
    private readonly IDbContext _db;
    
    public async Task SaveMediaAsync(MediaFile media)
    {
        _db.MediaFiles.Add(media);
        await _db.SaveChangesAsync();
    }
}

// Register in DI
services.AddScoped<IMediaRepository, SqlServerMediaRepository>();
```

### Webhook Notifications

Implement custom notification on completion:

```csharp
public class WebhookNotifier
{
    public async Task NotifyCompletionAsync(ConversionResult result)
    {
        using var client = new HttpClient();
        await client.PostAsJsonAsync("https://example.com/webhook", result);
    }
}
```

## Performance Considerations

### Concurrency Limits

`MaxConcurrentOperations` prevents resource exhaustion:
- Set to CPU count for I/O bound (typical)
- Set to 1 for CPU-limited environments
- Test on target hardware

### Memory Usage

- FFmpeg itself uses ~200-500 MB per process
- Total memory = (FFmpeg size) × (concurrent operations)
- Example: 4 concurrent × 300 MB = 1.2 GB

### Disk Space

Transcoding requires temporary space:
- Safe minimum: input file size × 1.5
- Store temp files on SSD if available
- Use `WorkingDirectory` option

### Codec Selection

Performance by codec:
1. **H.264** – Fastest, good quality (default)
2. **H.265** – 30% slower, better quality
3. **VP9** – 3-5x slower, excellent quality
4. **AV1** – Slowest, best quality (research only)

## Security Considerations

### Command Injection Prevention

ProcessUtilities uses array-based arguments (immune to injection):

```csharp
// Safe – array parameters can't be injected
var args = new[] { "-i", inputFile, "-c:v", "libx264", outputFile };
process.StartInfo.ArgumentList.AddRange(args);

// NOT USED (vulnerable)
// process.StartInfo.Arguments = $"-i {inputFile}";  // Don't do this
```

### Path Traversal Prevention

FileUtilities validates paths:

```csharp
var fullPath = Path.GetFullPath(userProvidedPath);
var basePath = Path.GetFullPath(allowedDirectory);

if (!fullPath.StartsWith(basePath))
    throw new InvalidOperationException("Path traversal detected");
```

### Input Validation

All user inputs validated before use:
- File paths must exist and be readable
- Bitrates bounded (16-192000 kbps)
- Dimensions bounded (16-7680 pixels)
- Timeouts bounded (1 second - 24 hours)
