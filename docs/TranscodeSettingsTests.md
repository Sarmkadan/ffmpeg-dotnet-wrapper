# TranscodeSettingsTests
The `TranscodeSettingsTests` class is designed to test the functionality of the `TranscodeSettings` class, which is part of the `ffmpeg-dotnet-wrapper` project. This class contains a series of test methods that verify the behavior of the `TranscodeSettings` class under various conditions, including valid and invalid input values, different encoding settings, and error handling scenarios.

## API
The `TranscodeSettingsTests` class contains the following public members:
* `Constructor_CreatesDefaultSettings`: Verifies that the default constructor creates a `TranscodeSettings` object with default settings.
* `VideoBitrate_WithValidValue_AcceptsValue`: Tests that setting a valid video bitrate is accepted.
* `VideoBitrate_OutsideValidRange_ThrowsException`: Verifies that setting a video bitrate outside the valid range throws an exception.
* `AudioBitrate_WithValidValue_AcceptsValue`: Tests that setting a valid audio bitrate is accepted.
* `AudioBitrate_OutsideValidRange_ThrowsException`: Verifies that setting an audio bitrate outside the valid range throws an exception.
* `FrameRate_WithValidValue_AcceptsValue`: Tests that setting a valid frame rate is accepted.
* `FrameRate_OutsideValidRange_ThrowsException`: Verifies that setting a frame rate outside the valid range throws an exception.
* `Width_WithPositiveValue_AcceptsValue`: Tests that setting a positive width is accepted.
* `Width_WithZeroOrNegative_ThrowsException`: Verifies that setting a width of zero or a negative value throws an exception.
* `Validate_H264InMP4_IsValid`: Tests that validating H.264 encoding in an MP4 container is successful.
* `Validate_VP9InWebM_IsValid`: Tests that validating VP9 encoding in a WebM container is successful.
* `Validate_H264InWebM_ThrowsException`: Verifies that validating H.264 encoding in a WebM container throws an exception.
* `Validate_AutoScaleWithTooSmallMaxDimensions_ThrowsException`: Tests that validating auto-scaling with too small maximum dimensions throws an exception.
* `Validate_AudioNormalizationWithValidLoudness_IsValid`: Tests that validating audio normalization with a valid loudness value is successful.
* `Validate_AudioNormalizationWithInvalidLoudness_ThrowsException`: Verifies that validating audio normalization with an invalid loudness value throws an exception.
* `Clone_CreatesIndependentCopy`: Tests that cloning a `TranscodeSettings` object creates an independent copy.
* `HardwareAcceleration_SupportsAllValues`: Verifies that hardware acceleration supports all possible values.
* `CustomFFmpegArgs_AllowsArbitraryArguments`: Tests that custom FFmpeg arguments allow arbitrary arguments.
* `TrimSettingsTests`: This appears to be a separate test class, but its presence here may indicate a need for further testing or integration with the `TranscodeSettingsTests` class.

## Usage
Here are two examples of using the `TranscodeSettingsTests` class:
```csharp
// Example 1: Testing video bitrate settings
var settings = new TranscodeSettings();
settings.VideoBitrate = 100000; // Set a valid video bitrate
Assert.IsTrue(settings.VideoBitrate == 100000); // Verify the bitrate was set correctly

// Example 2: Testing audio normalization
var settings2 = new TranscodeSettings();
settings2.AudioNormalization = true;
settings2.Loudness = -20; // Set a valid loudness value
Assert.IsTrue(settings2.Loudness == -20); // Verify the loudness value was set correctly
```

## Notes
When using the `TranscodeSettingsTests` class, keep in mind the following edge cases and thread-safety considerations:
* The `TranscodeSettings` class may throw exceptions when invalid values are set, so it's essential to handle these exceptions properly in your application.
* The `Clone` method creates an independent copy of the `TranscodeSettings` object, which can be useful for creating multiple settings objects with different configurations.
* The `HardwareAcceleration` and `CustomFFmpegArgs` properties support a wide range of values, but it's crucial to ensure that these values are valid and compatible with your specific use case.
* The `TranscodeSettingsTests` class is designed to be thread-safe, but it's still important to follow standard threading best practices when using this class in a multi-threaded environment.
