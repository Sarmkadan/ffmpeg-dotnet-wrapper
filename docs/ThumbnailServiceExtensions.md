# ThumbnailServiceExtensions

Provides extension methods for generating thumbnails from video files at specific positions (first frame, last frame, middle frame, or a custom percentage) using FFmpeg.

## API

### `ExtractFirstFrameAsync`

Extracts the first frame from the specified video file as a thumbnail.

**Parameters:**
- `sourcePath` (string): The file path to the source video.
- `outputPath` (string): The file path where the thumbnail will be saved.
- `cancellationToken` (CancellationToken, optional): A token to monitor for cancellation requests.

**Return Value:**
- `Task<ThumbnailResult>`: A task that represents the asynchronous operation. The result contains metadata about the generated thumbnail, including its path and dimensions.

**Exceptions:**
- Throws `ArgumentNullException` if `sourcePath` or `outputPath` is null.
- Throws `FileNotFoundException` if the source video file does not exist.
- Throws `InvalidOperationException` if FFmpeg fails to process the video.

---

### `ExtractLastFrameAsync`

Extracts the last frame from the specified video file as a thumbnail.

**Parameters:**
- `sourcePath` (string): The file path to the source video.
- `outputPath` (string): The file path where the thumbnail will be saved.
- `cancellationToken` (CancellationToken, optional): A token to monitor for cancellation requests.

**Return Value:**
- `Task<ThumbnailResult>`: A task that represents the asynchronous operation. The result contains metadata about the generated thumbnail, including its path and dimensions.

**Exceptions:**
- Throws `ArgumentNullException` if `sourcePath` or `outputPath` is null.
- Throws `FileNotFoundException` if the source video file does not exist.
- Throws `InvalidOperationException` if FFmpeg fails to process the video.

---
### `ExtractMiddleFrameAsync`

Extracts the middle frame from the specified video file as a thumbnail.

**Parameters:**
- `sourcePath` (string): The file path to the source video.
- `outputPath` (string): The file path where the thumbnail will be saved.
- `cancellationToken` (CancellationToken, optional): A token to monitor for cancellation requests.

**Return Value:**
- `Task<ThumbnailResult>`: A task that represents the asynchronous operation. The result contains metadata about the generated thumbnail, including its path and dimensions.

**Exceptions:**
- Throws `ArgumentNullException` if `sourcePath` or `outputPath` is null.
- Throws `FileNotFoundException` if the source video file does not exist.
- Throws `InvalidOperationException` if FFmpeg fails to process the video.

---
### `ExtractAtPercentageAsync`

Extracts a frame from the specified video file at the given percentage position as a thumbnail.

**Parameters:**
- `sourcePath` (string): The file path to the source video.
- `outputPath` (string): The file path where the thumbnail will be saved.
- `percentage` (double): The position in the video (0.0 to 100.0) where the frame should be extracted.
- `cancellationToken` (CancellationToken, optional): A token to monitor for cancellation requests.

**Return Value:**
- `Task<ThumbnailResult>`: A task that represents the asynchronous operation. The result contains metadata about the generated thumbnail, including its path and dimensions.

**Exceptions:**
- Throws `ArgumentNullException` if `sourcePath` or `outputPath` is null.
- Throws `ArgumentOutOfRangeException` if `percentage` is outside the range [0.0, 100.0].
- Throws `FileNotFoundException` if the source video file does not exist.
- Throws `InvalidOperationException` if FFmpeg fails to process the video.

## Usage

### Example 1: Extract the first frame
