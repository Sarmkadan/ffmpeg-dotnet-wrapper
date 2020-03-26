# ValidationUtilitiesTests

ValidationUtilitiesTests is a test suite within the ffmpeg-dotnet-wrapper project designed to verify the correctness and reliability of input validation and time formatting utilities used in FFmpeg command generation. These tests ensure that parameters such as bitrates, codecs, output formats, resolutions, and trim times conform to expected constraints and formats before being passed to FFmpeg processes.

## API

### IsValidBitrate_WithinRange_ReturnsTrue
**Purpose:** Validates that a bitrate value within the acceptable range returns true.  
**Parameters:** `int bitrate` (valid range not specified in method name).  
**Return Value:** `void` (asserts method under test returns `true`).  
**Exceptions:** None thrown by this test method; underlying method may throw if invalid type is passed.

### IsValidBitrate_OutsideRange_ReturnsFalse
**Purpose:** Validates that a bitrate value outside the acceptable range returns false.  
**Parameters:** `int bitrate` (out-of-range value).  
**Return Value:** `void` (asserts method under test returns `false`).  
**Exceptions:** None thrown by this test method.

### IsValidCodec_SupportedCodec_ReturnsTrue
**Purpose:** Validates that a supported codec string returns true.  
**Parameters:** `string codec` (e.g., "libx264").  
**Return Value:** `void` (asserts method under test returns `true`).  
**Exceptions:** None thrown by this test method.

### IsValidCodec_UnsupportedOrEmpty_ReturnsFalse
**Purpose:** Validates that an unsupported or empty codec string returns false.  
**Parameters:** `string codec` (empty or unsupported codec).  
**Return Value:** `void` (asserts method under test returns `false`).  
**Exceptions:** None thrown by this test method.

### IsValidOutputFormat_SupportedFormat_ReturnsTrue
**Purpose:** Validates that a supported output format string returns true.  
**Parameters:** `string format` (e.g., "mp4").  
**Return Value:** `void` (asserts method under test returns `true`).  
**Exceptions:** None thrown by this test method.

### IsValidOutputFormat_UnrecognizedFormat_ReturnsFalse
**Purpose:** Validates that an unrecognized output format string returns false.  
**Parameters:** `string format` (unrecognized format).  
**Return Value:** `void` (asserts method under test returns `false`).  
**Exceptions:** None thrown by this test method.

### ParseTimeToSeconds_HhMmSsFormat_ReturnsCorrectSeconds
**Purpose:** Validates parsing of time strings in HH:MM:SS format to total seconds.  
**Parameters:** `string time` (e.g., "01:30:45").  
**Return Value:** `void` (asserts method under test returns correct seconds).  
**Exceptions:** None thrown by this test method.

### ParseTimeToSeconds_PureSecondsString_ReturnsValue
**Purpose:** Validates parsing of pure seconds strings (e.g., "120") to integer seconds.  
**Parameters:** `string time` (e.g., "120").  
**Return Value:** `void` (asserts method under test returns correct seconds).  
**Exceptions:** None thrown by this test method.

### ParseTimeToSeconds_InvalidOrEmpty_ReturnsNull
**Purpose:** Validates that invalid or empty time strings return null.  
**Parameters:** `string time` (invalid or empty).  
**Return Value:** `void` (asserts method under test returns `null`).  
**Exceptions:** None thrown by this test method.

### FormatSecondsToTime_VariousValues_ReturnsHhMmSs
**Purpose:** Validates formatting of seconds to HH:MM:SS string format.  
**Parameters:** `int seconds` (various valid values).  
**Return Value:** `void` (asserts method under test returns correct HH:MM:SS string).  
**Exceptions:** None thrown by this test method.

### FormatSecondsToTime_NegativeSeconds_ClampsToZero
**Purpose:** Validates that negative seconds are clamped to zero before formatting.  
**Parameters:** `int seconds` (negative value).  
**Return Value:** `void` (asserts method under test returns "00:00:00").  
**Exceptions:** None thrown by this test method.

### IsValidResolution_ValidFormat_ReturnsTrue
**Purpose:** Validates that a resolution string in WxH format (e.g., "1920x1080") returns true.  
**Parameters:** `string resolution` (valid format).  
**Return Value:** `void` (asserts method under test returns `true`).  
**Exceptions:** None thrown by this test method.

