# GifExportService

`GifExportService` exports a segment of a video as an optimized GIF. It invokes FFmpeg twice: once to create a temporary color palette and once to render the GIF with that palette. `GifExportCliCommands` supplies the related `gif` command definition and parsing helpers; it does not execute the export itself.

## Purpose

The service selects a segment with a start time and duration, scales it while preserving its aspect ratio, applies the configured frame rate and dithering mode, and writes the GIF beside the source video. A custom FFmpeg executable path can be supplied; otherwise, the executable name is `ffmpeg` and is resolved through the system `PATH`.

The generated filename has the form `<source-name>_<rounded-start-seconds>s_<rounded-duration-seconds>s.gif`. The source path is resolved and required to remain within `AppContext.BaseDirectory`.

## Public API

| Type | Member | Description |
| --- | --- | --- |
| `GifExportService` | `GifExportService(string? ffmpegExecutablePath = null)` | Creates the service. A null path uses `ffmpeg`; an empty string is retained as the executable path. |
| `GifExportService` | `Task<ConversionResult> ExportGifAsync(string sourcePath, TimeSpan start, TimeSpan duration)` | Exports with a new, default `GifExportSettings` instance. |
| `GifExportService` | `Task<ConversionResult> ExportGifAsync(string sourcePath, TimeSpan start, TimeSpan duration, GifExportSettings settings)` | Exports with the supplied frame rate, effective width, dithering mode, and loop count. |
| `GifExportCliCommands` | `void RegisterGifExportCommands(this CliCommandParser parser)` | Registers the `gif` command, its two required positional arguments, and its options. |
| `GifExportCliCommands` | `TimeSpan ParseTimeString(string timeString)` | Parses `MM:SS` or `HH:MM:SS` text into a `TimeSpan`. |
| `GifExportCliCommands` | `DitherMode ParseDitherMode(string ditherString)` | Parses a supported dither name, case-insensitively. Hyphenated aliases are accepted for `floyd-steinberg` and `sierra2-4a`. |
| `GifExportCliCommands` | `GifQualityPreset ParseQualityPreset(string qualityString)` | Parses `low`, `medium`, or `high`, case-insensitively. |

### Registered `gif` command

The command declares required `source-path` and `output-path` positional arguments. It also declares `--start`/`-s` (default `00:00:00`), `--duration`/`-d` (default `00:00:10`), `--fps`/`-f` (default `10`), `--width`/`-w` (default `640`), `--max-width`/`-mw`, `--dither`/`-dt` (default `sierra2_4a`), `--loop`/`-l` (default `-1`), and `--quality`/`-q` (default `medium`). This class only registers and parses those values; it contains no CLI handler that maps them to `ExportGifAsync`, and the service itself determines its output path rather than accepting one.

## Two-pass palette approach

1. The palette pass seeks to `start`, reads `duration`, and applies `fps`, `scale=<effective-width>:-1:flags=lanczos`, and `palettegen=stats_mode=diff:max_colors=256`. The palette is written to a temporary `.png` file. If FFmpeg reports failure, the method returns a failed `ConversionResult` prefixed with `Failed to generate color palette`.
2. The GIF pass seeks over the same segment and supplies both the source and palette as inputs. It repeats the frame-rate and Lanczos scaling filters, then applies `paletteuse` with the `DitherMode` mapped to FFmpeg's dither value. A settings loop value of `-1` is passed to FFmpeg as `-loop 0`; other values are passed unchanged. If this pass fails, the method returns a failed result prefixed with `Failed to create GIF`.

Both commands overwrite their targets (`-y`) and use `-hide_banner -loglevel error`. The temporary palette is deleted in a `finally` block; cleanup errors are ignored. A successful result contains the generated GIF path in `OutputFilePath`.

## C# usage

```csharp
using FFmpegDotnetWrapper.Models;
using FFmpegDotnetWrapper.Services;

var exporter = new GifExportService();
var sourcePath = Path.Combine(AppContext.BaseDirectory, "media", "clip.mp4");

var settings = new GifExportSettings
{
    Fps = 12,
    Width = 480,
    DitherMode = DitherMode.Sierra2_4a,
    Loop = -1
};

ConversionResult result = await exporter.ExportGifAsync(
    sourcePath,
    start: TimeSpan.FromSeconds(5),
    duration: TimeSpan.FromSeconds(8),
    settings);

if (result.IsSuccess)
    Console.WriteLine(result.OutputFilePath);
else
    Console.Error.WriteLine(result.ErrorMessage);
```

## Exceptions

- `ExportGifAsync` throws `ArgumentException` when the three-argument overload receives a null, empty, or whitespace source path, when settings validation fails, or when the resolved source path is invalid or outside `AppContext.BaseDirectory`.
- The settings overload throws `ArgumentNullException` when `settings` or `sourcePath` is null.
- `FileNotFoundException` is thrown when the validated source file does not exist.
- File-system and process-start exceptions can propagate while creating the palette file, preparing the output, or launching FFmpeg. Exceptions caught during the export are recorded as a failed result and then rethrown. Failure to delete the temporary palette does not propagate.
- `RegisterGifExportCommands` throws `ArgumentNullException` when `parser` is null.
- Each CLI parsing helper throws `ArgumentNullException` for a null string and `ArgumentException` for an empty string or an unsupported format/value. `ParseTimeString` accepts only two or three colon-separated integer components; it does not separately constrain minute or second ranges.

An FFmpeg nonzero exit is represented by a failed `ConversionResult`, not thrown as an exception by `GifExportService`.
