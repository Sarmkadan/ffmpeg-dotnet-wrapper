# FFmpegService

`FFmpegService` is the central class for interacting with FFmpeg in the `ffmpeg-dotnet-wrapper` library. It encapsulates FFmpeg process management, providing a high-level, asynchronous API for common media operations such as transcoding, trimming, merging, watermarking, analysis, and more. The service abstracts away command-line construction, temporary file handling, and progress monitoring, returning structured result objects that indicate success or failure along with relevant output paths.

## API

### Constructors

- **`public FFmpegService`**
  Initializes a new instance of the service. The constructor typically accepts configuration options such as the path to the FFmpeg binary, default working directories, and logging preferences. The exact parameters depend on the underlying implementation, but the service is always instantiated before any operations are performed.

### Methods

- **`public async Task<ConversionResult> TranscodeAsync`**
  Transcodes a media file from one format or codec to another. Accepts input and output file paths along with optional encoding parameters (codec, bitrate, resolution, etc.). Returns a `ConversionResult` indicating success or failure, including the output file path and any error messages. Throws an `ArgumentException` if input paths are invalid or required parameters are missing. Throws an `InvalidOperationException` if FFmpeg is not available.

- **`public async Task<ConversionResult> TrimAsync`**
  Extracts a segment from a media file between specified start and end timestamps. Accepts the input file, output file, start time, and optional duration. Returns a `ConversionResult` with the path to the trimmed output. Throws an `ArgumentException` if timestamps are negative, out of order, or exceed the source duration when known. Throws an `InvalidOperationException` if FFmpeg is not available.

- **`public async Task<ConversionResult> MergeAsync`**
  Concatenates multiple media files into a single output file. Accepts a list of input file paths and an output path. All inputs must share compatible codecs and container formats for a successful merge. Returns a `ConversionResult` with the merged output path. Throws an `ArgumentException` if fewer than two inputs are provided or any input file does not exist. Throws an `InvalidOperationException` if FFmpeg is not available.

- **`public async Task<ConversionResult> AddWatermarkAsync`**
  Overlays a watermark image onto a video. Accepts the source video path, watermark image path, output path, and positioning parameters (e.g., corner, offset). Returns a `ConversionResult` with the watermarked output path. Throws an `ArgumentException` if the watermark image is missing or in an unsupported format. Throws an `InvalidOperationException` if FFmpeg is not available.

- **`public async Task<MediaFile> AnalyzeMediaAsync`**
  Probes a media file and returns detailed metadata. Accepts a file path and returns a `MediaFile` object containing information such as duration, codec types, bitrate, resolution, stream details, and container format. Throws an `ArgumentException` if the file does not exist or is unreadable. Throws an `InvalidOperationException` if FFprobe (or the equivalent analysis tool) is unavailable.

- **`public async Task<ConversionResult> ExecuteCustomOperationAsync`**
  Executes a user-defined FFmpeg command. Accepts a raw argument string or a structured representation of FFmpeg arguments. Returns a `ConversionResult` with the output path if one is specified in the arguments. This method provides an escape hatch for operations not covered by the built-in methods. Throws an `ArgumentException` if the arguments are null or empty. Throws an `InvalidOperationException` if FFmpeg is not available.

- **`public async Task<string> GetFFmpegVersionAsync`**
  Retrieves the version string of the FFmpeg binary in use. Returns a string such as `"ffmpeg version n6.1.1"`. Throws an `InvalidOperationException` if FFmpeg cannot be executed or the version output cannot be parsed.

- **`public Task<bool> IsFFmpegAvailableAsync`**
  Checks whether the FFmpeg binary is present and executable. Returns `true` if the binary can be launched and produces expected output; otherwise `false`. This method does not throw exceptions under normal circumstances, but may return `false` if the binary path is misconfigured or permissions are insufficient.

- **`public async Task<ConversionResult> ExtractAudioAsync`**
  Extracts the audio stream from a video file and saves it as a standalone audio file. Accepts the source video path, output audio path, and optional audio codec and bitrate settings. Returns a `ConversionResult` with the audio file path. Throws an `ArgumentException` if the source file has no audio stream. Throws an `InvalidOperationException` if FFmpeg is not available.

- **`public async Task<List<ConversionResult>> BatchTranscodeAsync`**
  Transcodes multiple files in sequence. Accepts a collection of input-output pairs or a directory and conversion profile. Returns a list of `ConversionResult` objects, one per file, where each result independently indicates success or failure. Throws an `ArgumentException` if the input collection is empty. Throws an `InvalidOperationException` if FFmpeg is not available. Individual failures do not abort the entire batch.

- **`public async Task<ConversionResult> CreateHlsAsync`**
  Creates an HTTP Live Streaming (HLS) playlist and segmented video files from a source video. Accepts the source path, output directory, and segment duration/encoding options. Returns a `ConversionResult` with the path to the master playlist (`.m3u8`). Throws an `ArgumentException` if the output directory cannot be created or written to. Throws an `InvalidOperationException` if FFmpeg is not available.

