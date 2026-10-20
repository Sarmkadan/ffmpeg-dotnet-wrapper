# FfmpegNotFoundException

`FfmpegNotFoundException` is thrown when the `ffmpeg` or `ffprobe` executable cannot be used. It derives from [`FFmpegException`](FFmpegException.md).

It is raised by [`FfmpegRunnerOptions.EnsureValid()`](FfmpegRunnerOptions.md) and [`AddFfmpeg`](FfmpegRunnerOptions.md#registration) when:

- a binary is not on `PATH` (when its path option is unset) or the configured path does not exist;
- `ffmpeg -version` cannot be launched, exits with a non-zero code, times out, or does not print an `ffmpeg version` banner.

Configuration rules that are broken, such as a non-positive `Timeout` or an unknown `LogLevel`, throw [`OptionsValidationException`](OptionsValidationException.md) instead.

## Constructors

- `FfmpegNotFoundException(string message)` throws `ArgumentNullException` for `null` and `ArgumentException` for an empty message.
- `FfmpegNotFoundException(string message, Exception? innerException)` also keeps the underlying failure, such as the process-launch error.

```
FfmpegPath '/usr/local/bin/ffmpeg' does not exist or is not a file.
```
