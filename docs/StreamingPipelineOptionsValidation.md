# StreamingPipelineOptionsValidation

`StreamingPipelineOptionsValidation` provides extension methods for checking a [`StreamingPipelineOptions`](StreamingPipelineOptions.md) instance before it is used.

## API

### `Validate`

```csharp
IReadOnlyList<string> errors = options.Validate();
```

Returns a read-only list containing every validation error found. The list is empty when the options are valid. Passing `null` throws `ArgumentNullException`.

### `IsValid`

```csharp
bool valid = options.IsValid();
```

Returns `true` when `Validate` produces no errors and `false` otherwise. Passing `null` throws `ArgumentNullException` through `Validate`.

### `EnsureValid`

```csharp
options.EnsureValid();
```

Returns normally when validation succeeds. If validation fails, it throws an `ArgumentException` whose message contains all validation errors, one per line. Passing `null` throws `ArgumentNullException`.

## Validation rules

The validator applies these rules:

| Property | Requirement |
| --- | --- |
| `DefaultSegmentDurationSeconds` | Must be greater than `0`. |
| `DefaultPlaylistWindowSize` | Must be greater than or equal to `0`; `0` is valid. |
| `MaxConcurrentPipelines` | Must be greater than `0`. |
| `MaxConcurrentRenditionsPerPipeline` | Must be greater than `0`. |
| `BitrateDecisionWindowSegments` | Must be greater than `0`. |
| `DowngradeSpeedThreshold` | Must be greater than `0`. |
| `UpgradeSpeedThreshold` | Must be greater than `0`. |
| `DefaultProfiles` | Must not be `null`. The list may be empty. |
| `DefaultOutputBaseDirectory` | When non-null and non-whitespace, the trimmed path must be absolute and no longer than 260 characters. Invalid path syntax is reported as an error. |

Each non-null entry in `DefaultProfiles` is also validated:

| Profile property | Requirement |
| --- | --- |
| Profile entry | Must not be `null`. |
| `Name` | Must not be null, empty, or whitespace. |
| `Width` | Must be greater than `0`. |
| `Height` | Must be greater than `0`. |
| `VideoBitrateKbps` | Must be greater than `0`. |
| `AudioBitrateKbps` | Must be greater than or equal to `0`. |
| `FrameRate` | Must be greater than or equal to `0`. |

Properties not listed above are not checked by this validator. Validation does not test whether the output directory exists, is writable, or is accessible.

## Example

```csharp
using FFmpegDotnetWrapper.Configuration;

var options = new StreamingPipelineOptions
{
    DefaultSegmentDurationSeconds = 6,
    DefaultPlaylistWindowSize = 5,
    MaxConcurrentPipelines = 3,
    MaxConcurrentRenditionsPerPipeline = 2,
    BitrateDecisionWindowSegments = 3,
    DowngradeSpeedThreshold = 0.9,
    UpgradeSpeedThreshold = 1.5,
    DefaultOutputBaseDirectory = Path.GetFullPath("stream-output"),
    DefaultProfiles =
    [
        new StreamingProfileOptions
        {
            Name = "720p",
            Width = 1280,
            Height = 720,
            VideoBitrateKbps = 2800,
            AudioBitrateKbps = 128,
            FrameRate = 30
        }
    ]
};

var errors = options.Validate();
if (errors.Count > 0)
{
    foreach (var error in errors)
    {
        Console.Error.WriteLine(error);
    }
}

// Alternatively, throw ArgumentException when any rule fails.
options.EnsureValid();
```