- **`public async Task<ConversionResult> EmbedSubtitlesAsync`**
  Burns or muxes a subtitle track into a video file. Accepts the source video path, subtitle file path (e.g., `.srt`, `.ass`), output path, and a flag indicating whether to burn the subtitles into the video stream or add them as a selectable track. Returns a `ConversionResult` with the output path. Throws an `ArgumentException` if the subtitle file is missing or in an unsupported format. Throws an `InvalidOperationException` if FFmpeg is not available.

- **`public async Task<ThumbnailResult> ExtractThumbnailsAsync`**
  Captures one or more thumbnail images from a video at specified timestamps or at regular intervals. Accepts the source video path, output directory, and timing parameters. Returns a `ThumbnailResult` containing the list of generated image paths and their corresponding timestamps. Throws an `ArgumentException` if timestamps are invalid or the output directory is not writable. Throws an `InvalidOperationException` if FFmpeg is not available.

## Usage

### Example 1: Transcoding and Trimming a Video

```csharp
var service = new FFmpegService(new FFmpegOptions
{
    FFmpegPath = "/usr/bin/ffmpeg",
    WorkingDirectory = "/tmp/ffmpeg-work"
});

// Check availability before proceeding
if (!await service.IsFFmpegAvailableAsync())
{
    Console.WriteLine("FFmpeg is not available.");
    return;
}

// Transcode to H.264
ConversionResult transcodeResult = await service.TranscodeAsync(
    inputPath: "/videos/source.mov",
    outputPath: "/videos/output.mp4",
    codec: "libx264",
    bitrate: "2M"
);

if (transcodeResult.Success)
{
    // Trim the transcoded output to the first 30 seconds
    ConversionResult trimResult = await service.TrimAsync(
        inputPath: transcodeResult.OutputPath,
        outputPath: "/videos/output_trimmed.mp4",
        startTime: TimeSpan.Zero,
        duration: TimeSpan.FromSeconds(30)
    );

    Console.WriteLine($"Trimmed file: {trimResult.OutputPath}");
}
else
{
    Console.WriteLine($"Transcode failed: {transcodeResult.ErrorMessage}");
}
```

### Example 2: Batch Processing with Analysis and Watermarking

```csharp
var service = new FFmpegService(new FFmpegOptions
{
    FFmpegPath = "/usr/bin/ffmpeg",
    WorkingDirectory = "/tmp/ffmpeg-work"
});

string[] inputFiles = Directory.GetFiles("/videos/incoming", "*.mp4");
var batchResults = new List<ConversionResult>();

// Analyze each file, then add a watermark if resolution is at least 1080p
foreach (var file in inputFiles)
{
    MediaFile info = await service.AnalyzeMediaAsync(file);
    
    if (info.VideoStreams.Any(v => v.Height >= 1080))
    {
        ConversionResult result = await service.AddWatermarkAsync(
            sourcePath: file,
            watermarkPath: "/assets/logo.png",
            outputPath: Path.Combine("/videos/processed", Path.GetFileName(file)),
            position: WatermarkPosition.BottomRight,
            offsetX: 10,
            offsetY: 10
        );
        batchResults.Add(result);
    }
}

int successCount = batchResults.Count(r => r.Success);
Console.WriteLine($"Watermarked {successCount} of {inputFiles.Length} files.");
```

## Notes

- **Thread Safety**: `FFmpegService` is designed to be used from a single thread or with external synchronization. Concurrent calls to methods such as `TranscodeAsync` or `ExecuteCustomOperationAsync` may spawn multiple FFmpeg processes simultaneously, which can lead to resource contention (CPU, memory, I/O). If parallel processing is desired, consider using `BatchTranscodeAsync` or implementing a queue with controlled concurrency.
- **FFmpeg Availability**: Always call `IsFFmpegAvailableAsync` or `GetFFmpegVersionAsync` before executing operations. If the FFmpeg binary is missing, not executable, or an incompatible version is installed, most methods will throw an `InvalidOperationException`.
- **Input Validation**: Methods throw `ArgumentException` for invalid arguments (null paths, empty file lists, negative timestamps). It is the caller's responsibility to ensure files exist and are readable before invoking operations; the service does not pre-validate file existence beyond what FFmpeg itself reports during execution.
- **Partial Failures in Batch Operations**: `BatchTranscodeAsync` returns a list of individual `ConversionResult` objects. A failure in one file does not stop the batch; the remaining files continue processing. Inspect each result's `Success` property to determine per-file outcomes.
- **Temporary Files**: The service may create temporary files in the configured working directory. Ensure sufficient disk space and appropriate cleanup policies. The service does not automatically delete intermediate files unless the specific method documents such behavior.
- **Subtitle Handling**: `EmbedSubtitlesAsync` distinguishes between burning subtitles (permanently overlaying them onto the video frames) and muxing them as a separate track. Burning is irreversible and increases encoding time; muxing preserves the original video stream but requires player support for subtitle track selection.
- **HLS Output**: `CreateHlsAsync` generates multiple segment files alongside the playlist. Ensure the output directory is dedicated to the HLS output to avoid file collisions. The method does not manage uploads or CDN distribution of the resulting segments.
