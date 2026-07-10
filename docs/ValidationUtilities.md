# ValidationUtilities

A utility class providing validation and conversion methods for FFmpeg parameters such as bitrate, codec, format, resolution, and time values. It also exposes methods to retrieve supported codecs and formats from the underlying FFmpeg installation.

## API

### `public static bool IsValidBitrate(string bitrate)`

Determines whether the provided bitrate string is valid for FFmpeg. The bitrate must follow FFmpeg's format (e.g., "1000k", "2.5M").

- **Parameters**
  - `bitrate`: The bitrate string to validate.
- **Returns**
  - `true` if the bitrate is valid; otherwise, `false`.
- **Throws**
  - Does not throw exceptions.

---

### `public static bool IsValidCodec(string codec)`

Checks if the specified codec is supported by the installed FFmpeg version.

- **Parameters**
  - `codec`: The codec identifier (e.g., "libx264", "aac").
- **Returns**
  - `true` if the codec is supported; otherwise, `false`.
- **Throws**
  - Does not throw exceptions.

---

### `public static bool IsValidOutputFormat(string format)`

Validates whether the given format is supported for output by FFmpeg.

- **Parameters**
  - `format`: The output format identifier (e.g., "mp4", "mov").
- **Returns**
  - `true` if the format is supported; otherwise, `false`.
- **Throws**
  - Does not throw exceptions.

---
### `public static bool IsVideoFile(string filePath)`

Determines whether the specified file path points to a video file based on its extension.

- **Parameters**
  - `filePath`: The path to the file.
- **Returns**
  - `true` if the file has a video file extension; otherwise, `false`.
- **Throws**
  - Does not throw exceptions.

---
### `public static double? ParseTimeToSeconds(string time)`

Parses a time string in FFmpeg format (e.g., "00:01:23.456") into total seconds. Returns `null` if parsing fails.

- **Parameters**
  - `time`: The time string to parse.
- **Returns**
  - The total seconds as a `double`, or `null` if parsing fails.
- **Throws**
  - Does not throw exceptions.

---
### `public static string FormatSecondsToTime(double seconds)`

Converts a duration in seconds into an FFmpeg-compatible time string (HH:MM:SS.mmm).

- **Parameters**
  - `seconds`: The duration in seconds.
- **Returns**
  - A formatted time string.
- **Throws**
  - Does not throw exceptions.

---
### `public static bool IsValidQualitySetting(int quality)`

Validates whether the provided quality value is within the acceptable range for FFmpeg encoders.

- **Parameters**
  - `quality`: The quality value to validate.
- **Returns**
  - `true` if the quality is valid; otherwise, `false`.
- **Throws**
  - Does not throw exceptions.

---
### `public static bool IsValidWatermarkPosition(string position)`

Checks if the watermark position string is valid (e.g., "top-left", "center", "bottom-right").

- **Parameters**
  - `position`: The position identifier.
- **Returns**
  - `true` if the position is valid; otherwise, `false`.
- **Throws**
  - Does not throw exceptions.

---
### `public static bool IsValidWatermarkScale(double scale)`

Validates whether the watermark scale value is within the acceptable range (typically between 0.0 and 1.0).

- **Parameters**
  - `scale`: The scale value to validate.
- **Returns**
  - `true` if the scale is valid; otherwise, `false`.
- **Throws**
  - Does not throw exceptions.

---
### `public static bool IsValidOpacity(int opacity)`

Checks if the opacity value is within the valid range (typically 0 to 100).

- **Parameters**
  - `opacity`: The opacity value (0–100).
- **Returns**
  - `true` if the opacity is valid; otherwise, `false`.
- **Throws**
  - Does not throw exceptions.

---
### `public static bool IsValidResolution(string resolution)`

Validates whether the resolution string is in a supported format (e.g., "1920x1080", "1280x720").

- **Parameters**
  - `resolution`: The resolution string to validate.
- **Returns**
  - `true` if the resolution is valid; otherwise, `false`.
- **Throws**
  - Does not throw exceptions.

---
### `public static bool IsValidFrameRate(double frameRate)`

Checks if the provided frame rate is within a reasonable range for video (e.g., 15.0 to 120.0).

- **Parameters**
  - `frameRate`: The frame rate to validate.
- **Returns**
  - `true` if the frame rate is valid; otherwise, `false`.
- **Throws**
  - Does not throw exceptions.

---
### `public static bool IsValidAspectRatio(string aspectRatio)`

Validates whether the aspect ratio string is in a supported format (e.g., "16:9", "4:3").

- **Parameters**
  - `aspectRatio`: The aspect ratio string to validate.
- **Returns**
  - `true` if the aspect ratio is valid; otherwise, `false`.
- **Throws**
  - Does not throw exceptions.

---
### `public static bool ValidateTrimTimes(double startSeconds, double endSeconds)`

Checks if the start and end times for a trim operation are valid (i.e., `startSeconds` ≤ `endSeconds` and both are non-negative).

- **Parameters**
  - `startSeconds`: The start time in seconds.
  - `endSeconds`: The end time in seconds.
- **Returns**
  - `true` if the times are valid; otherwise, `false`.
- **Throws**
  - Does not throw exceptions.

---
### `public static IEnumerable<string> GetSupportedCodecs()`

Returns a collection of codec identifiers supported by the installed FFmpeg version.

- **Returns**
  - An `IEnumerable<string>` of supported codec names.
- **Throws**
  - Does not throw exceptions.

---
### `public static IEnumerable<string> GetSupportedFormats()`

Returns a collection of output format identifiers supported by the installed FFmpeg version.

- **Returns**
  - An `IEnumerable<string>` of supported format names.
- **Throws**
  - Does not throw exceptions.

## Usage
