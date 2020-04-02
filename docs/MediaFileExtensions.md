# MediaFileExtensions

Utility class providing extension methods for analyzing media file metadata, particularly for video and audio files processed via FFmpeg. These methods interpret raw FFmpeg output (e.g., duration, resolution, bitrate, codec info) into human-readable or programmatically useful formats such as quality indicators, aspect ratios, and file size estimates.

## API

### `public static bool IsHighDefinition(MediaFile mediaFile)`

Determines whether the media file has a resolution classified as high definition (HD). Resolution thresholds are based on common industry standards (e.g., 720p or higher).

- **Parameters**: `mediaFile` – The media file object containing metadata such as width and height.
- **Return value**: `true` if the resolution is ≥ 1280×720; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `mediaFile` is `null`.

---

### `public static bool Is4K(MediaFile mediaFile)`

Determines whether the media file has a resolution classified as 4K Ultra High Definition.

- **Parameters**: `mediaFile` – The media file object containing metadata such as width and height.
- **Return value**: `true` if the resolution is ≥ 3840×2160; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `mediaFile` is `null`.

---

### `public static string? GetAspectRatio(MediaFile mediaFile)`

Returns the aspect ratio of the video as a formatted string (e.g., "16:9", "4:3").

- **Parameters**: `mediaFile` – The media file object containing width and height.
- **Return value**: A string representing the aspect ratio in the format "W:H", or `null` if width or height is not available.
- **Throws**: `ArgumentNullException` if `mediaFile` is `null`.

---

### `public static string GetFormattedDuration(MediaFile mediaFile)`

Converts the media file’s duration (in seconds) into a human-readable time string (e.g., "02:30:15" for 2 hours, 30 minutes, and 15 seconds).

- **Parameters**: `mediaFile` – The media file object containing duration in seconds.
- **Return value**: A formatted time string in `HH:MM:SS` format. If duration is `null` or negative, returns "00:00:00".
- **Throws**: `ArgumentNullException` if `mediaFile` is `null`.

---

### `public static string GetFormattedFileSize(MediaFile mediaFile)`

Returns the file size in a human-readable format (e.g., "2.45 GB", "1.2 MB").

- **Parameters**: `mediaFile` – The media file object containing file size in bytes.
- **Return value**: A string with size and unit (e.g., "1.5 GB"). Returns "0 B" if size is `null` or ≤ 0.
- **Throws**: `ArgumentNullException` if `mediaFile` is `null`.

---
### `public static string GetVideoQualityDescription(MediaFile mediaFile)`

Generates a descriptive string summarizing video quality based on resolution, bitrate, and codec (e.g., "High (1080p, H.264, 8 Mbps)").

- **Parameters**: `mediaFile` – The media file object containing video metadata.
- **Return value**: A non-empty string describing video quality. Returns "Unknown" if insufficient data is available.
- **Throws**: `ArgumentNullException` if `mediaFile` is `null`.

---
### `public static string GetAudioQualityDescription(MediaFile mediaFile)`

Generates a descriptive string summarizing audio quality based on bitrate, sample rate, and codec (e.g., "High (44.1 kHz, 320 kbps, AAC)").

- **Parameters**: `mediaFile` – The media file object containing audio metadata.
- **Return value**: A non-empty string describing audio quality. Returns "Unknown" if insufficient data is available.
- **Throws**: `ArgumentNullException` if `mediaFile` is `null`.

---
### `public static long? GetFrameCount(MediaFile mediaFile)`

Returns the estimated number of frames in the video, if available.

- **Parameters**: `mediaFile` – The media file object containing frame count or FPS and duration.
- **Return value**: The frame count as a `long`, or `null` if not determinable (e.g., missing FPS or duration).
- **Throws**: `ArgumentNullException` if `mediaFile` is `null`.

---
### `public static long? GetEstimatedFileSize(MediaFile mediaFile)`

Computes an estimated file size based on bitrate and duration, useful when actual file size is unavailable.

- **Parameters**: `mediaFile` – The media file object containing bitrate and duration.
- **Return value**: Estimated file size in bytes, or `null` if bitrate or duration is missing or invalid.
- **Throws**: `ArgumentNullException` if `mediaFile` is `null`.

---
### `public static bool HasHDRMetadata(MediaFile mediaFile)`

Indicates whether the media file includes HDR (High Dynamic Range) metadata.

- **Parameters**: `mediaFile` – The media file object containing HDR-related metadata flags.
- **Return value**: `true` if HDR metadata is detected; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `mediaFile` is `null`.

---
### `public static string GetLocalizedCreationDate(MediaFile mediaFile)`

Returns the media file’s creation date in a localized string format (e.g., "Jan 15, 2023").

- **Parameters**: `mediaFile` – The media file object containing creation timestamp.
- **Return value**: A localized date string, or "Unknown" if timestamp is missing or invalid.
- **Throws**: `ArgumentNullException` if `mediaFile` is `null`.

## Usage
