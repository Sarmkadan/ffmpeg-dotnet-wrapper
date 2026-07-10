# FFmpegController
The `FFmpegController` class is a central component in the `ffmpeg-dotnet-wrapper` project, providing a programmatic interface to interact with FFmpeg, a powerful, open-source media processing tool. It enables .NET applications to leverage FFmpeg's capabilities for various media operations, including video and audio transcoding, trimming, merging, watermarking, and more, all through an asynchronous and API-driven approach.

## API
The `FFmpegController` class exposes several public members that facilitate different aspects of media processing:
- `public FFmpegController`: The constructor for the `FFmpegController` class, initializing a new instance.
- `public async Task<ApiResponse<ConversionResult>> TranscodeAsync`: Transcodes a media file from one format to another. This method returns an `ApiResponse` containing a `ConversionResult` object, which encapsulates the outcome of the transcoding operation. It may throw exceptions related to FFmpeg execution errors or invalid input parameters.
- `public async Task<ApiResponse<ConversionResult>> TrimAsync`: Trims a portion of a media file. Similar to `TranscodeAsync`, it returns an `ApiResponse` with a `ConversionResult`, and may throw exceptions for execution errors or invalid parameters.
- `public async Task<ApiResponse<ConversionResult>> MergeAsync`: Merges multiple media files into a single file. The return type and potential exceptions are consistent with `TranscodeAsync` and `TrimAsync`.
- `public async Task<ApiResponse<ConversionResult>> WatermarkAsync`: Adds a watermark to a media file. The method's return type and exception behavior mirror those of the transcoding and trimming operations.
- `public ApiResponse<MediaFile> GetMediaInfoAsync`: Retrieves information about a media file. This method returns an `ApiResponse` containing a `MediaFile` object, which holds metadata about the file. It may throw exceptions if the file does not exist, is inaccessible, or if there's an error parsing the media information.
- `public async Task<ApiResponse<ConversionResult>> EmbedSubtitlesAsync`: Embeds subtitles into a media file. The return value and potential exceptions are similar to other media processing methods.
- `public async Task<ApiResponse<ThumbnailResult>> ExtractThumbnailsAsync`: Extracts thumbnails from a media file. This method returns an `ApiResponse` with a `ThumbnailResult`, which contains the extracted thumbnails. Exceptions may be thrown for FFmpeg execution errors or if the input parameters are invalid.

## Usage
The following examples demonstrate how to use the `FFmpegController` class for common media processing tasks:
```csharp
// Example 1: Transcoding a video file
var controller = new FFmpegController();
var transcodingResult = await controller.TranscodeAsync("input.mp4", "output.webm");
if (transcodingResult.IsSuccess)
{
    Console.WriteLine("Transcoding successful.");
}
else
{
    Console.WriteLine("Transcoding failed: " + transcodingResult.ErrorMessage);
}

// Example 2: Extracting thumbnails from a video
var thumbnailController = new FFmpegController();
var thumbnailResult = await thumbnailController.ExtractThumbnailsAsync("video.mp4", 10); // Extract 10 thumbnails
if (thumbnailResult.IsSuccess)
{
    foreach (var thumbnail in thumbnailResult.Data.Thumbnails)
    {
        Console.WriteLine("Thumbnail extracted: " + thumbnail.FilePath);
    }
}
else
{
    Console.WriteLine("Thumbnail extraction failed: " + thumbnailResult.ErrorMessage);
}
```

## Notes
- **Thread Safety**: The `FFmpegController` class is designed to be thread-safe, allowing multiple instances to be used concurrently without fear of data corruption or other threading issues. However, the safety of concurrent access to shared resources (like files) depends on the implementation details of the calling code.
- **Error Handling**: All asynchronous methods may throw exceptions if FFmpeg encounters errors during execution, if input parameters are invalid, or if there are issues accessing the media files. It's crucial to handle these exceptions appropriately in the calling code to ensure robustness.
- **FFmpeg Version Compatibility**: The behavior and capabilities of the `FFmpegController` can depend on the version of FFmpeg being used. Ensure that the FFmpeg version installed on the system is compatible with the expectations of the `ffmpeg-dotnet-wrapper` project to avoid unexpected behavior or errors.
