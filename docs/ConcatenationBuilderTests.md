# ConcatenationBuilderTests

The `ConcatenationBuilderTests` class contains unit tests for the `ConcatenationBuilder` type, which is used to construct media concatenation operations. Each test method validates a specific behavior of the builder, including segment addition, insertion, removal, transition configuration, re-encoding options, build validation, and reset functionality. The class implements `IDisposable` to clean up any temporary resources created during test execution.

## API

### `public ConcatenationBuilderTests()`
Initializes a new instance of the test class. Sets up any required test fixtures or mock objects.

### `public void Dispose()`
Performs cleanup of resources used during the test run. Releases temporary files, directories, or other disposable objects created by the test methods.

### `public void Add_SingleFile_AddsToSegments()`
Verifies that calling `Add` with a single file path results in that file being added to the internal segment list.

### `public void Add_MultipleFiles_PreservesOrder()`
Verifies that adding multiple files in sequence preserves the order in which they were added.

### `public void Add_WithTrimParameters_SetsSegmentProperties()`
Verifies that when a file is added with trim start, end, or duration parameters, the corresponding segment object has those properties set correctly.

### `public void Add_WithBothTrimEndAndDuration_ThrowsException()`
Verifies that adding a file with both a trim end time and a duration throws an `InvalidOperationException` (or similar), as these two parameters are mutually exclusive.

### `public void Add_WithNonexistentFile_ThrowsException()`
Verifies that adding a file path that does not exist on disk throws a `FileNotFoundException`.

### `public void Insert_AtValidIndex_InsertsAtPosition()`
Verifies that inserting a segment at a valid index places it at the expected position in the segment list.

### `public void Remove_RemovesMatchingSegment()`
Verifies that removing a segment that matches a given predicate (e.g., by file path) correctly removes it from the list.

### `public void WithTransition_SetsCrossfade()`
Verifies that calling `WithTransition` with a positive duration sets the crossfade duration on the builder.

### `public void WithTransition_ZeroDuration_ThrowsException()`
Verifies that calling `WithTransition` with a duration of zero throws an `ArgumentException` (or similar), as a transition must have a positive duration.

### `public void WithReencode_SetsTranscodeOnMerge()`
Verifies that calling `WithReencode` sets a flag indicating that the final merge should re-encode the segments rather than using stream copy.

### `public void Build_WithLessThanTwoSegments_ThrowsException()`
Verifies that calling `Build` when fewer than two segments have been added throws an `InvalidOperationException`, because concatenation requires at least two inputs.

### `public void Build_WithTwoSegments_ReturnsValidMergeSettings()`
Verifies that calling `Build` with exactly two segments returns a `MergeSettings` object (or equivalent) that is properly configured.

### `public void Reset_ClearsAllSegmentsAndOptions()`
Verifies that calling `Reset` removes all added segments and resets all configuration options (transition, re-encode, etc.) to their defaults.

### `public void Build_WithCustomTranscodeSettings_PropagatesSettings()`
Verifies that custom transcode settings (e.g., codec, bitrate) provided to the builder are propagated to the resulting merge settings.

### `public void ConcatenationSegment_WithNullPath_ThrowsException()`
Verifies that creating a `ConcatenationSegment` with a null file path throws an `ArgumentNullException`.

### `public void FluentChaining_BuildsCorrectly()`
Verifies that the builder supports fluent method chaining and that a chain of calls (e.g., `Add().WithTransition().WithReencode().Build()`) produces the expected result.

## Usage

The following examples demonstrate typical usage of the `ConcatenationBuilder` class, which is the subject of these tests.

```csharp
// Example 1: Basic concatenation of two files with a crossfade transition
var builder = new ConcatenationBuilder();
builder.Add("intro.mp4");
builder.Add("main.mp4");
builder.WithTransition(TimeSpan.FromSeconds(1.5));
var settings = builder.Build();
// settings now contains the merge configuration with a 1.5-second crossfade
```

```csharp
// Example 2: Fluent chaining with trim parameters and re-encoding
var builder = new ConcatenationBuilder();
var settings = builder
    .Add("clip1.mp4", trimStart: TimeSpan.Zero, trimEnd: TimeSpan.FromSeconds(10))
    .Add("clip2.mp4", trimStart: TimeSpan.FromSeconds(5), duration: TimeSpan.FromSeconds(15))
    .WithReencode()
    .Build();
// The resulting settings will re-encode the concatenated output,
// with clip1 trimmed from start to 10 seconds, and clip2 trimmed from 5 seconds for 15 seconds.
```

## Notes

- **Edge Cases**: The builder throws exceptions for invalid inputs: null or nonexistent file paths, zero transition duration, conflicting trim parameters (both end and duration), and building with fewer than two segments. These are enforced at the time of the call, not deferred.
- **Thread Safety**: The `ConcatenationBuilder` is not designed for concurrent use. All operations should be performed from a single thread. The test class itself is not thread-safe.
- **Disposal**: The test class implements `IDisposable` to release temporary files created during test setup. In production code, the `ConcatenationBuilder` does not own any unmanaged resources and does not require disposal.
- **Fluent API**: All configuration methods return the builder instance, enabling method chaining. The order of calls does not affect the final result except for segment order, which is determined by the sequence of `Add` and `Insert` calls.
