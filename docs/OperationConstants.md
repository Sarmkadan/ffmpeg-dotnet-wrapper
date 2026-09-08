# Operation constants

`FFmpegDotnetWrapper.Constants` provides enums for describing operations and static classes containing commonly used FFmpeg values, presets, limits, and helper lookups.

## Enums

### `OperationType`

Identifies the kind of media operation being performed. Because the members do not declare numeric values, their underlying values follow declaration order.

| Member | Value | Meaning |
| --- | ---: | --- |
| `Unknown` | 0 | Unknown operation type. |
| `Transcode` | 1 | Media transcoding. |
| `Trim` | 2 | Media trimming. |
| `Merge` | 3 | Media merging. |
| `Watermark` | 4 | Watermarking. |
| `ExtractAudio` | 5 | Audio extraction. |
| `ExtractFrames` | 6 | Frame extraction. |
| `GenerateThumbnail` | 7 | Thumbnail generation. |
| `ResizeVideo` | 8 | Video resizing. |
| `RotateVideo` | 9 | Video rotation. |
| `FlipVideo` | 10 | Video flipping. |
| `AdjustQuality` | 11 | Quality adjustment. |
| `AddSubtitles` | 12 | Adding subtitles. |
| `RemoveAudio` | 13 | Removing audio. |
| `ChangeAspectRatio` | 14 | Changing the aspect ratio. |
| `CreatePlaylist` | 15 | Playlist creation. |

### `LogLevel`

Defines the available structured logging levels, from most detailed to disabled.

| Member | Value | Meaning |
| --- | ---: | --- |
| `Trace` | 0 | Trace-level logging. |
| `Debug` | 1 | Debug-level logging. |
| `Information` | 2 | Informational logging. |
| `Warning` | 3 | Warning-level logging. |
| `Error` | 4 | Error-level logging. |
| `Critical` | 5 | Critical-level logging. |
| `None` | 6 | Logging is disabled. |

### `ErrorCode`

Provides stable numeric codes for programmatic error handling.

| Member | Value | Meaning |
| --- | ---: | --- |
| `Success` | 0 | Successful completion. |
| `Unknown` | -1 | Unknown error. |
| `InputFileNotFound` | 1000 | Input file was not found. |
| `OutputPathInvalid` | 1001 | Output path is invalid. |
| `UnsupportedFormat` | 1002 | Media format is unsupported. |
| `InvalidArguments` | 1003 | One or more arguments are invalid. |
| `OperationTimeout` | 1004 | Operation timed out. |
| `InsufficientDiskSpace` | 1005 | Insufficient disk space is available. |
| `PermissionDenied` | 1006 | Access was denied. |
| `FFmpegNotInstalled` | 1007 | FFmpeg is not installed. |
| `FileIsLocked` | 1008 | A required file is locked. |
| `InvalidCodec` | 1009 | The specified codec is invalid. |
| `RateLimitExceeded` | 2000 | A rate limit was exceeded. |
| `ServiceUnavailable` | 3000 | The service is unavailable. |
| `InternalError` | 9999 | An internal error occurred. |

### `OperationState`

Describes the lifecycle state of an operation.

| Member | Value | Meaning |
| --- | ---: | --- |
| `Pending` | 0 | The operation is pending. |
| `Started` | 1 | The operation has started. |
| `Processing` | 2 | The operation is processing. |
| `Paused` | 3 | The operation is paused. |
| `Completed` | 4 | The operation completed successfully. |
| `Failed` | 5 | The operation failed. |
| `Cancelled` | 6 | The operation was cancelled. |

## Constant classes

### `CodecConstants`

Defines codec identifiers and case-insensitive sets of supported codecs.

| Member | Value |
| --- | --- |
| `H264` | `"h264"` |
| `H265` | `"h265"` |
| `HEVC` | `"hevc"` |
| `VP8` | `"vp8"` |
| `VP9` | `"vp9"` |
| `AV1` | `"av1"` |
| `MPEG2` | `"mpeg2"` |
| `AAC` | `"aac"` |
| `MP3` | `"mp3"` |
| `OPUS` | `"opus"` |
| `VORBIS` | `"vorbis"` |

`SupportedVideoCodecs` is a case-insensitive `HashSet<string>` containing `H264`, `H265`, `HEVC`, `VP8`, `VP9`, `AV1`, and `MPEG2`. `SupportedAudioCodecs` is a case-insensitive `HashSet<string>` containing `AAC`, `MP3`, `OPUS`, and `VORBIS`.

### `FormatConstants`

Defines output container and playlist format identifiers.

| Member | Value |
| --- | --- |
| `MP4` | `"mp4"` |
| `MKV` | `"mkv"` |
| `WEBM` | `"webm"` |
| `AVI` | `"avi"` |
| `MOV` | `"mov"` |
| `FLV` | `"flv"` |
| `TS` | `"ts"` |
| `M3U8` | `"m3u8"` |

