# TranscodeSettings

The `TranscodeSettings` class serves as the primary configuration object for defining media transcoding parameters within the `ffmpeg-dotnet-wrapper` library. It encapsulates all necessary options for video and audio codec selection, resolution scaling, quality presets, hardware acceleration, and audio normalization, providing a strongly-typed interface to construct FFmpeg command-line arguments. This class includes validation logic to ensure parameter consistency before execution and supports cloning to facilitate immutable configuration patterns or template-based setups.

## API

### Properties

#### `VideoCodec`
*   **Type:** `VideoCodec`
*   **Description:** Specifies the video codec to be used for encoding the output stream (e.g., H.264, HEVC, VP9).
*   **Remarks:** Must be compatible with the selected `Container` format.

#### `AudioCodec`
*   **Type:** `AudioCodec`
*   **Description:** Specifies the audio codec to be used for encoding the output audio stream (e.g., AAC, MP3, Opus).
*   **Remarks:** If audio processing is not required, this may be set to a value representing "no audio" depending on the enum definition.

#### `Container`
*   **Type:** `ContainerFormat`
*   **Description:** Defines the output container format (e.g., MP4, MKV, WebM).
*   **Remarks:** The chosen container must support the selected `VideoCodec` and `AudioCodec`.

#### `Width`
*   **Type:** `int?`
*   **Description:** Sets the explicit target width for the output video in pixels.
*   **Remarks:** If `EnableAutoScale` is true, this value may be overridden or used as a constraint depending on the scaling mode.

#### `Height`
*   **Type:** `int?`
*   **Description:** Sets the explicit target height for the output video in pixels.
*   **Remarks:** If `EnableAutoScale` is true, this value may be overridden or used as a constraint depending on the scaling mode.

#### `Quality`
*   **Type:** `QualityPreset`
*   **Description:** Determines the compression quality level, balancing file size against visual fidelity.
*   **Remarks:** The specific mapping of this preset to CRF, bitrate, or quantizer values depends on the selected `VideoCodec`.

#### `EnableAutoScale`
*   **Type:** `bool`
*   **Description:** When `true`, automatically calculates output dimensions based on `MaxWidth`, `MaxHeight`, and `ScalingMode` rather than using fixed `Width` and `Height` values.
*   **Remarks:** Useful for ensuring videos do not exceed specific resolution boundaries while maintaining aspect ratio logic.

#### `MaxWidth`
*   **Type:** `int?`
*   **Description:** Defines the maximum allowable width for the output video when `EnableAutoScale` is enabled.
*   **Remarks:** Ignored if `EnableAutoScale` is `false`.

#### `MaxHeight`
*   **Type:** `int?`
*   **Description:** Defines the maximum allowable height for the output video when `EnableAutoScale` is enabled.
*   **Remarks:** Ignored if `EnableAutoScale` is `false`.

#### `ScalingMode`
*   **Type:** `ScalingMode`
*   **Description:** Specifies the algorithm used for resizing video frames (e.g., Bicubic, Lanczos, Nearest Neighbor).
*   **Remarks:** Affects both performance and visual quality during resolution changes.

#### `PreserveAspectRatio`
*   **Type:** `bool`
*   **Description:** When `true`, ensures the output video maintains the original aspect ratio, potentially adjusting calculated dimensions to prevent stretching.
*   **Remarks:** Highly recommended when using `EnableAutoScale` or setting only one dimension (`Width` or `Height`).

#### `EnableAudioNormalization`
*   **Type:** `bool`
*   **Description:** Enables audio volume normalization to adjust the perceived loudness of the output track.
*   **Remarks:** Requires `TargetLoudness` to be set for specific control; otherwise, a default target may be applied.

#### `TargetLoudness`
*   **Type:** `double?`
*   **Description:** Sets the target loudness level in LUFS (Loudness Units Full Scale) when `EnableAudioNormalization` is active.
*   **Remarks:** Common broadcast standards include -23.0 LUFS (EBU R128) or -24.0 LUFS (ATSC A/85).

#### `TwoPass`
*   **Type:** `bool`
*   **Description:** Enables two-pass encoding, where the first pass analyzes the video statistics and the second pass performs the actual encoding.
*   **Remarks:** Increases encoding time significantly but generally yields better quality at lower bitrates.

#### `CustomFFmpegArgs`
*   **Type:** `string?`
*   **Description:** Allows injection of raw, custom FFmpeg command-line arguments that are not covered by the strongly-typed properties.
*   **Remarks:** Use with caution; invalid arguments will cause the transcoding process to fail. These arguments are appended to the generated command.

