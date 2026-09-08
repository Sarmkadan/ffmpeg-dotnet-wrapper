# GifExportSettings

`GifExportSettings` configures GIF frame rate, output width, palette dithering, looping, and a quality preset. `GifQualityPreset` supplies predefined frame-rate and width combinations, while `DitherMode` identifies the palette-conversion dithering algorithm.

## Public API

| Type | Member | Description |
| --- | --- | --- |
| `GifExportSettings` | `GifExportSettings()` | Creates an instance with the default settings. |
| `GifExportSettings` | `GifExportSettings(GifQualityPreset quality)` | Creates an instance and applies the specified quality preset. |
| `GifExportSettings` | `int Fps { get; set; }` | Frames per second. Assigning zero or a negative value throws `ArgumentOutOfRangeException`. |
| `GifExportSettings` | `int Width { get; set; }` | Target width in pixels; height is scaled to preserve aspect ratio. It is ignored when `MaxWidth` has a value. Assigning zero or a negative value throws `ArgumentOutOfRangeException`. |
| `GifExportSettings` | `int? MaxWidth { get; set; }` | Optional maximum output width. When set, it takes precedence over `Width`. Assigning a non-null value that is zero or negative throws `ArgumentOutOfRangeException`. |
| `GifExportSettings` | `DitherMode DitherMode { get; set; }` | Dithering mode used for palette conversion. |
| `GifExportSettings` | `int Loop { get; set; }` | GIF loop count. `-1` represents infinite looping. Assigning a value below `-1` throws `ArgumentOutOfRangeException`. |
| `GifExportSettings` | `GifQualityPreset Quality { get; set; }` | Current quality preset. Setting this property alone does not apply the preset's frame rate or width. |
| `GifExportSettings` | `void ApplyQualityPreset(GifQualityPreset preset)` | Sets `Quality` and changes `Fps` and `Width` for a recognized preset. |
| `GifExportSettings` | `IReadOnlyList<string> Validate()` | Returns all validation messages found for the current settings. |
| `GifExportSettings` | `bool IsValid()` | Returns `true` when `Validate()` returns no errors. |
| `GifExportSettings` | `void EnsureValid()` | Throws `ArgumentException` containing the validation messages when the settings are invalid. |
| `GifExportSettings` | `int GetEffectiveWidth()` | Returns `MaxWidth` when it has a value; otherwise returns `Width`. |

### `GifQualityPreset`

| Value | Effect when applied |
| --- | --- |
| `Low` | Sets `Fps` to `8` and `Width` to `480`. |
| `Medium` | Sets `Fps` to `10` and `Width` to `640`. |
| `High` | Sets `Fps` to `15` and `Width` to `800`. |

### `DitherMode`

| Value | Numeric value | Description |
| --- | ---: | --- |
| `None` | `0` | No dithering. |
| `Bayer` | `1` | Ordered 8x8 Bayer dithering. |
| `Heckbert` | `2` | Heckbert simple error-diffusion dithering. |
| `FloydSteinberg` | `3` | Floyd-Steinberg error-diffusion dithering. |
| `Sierra2` | `4` | Sierra version 2 error-diffusion dithering. |
| `Sierra2_4a` | `5` | Sierra version 2 "Lite" error-diffusion dithering. |
| `Sierra3` | `6` | Sierra version 3 error-diffusion dithering. |
| `Burkes` | `7` | Burkes error-diffusion dithering. |
| `Atkinson` | `8` | Atkinson error-diffusion dithering. |

## Defaults

| Property | Default |
| --- | --- |
| `Fps` | `10` |
| `Width` | `640` |
| `MaxWidth` | `null` |
| `DitherMode` | `DitherMode.Sierra2_4a` |
| `Loop` | `-1` (infinite) |
| `Quality` | `GifQualityPreset.Medium` |

The parameterless constructor retains these defaults. The preset constructor immediately calls `ApplyQualityPreset`; applying `Medium` produces the same frame rate and width as the defaults.

## C# usage

```csharp
using FFmpegDotnetWrapper.Models;

var settings = new GifExportSettings(GifQualityPreset.High)
{
    MaxWidth = 720,
    DitherMode = DitherMode.FloydSteinberg,
    Loop = -1
};

settings.EnsureValid();

int outputWidth = settings.GetEffectiveWidth(); // 720 (MaxWidth takes precedence)
```

## Validation rules

Property setters reject `Fps <= 0`, `Width <= 0`, non-null `MaxWidth <= 0`, and `Loop < -1` with `ArgumentOutOfRangeException` at assignment time.

`Validate()` checks the same numeric conditions and additionally reports a validation error when a non-null `MaxWidth` is less than `160`. It returns a read-only list and does not throw. `IsValid()` is equivalent to checking whether that list is empty, and `EnsureValid()` throws `ArgumentException` when it is not.

The code does not validate whether `Quality` or `DitherMode` is a defined enum value. `ApplyQualityPreset` always assigns `Quality`; if an undefined enum value is supplied, its switch does not change `Fps` or `Width`. Although the `Loop` exception text describes a positive loop count, the implemented condition permits both `0` and positive values in addition to `-1`.
