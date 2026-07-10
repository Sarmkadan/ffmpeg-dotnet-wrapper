# TranscodeService

A service class that provides high-level, asynchronous methods for common FFmpeg transcoding and media-processing tasks such as format conversion, bitrate adjustment, audio extraction, and video resizing.

## API

### `TranscodeService`
The main service class that encapsulates FFmpeg transcoding operations. It is designed to be instantiated and used as a dependency in .NET applications.

### `async Task<ConversionResult> TranscodeToWebAsync(string inputPath, string outputPath)`
Converts the input media file to a web-optimized format (e.g., H.264 with AAC audio and MP4 container).
- **Parameters**:
  - `inputPath` (string): Path to the source media file.
  - `outputPath` (string): Destination path for the transcoded output.
- **Returns**: A `Task<ConversionResult>` that resolves to a `ConversionResult` indicating success or failure and containing metadata.
- **Throws**: `ArgumentNullException` if `inputPath` or `outputPath` is null.
- **Throws**: `FileNotFoundException` if `inputPath` does not exist.

### `async Task<ConversionResult> TranscodeToH265Async(string inputPath, string outputPath)`
Converts the input media to H.265/HEVC-encoded video with AAC audio in an MP4 container.
- **Parameters**:
  - `inputPath` (string): Path to the source media file.
  - `outputPath` (string): Destination path for the transcoded output.
- **Returns**: A `Task<ConversionResult>` indicating the outcome of the operation.
- **Throws**: `ArgumentNullException` if `inputPath` or `outputPath` is null.
- **Throws**: `FileNotFoundException` if `inputPath` does not exist.

### `async Task<ConversionResult> TranscodeToMobileAsync(string inputPath, string outputPath)`
Converts the input media to a mobile-friendly format (e.g., H.264 with lower resolution and bitrate).
- **Parameters**:
  - `inputPath` (string): Path to the source media file.
  - `outputPath` (string): Destination path for the transcoded output.
- **Returns**: A `Task<ConversionResult>` with the result of the conversion.
- **Throws**: `ArgumentNullException` if `inputPath` or `outputPath` is null.
- **Throws**: `FileNotFoundException` if `inputPath` does not exist.

### `async Task<ConversionResult> TranscodeToHighQualityAsync(string inputPath, string outputPath)`
Converts the input media to a high-quality format (e.g., H.264 with higher bitrate and resolution).
- **Parameters**:
  - `inputPath` (string): Path to the source media file.
  - `outputPath` (string): Destination path for the transcoded output.
- **Returns**: A `Task<ConversionResult>` indicating the outcome.
- **Throws**: `ArgumentNullException` if `inputPath` or `outputPath` is null.
- **Throws**: `FileNotFoundException` if `inputPath` does not exist.

### `async Task<ConversionResult> TranscodeWithBitrateAsync(string inputPath, string outputPath, int targetBitrateKbps)`
Converts the input media using a specified target video bitrate (in kbps).
- **Parameters**:
  - `inputPath` (string): Path to the source media file.
  - `outputPath` (string): Destination path for the transcoded output.
  - `targetBitrateKbps` (int): Desired video bitrate in kilobits per second.
- **Returns**: A `Task<ConversionResult>` with the result of the operation.
- **Throws**: `ArgumentNullException` if `inputPath` or `outputPath` is null.
- **Throws**: `ArgumentOutOfRangeException` if `targetBitrateKbps` is less than 1.
- **Throws**: `FileNotFoundException` if `inputPath` does not exist.

### `async Task<ConversionResult> ExtractAudioAsync(string inputPath, string outputPath)`
Extracts the audio stream from the input media file and saves it as a standalone audio file (e.g., AAC or MP3).
- **Parameters**:
  - `inputPath` (string): Path to the source media file.
  - `outputPath` (string): Destination path for the extracted audio.
- **Returns**: A `Task<ConversionResult>` indicating success or failure.
- **Throws**: `ArgumentNullException` if `inputPath` or `outputPath` is null.
- **Throws**: `FileNotFoundException` if `inputPath` does not exist.

### `async Task<ConversionResult> ResizeVideoAsync(string inputPath, string outputPath, int width, int height)`
Resizes the video to the specified dimensions while maintaining aspect ratio and encoding with H.264.
- **Parameters**:
  - `inputPath` (string): Path to the source media file.
  - `outputPath` (string): Destination path for the resized video.
  - `width` (int): Target width in pixels.
  - `height` (int): Target height in pixels.
- **Returns**: A `Task<ConversionResult>` with the result of the resize operation.
- **Throws**: `ArgumentNullException` if `inputPath` or `outputPath` is null.
- **Throws**: `ArgumentOutOfRangeException` if `width` or `height` is less than 1.
- **Throws**: `FileNotFoundException` if `inputPath` does not exist.

## Usage

```csharp
// Example 1: Convert a video to web-optimized format
var transcodeService = new TranscodeService();
var result = await transcodeService.TranscodeToWebAsync("input.mp4", "output_web.mp4");

if (result.Success)
{
    Console.WriteLine($"Web transcoding complete. Output: {result.OutputPath}");
}
else
{
    Console.WriteLine($"Web transcoding failed: {result.ErrorMessage}");
}

// Example 2: Extract audio from a video file
var extractResult = await transcodeService.ExtractAudioAsync("input.mp4", "output_audio.aac");

if (extractResult.Success)
{
    Console.WriteLine($"Audio extracted to: {extractResult.OutputPath}");
}
else
{
    Console.WriteLine($"Audio extraction failed: {extractResult.ErrorMessage}");
}
```

## Notes

- All methods are thread-safe and can be called concurrently from multiple threads.
- File system operations (e.g., reading input, writing output) are performed synchronously within the async methods; ensure adequate I/O performance on the target system.
- Long-running operations may block the calling thread if the thread pool is saturated; consider configuring `ThreadPool` or using `Task.Run` for CPU-bound workarounds.
- Paths must be accessible and valid; no automatic cleanup of partial outputs is performed on failure.
- FFmpeg must be installed and available in the system PATH for the service to function.
