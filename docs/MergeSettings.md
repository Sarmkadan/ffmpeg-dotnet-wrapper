# MergeSettings

`MergeSettings` defines the parameters and input file collection used to configure a media merge operation. It controls which streams to preserve, whether to transcode during the merge, and optional crossfade behavior between consecutive inputs. The type provides methods to manage the list of input files, validate the current configuration, and create independent copies for reuse or modification.

## API

### `public bool PreserveAudio`

Gets or sets whether audio streams from the input files are kept in the merged output. When `true`, audio tracks are included; when `false`, they are discarded.

### `public bool PreserveVideo`

Gets or sets whether video streams from the input files are kept in the merged output. When `true`, video tracks are included; when `false`, they are discarded.

### `public bool TranscodeOnMerge`

Gets or sets whether the merge operation should transcode streams rather than copying them directly. When `true`, the settings provided in `TranscodeSettings` are applied to the merged output. When `false`, compatible streams are copied without re-encoding where possible.

### `public TranscodeSettings? TranscodeSettings`

Gets or sets the transcode parameters used when `TranscodeOnMerge` is `true`. This property is nullable; a `null` value indicates no transcode configuration has been supplied. If `TranscodeOnMerge` is `true` and this property is `null` at validation time, `Validate` throws an exception.

### `public bool Crossfade`

Gets or sets whether a crossfade transition is applied between consecutive input files. When `true`, the duration specified by `CrossfadeDuration` determines the overlap. Only relevant when merging multiple inputs.

### `public double CrossfadeDuration`

Gets or sets the duration of the crossfade transition in seconds. This value is used only when `Crossfade` is `true`. Must be a positive value; `Validate` throws if it is zero or negative while `Crossfade` is enabled.

### `public void AddInputFile(string filePath)`

Adds an input file path to the internal list of files to be merged.

- **Parameters:** `filePath` — the path to the media file to add.
- **Throws:** `ArgumentException` if `filePath` is `null` or empty.

### `public void RemoveInputFile(string filePath)`

Removes the specified input file path from the internal list. If the path is not present, the call has no effect.

- **Parameters:** `filePath` — the path to the media file to remove.
- **Throws:** `ArgumentException` if `filePath` is `null` or empty.

### `public void Validate()`

Validates the current merge configuration. Checks that at least one input file has been added, that `TranscodeSettings` is not `null` when `TranscodeOnMerge` is `true`, and that `CrossfadeDuration` is positive when `Crossfade` is enabled.

- **Throws:** `InvalidOperationException` if the configuration is invalid.

### `public int GetInputFileCount()`

Returns the number of input files currently added to the merge list.

- **Returns:** An `int` representing the count of input files.

### `public void ClearInputFiles()`

Removes all input files from the merge list, resetting the collection to an empty state.

### `public MergeSettings Clone()`

Creates a deep copy of the current `MergeSettings` instance. The cloned object contains independent copies of all property values and a separate input file list, so modifications to the clone do not affect the original.

- **Returns:** A new `MergeSettings` instance with identical configuration.

## Usage

### Example 1: Simple merge preserving both streams

```csharp
var settings = new MergeSettings
{
    PreserveAudio = true,
    PreserveVideo = true,
    TranscodeOnMerge = false
};

settings.AddInputFile("part1.mp4");
settings.AddInputFile("part2.mp4");
settings.AddInputFile("part3.mp4");

settings.Validate();

// Pass settings to the merge executor
// mergeExecutor.Execute(settings, "output.mp4");
```

### Example 2: Merge with transcode and crossfade

```csharp
var settings = new MergeSettings
{
    PreserveAudio = true,
    PreserveVideo = true,
    TranscodeOnMerge = true,
    TranscodeSettings = new TranscodeSettings
    {
        VideoCodec = "libx264",
        AudioCodec = "aac"
    },
    Crossfade = true,
    CrossfadeDuration = 2.5
};

settings.AddInputFile("scene1.mov");
settings.AddInputFile("scene2.mov");

settings.Validate();

// Clone for a variant with different crossfade duration
var alternateSettings = settings.Clone();
alternateSettings.CrossfadeDuration = 1.0;
alternateSettings.Validate();

// mergeExecutor.Execute(alternateSettings, "output_fast_crossfade.mov");
```

## Notes

- **Validation order:** Call `Validate` after all properties and input files are configured. Modifying properties or the file list after validation requires re-validation.
- **Empty file list:** `Validate` throws if no input files have been added. At least one file is required for a merge operation.
- **Crossfade constraints:** `CrossfadeDuration` must be strictly greater than zero when `Crossfade` is `true`. Setting `Crossfade` to `false` ignores the duration value entirely.
- **Transcode dependency:** When `TranscodeOnMerge` is `true`, `TranscodeSettings` must be non-null. Setting `TranscodeOnMerge` to `false` allows `TranscodeSettings` to be `null` without causing validation errors.
- **Thread safety:** This type is not thread-safe. Concurrent calls to `AddInputFile`, `RemoveInputFile`, `ClearInputFiles`, or property setters from multiple threads may corrupt internal state. Synchronize external access if shared across threads.
- **Clone independence:** `Clone` produces a fully independent instance. Changes to the clone’s file list or properties do not propagate to the original, making it suitable for creating configuration variants without side effects.
- **Duplicate file paths:** The internal list preserves insertion order and does not deduplicate paths. Adding the same path multiple times results in that file being processed more than once during the merge.
