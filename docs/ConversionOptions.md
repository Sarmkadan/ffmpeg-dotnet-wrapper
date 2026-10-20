# ConversionOptions

`ConversionOptions` describes a single file conversion: input and output paths, video and audio encoding parameters, and an optional time range. Call `Validate()` before running the conversion.

## Validation

`Validate()` checks every rule and throws one [`OptionsValidationException`](OptionsValidationException.md) listing all violations, not just the first.

| Field(s) | Rule |
|----------|------|
| `InputPath` | Must be set and the file must exist. |
| `OutputPath` | Must be set, must not be an existing directory, and its directory must exist or be creatable. |
| `OutputPath` / `InputPath` | Must differ. |
| `VideoBitrateKbps` / `Crf` | Mutually exclusive. `VideoBitrateKbps` must be within `FFmpegConstants.MinBitrate`..`MaxBitrate`; `Crf` must be within `0`..`51`. |
| `Width` / `Height` | When set, positive and even. |
| `FrameRate` | When set, finite and greater than zero. |
| `AudioBitrateKbps` / `AudioSampleRate` / `AudioChannels` | When set, positive. |
| `VideoCodec` / `AudioCodec` | When set, not empty or whitespace. |
| `StartTime` / `EndTime` | `StartTime` not negative; `EndTime` after `StartTime` (zero when unset). |
| `SourceDuration` | When set, greater than zero; `StartTime` and `EndTime` must lie within it. |

## Example

```csharp
var options = new ConversionOptions
{
    InputPath = "input.mov",
    OutputPath = "output/clip.mp4",
    Crf = 23,
    Width = 1280,
    Height = 720,
    StartTime = TimeSpan.FromSeconds(5),
    EndTime = TimeSpan.FromSeconds(30),
    SourceDuration = TimeSpan.FromMinutes(2)
};

try
{
    options.Validate();
}
catch (OptionsValidationException ex)
{
    foreach (var violation in ex.Violations)
        Console.WriteLine(violation);
}
```

## Use with the process runner

Set `FFmpegProcessRequest.ConversionOptions` and `FFmpegProcessRunner.RunAsync` validates the options before starting ffmpeg. Invalid options throw `OptionsValidationException` and no process is spawned.
