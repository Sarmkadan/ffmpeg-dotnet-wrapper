# FfmpegRunnerOptions

`FfmpegRunnerOptions` configures how the `ffmpeg` and `ffprobe` executables are launched. `EnsureValid()` checks the configuration eagerly and verifies that FFmpeg is usable. [`AddFfmpeg`](#registration) runs it at DI registration time.

## Members

- `FfmpegPath` (`string?`): full path to `ffmpeg`. When `null` or whitespace, `ffmpeg` is resolved from `PATH`. A set path must point to an existing file.
- `FfprobePath` (`string?`): full path to `ffprobe`. Resolved and checked the same way as `FfmpegPath`.
- `Timeout` (`TimeSpan`, default 300 seconds): maximum time for one FFmpeg process. Must be greater than zero.
- `LogLevel` (`string`, default `"info"`): passed to `ffmpeg -loglevel`. Must be one of `quiet`, `panic`, `fatal`, `error`, `warning`, `info`, `verbose`, `debug`, `trace` (lowercase).
- `WorkingDirectory` (`string?`): directory FFmpeg runs in. `null` means the current directory. A set value must exist.
- `DetectedVersion` (`FfmpegVersion?`, read-only): the version parsed by the last successful `EnsureValid()`. `null` before that.

## EnsureValid

`FfmpegVersion EnsureValid()` runs these checks and returns the detected version:

1. `FfmpegPath` and `FfprobePath` resolve to existing files. A missing binary throws [`FfmpegNotFoundException`](FfmpegNotFoundException.md).
2. `Timeout`, `LogLevel`, `WorkingDirectory`, and the executables' permissions are valid. Every failed rule is reported together in one [`OptionsValidationException`](OptionsValidationException.md).
3. `ffmpeg -version` runs within `Timeout`, exits with code 0, and prints an `ffmpeg version` banner. Otherwise an `FfmpegNotFoundException` is thrown.

The `-version` probe runs once per binary path and write time. Later calls reuse the cached [`FfmpegVersion`](FfmpegVersion.md), so repeated validation does not launch FFmpeg again. Replacing the binary changes its write time and triggers a new probe.

```csharp
var options = new FfmpegRunnerOptions { Timeout = TimeSpan.FromMinutes(5), LogLevel = "warning" };
var version = options.EnsureValid();   // e.g. version.Raw == "6.1.1-1ubuntu1"
```

Executability is checked from the Unix file mode on Linux and macOS. On Windows the existence check stands in for it.

## Registration

`AddFfmpeg` lives on `IServiceCollection`:

```csharp
services.AddFfmpeg(o =>
{
    o.FfmpegPath = "/opt/ffmpeg/bin/ffmpeg";
    o.Timeout = TimeSpan.FromMinutes(2);
});
```

`AddFfmpeg` applies the configuration to a new instance and calls `EnsureValid()` before it returns, so a bad configuration fails at startup. It also registers `FfmpegRunnerOptionsValidator`, an `IValidateOptions<FfmpegRunnerOptions>`. Resolving `IOptions<FfmpegRunnerOptions>` runs the same checks and reports failures through `ValidateOptionsResult`.
