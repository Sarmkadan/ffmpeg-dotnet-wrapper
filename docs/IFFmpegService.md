# IFFmpegService

`IFFmpegService` is the core interface for orchestrating FFmpeg media operations in the `ffmpeg-dotnet-wrapper` library. It defines a high-level, asynchronous API for common media operations such as transcoding, trimming, merging, watermarking, analysis, and more. Implementations abstract away FFmpeg process management, command-line construction, temporary file handling, and progress monitoring, returning structured result objects that indicate success or failure along with relevant output paths.

## API

### Methods

#### `Task<ConversionResult> TranscodeAsync(MediaFile inputMedia, string outputPath, TranscodeSettings settings, CancellationToken cancellationToken = default)`

Transcodes a media file to a different codec, container, or bitrate using the provided settings. Supports hardware acceleration when configured.

- **Parameters**:
  - `inputMedia`: The source media file with pre-analyzed metadata.
  - `outputPath`: Destination file path for the transcoded output.
  - `settings`: Transcoding settings including codec, bitrate, resolution, and audio parameters.
  - `cancellationToken`: Token to cancel the FFmpeg process.
- **Returns**: A `Task<ConversionResult>` with output file info, duration, and success status.
- **Settings Model**: `TranscodeSettings`
- **Cancellation**: Supported via `cancellationToken`.

#### `Task<ConversionResult> TranscodeAsync(MediaFile inputMedia, string outputPath, TranscodeSettings settings, IProgress<FFmpegProgressUpdate> progress, CancellationToken cancellationToken = default)`

Transcodes a media file exactly like the above overload, while additionally streaming incremental `FFmpegProgressUpdate` snapshots to `progress` as FFmpeg reports them via its `-progress pipe:1` machine-readable output. Progress lines are parsed one at a time as they arrive, so memory usage stays constant regardless of job length.

- **Parameters**:
  - `inputMedia`: The source media file with pre-analyzed metadata, used to derive total duration for percentage calculation.
  - `outputPath`: Destination file path for the transcoded output.
  - `settings`: Transcoding settings including codec, bitrate, resolution, and audio parameters.
  - `progress`: Receiver of incremental progress snapshots. Must not be `null`.
  - `cancellationToken`: Token to cancel the FFmpeg process.
- **Returns**: A `Task<ConversionResult>` with output file info, duration, and success status.
- **Settings Model**: `TranscodeSettings`
- **Cancellation**: Supported via `cancellationToken`.

#### `Task<ConversionResult> TrimAsync(MediaFile inputMedia, string outputPath, TrimSettings settings, CancellationToken cancellationToken = default)`

Trims a media file to the time range specified in `settings`, using stream copy when possible to avoid re-encoding.

- **Parameters**:
  - `inputMedia`: The source media file to trim.
  - `outputPath`: Destination file path for the trimmed output.
  - `settings`: Trim settings including start time, end time, and re-encoding preference.
  - `cancellationToken`: Token to cancel the FFmpeg process.
- **Returns**: A `Task<ConversionResult>` with output metadata.
- **Settings Model**: `TrimSettings`
- **Cancellation**: Supported via `cancellationToken`.

#### `Task<ConversionResult> MergeAsync(IEnumerable<string> inputFiles, string outputPath, MergeSettings settings, CancellationToken cancellationToken = default)`

Concatenates multiple media files into a single output file. Input files must share the same codec and stream parameters for concat demuxer compatibility.

- **Parameters**:
  - `inputFiles`: Ordered collection of file paths to merge.
  - `outputPath`: Destination file path for the merged output.
  - `settings`: Merge settings including transition effects and re-encoding options.
  - `cancellationToken`: Token to cancel the FFmpeg process.
- **Returns**: A `Task<ConversionResult>` with the merged file metadata.
- **Settings Model**: `MergeSettings`
- **Cancellation**: Supported via `cancellationToken`.

