# WatermarkSettings

Configuration class for watermark overlay settings in FFmpeg video processing. Defines position, timing, scaling, and animation properties for watermark placement on video frames.

## API

### `public WatermarkPosition Position`
Gets or sets the base position of the watermark on the video frame. Defaults to `WatermarkPosition.Center`.

### `public int? XOffset`
Gets or sets the horizontal offset in pixels from the base position. Positive values move right, negative values move left. When `null`, no offset is applied.

### `public int? YOffset`
Gets or sets the vertical offset in pixels from the base position. Positive values move down, negative values move up. When `null`, no offset is applied.

### `public double? Scale`
Gets or sets the scaling factor for the watermark. Values less than 1.0 reduce size, values greater than 1.0 increase size. When `null`, the watermark is rendered at native size.

### `public bool PreserveAspectRatio`
Gets or sets whether to preserve the original aspect ratio of the watermark when scaling. When `true`, the watermark is scaled uniformly to fit within the target dimensions without distortion. Defaults to `true`.

### `public TimeSpan? StartTime`
Gets or sets the time when the watermark should first appear in the video. When `null`, the watermark is visible from the start of the video.

### `public TimeSpan? Duration`
Gets or sets how long the watermark should remain visible in the video. When `null`, the watermark remains visible for the entire duration of the video.

### `public bool AnimateIn`
Gets or sets whether the watermark should fade in when it first appears. When `true`, the watermark transitions from transparent to fully opaque over the duration specified by `AnimateInDuration`. Defaults to `false`.

### `public TimeSpan? AnimateInDuration`
Gets or sets the duration of the fade-in animation when `AnimateIn` is `true`. When `null` and `AnimateIn` is `true`, a default duration of 500 milliseconds is used.

### `public void Validate()`
Validates the current configuration. Throws `InvalidOperationException` if any of the following conditions are violated:
- `XOffset` is not `null` and is negative when `Position` is `TopLeft`, `TopRight`, `BottomLeft`, or `BottomRight`.
- `YOffset` is not `null` and is negative when `Position` is `TopLeft`, `TopCenter`, or `TopRight`.
- `Scale` is not `null` and is less than or equal to 0.
- `AnimateInDuration` is not `null` and is less than or equal to `TimeSpan.Zero`.
- `StartTime` is not `null` and is negative.
- `Duration` is not `null` and is less than or equal to `TimeSpan.Zero`.

### `public (int X, int Y) CalculatePosition(int videoWidth, int videoHeight)`
Calculates the absolute position of the watermark on a video frame of the given dimensions.

**Parameters:**
- `videoWidth`: The width of the video frame in pixels.
- `videoHeight`: The height of the video frame in pixels.

**Returns:**
A tuple `(X, Y)` representing the top-left corner coordinates where the watermark should be placed.

**Throws:**
`ArgumentOutOfRangeException` if `videoWidth` or `videoHeight` is less than or equal to 0.

### `public WatermarkSettings Clone()`
Creates a deep copy of the current `WatermarkSettings` instance.

**Returns:**
A new `WatermarkSettings` instance with all properties copied from the original.

## Usage

### Basic Watermark Placement