### IsValidResolution_InvalidFormat_ReturnsFalse
**Purpose:** Validates that an invalid resolution string (e.g., "invalid") returns false.  
**Parameters:** `string resolution` (invalid format).  
**Return Value:** `void` (asserts method under test returns `false`).  
**Exceptions:** None thrown by this test method.

### ValidateTrimTimes_StartBeforeEnd_ReturnsTrue
**Purpose:** Validates that trim times where start is before end return true.  
**Parameters:** `string start`, `string end` (valid time strings with start < end).  
**Return Value:** `void` (asserts method under test returns `true`).  
**Exceptions:** None thrown by this test method.

### ValidateTrimTimes_StartGreaterThanEnd_ReturnsFalse
**Purpose:** Validates that trim times where start is after end return false.  
**Parameters:** `string start`, `string end` (invalid time strings with start > end).  
**Return Value:** `void` (asserts method under test returns `false`).  
**Exceptions:** None thrown by this test method.

### ValidateTrimTimes_NegativeStart_ReturnsFalse
**Purpose:** Validates that a negative start time returns false.  
**Parameters:** `string start` (negative time string).  
**Return Value:** `void` (asserts method under test returns `false`).  
**Exceptions:** None thrown by this test method.

### ValidateTrimTimes_WithDurationOnly_ReturnsTrue
**Purpose:** Validates that trim times with only a duration specified return true.  
**Parameters:** `string duration` (valid time string).  
**Return Value:** `void` (asserts method under test returns `true`).  
**Exceptions:** None thrown by this test method.

### ValidateTrimTimes_NoEndOrDuration_ReturnsFalse
**Purpose:** Validates that trim times without end or duration return false.  
**Parameters:** `string start` (no end or duration provided).  
**Return Value:** `void` (asserts method under test returns `false`).  
**Exceptions:** None thrown by this test method.

### IsValidWatermarkScale_ValidRange_ReturnsTrue
**Purpose:** Validates that a watermark scale value within the valid range returns true.  
**Parameters:** `double scale` (valid range not specified in method name).  
**Return Value:** `void` (asserts method under test returns `true`).  
**Exceptions:** None thrown by this test method.

### IsValidWatermarkScale_OutsideRange_ReturnsFalse
**Purpose:** Validates that a watermark scale value outside the valid range returns false.  
**Parameters:** `double scale` (out-of-range value).  
**Return Value:** `void` (asserts method under test returns `false`).  
**Exceptions:** None thrown by this test method.

## Usage

```csharp
// Example 1: Validating a bitrate and codec before generating an FFmpeg command
var bitrate = 5000;
var codec = "libx264";

if (!ValidationUtilities.IsValidBitrate(bitrate))
{
    throw new ArgumentException("Invalid bitrate value.");
}

if (!ValidationUtilities.IsValidCodec(codec))
{
    throw new ArgumentException("Unsupported codec.");
}

// Proceed with FFmpeg command generation using validated parameters
var arguments = $"-b:v {bitrate}k -c:v {codec}";
```

```csharp
// Example 2: Parsing and formatting time values for trimming
var startTime = "00:01:30";
var endTime = "00:05:00";

var startSeconds = ValidationUtilities.ParseTimeToSeconds(startTime);
var endSeconds = ValidationUtilities.ParseTimeToSeconds(endTime);

if (startSeconds == null || endSeconds == null || startSeconds >= endSeconds)
{
    throw new ArgumentException("Invalid trim times.");
}

var formattedStart = ValidationUtilities.FormatSecondsToTime((int)startSeconds);
var formattedEnd = ValidationUtilities.FormatSecondsToTime((int)endSeconds);

// Use formatted times in FFmpeg trim command
var trimArgs = $"-ss {formattedStart} -to {formattedEnd}";
```

## Notes

- **Edge Cases:**  
  - `ParseTimeToSeconds` returns `null` for malformed or empty strings, requiring null checks in consuming code.  
  - `FormatSecondsToTime` clamps negative values to zero, ensuring non-negative output.  
  - `ValidateTrimTimes` enforces strict ordering (start must be before end) and rejects negative start times.  
  - `IsValidWatermarkScale` likely enforces a range (e.g., 0.0–1.0), but exact bounds are not specified in test names.  

- **Thread Safety:**  
  - The test class itself is not thread-safe, as it is designed for sequential execution in a test runner.  
  - The underlying validation and time formatting methods are assumed to be stateless and thread-safe if implemented as static methods without shared mutable state.
