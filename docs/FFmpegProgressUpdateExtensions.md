# FFmpegProgressUpdateExtensions

Extension methods for `FFmpegProgressUpdate` that provide computed progress information and estimated completion metrics for FFmpeg operations.

## API

### `GetRemainingDuration`

Calculates the estimated remaining duration of the FFmpeg operation based on the current progress and elapsed time.

- **Parameters**: None.
- **Return value**: `TimeSpan` representing the estimated remaining time until completion.
- **Exceptions**: Throws `InvalidOperationException` if the current progress is zero or the elapsed time cannot be determined.

### `IsCompleted`

Determines whether the FFmpeg operation has completed based on the progress update.

- **Parameters**: None.
- **Return value**: `bool` indicating whether the operation is complete (`progress >= 100`).
- **Exceptions**: None.

### `GetFormattedPercentage`

Returns the current progress as a formatted percentage string (e.g., "75%").

- **Parameters**: None.
- **Return value**: `string` representing the progress as a percentage with a "%" suffix.
- **Exceptions**: None.

### `GetEstimatedCompletionTime`

Computes the estimated time of completion based on the current progress and elapsed time.

- **Parameters**: None.
- **Return value**: `DateTime` representing the estimated completion time.
- **Exceptions**: Throws `InvalidOperationException` if the current progress is zero or the elapsed time cannot be determined.

## Usage
