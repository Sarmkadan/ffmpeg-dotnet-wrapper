# FFmpegControllerExtensions

Extension methods for `FFmpegController` that provide high-level FFmpeg operations through a fluent API. These methods wrap common media processing tasks such as format conversion, trimming, merging, watermarking, thumbnail extraction, and subtitle embedding, returning structured responses that include success status, messages, and typed results.

## API

### `ExtractMediaInfo`
Extracts metadata and technical information from a media file without performing any transcoding. The returned `MediaFile` object contains details such as duration, bitrate, codec information, resolution, and stream metadata.

**Parameters:**
- `inputPath` (string): Path to the input media file.

**Returns:**
- `ApiResponse<MediaFile>`: Response containing the extracted media information or an error message if the operation fails.

**Throws:**
- `ArgumentNullException` if `inputPath` is null or empty.
- `FileNotFoundException` if the input file does not exist.
- `InvalidOperationException` if FFmpeg fails to process the file.

---

### `TranscodeAsync`
Converts a media file from one format to another using specified output settings. Supports format conversion, codec selection, bitrate adjustment, and resolution scaling.

**Parameters:**
- `inputPath` (string): Path to the input media file.
- `outputPath` (string): Destination path for the transcoded output.
- `settings` (TranscodeSettings): Configuration for the transcoding process, including codec, bitrate, resolution, and other FFmpeg parameters.

**Returns:**
- `ApiResponse<ConversionResult>`: Response containing the output file path and metadata if successful, or an error message otherwise.

**Throws:**
- `ArgumentNullException` if `inputPath`, `outputPath`, or `settings` is null.
- `FileNotFoundException` if the input file does not exist.
- `UnauthorizedAccessException` if the output path is not writable.
- `InvalidOperationException` if FFmpeg fails to transcode the file.

---

### `TrimFromStartAsync`
Trims a media file starting from the beginning up to a specified duration. Useful for removing leading segments such as intros or silence.

**Parameters:**
- `inputPath` (string): Path to the input media file.
- `outputPath` (string): Destination path for the trimmed output.
- `duration` (TimeSpan): Length of the segment to keep from the start.

**Returns:**
- `ApiResponse<ConversionResult>`: Response containing the output file path and metadata if successful, or an error message otherwise.

**Throws:**
- `ArgumentNullException` if `inputPath` or `outputPath` is null.
- `ArgumentOutOfRangeException` if `duration` is negative or exceeds the input file duration.
- `FileNotFoundException` if the input file does not exist.
- `InvalidOperationException` if FFmpeg fails to trim the file.

---

### `TrimAsync`
Trims a media file between two specified timestamps. Removes content outside the specified range.

**Parameters:**
- `inputPath` (string): Path to the input media file.
- `outputPath` (string): Destination path for the trimmed output.
- `start` (TimeSpan): Start time of the segment to keep.
- `end` (TimeSpan): End time of the segment to keep.

**Returns:**
- `ApiResponse<ConversionResult>`: Response containing the output file path and metadata if successful, or an error message otherwise.

**Throws:**
- `ArgumentNullException` if `inputPath` or `outputPath` is null.
- `ArgumentOutOfRangeException` if `start` or `end` is negative, or if `start` is greater than `end`, or if either timestamp exceeds the input file duration.
- `FileNotFoundException` if the input file does not exist.
- `InvalidOperationException` if FFmpeg fails to trim the file.

---
### `MergeAsync`
Combines multiple media files into a single output file. Supports merging video, audio, and subtitle streams in sequence.

**Parameters:**
- `inputPaths` (IEnumerable<string>): List of input file paths to merge.
- `outputPath` (string): Destination path for the merged output.
- `settings` (MergeSettings): Configuration for merging, including stream selection and ordering.

**Returns:**
- `ApiResponse<ConversionResult>`: Response containing the output file path and metadata if successful, or an error message otherwise.

