# FFmpeg Constants

The types in `FFmpegDotnetWrapper.Constants` provide strongly typed selections and shared literal values used when configuring FFmpeg operations. They are declared in `src/Constants/FFmpegConstants.cs`.

## Enumerations

### `VideoCodec`

Represents a video codec selection.

| Value | Description |
|---|---|
| `H264` | H.264 video codec. |
| `H265` | H.265 video codec. |
| `VP8` | VP8 video codec. |
| `VP9` | VP9 video codec. |
| `AV1` | AV1 video codec. |
| `MPEG2` | MPEG-2 video codec. |

### `AudioCodec`

Represents an audio codec selection.

| Value | Description |
|---|---|
| `AAC` | AAC audio codec. |
| `MP3` | MP3 audio codec. |
| `OPUS` | Opus audio codec. |
| `FLAC` | FLAC audio codec. |
| `PCM` | PCM audio codec. |
| `VORBIS` | Vorbis audio codec. |

### `ContainerFormat`

Represents an output container format.

| Value | Description |
|---|---|
| `MP4` | MP4 container. |
| `Matroska` | Matroska container. |
| `AVI` | AVI container. |
| `QuickTime` | QuickTime container. |
| `WebM` | WebM container. |
| `FLV` | Flash Video container. |
| `WAV` | WAV audio container. |
| `MP3` | MP3 audio container. |
| `AAC` | AAC audio container. |
| `FLAC` | FLAC audio container. |
| `HLS` | HTTP Live Streaming output, producing an `.m3u8` playlist and `.ts` segments. |

### `QualityPreset`

Represents an encoding speed and compression-efficiency preset.

| Value | Description |
|---|---|
| `Ultrafast` | Ultrafast preset. |
| `Superfast` | Superfast preset. |
| `Veryfast` | Very fast preset. |
| `Faster` | Faster preset. |
| `Fast` | Fast preset. |
| `Medium` | Medium preset. |
| `Slow` | Slow preset. |
| `Slower` | Slower preset. |
| `Veryslow` | Very slow preset. |

### `ScalingMode`

Represents a video scaling algorithm.

| Value | Description |
|---|---|
| `Bilinear` | Bilinear scaling. |
| `Bicubic` | Bicubic scaling. |
| `Lanczos` | Lanczos scaling. |
| `Neighbor` | Nearest-neighbor scaling. |
| `Area` | Area-based scaling. |

### `AudioSampleRate`

Represents an audio sample rate. Each member has an explicit integer value in hertz.

| Value | Integer value | Description |
|---|---:|---|
| `Hz8000` | `8000` | 8,000 Hz. |
| `Hz16000` | `16000` | 16,000 Hz. |
| `Hz22050` | `22050` | 22,050 Hz. |
| `Hz44100` | `44100` | 44,100 Hz. |
| `Hz48000` | `48000` | 48,000 Hz. |
| `Hz96000` | `96000` | 96,000 Hz. |
| `Hz192000` | `192000` | 192,000 Hz. |

### `AudioChannels`

Represents an audio channel configuration. Each member's integer value is the channel count.

| Value | Integer value | Description |
|---|---:|---|
| `Mono` | `1` | Mono audio. |
| `Stereo` | `2` | Stereo audio. |
| `Surround5` | `5` | Five-channel surround audio. |
| `Surround51` | `6` | 5.1 surround audio. |
| `Surround7` | `7` | Seven-channel surround audio. |
| `Surround71` | `8` | 7.1 surround audio. |

### `HwAccel`

Represents the hardware acceleration backend used for video encoding.

| Value | Description |
|---|---|
| `None` | Disables hardware acceleration and uses CPU-based encoding. |
| `NVENC` | NVIDIA NVENC; requires an NVIDIA GPU and drivers. |
| `VAAPI` | Intel/AMD VAAPI; Linux only. |
| `QSV` | Intel Quick Sync Video; requires a compatible Intel GPU. |
| `Auto` | Lets FFmpeg select an available accelerator with `-hwaccel auto`, falling back to software when none is available. |

## Constant classes

All members below are public compile-time constants.

### `FFmpegConstants`

| Constant | Type | Value | Description |
|---|---|---:|---|
| `FFmpegExecutableName` | `string` | `"ffmpeg"` | FFmpeg executable name. |
| `FFprobeExecutableName` | `string` | `"ffprobe"` | FFprobe executable name. |
| `DefaultTimeoutSeconds` | `int` | `300` | Default operation timeout in seconds. |
| `MaxTimeoutSeconds` | `int` | `3600` | Maximum operation timeout in seconds. |
| `MinTimeoutSeconds` | `int` | `10` | Minimum operation timeout in seconds. |
| `DefaultBitrate` | `int` | `5000` | Default video bitrate in kilobits per second. |
| `MinBitrate` | `int` | `100` | Minimum video bitrate in kilobits per second. |
| `MaxBitrate` | `int` | `50000` | Maximum video bitrate in kilobits per second. |
| `DefaultAudioBitrate` | `int` | `128` | Default audio bitrate in kilobits per second. |
| `MinAudioBitrate` | `int` | `32` | Minimum audio bitrate in kilobits per second. |
| `MaxAudioBitrate` | `int` | `320` | Maximum audio bitrate in kilobits per second. |
| `DefaultFrameRate` | `int` | `30` | Default video frame rate. |
| `MinFrameRate` | `int` | `1` | Minimum video frame rate. |
| `MaxFrameRate` | `int` | `120` | Maximum video frame rate. |

The following constant classes are nested inside `FFmpegConstants`.

### `FileExtensions`

| Constant | Value |
|---|---|
| `MP4` | `".mp4"` |
| `MKV` | `".mkv"` |
| `AVI` | `".avi"` |
| `MOV` | `".mov"` |
| `FLV` | `".flv"` |
| `WEBM` | `".webm"` |
| `WAV` | `".wav"` |
| `MP3` | `".mp3"` |
| `AAC` | `".aac"` |
| `FLAC` | `".flac"` |

### `VideoCodecNames`

| Constant | Value |
|---|---|
| `H264` | `"h264"` |
| `H265` | `"hevc"` |
| `VP8` | `"vp8"` |
| `VP9` | `"vp9"` |
| `AV1` | `"av1"` |
| `MPEG2` | `"mpeg2video"` |

### `AudioCodecNames`

| Constant | Value |
|---|---|
| `AAC` | `"aac"` |
| `MP3` | `"libmp3lame"` |
| `OPUS` | `"libopus"` |
| `FLAC` | `"flac"` |
| `PCM` | `"pcm_s16le"` |
| `VORBIS` | `"libvorbis"` |

### `PresetLevels`

| Constant | Value |
|---|---|
| `Ultrafast` | `"ultrafast"` |
| `Superfast` | `"superfast"` |
| `Veryfast` | `"veryfast"` |
| `Faster` | `"faster"` |
| `Fast` | `"fast"` |
| `Medium` | `"medium"` |
| `Slow` | `"slow"` |
| `Slower` | `"slower"` |
| `Veryslow` | `"veryslow"` |