#### `HardwareAcceleration`
*   **Type:** `HwAccel`
*   **Description:** Specifies the hardware acceleration backend to utilize (e.g., NVENC, QSV, VAAPI) if available on the host system.
*   **Remarks:** If set to a hardware encoder not supported by the current GPU or driver, the operation may fail or fallback to software depending on library configuration.

### Methods

#### `Validate()`
*   **Signature:** `public void Validate()`
*   **Description:** Verifies the internal consistency of the current settings configuration.
*   **Parameters:** None.
*   **Return Value:** `void`.
*   **Exceptions:** Throws an exception (typically `InvalidOperationException` or a custom validation exception) if incompatible settings are detected, such as a codec unsupported by the selected container, invalid dimension values, or conflicting scaling parameters.

#### `Clone()`
*   **Signature:** `public TranscodeSettings Clone()`
*   **Description:** Creates a deep copy of the current `TranscodeSettings` instance.
*   **Parameters:** None.
*   **Return Value:** Returns a new `TranscodeSettings` object with identical property values.
*   **Remarks:** Useful for creating variations of a base configuration without mutating the original instance.

## Usage

### Example 1: Standard H.264 Transcoding with Audio Normalization
This example configures a standard MP4 output using H.264 video and AAC audio, enabling audio normalization to the broadcast standard of -23 LUFS.

```csharp
using FfmpegDotNetWrapper;
using FfmpegDotNetWrapper.Models;

var settings = new TranscodeSettings
{
    VideoCodec = VideoCodec.H264,
    AudioCodec = AudioCodec.Aac,
    Container = ContainerFormat.Mp4,
    Quality = QualityPreset.Medium,
    Width = 1920,
    Height = 1080,
    PreserveAspectRatio = true,
    EnableAudioNormalization = true,
    TargetLoudness = -23.0,
    HardwareAcceleration = HwAccel.None
};

// Validate before passing to the transcoder
settings.Validate();

// Usage with a hypothetical Transcoder service
// await transcoder.ExecuteAsync(inputPath, outputPath, settings);
```

### Example 2: Hardware-Accelerated Scaling with Custom Arguments
This example demonstrates a configuration for hardware-accelerated encoding (NVENC), automatic downscaling to a maximum of 720p while preserving aspect ratio, and the injection of custom FFmpeg arguments for specific pixel formatting.

```csharp
using FfmpegDotNetWrapper;
using FfmpegDotNetWrapper.Models;

var baseSettings = new TranscodeSettings
{
    VideoCodec = VideoCodec.H264,
    AudioCodec = AudioCodec.Aac,
    Container = ContainerFormat.Mp4,
    Quality = QualityPreset.Fast,
    EnableAutoScale = true,
    MaxWidth = 1280,
    MaxHeight = 720,
    ScalingMode = ScalingMode.Bicubic,
    PreserveAspectRatio = true,
    HardwareAcceleration = HwAccel.Nvenc,
    TwoPass = false,
    CustomFFmpegArgs = "-pix_fmt yuv420p -movflags +faststart"
};

// Create a variation for a different output without modifying the base
var highQualityClone = baseSettings.Clone();
highQualityClone.Quality = QualityPreset.Slow;
highQualityClone.TwoPass = true;
highQualityClone.CustomFFmpegArgs = null; // Remove custom args for the high-quality pass

highQualityClone.Validate();
```

## Notes

*   **Validation Logic:** The `Validate()` method must be called prior to executing any transcoding job. It is the sole responsibility of this method to detect logical inconsistencies (e.g., setting `Width` without `Height` when `PreserveAspectRatio` is false, or selecting a codec incompatible with the container). Failure to call this may result in runtime errors from the underlying FFmpeg process that are harder to debug.
*   **Thread Safety:** The `TranscodeSettings` class is not inherently thread-safe for mutation. While multiple threads can read from a single instance simultaneously, properties should not be modified concurrently. The `Clone()` method is provided specifically to allow threads to work on isolated copies of a configuration safely.
*   **Nullable Dimensions:** When `Width` or `Height` is `null`, the behavior depends on `EnableAutoScale`. If auto-scaling is disabled and a dimension is null, `Validate()` will likely throw an exception unless the specific codec/container combination implies a default behavior, which should not be relied upon.
*   **Custom Arguments:** The `CustomFFmpegArgs` string is appended directly to the command line. Users must ensure proper escaping of spaces and special characters within this string, as the wrapper does not perform additional parsing or sanitization on this specific field.
*   **Hardware Acceleration:** Setting `HardwareAcceleration` does not guarantee its usage; it depends on the runtime environment having the necessary drivers and hardware. If the specified hardware encoder is unavailable, the underlying process may fail immediately unless the FFmpeg build includes fallback logic, which is outside the scope of this settings object.
