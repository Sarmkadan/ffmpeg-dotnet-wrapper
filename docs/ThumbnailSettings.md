# ThumbnailSettings

`ThumbnailSettings` encapsulates the parameters required to generate one or more thumbnail images from a video file. It specifies the timestamps at which thumbnails are captured, the output image format, and optional dimensions. Instances are typically passed to a media processing method that extracts frames from a video source.

## API

### `Times`
`public List<TimeSpan> Times`

A list of time offsets from the start of the video where thumbnails should be taken. Each `TimeSpan` value represents a point in the video timeline. The collection may be empty, in which case no thumbnails are produced. The list is mutable; modifications after the settings are used may affect subsequent operations depending on the consumer implementation.

### `Format`
`public ThumbnailFormat Format`

The image format for the generated thumbnails. `ThumbnailFormat` is an enumeration (e.g., `Jpeg`, `Png`, `Bmp`). This property determines the file extension and encoding used when writing thumbnail files.

### `Width`
`public int? Width`

The desired width of the thumbnail in pixels. When `null`, the width is determined automatically from the source video’s aspect ratio and the specified `Height` (if any). If both `Width` and `Height` are `null`, the original video frame dimensions are used.

### `Height`
`public int? Height`

The desired height of the thumbnail in pixels. When `null`, the height is derived from the source video’s aspect ratio and the specified `Width` (if any). If both `Width` and `Height` are `null`, the original video frame dimensions are used.

### `Validate`
`public void Validate()`

Validates the current settings and throws an exception if any property is in an invalid state. The method checks for:
- Negative or zero `Width` or `Height` values (when not `null`).
- Negative `TimeSpan` values in the `Times` list.
- An undefined or unsupported `Format` value.

**Throws:**  
`InvalidOperationException` – if any validation rule is violated.

### `Clone`
`public ThumbnailSettings Clone()`

Creates a deep copy of the current `ThumbnailSettings` instance. The returned object has the same property values as the original, but the `Times` list is a new list containing the same `TimeSpan` elements. Modifications to the clone do not affect the original.

**Returns:**  
A new `ThumbnailSettings` instance with identical state.

## Usage

### Example 1: Basic thumbnail extraction at two timestamps

```csharp
var settings = new ThumbnailSettings
{
    Times = new List<TimeSpan>
    {
        TimeSpan.FromSeconds(10),
        TimeSpan.FromSeconds(30)
    },
    Format = ThumbnailFormat.Jpeg,
    Width = 320,
    Height = null // height will be auto-calculated
};

settings.Validate();

// Pass settings to a media processing method (not shown)
// ProcessVideo(inputFile, outputDirectory, settings);
```

### Example 2: Cloning settings for parallel processing

```csharp
var baseSettings = new ThumbnailSettings
{
    Times = new List<TimeSpan> { TimeSpan.FromSeconds(5) },
    Format = ThumbnailFormat.Png,
    Width = 640,
    Height = 480
};

// Clone for use in a separate thread
var threadSettings = baseSettings.Clone();
threadSettings.Times.Add(TimeSpan.FromSeconds(15));

// baseSettings.Times still contains only one element
```

## Notes

- **Edge cases:**  
  - An empty `Times` list is valid but produces no thumbnails.  
  - Setting both `Width` and `Height` to `null` uses the original video frame size.  
  - Negative `TimeSpan` values cause `Validate()` to throw.  
  - A `Format` value that is not defined in the `ThumbnailFormat` enumeration (e.g., a cast from an invalid integer) will also cause `Validate()` to throw.

- **Thread safety:**  
  `ThumbnailSettings` is not thread-safe for concurrent read/write access. If an instance is shared across threads, external synchronization is required. The `Clone()` method provides a safe way to obtain independent copies for parallel operations. The `Times` list is mutable; modifications after cloning do not affect the original.
