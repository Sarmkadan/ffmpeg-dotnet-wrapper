# ThumbnailService

`ThumbnailService` provides high-level, asynchronous operations for extracting thumbnails and storyboard tiles from video files. It wraps lower-level FFmpeg invocation logic to simplify common thumbnail generation workflows, returning structured results that include the generated image paths and associated metadata.

## API

### `public ThumbnailService`

Creates a new instance of the service. The constructor accepts configuration options that govern FFmpeg binary location, temporary file handling, and default encoding parameters. Once constructed, the instance is ready to accept extraction requests.

### `public async Task<ThumbnailResult> ExtractSingleAsync`

Extracts a single thumbnail frame from a video at a specified timestamp or percentage position.

**Parameters:**
- `inputPath` (`string`): Absolute or relative path to the source video file.
- `outputPath` (`string`): Target file path for the generated thumbnail image. The file extension determines the output format (e.g., `.jpg`, `.png`).
- `position` (`TimeSpan` or `double`): The temporal location for the frame capture. When a `TimeSpan` is supplied, it represents an absolute timestamp. When a `double` between 0 and 1 is supplied, it is interpreted as a fractional position of the total duration.
- `options` (`ThumbnailOptions?`, optional): Overrides for dimensions, quality, and seek precision. If `null`, sensible defaults are applied.

**Returns:**
`Task<ThumbnailResult>` whose result contains the confirmed output file path, the actual timestamp used, and the dimensions of the generated image.

**Throws:**
- `ArgumentException` if `inputPath` or `outputPath` is null or empty, or if the position exceeds the video duration.
- `FileNotFoundException` if `inputPath` does not exist.
- `InvalidOperationException` if FFmpeg execution fails or produces no valid output.
- `OperationCanceledException` if the cancellation token passed via `options.CancellationToken` is signaled.

### `public async Task<ThumbnailResult> ExtractStoryboardAsync`

Generates a storyboard image grid composed of uniformly spaced thumbnail tiles across the entire video duration.

**Parameters:**
- `inputPath` (`string`): Path to the source video file.
- `outputPath` (`string`): Target file path for the storyboard image.
- `columns` (`int`): Number of tile columns in the grid.
- `rows` (`int`): Number of tile rows in the grid. The total tile count is `columns * rows`.
- `options` (`StoryboardOptions?`, optional): Overrides for tile dimensions, border styling, timestamp overlay, and quality settings. If `null`, defaults are used.

**Returns:**
`Task<ThumbnailResult>` whose result contains the output file path, the grid dimensions, the total tile count, and the interval between consecutive captures.

**Throws:**
- `ArgumentException` if `columns` or `rows` is less than 1, or if paths are invalid.
- `FileNotFoundException` if `inputPath` does not exist.
- `InvalidOperationException` if FFmpeg cannot process the video or the generated output is empty.
- `OperationCanceledException` if cancellation is requested via `options.CancellationToken`.

### `public async Task<ThumbnailResult> ExtractAtTimestampsAsync`

Extracts multiple independent thumbnails at explicitly specified timestamps in a single FFmpeg invocation.

**Parameters:**
- `inputPath` (`string`): Path to the source video file.
- `outputDirectory` (`string`): Directory where the generated thumbnail files will be written. File names are derived automatically from the timestamps or an optional naming pattern.
- `timestamps` (`IReadOnlyList<TimeSpan>`): The exact timestamps at which to capture frames. Duplicates are allowed but may produce identical images.
- `options` (`BatchThumbnailOptions?`, optional): Overrides for dimensions, quality, file naming pattern, and parallelization limits. If `null`, defaults are used.

**Returns:**
`Task<ThumbnailResult>` whose result contains a collection of output file paths, one per requested timestamp, along with the corresponding timestamps and per-image metadata.

**Throws:**
- `ArgumentException` if `timestamps` is null or empty, or if any timestamp is negative.
- `FileNotFoundException` if `inputPath` does not exist.
- `DirectoryNotFoundException` if `outputDirectory` does not exist and cannot be created.
- `InvalidOperationException` if FFmpeg fails or produces fewer output files than requested timestamps.
- `OperationCanceledException` if cancellation is signaled via `options.CancellationToken`.

## Usage

### Extract a single thumbnail at the 30-second mark

```csharp
var service = new ThumbnailService(new ThumbnailServiceOptions
{
    FfmpegBinaryPath = "/usr/bin/ffmpeg"
});

ThumbnailResult result = await service.ExtractSingleAsync(
    inputPath: "/videos/sample.mp4",
    outputPath: "/thumbnails/frame_at_30s.jpg",
    position: TimeSpan.FromSeconds(30)
);

Console.WriteLine($"Thumbnail saved to: {result.OutputPath}");
Console.WriteLine($"Actual timestamp: {result.ActualTimestamp}");
Console.WriteLine($"Dimensions: {result.Width}x{result.Height}");
```

### Generate a 4x4 storyboard with timestamp overlays

```csharp
var service = new ThumbnailService(new ThumbnailServiceOptions
{
    FfmpegBinaryPath = "/usr/bin/ffmpeg",
    TempDirectory = "/tmp/thumbnails"
});

var storyboardOptions = new StoryboardOptions
{
    TileWidth = 320,
    TileHeight = 180,
    ShowTimestamp = true,
    TimestampFontSize = 12,
    Quality = 85
};

ThumbnailResult result = await service.ExtractStoryboardAsync(
    inputPath: "/videos/presentation.mp4",
    outputPath: "/thumbnails/storyboard.jpg",
    columns: 4,
    rows: 4,
    options: storyboardOptions
);

Console.WriteLine($"Storyboard grid: {result.Width}x{result.Height}");
Console.WriteLine($"Total tiles: {result.TileCount}");
Console.WriteLine($"Interval between captures: {result.Interval}");
```

## Notes

- **Thread safety:** Instance methods are not thread-safe. Concurrent calls on the same `ThumbnailService` instance may interfere with shared temporary state. Create separate instances or serialize access when multiple extractions must run in parallel.
- **Output overwriting:** All methods overwrite existing files at the specified output paths without warning. Callers must ensure unique paths or implement their own file-existence checks when preservation is required.
- **Timestamp precision:** `ExtractSingleAsync` and `ExtractAtTimestampsAsync` perform accurate seeks to the nearest keyframe when precise seeking is disabled. Enable precise seeking in `ThumbnailOptions` to obtain frame-exact captures at the cost of slower extraction.
- **Storyboard interval calculation:** `ExtractStoryboardAsync` computes the interval between tiles as `duration / (columns * rows)`. For very short videos, this interval may round to zero, causing duplicate frames in adjacent tiles.
- **Cancellation:** All async methods accept an optional `CancellationToken` via their respective options classes. Cancellation terminates the underlying FFmpeg process and cleans up partial output files where possible.
- **Format support:** Output format is determined by the file extension of `outputPath` (for single and storyboard) or the naming pattern extension (for batch). Formats must be supported by the underlying FFmpeg build; common choices include JPEG, PNG, and WebP.