**Throws:**
- `ArgumentNullException` if `inputPaths` or `outputPath` is null, or if `inputPaths` contains a null or empty string.
- `FileNotFoundException` if any input file does not exist.
- `InvalidOperationException` if FFmpeg fails to merge the files or if input formats are incompatible.

---
### `AddWatermarkAsync`
Applies a static image watermark (e.g., logo) to a video file at a specified position.

**Parameters:**
- `inputPath` (string): Path to the input video file.
- `outputPath` (string): Destination path for the watermarked output.
- `watermarkPath` (string): Path to the watermark image file (PNG recommended).
- `position` (WatermarkPosition): Placement of the watermark (e.g., top-left, bottom-right).
- `opacity` (float): Opacity level of the watermark (0.0 to 1.0).

**Returns:**
- `ApiResponse<ConversionResult>`: Response containing the output file path and metadata if successful, or an error message otherwise.

**Throws:**
- `ArgumentNullException` if any input or output path is null.
- `FileNotFoundException` if the input or watermark file does not exist.
- `ArgumentOutOfRangeException` if `opacity` is outside the range [0.0, 1.0].
- `InvalidOperationException` if FFmpeg fails to apply the watermark or if the watermark image is invalid.

---
### `ExtractThumbnailsAsync`
Generates thumbnail images from a video file at specified timestamps or intervals.

**Parameters:**
- `inputPath` (string): Path to the input video file.
- `outputDirectory` (string): Directory where thumbnails will be saved.
- `timestamps` (IEnumerable<TimeSpan>): Specific times at which to extract thumbnails.
- `settings` (ThumbnailSettings): Configuration for thumbnail generation, including size, format, and quality.

**Returns:**
- `ApiResponse<ThumbnailResult>`: Response containing a list of saved thumbnail file paths and metadata if successful, or an error message otherwise.

**Throws:**
- `ArgumentNullException` if `inputPath` or `outputDirectory` is null.
- `DirectoryNotFoundException` if `outputDirectory` does not exist and cannot be created.
- `FileNotFoundException` if the input file does not exist.
- `InvalidOperationException` if FFmpeg fails to extract thumbnails or if no thumbnails were generated.

---
### `EmbedSubtitlesAsync`
Burns subtitles into a video file as hardcoded text.

**Parameters:**
- `inputPath` (string): Path to the input video file.
- `outputPath` (string): Destination path for the output with embedded subtitles.
- `subtitlePath` (string): Path to the subtitle file (SRT, ASS, etc.).
- `subtitleSettings` (SubtitleSettings): Configuration for subtitle appearance (font, size, color, position).

**Returns:**
- `ApiResponse<ConversionResult>`: Response containing the output file path and metadata if successful, or an error message otherwise.

**Throws:**
- `ArgumentNullException` if any path is null.
- `FileNotFoundException` if the input or subtitle file does not exist.
- `InvalidOperationException` if FFmpeg fails to burn subtitles or if the subtitle file is malformed.

---
### `TrimWatermarkTranscodeAsync`
Combines trimming, watermarking, and transcoding into a single operation. Efficient for processing segments of video with branding applied.

**Parameters:**
- `inputPath` (string): Path to the input video file.
- `outputPath` (string): Destination path for the processed output.
- `start` (TimeSpan): Start time of the segment to keep.
- `end` (TimeSpan): End time of the segment to keep.
- `watermarkPath` (string): Path to the watermark image file.
- `position` (WatermarkPosition): Placement of the watermark.
- `opacity` (float): Opacity level of the watermark.
- `settings` (TranscodeSettings): Configuration for transcoding (codec, bitrate, resolution).

**Returns:**
- `ApiResponse<ConversionResult>`: Response containing the output file path and metadata if successful, or an error message otherwise.

**Throws:**
- `ArgumentNullException` if any required parameter is null.
- `ArgumentOutOfRangeException` if `start`, `end`, or `opacity` are invalid.
- `FileNotFoundException` if the input or watermark file does not exist.
- `InvalidOperationException` if FFmpeg fails to process the file.

## Usage

### Example 1: Transcoding a video to H.264 with AAC audio
