# TrimSettings

Configuration object used to control trimming behavior when processing media files with FFmpeg. It allows specifying temporal boundaries, preserving or dropping audio/video streams, and enforcing keyframe alignment during trimming operations.

## API

### `TimeSpan? EndTime`
Gets or sets the end time of the trimmed segment. If `null`, the end of the source media will be used. When set to a value earlier than `StartTime`, `Validate` will throw an `ArgumentException`.

### `bool PreserveAudio`
Gets or sets a value indicating whether the audio stream should be preserved during trimming. If `false`, the audio stream will be excluded from the output. Defaults to `true`.

### `bool PreserveVideo`
Gets or sets a value indicating whether the video stream should be preserved during trimming. If `false`, the video stream will be excluded from the output. Defaults to `true`.

### `bool Keyframe`
Gets or sets a value indicating whether trimming must align to keyframes. When `true`, the actual end time may be adjusted forward to the nearest keyframe. Defaults to `false`.

### `void Validate()`
Validates the current settings. Throws an `ArgumentException` if:
- `EndTime` is set and is earlier than `StartTime`.
- `EndTime` is set and `Keyframe` is `true`, but no keyframe exists between `StartTime` and `EndTime`.

### `TimeSpan CalculateEndTime(TimeSpan sourceDuration)`
Calculates the effective end time based on the current settings and the source media duration.

- **Parameters**
  - `sourceDuration` – The total duration of the source media.
- **Returns**
  - The resolved end time, adjusted for keyframe alignment if required.
- **Throws**
  - `ArgumentException` if `EndTime` is set and earlier than `StartTime`.

### `TimeSpan GetTrimmedDuration(TimeSpan sourceDuration)`
Computes the duration of the trimmed segment.

- **Parameters**
  - `sourceDuration` – The total duration of the source media.
- **Returns**
  - The duration of the trimmed segment, accounting for `StartTime`, `EndTime`, and keyframe alignment.

### `TrimSettings Clone()`
Creates a deep copy of the current `TrimSettings` instance.

- **Returns**
  - A new `TrimSettings` with identical property values.

## Usage

### Basic trimming with audio and video preserved
