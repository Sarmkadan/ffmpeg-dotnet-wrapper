# ThumbnailSettingsTests

Unit test class for verifying the behavior and constraints of the `ThumbnailSettings` type in the `ffmpeg-dotnet-wrapper` project. This class exercises validation rules, property setters, cloning, and edge cases to ensure correct operation of thumbnail generation configuration.

## API

### `public ThumbnailSettingsTests()`
Constructor for the test class. Initializes a new instance of the test fixture.

### `public void Dispose()`
Disposes of resources used by the test class. Typically used to clean up any unmanaged resources or test context.

### `public void Constructor_DefaultValues_AreCorrect()`
Verifies that a newly constructed `ThumbnailSettings` instance has default values as expected. Ensures predictable initialization behavior.

### `public void Count_WithValidValue_AcceptsValue(int value)`
Tests that setting the `Count` property to a valid positive integer is accepted without throwing an exception. Valid values are typically between 1 and 10.

**Parameters:**
- `value`: A valid integer within the acceptable range for thumbnail count.

### `public void Count_OutsideValidRange_ThrowsException(int value)`
Ensures that setting the `Count` property to a value outside the valid range (e.g., zero or negative, or above maximum) throws an appropriate exception.

**Parameters:**
- `value`: An invalid integer outside the acceptable range for thumbnail count.

### `public void JpegQuality_WithValidValue_AcceptsValue(int value)`
Tests that setting the `JpegQuality` property to a valid integer between 1 and 100 is accepted without throwing an exception.

**Parameters:**
- `value`: A valid JPEG quality value in the range [1, 100].

### `public void JpegQuality_OutsideValidRange_ThrowsException(int value)`
Ensures that setting the `JpegQuality` property to a value outside the valid range (less than 1 or greater than 100) throws an appropriate exception.

**Parameters:**
- `value`: An invalid JPEG quality value outside the acceptable range.

### `public void Validate_WithTimestampBeyondDuration_ThrowsException(TimeSpan timestamp, TimeSpan duration)`
Validates that a timestamp exceeding the media duration results in an exception during validation.

**Parameters:**
- `timestamp`: The timestamp to validate, which exceeds `duration`.
- `duration`: The total duration of the media.

### `public void Validate_WithNegativeTimestamp_ThrowsException(TimeSpan timestamp)`
Ensures that a negative timestamp causes validation to fail with an exception.

**Parameters:**
- `timestamp`: A negative timestamp value.

### `public void Validate_WithValidExplicitTimestamps_DoesNotThrow(TimeSpan[] timestamps)`
Confirms that providing an array of valid timestamps (within bounds) does not cause validation to throw an exception.

**Parameters:**
- `timestamps`: An array of valid `TimeSpan` values within the media duration.

### `public void Validate_WithInvalidWidth_ThrowsException(int width)`
Tests that an invalid width (e.g., zero or negative) causes validation to fail with an exception.

**Parameters:**
- `width`: An invalid width value.

### `public void Validate_WithAutoWidth_DoesNotThrow()`
Ensures that setting width to auto (e.g., zero or a sentinel value) does not cause validation to throw an exception.

### `public void Clone_ProducesIndependentCopy()`
Verifies that calling `Clone()` on a `ThumbnailSettings` instance produces a deep copy that can be modified independently without affecting the original.

## Usage
