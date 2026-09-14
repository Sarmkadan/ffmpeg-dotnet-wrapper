# FFmpegServiceValidation

`FFmpegServiceValidation` is a static helper class that provides a set of validation utilities for the `IFFmpegService` interface. It validates that FFmpeg service instances are properly initialized and not null. The class is intentionally stateless and thread‑safe, making it suitable for use in both synchronous and asynchronous contexts.

## API

### `public static IReadOnlyList<string> Validate(this IFFmpegService value)`

Validates the supplied `IFFmpegService` instance and returns a read‑only list of error messages.
- **Parameters**:
  - `value`: The FFmpeg service instance to validate.
- **Return value**: A list of validation error strings; an empty list indicates a valid instance.
- **Throws**: `ArgumentNullException` if `value` is `null`.

### `public static bool IsValid(this IFFmpegService value)`

Convenience wrapper that returns `true` when `Validate` yields no errors.
- **Parameters**:
  - `value`: The FFmpeg service instance to test.
- **Return value**: `true` if the instance is valid; otherwise `false`.
- **Throws**: `ArgumentNullException` if `value` is `null`.

### `public static void EnsureValid(this IFFmpegService value)`

Validates the instance and throws an exception if any errors are found.
- **Parameters**:
  - `value`: The FFmpeg service instance to validate.
- **Return value**: None.
- **Throws**: `ArgumentNullException` if `value` is `null`.

## Usage