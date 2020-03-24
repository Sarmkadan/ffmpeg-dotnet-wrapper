# FormattingUtilities

A collection of static helper methods for formatting common multimedia-related values such as durations, byte sizes, bitrates, timestamps, and command-line strings. Designed to standardize presentation across ffmpeg-dotnet-wrapper consumers.

## API

### `public static string FormatDuration(TimeSpan duration)`

Formats a `TimeSpan` into a human-readable duration string (e.g., `00:02:30.123`). Uses the format `HH:mm:ss.fff` with optional zero-padding.

- **Parameters**
  - `duration`: The time span to format.
- **Return value**
  - A string representation of the duration.
- **Exceptions**
  - Throws `ArgumentNullException` if `duration` is `null`.

---

### `public static string FormatBytes(long bytes)`

Formats a byte count into a human-readable string with appropriate unit (B, KB, MB, GB, TB).

- **Parameters**
  - `bytes`: The number of bytes to format.
- **Return value**
  - A string with the value and unit (e.g., `1.23 MB`).
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `bytes` is negative.

---

### `public static string FormatBitrate(double bitrate)`

Formats a bitrate in bits per second into a human-readable string with appropriate unit (bps, kbps, Mbps, Gbps).

- **Parameters**
  - `bitrate`: The bitrate in bits per second.
- **Return value**
  - A string with the value and unit (e.g., `1.23 Mbps`).
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `bitrate` is negative.

---

### `public static string FormatFFmpegCommand(IEnumerable<string> args)`

Joins a collection of command-line arguments into a single string suitable for display or logging. Handles quoting and escaping of arguments containing spaces or special characters.

- **Parameters**
  - `args`: The sequence of arguments to join.
- **Return value**
  - A single string representing the full command.
- **Exceptions**
  - Throws `ArgumentNullException` if `args` is `null`.
  - Throws `ArgumentException` if any element in `args` is `null`.

---

### `public static string ExtractProgressSummary(string ffmpegOutput)`

Parses an ffmpeg progress output line and extracts a concise summary string (e.g., `frame=1234 fps=30.00 q=28.0 size=12345kB time=00:00:41.23 bitrate=2412.3kbits/s speed=1.20x`).

- **Parameters**
  - `ffmpegOutput`: A line from ffmpeg’s progress output.
- **Return value**
  - A summary string containing key metrics, or `null` if parsing fails.
- **Exceptions**
  - Throws `ArgumentNullException` if `ffmpegOutput` is `null`.

---
### `public static string FormatProgressTime(TimeSpan current, TimeSpan total)`

Formats the current progress time and total duration into a string (e.g., `00:00:41 / 01:30:00`).

- **Parameters**
  - `current`: The current elapsed time.
  - `total`: The total duration.
- **Return value**
  - A formatted progress string.
- **Exceptions**
  - Throws `ArgumentNullException` if `current` or `total` is `null`.
  - Throws `ArgumentOutOfRangeException` if `current` exceeds `total`.

---
### `public static string FormatETA(TimeSpan elapsed, TimeSpan total)`

Estimates and formats the remaining time based on progress (e.g., `ETA: 00:15:30`).

- **Parameters**
  - `elapsed`: The time already processed.
  - `total`: The total duration.
- **Return value**
  - A formatted ETA string, or `"N/A"` if estimation is not possible.
- **Exceptions**
  - Throws `ArgumentNullException` if `elapsed` or `total` is `null`.
  - Throws `ArgumentOutOfRangeException` if `elapsed` exceeds `total` or if `total` is zero.

---
### `public static string FormatTimestamp(TimeSpan time)`

Formats a `TimeSpan` into a timestamp string suitable for ffmpeg (e.g., `00:01:23.456`).

- **Parameters**
  - `time`: The time to format.
- **Return value**
  - A string in `HH:mm:ss.fff` format.
- **Exceptions**
  - Throws `ArgumentNullException` if `time` is `null`.

---
### `public static string FormatResolution(int width, int height)`

Formats a resolution into a string (e.g., `1920x1080`).

- **Parameters**
  - `width`: The frame width.
  - `height`: The frame height.
- **Return value**
  - A resolution string.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `width` or `height` is negative.

---
### `public static string FormatPercentage(double value)`

Formats a percentage value into a string with two decimal places (e.g., `42.37%`).

- **Parameters**
  - `value`: The percentage value (0.0 to 100.0).
- **Return value**
  - A formatted percentage string.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `value` is outside [0.0, 100.0].

---
### `public static string TruncateString(string input, int maxLength)`

Truncates a string to a maximum length, appending an ellipsis (`…`) if truncated.

- **Parameters**
  - `input`: The string to truncate.
  - `maxLength`: The maximum allowed length (must be ≥ 3).
- **Return value**
  - The truncated string, or the original if within limit.
- **Exceptions**
  - Throws `ArgumentNullException` if `input` is `null`.
  - Throws `ArgumentOutOfRangeException` if `maxLength` is less than 3.

---
### `public static string SanitizeForDisplay(string input)`

Removes or escapes control characters and non-printable Unicode from a string for safe display.

- **Parameters**
  - `input`: The string to sanitize.
- **Return value**
  - A sanitized string with control characters removed or escaped.
- **Exceptions**
  - Throws `ArgumentNullException` if `input` is `null`.

---
### `public static string TitleCase(string input)`

Converts a string to title case (e.g., `"hello world"` → `"Hello World"`).

- **Parameters**
  - `input`: The string to convert.
- **Return value**
  - A title-cased string.
- **Exceptions**
  - Throws `ArgumentNullException` if `input` is `null`.

## Usage

```csharp
// Example 1: Formatting progress during encoding
var duration = TimeSpan.FromSeconds(93.2);
var current = TimeSpan.FromSeconds(41.5);
Console.WriteLine(FormattingUtilities.FormatProgressTime(current, duration));
// Output: "00:00:41 / 00:01:33"

var elapsed = TimeSpan.FromSeconds(41.5);
var total = TimeSpan.FromSeconds(93.2);
Console.WriteLine(FormattingUtilities.FormatETA(elapsed, total));
// Output: "ETA: 00:00:51"
```

```csharp
// Example 2: Sanitizing and displaying user-provided metadata
var unsafeTitle = "My Video \u0007with\ttabs";
var safeTitle = FormattingUtilities.SanitizeForDisplay(unsafeTitle);
var displayTitle = FormattingUtilities.TitleCase(safeTitle);
Console.WriteLine(displayTitle);
// Output: "My Video with tabs"
```

## Notes

- All methods are thread-safe for concurrent read access. No internal state is mutated.
- Edge cases such as zero durations, negative sizes, or malformed progress lines are handled by throwing exceptions rather than returning sentinel values.
- `FormatETA` returns `"N/A"` only when estimation is mathematically impossible (e.g., zero total duration), not for transient conditions.
