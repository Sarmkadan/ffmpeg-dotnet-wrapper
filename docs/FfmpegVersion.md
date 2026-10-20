# FfmpegVersion

`FfmpegVersion` is the version reported by the first line of `ffmpeg -version`, for example `ffmpeg version 6.1.1-1ubuntu1 Copyright ...`. [`FfmpegRunnerOptions.EnsureValid()`](FfmpegRunnerOptions.md) returns one and caches it in `DetectedVersion`.

## Members

- `Raw` (`string`): the version token exactly as printed, such as `6.1.1-1ubuntu1`, `n7.0`, or `N-112345-g1a2b3c4d5e`.
- `Number` (`Version?`): the numeric `major.minor.patch` part of `Raw`. Missing components default to `0`. `null` for development builds without a numeric release, such as git snapshots.

## TryParse

`FfmpegVersion? TryParse(string? output)` reads the `ffmpeg version` banner from `ffmpeg -version` output. It returns `null` when there is no banner, for example for `ffprobe` output or an empty string.
