# SubtitleSettingsValidation

`SubtitleSettingsValidation` provides extension methods for validating a [`SubtitleSettings`](SubtitleSettings.md) instance. Validation reports all detected problems rather than stopping at the first one.

## API

### `Validate`

```csharp
IReadOnlyList<string> problems = settings.Validate();
```

Returns a read-only list of human-readable validation problems. An empty list means the settings are valid. Passing `null` throws `ArgumentNullException`.

### `IsValid`

```csharp
bool valid = settings.IsValid();
```

Returns `true` when `Validate` returns no problems. Passing `null` throws `ArgumentNullException` through `Validate`.

### `EnsureValid`

```csharp
settings.EnsureValid();
```

Returns normally for a valid settings instance. For an invalid instance, it throws `ArgumentException` with all problems joined into the exception message. Passing `null` throws `ArgumentNullException`.

## Validation rules

| Property | Requirement |
| --- | --- |
| `FontSize` | Must be between 6 and 120 (inclusive). |
| `SubtitleStreamIndex` | Must be non-negative. |
| `FontName` | If set, must not be whitespace-only. |
| `Language` | If set, must not be whitespace and must not exceed 10 characters in length. |

## Example

This example validates a valid subtitle settings instance.

```csharp
using FFmpegDotnetWrapper.Models;

var settings = new SubtitleSettings
{
    FontSize = 24,
    SubtitleStreamIndex = 0,
    FontName = "Arial",
    Language = "en"
};

IReadOnlyList<string> problems = settings.Validate();
if (problems.Count == 0)
{
    settings.EnsureValid();
}
```