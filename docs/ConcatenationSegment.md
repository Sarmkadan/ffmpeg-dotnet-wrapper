# ConcatenationSegment

`ConcatenationSegment` represents a single media segment within a concatenation operation in the `ffmpeg-dotnet-wrapper` library. It defines the source file, optional trimming parameters, and metadata for a segment that will be combined with others to form a merged output. This type is used in conjunction with `ConcatenationBuilder` to construct and configure a sequence of media segments for concatenation, re-encoding, or transcoding.

## API

### `public string FilePath`
The filesystem path to the source media file for this segment.
- **Purpose**: Specifies the input file to be included in the concatenation.
- **Throws**:
  - `ArgumentNullException` if set to `null`.
  - `FileNotFoundException` if the file does not exist when validated during the build process.

### `public TimeSpan? TrimStart`
The starting point from which to trim the segment. If `null`, no trimming is applied at the start.
- **Purpose**: Defines the beginning of the segment's active duration, relative to the start of the source file.
- **Throws**:
  - `ArgumentOutOfRangeException` if the value is negative or exceeds the source file's duration when validated.

### `public TimeSpan? TrimEnd`
The ending point up to which to trim the segment. If `null`, no trimming is applied at the end.
- **Purpose**: Defines the end of the segment's active duration, relative to the start of the source file.
- **Throws**:
  - `ArgumentOutOfRangeException` if the value is negative, less than `TrimStart`, or exceeds the source file's duration when validated.

### `public TimeSpan? TrimDuration`
The duration of the segment. If `null`, the segment's duration is determined by `TrimStart` and `TrimEnd`.
- **Purpose**: Alternative to `TrimEnd` for specifying the segment's duration directly. If both `TrimDuration` and `TrimEnd` are provided, `TrimDuration` takes precedence.
- **Throws**:
  - `ArgumentOutOfRangeException` if the value is negative or exceeds the remaining duration of the source file when validated.

### `public string Label`
An optional human-readable identifier for the segment.
- **Purpose**: Used for logging, debugging, or user-facing output to distinguish segments. Does not affect processing.

### `public ConcatenationSegment()`
Initializes a new instance of `ConcatenationSegment` with default values.
- **Purpose**: Constructs an empty segment that must be configured before use.

### `public ConcatenationBuilder Add(ConcatenationSegment segment)`
Adds a new segment to the end of the concatenation sequence.
- **Parameters**:
  - `segment`: The `ConcatenationSegment` to append.
- **Returns**: The `ConcatenationBuilder` instance for method chaining.
- **Throws**:
  - `ArgumentNullException` if `segment` is `null`.

### `public ConcatenationBuilder Add(string filePath)`
Creates and appends a new segment with the specified file path.
- **Parameters**:
  - `filePath`: The path to the source media file.
- **Returns**: The `ConcatenationBuilder` instance for method chaining.
- **Throws**:
  - `ArgumentNullException` if `filePath` is `null`.
  - `FileNotFoundException` if the file does not exist.

### `public ConcatenationBuilder Insert(int index, ConcatenationSegment segment)`
Inserts a segment at the specified position in the sequence.
- **Parameters**:
  - `index`: The zero-based position at which to insert the segment.
  - `segment`: The `ConcatenationSegment` to insert.
- **Returns**: The `ConcatenationBuilder` instance for method chaining.
- **Throws**:
  - `ArgumentNullException` if `segment` is `null`.
  - `ArgumentOutOfRangeException` if `index` is negative or exceeds the current segment count.

### `public ConcatenationBuilder Remove(int index)`
Removes the segment at the specified position.
- **Parameters**:
  - `index`: The zero-based position of the segment to remove.
- **Returns**: The `ConcatenationBuilder` instance for method chaining.
- **Throws**:
  - `ArgumentOutOfRangeException` if `index` is negative or exceeds the current segment count.

### `public ConcatenationBuilder WithTransition(TimeSpan duration, string filter = null)`
Applies a transition effect between this segment and the next one.
- **Parameters**:
  - `duration`: The duration of the transition.
  - `filter`: Optional FFmpeg filter string to customize the transition (e.g., `"xfade=transition=fade"`). If `null`, a default fade transition is used.
- **Returns**: The `ConcatenationBuilder` instance for method chaining.
- **Throws**:
  - `ArgumentOutOfRangeException` if `duration` is negative or exceeds the segment's duration.
  - `InvalidOperationException` if called on the last segment in the sequence.

### `public ConcatenationBuilder WithReencode(bool reencode = true)`
Enables or disables re-encoding for this segment.
- **Parameters**:
  - `reencode`: If `true`, forces re-encoding of the segment; if `false`, attempts stream copy where possible.
- **Returns**: The `ConcatenationBuilder` instance for method chaining.
- **Purpose**: Overrides the default behavior (stream copy) to ensure consistent output formats or apply filters.

### `public ConcatenationBuilder WithTranscodeSettings(Action<TranscodeSettings> configure)`
Configures transcoding settings for this segment.
- **Parameters**:
  - `configure`: A delegate that modifies the `TranscodeSettings` instance for this segment.
- **Returns**: The `ConcatenationBuilder` instance for method chaining.
- **Throws**:
  - `ArgumentNullException` if `configure` is `null`.

### `public MergeSettings Build()`
Finalizes the concatenation configuration and returns the `MergeSettings` object.
- **Returns**: A `MergeSettings` instance representing the configured concatenation operation.
- **Throws**:
  - `InvalidOperationException` if no segments are added or if required fields (e.g., `FilePath`) are invalid.

### `public ConcatenationBuilder Reset()`
Clears all segments and resets the builder to its initial state.
- **Returns**: The `ConcatenationBuilder` instance for method chaining.

## Usage

### Example 1: Simple Concatenation with Transitions