#### `Task<ConversionResult> AddWatermarkAsync(MediaFile inputMedia, string outputPath, WatermarkSettings settings, CancellationToken cancellationToken = default)`

Overlays a watermark image or text onto a video using FFmpeg's overlay filter.

- **Parameters**:
  - `inputMedia`: The source video file.
  - `outputPath`: Destination file path for the watermarked output.
  - `settings`: Watermark configuration including image path, position, opacity, and scaling.
  - `cancellationToken`: Token to cancel the FFmpeg process.
- **Returns**: A `Task<ConversionResult>` with output metadata.
- **Settings Model**: `WatermarkSettings`
- **Cancellation**: Supported via `cancellationToken`.

#### `Task<MediaFile> AnalyzeMediaAsync(string filePath, CancellationToken cancellationToken = default)`

Probes a media file using ffprobe and extracts detailed metadata including codec info, duration, bitrate, resolution, and stream details.

- **Parameters**:
  - `filePath`: Path to the media file to analyze.
  - `cancellationToken`: Token to cancel the probe operation.
- **Returns**: A `Task<MediaFile>` populated with the extracted metadata.
- **Settings Model**: None (uses ffprobe directly)
- **Cancellation**: Supported via `cancellationToken`.

#### `Task<ConversionResult> ExecuteCustomOperationAsync(FFmpegOperation operation, CancellationToken cancellationToken = default)`

Executes a custom FFmpeg operation with user-defined input/output arguments. Use for operations not covered by the typed methods above.

- **Parameters**:
  - `operation`: The custom operation definition with raw FFmpeg arguments.
  - `cancellationToken`: Token to cancel the FFmpeg process.
- **Returns**: A `Task<ConversionResult>` with process exit code and output.
- **Settings Model**: `FFmpegOperation` (custom operation)
- **Cancellation**: Supported via `cancellationToken`.

#### `Task<string> GetFFmpegVersionAsync(CancellationToken cancellationToken = default)`

Returns the installed FFmpeg version string (e.g., "ffmpeg version 6.1.1").

- **Parameters**:
  - `cancellationToken`: Cancellation token.
- **Returns**: The FFmpeg version string from stdout.
- **Settings Model**: None
- **Cancellation**: Supported via `cancellationToken`.

#### `Task<bool> IsFFmpegAvailableAsync(CancellationToken cancellationToken = default)`

Checks whether FFmpeg is installed and accessible on the system PATH.

- **Parameters**:
  - `cancellationToken`: Cancellation token.
- **Returns**: `true` if FFmpeg is available; otherwise `false`.
- **Settings Model**: None
- **Cancellation**: Supported via `cancellationToken`.

#### `Task<ConversionResult> EmbedSubtitlesAsync(MediaFile inputMedia, string outputPath, SubtitleSettings settings, CancellationToken cancellationToken = default)`

Embeds a subtitle file into a video, either as a soft-coded subtitle stream or burned directly into the video frames based on `SubtitleSettings.HardEmbed`.

- **Parameters**:
  - `inputMedia`: The source video file with pre-analyzed metadata.
  - `outputPath`: Destination file path for the output with embedded subtitles.
  - `settings`: Subtitle settings including path, encoding mode, font, and language.
  - `cancellationToken`: Token to cancel the FFmpeg process.
- **Returns**: A `Task<ConversionResult>` with the output file metadata.
- **Settings Model**: `SubtitleSettings`
- **Cancellation**: Supported via `cancellationToken`.

#### `Task<ThumbnailResult> ExtractThumbnailsAsync(MediaFile inputMedia, string outputPattern, ThumbnailSettings settings, CancellationToken cancellationToken = default)`

Extracts one or more thumbnail images from a video file.

- **Parameters**:
  - `inputMedia`: The source video file with pre-analyzed metadata.
  - `outputPattern`: Output file path pattern. Use `%03d` for sequential numbering when extracting multiple thumbnails (e.g., `/output/thumb_%03d.jpg`).
  - `settings`: Thumbnail settings including timestamps, format, and dimensions.
  - `cancellationToken`: Token to cancel the FFmpeg process.
