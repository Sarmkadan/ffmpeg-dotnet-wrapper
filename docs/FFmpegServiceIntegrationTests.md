# FFmpegServiceIntegrationTests

Integration test suite for the `FFmpegService` class, validating end-to-end workflows and error handling scenarios using real FFmpeg binaries. Tests cover core operations such as transcoding, trimming, merging, watermarking, batch processing, and configuration combinations, ensuring correct behavior across different codecs, containers, hardware accelerators, and quality presets.

## API

### `FFmpegServiceIntegrationTests`
Test fixture class containing integration tests for FFmpeg operations. Inherits from `Xunit.IAsyncLifetime` to initialize and clean up test resources. All test methods are marked as `Fact` or `Theory` and run asynchronously to support cancellation and parallel execution.

### `Task TranscodeWorkflow_BasicMP4ToWebM_ExecutesSuccessfully()`
Validates that a basic transcoding operation from MP4 to WebM completes successfully. No parameters. Returns a `Task` that completes when the operation finishes. Throws if FFmpeg execution fails or output validation checks do not pass.

### `Task TranscodeWorkflow_WithHardwareAcceleration_UsesSpecifiedAccelerator()`
Ensures that hardware acceleration is correctly applied during transcoding when specified. No parameters. Returns a `Task`. Throws if the specified accelerator is not detected or if the output does not reflect hardware-accelerated encoding.

### `Task TranscodeWorkflow_WithAudioNormalization_IncludesNormalizationSettings()`
Confirms that audio normalization filters are applied during transcoding when enabled. No parameters. Returns a `Task`. Throws if normalization settings are not present in the FFmpeg command or if the output audio levels are outside expected bounds.

### `Task TrimWorkflow_TrimClipFromVideo_ExecutesSuccessfully()`
Tests successful trimming of a video clip from a source file. No parameters. Returns a `Task`. Throws if the trimmed output duration does not match the requested range or if the operation fails.

### `Task TrimWorkflow_PreserveOnlyAudio_ExecutesSuccessfully()`
Validates trimming behavior when preserving only the audio stream. No parameters. Returns a `Task`. Throws if the output file contains video data or if the audio stream is missing or corrupted.

### `Task TrimWorkflow_MultipleTrimsOnSameSource_ExecutesIndependently()`
Ensures that multiple independent trim operations on the same source file do not interfere with each other. No parameters. Returns a `Task`. Throws if any trim output is incorrect or if file access conflicts occur.

### `Task MergeWorkflow_ConcatenateTwoVideos_ExecutesSuccessfully()`
Tests the concatenation of two video files into a single output. No parameters. Returns a `Task`. Throws if the merged output duration does not equal the sum of input durations or if synchronization artifacts are present.

### `Task MergeWorkflow_MergeMultipleVideos_ExecutesSuccessfully()`
Validates merging of more than two video files into a single output. No parameters. Returns a `Task`. Throws if the final output is missing segments, has timing discontinuities, or fails to encode.

### `Task MergeWorkflow_MergeWithCrossfade_ConfiguresTransition()`
Confirms that crossfade transitions are correctly configured during merge operations. No parameters. Returns a `Task`. Throws if the crossfade duration is incorrect or if visual artifacts appear at transition points.

### `Task MergeWorkflow_MergeWithTranscode_AppliesEncodingSettings()`
Ensures that transcoding settings are applied during merge operations when required. No parameters. Returns a `Task`. Throws if the output codec or container does not match the specified settings.

### `Task WatermarkWorkflow_AddWatermarkToVideo_ExecutesSuccessfully()`
Tests successful addition of a watermark to a video file. No parameters. Returns a `Task`. Throws if the watermark is not visible, mispositioned, or if the output file is corrupted.

### `Task WatermarkWorkflow_WatermarkAtDifferentPositions_CalculatesCorrectly()`
Validates watermark positioning logic across multiple predefined locations (e.g., top-left, center, bottom-right). No parameters. Returns a `Task`. Throws if the calculated coordinates do not match expected values or if the watermark is clipped.

### `Task BatchWorkflow_TranscodeMultipleFilesInParallel_ExecutesConcurrently()`
Ensures that multiple transcoding tasks can run in parallel without resource contention. No parameters. Returns a `Task`. Throws if any individual task fails or if outputs are corrupted due to race conditions.

### `Task BatchWorkflow_TrimMultipleClipsFromSingleSource_ExecutesIndependently()`
Confirms that multiple independent trim operations on the same source file execute safely without file access conflicts. No parameters. Returns a `Task`. Throws if any output is incorrect or if file locks prevent completion.

### `Task ErrorHandling_TranscodeWithInvalidInput_ReturnsFailureResult()`
Validates that invalid input files result in a failed operation with appropriate error information. No parameters. Returns a `Task`. Throws only if the test logic itself fails; expected behavior is a non-throwing test with assertions on failure results.

### `Task ErrorHandling_CancellationToken_CancelsPendingOperation()`
Ensures that long-running operations can be canceled via `CancellationToken` and terminate cleanly. No parameters. Returns a `Task`. Throws if cancellation does not occur within a reasonable time or if resources are not released.

### `Task ConfigurationCombinations_MultipleValidCodecContainerCombinations_ExecuteSuccessfully()`
Tests various valid combinations of codecs and containers to ensure compatibility. No parameters. Returns a `Task`. Throws if any combination fails to encode or if output format is invalid.

### `Task ConfigurationCombinations_DifferentQualityPresets_ExecuteSuccessfully()`
Validates that different quality presets (e.g., ultrafast, medium, slow) produce outputs with expected characteristics. No parameters. Returns a `Task`. Throws if preset application is incorrect or if output quality does not meet baseline expectations.

### `Task FFmpegUtilities_CheckFFmpegAvailable_ReturnsAvailability()`
Checks whether the FFmpeg binary is available and executable on the system. No parameters. Returns a `Task`. Throws only if the availability check mechanism fails; expected behavior is a passing test when FFmpeg is found and a failing test otherwise.

## Usage

### Example 1: Basic Transcoding