`SupportedFormats` is a case-insensitive `HashSet<string>` containing every format listed above.

### `QualityPresets`

Defines constant rate factor (CRF) presets. Lower CRF values indicate higher quality.

| Member | Value | Description |
| --- | ---: | --- |
| `VeryLow` | 40 | Poor quality and small file size. |
| `Low` | 32 | Lower quality. |
| `Medium` | 23 | Balanced quality and the default preset. |
| `High` | 18 | High quality. |
| `VeryHigh` | 10 | Very high quality and a large file size. |
| `Lossless` | 0 | Lossless encoding. |

`GetPresetName(int crf)` maps a CRF value to a display name using these ranges:

| CRF | Result |
| --- | --- |
| 40 or greater | `"Very Low"` |
| 32 through 39 | `"Low"` |
| 23 through 31 | `"Medium"` |
| 18 through 22 | `"High"` |
| 10 through 17 | `"Very High"` |
| 0 through 9 | `"Lossless"` |
| Less than 0 | `"Unknown"` |

### `BitrateConstants`

Defines bitrate values in kilobits per second (Kbps).

| Member | Value | Equivalent |
| --- | ---: | --- |
| `VideoLow` | 1,000 Kbps | 1 Mbps |
| `VideoMedium` | 5,000 Kbps | 5 Mbps |
| `VideoHigh` | 10,000 Kbps | 10 Mbps |
| `VideoVeryHigh` | 25,000 Kbps | 25 Mbps |
| `AudioMono` | 64 Kbps | 64 Kbps |
| `AudioStereo` | 128 Kbps | 128 Kbps |
| `AudioHigh` | 192 Kbps | 192 Kbps |
| `AudioHD` | 320 Kbps | 320 Kbps |

`GetRecommendedBitrate(int width, int height)` multiplies the dimensions and returns:

| Pixel count | Result |
| --- | ---: |
| At most 1,920 x 1,080 | 5,000 Kbps |
| At most 3,840 x 2,160 | 15,000 Kbps |
| At most 7,680 x 4,320 | 50,000 Kbps |
| Greater than 7,680 x 4,320 | 25,000 Kbps |

### `FFmpegCommandConstants`

Defines frequently used FFmpeg command-line options.

| Member | Value | Purpose |
| --- | --- | --- |
| `InputOption` | `"-i"` | Input. |
| `OutputFormat` | `"-f"` | Output format. |
| `Codec` | `"-c:v"` | Video codec. |
| `AudioCodec` | `"-c:a"` | Audio codec. |
| `Bitrate` | `"-b:v"` | Video bitrate. |
| `AudioBitrate` | `"-b:a"` | Audio bitrate. |
| `CRF` | `"-crf"` | Constant rate factor. |
| `Preset` | `"-preset"` | Encoding preset. |
| `FrameRate` | `"-r"` | Frame rate. |
| `Resolution` | `"-s"` | Resolution. |
| `NoAudio` | `"-an"` | Disable audio output. |
| `NoVideo` | `"-vn"` | Disable video output. |
| `Duration` | `"-t"` | Duration. |
| `StartTime` | `"-ss"` | Start time. |
| `Overwrite` | `"-y"` | Enable output overwriting. |
| `NoOverwrite` | `"-n"` | Disable output overwriting. |
| `Stats` | `"-stats"` | Display encoding statistics. |
| `HideLog` | `"-hide_banner"` | Hide the startup banner. |
| `ErrorLogLevel` | `"-loglevel error"` | Limit logging to errors. |

### `TempFileConstants`

Defines temporary-file naming and cleanup values.

| Member | Value | Meaning |
| --- | --- | --- |
| `TempFilePrefix` | `".ffmpeg-"` | Temporary FFmpeg file prefix. |
| `TempFileExtension` | `".tmp"` | Temporary file extension. |
| `FFmpegTempDir` | `"ffmpeg-dotnet-temp"` | Temporary directory name. |
| `MaxTempFileAge` | `86400` | Maximum age in seconds (24 hours). |

### `TimeoutConstants`

Defines timeout values in seconds.

| Member | Value | Equivalent |
| --- | ---: | --- |
| `DefaultOperationTimeoutSeconds` | 600 | 10 minutes |
| `MaxOperationTimeoutSeconds` | 3,600 | 1 hour |
| `MinOperationTimeoutSeconds` | 10 | 10 seconds |
| `ProbeTimeoutSeconds` | 30 | 30 seconds |
| `WebhookTimeoutSeconds` | 30 | 30 seconds |
| `HttpClientTimeoutSeconds` | 60 | 1 minute |