- **Returns**: A `Task<ThumbnailResult>` containing the paths of all extracted images.
- **Settings Model**: `ThumbnailSettings`
- **Cancellation**: Supported via `cancellationToken`.

#### `Task<ConversionResult> ExtractAudioAsync(MediaFile inputMedia, string outputPath, AudioCodec audioCodec = AudioCodec.MP3, int audioBitrate = 192, CancellationToken cancellationToken = default)`

Extracts the audio track from a media file, discarding video, and encodes it using the specified audio codec and bitrate.

- **Parameters**:
  - `inputMedia`: The source media file with pre-analyzed metadata.
  - `outputPath`: Destination file path for the extracted audio.
  - `audioCodec`: The audio codec to encode the extracted track with.
  - `audioBitrate`: The target audio bitrate in kbps.
  - `cancellationToken`: Token to cancel the FFmpeg process.
- **Returns**: A `Task<ConversionResult>` with the extracted audio file metadata.
- **Settings Model**: None (uses direct parameters for codec and bitrate)
- **Cancellation**: Supported via `cancellationToken`.

#### `Task<List<ConversionResult>> BatchTranscodeAsync(IEnumerable<MediaFile> inputFiles, string outputDirectory, TranscodeSettings settings, CancellationToken cancellationToken = default)`

Transcodes multiple media files sequentially into `outputDirectory`, applying the same `TranscodeSettings` to each input.

- **Parameters**:
  - `inputFiles`: The source media files to transcode.
  - `outputDirectory`: Directory that will receive the transcoded outputs.
  - `settings`: Transcoding settings applied to every input file.
  - `cancellationToken`: Token to cancel the batch operation.
- **Returns**: A `Task<List<ConversionResult>>` for each input file, in input order.
- **Settings Model**: `TranscodeSettings`
- **Cancellation**: Supported via `cancellationToken`.

## Mocking Example for Tests

When writing unit tests for classes that depend on `IFFmpegService`, you can use a mocking framework like Moq to simulate the service's behavior. Below is an example using Moq:

```csharp
using Moq;
using FFmpegDotnetwrapper.Models;
using FFmpegDotnetwrapper.Services;
using FFmpegDotnetwrapper.Models.Enums;

// Arrange
var mockFFmpegService = new Mock<IFFmpegService>();

// Setup a mock for TranscodeAsync
mockFFmpegService.Setup(service => service.TranscodeAsync(
        It.IsAny<MediaFile>(),
        It.IsAny<string>(),
        It.IsAny<TranscodeSettings>(),
        It.IsAny<CancellationToken>()))
    .ReturnsAsync(new ConversionResult
    {
        Success = true,
        OutputPath = "/fake/output.mp4",
        Duration = TimeSpan.FromSeconds(120)
    });

// Act
var result = await mockFFmpegService.Object.TranscodeAsync(
    new MediaFile { FilePath = "/fake/input.mp4" },
    "/fake/output.mp4",
    new TranscodeSettings { VideoCodec = VideoCodec.H264 },
    CancellationToken.None);

// Assert
Assert.True(result.Success);
Assert.Equal("/fake/output.mp4", result.OutputPath);
```

This example demonstrates how to mock the `TranscodeAsync` method. Similar setups can be done for other methods in the interface by configuring the appropriate return values and parameters.

## Notes

- **Implementation Notes**: Implementations of this interface should handle FFmpeg process lifecycle, error checking, and result aggregation as demonstrated by the `FFmpegService` class.
- **Thread Safety**: Implementations should consider thread safety if the service is intended to be used from multiple threads concurrently.
- **Exception Handling**: Methods may throw exceptions for invalid arguments (e.g., `ArgumentException`) or when FFmpeg is not available (e.g., `InvalidOperationException`). Implementations should document their specific exception behaviors.