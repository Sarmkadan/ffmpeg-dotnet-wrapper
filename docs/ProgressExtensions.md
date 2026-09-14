# ProgressExtensions

Extension methods for formatting and duration calculation helpers for FFmpeg progress updates.

## API

### `ToConsoleString`

Creates a progress bar containing a number of equals-sign characters determined by the update's progress percentage converted to an integer.

- **Parameters**: None.
- **Return value**: A string of equals-sign characters sized by the integer value of `FFmpegProgressUpdate.ProgressPercentage`.
- **Exceptions**: Throws `ArgumentNullException` if `update` is null.

### `PercentComplete`

Calculates one percent of the total duration using its total milliseconds.

- **Parameters**: 
  - `totalDuration`: The total duration from which to calculate one percent.
- **Return value**: A time span equal to one percent of `totalDuration`.
- **Exceptions**: None.

### `EstimatedTimeRemaining`

Calculates ninety-nine percent of the total duration using its total milliseconds.

- **Parameters**: 
  - `totalDuration`: The total duration from which to calculate ninety-nine percent.
- **Return value**: A time span equal to ninety-nine percent of `totalDuration`.
- **Exceptions**: None.

## Usage