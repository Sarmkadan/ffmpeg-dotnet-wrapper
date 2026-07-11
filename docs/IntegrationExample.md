# IntegrationExample

The `IntegrationExample` class provides a high-level demonstration of the ffmpeg-dotnet-wrapper library's capabilities. It exposes configurable properties for specifying input and output media files, codec and container settings, trimming parameters, and quality presets. Asynchronous methods enable common workflows such as web-optimized conversion and thumbnail generation. The class also exposes service instances (`VideoProcessingService` and `MediaAnalysisService`) that perform the underlying operations.

## API

### `public static void Main(string[] args)`
The application entry point. Typically creates an instance of `IntegrationExample`, configures its properties, and invokes the asynchronous conversion or analysis methods. Accepts command-line arguments (not used internally by the class itself).

### `public string InputFile`
Gets or sets the full path to the input media file. Must be a valid, accessible file path.

### `public string OutputFile`
Gets or sets the full path where the output file will be written. The directory must exist and be writable.

### `public VideoCodec VideoCodec`
Gets or sets the video codec to use for encoding (e.g., `H264`, `H265`, `VP9`). The selected codec must be supported by the underlying FFmpeg build.

### `public AudioCodec AudioCodec`
Gets or sets the audio codec to use for encoding (e.g., `AAC`, `MP3`, `Opus`). The selected codec must be supported.

### `public ContainerFormat Container`
Gets or sets the container format for the output file (e.g., `Mp4`, `WebM`, `Mkv`). Must be compatible with the chosen codecs.

### `public int VideoBitrate`
Gets or sets the video bitrate in bits per second. Typical values range from 500,000 (500 kbps) to 20,000,000 (20 Mbps). Must be positive.

### `public int AudioBitrate`
Gets or sets the audio bitrate in bits per second. Common values are 128,000 (128 kbps) or 320,000 (320 kbps). Must be positive.

### `public QualityPreset Quality`
Gets or sets the quality preset (e.g., `Low`, `Medium`, `High`, `Lossless`). This influences encoding speed and output quality, typically by adjusting CRF or similar parameters.

### `public double StartSeconds`
Gets or sets the start offset in seconds for trimming the input. Must be non-negative and less than the input duration.

### `public double DurationSeconds`
Gets or sets the duration in seconds to include from the start offset. Must be positive. If zero or negative, the entire remainder of the input is used.

### `public bool PreserveAudio`
Gets or sets a value indicating whether the original audio stream should be copied without re-encoding. When `true`, the `AudioCodec` and `AudioBitrate` properties are ignored for the audio stream.

### `public bool PreserveVideo`
Gets or sets a value indicating whether the original video stream should be copied without re-encoding. When `true`, the `VideoCodec` and `VideoBitrate` properties are ignored for the video stream.

### `public bool Keyframe`
Gets or sets a value indicating whether the output should be forced to start at a keyframe. When `true`, the start time is adjusted to the nearest preceding keyframe.

### `public VideoProcessingService VideoProcessingService`
Gets the `VideoProcessingService` instance used for conversion operations. This service is initialized internally and should not be replaced.

### `public MediaAnalysisService MediaAnalysisService`
Gets the `MediaAnalysisService` instance used for media analysis operations (e.g., probing file metadata). This service is initialized internally.

### `public async Task<ConversionResult> ConvertForWebOptimizationAsync()`
Asynchronously converts the input file to a web-optimized format using the current property values. The method applies settings such as codec, bitrate, container, and trimming. Returns a `ConversionResult` indicating success or failure, along with details (e.g., output path, duration, error messages).  
**Throws:** `FileNotFoundException` if `InputFile` does not exist; `InvalidOperationException` if required properties are not set; `FFmpegException` if the conversion fails.

### `public async Task<ConversionResult> CreateThumbnailAsync()`
Asynchronously creates a thumbnail image from the input file at the time specified by `StartSeconds` (or the first keyframe if `Keyframe` is `true`). The output file path is determined by `OutputFile`. Returns a `ConversionResult` with the thumbnail file path on success.  
**Throws:** Same as `ConvertForWebOptimizationAsync`; additionally `ArgumentException` if `OutputFile` does not have a supported image extension (e.g., `.jpg`, `.png`).

## Usage

### Example 1: Basic Conversion with Trimming

```csharp
using ffmpeg_dotnet_wrapper;

var example = new IntegrationExample
{
    InputFile = @"C:\videos\input.mp4",
    OutputFile = @"C:\videos\output.webm",
    VideoCodec = VideoCodec.VP9,
    AudioCodec = AudioCodec.Opus,
    Container = ContainerFormat.WebM,
    VideoBitrate = 2_000_000,
    AudioBitrate = 128_000,
    Quality = QualityPreset.Medium,
    StartSeconds = 10.0,
    DurationSeconds = 30.0,
    PreserveAudio = false,
    PreserveVideo = false,
    Keyframe = false
};

ConversionResult result = await example.ConvertForWebOptimizationAsync();
if (result.Success)
    Console.WriteLine($"Conversion succeeded: {result.OutputPath}");
else
    Console.WriteLine($"Conversion failed: {result.ErrorMessage}");
```

### Example 2: Thumbnail Generation with Keyframe Alignment

```csharp
using ffmpeg_dotnet_wrapper;

var example = new IntegrationExample
{
    InputFile = @"C:\videos\long_video.mkv",
    OutputFile = @"C:\thumbnails\preview.jpg",
    StartSeconds = 120.0,
    Keyframe = true
};

ConversionResult result = await example.CreateThumbnailAsync();
if (result.Success)
    Console.WriteLine($"Thumbnail created: {result.OutputPath}");
else
    Console.WriteLine($"Thumbnail failed: {result.ErrorMessage}");
```

## Notes

- **Thread safety:** Instances of `IntegrationExample` are not thread-safe. Properties should not be modified while an asynchronous method is executing. If concurrent operations are required, create separate instances for each workflow.
- **File validation:** The `InputFile` must exist and be readable. The directory of `OutputFile` must exist; the file will be overwritten if it already exists.
- **Codec/container compatibility:** Not all codec-container combinations are valid (e.g., VP9 in an MP4 container may not be supported). The underlying FFmpeg build determines compatibility. An `FFmpegException` is thrown if the combination is invalid.
- **Trimming edge cases:** If `StartSeconds` exceeds the input duration, the conversion will fail. If `DurationSeconds` extends beyond the end of the file, the output will contain only the available remainder.
- **Preserve flags:** When `PreserveAudio` or `PreserveVideo` is `true`, the corresponding codec and bitrate settings are ignored. The stream is copied using the original encoding. This may produce files that are not compatible with the chosen container.
- **Keyframe alignment:** Setting `Keyframe` to `true` adjusts the start time to the nearest preceding keyframe. This may result in a longer output than specified by `DurationSeconds`.
- **Large files:** For very large inputs, the asynchronous methods may take considerable time. Consider using cancellation tokens (not exposed in this class) or running on a background thread.
- **Static `Main`:** The `Main` method is provided for demonstration purposes. In production code, instantiate `IntegrationExample` and call its methods directly.
