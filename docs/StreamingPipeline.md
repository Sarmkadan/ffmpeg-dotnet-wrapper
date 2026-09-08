# Streaming pipeline models

The types in `FFmpegDotnetWrapper.Models` describe adaptive-bitrate streaming formats, pipeline lifecycle state, encoded segments, bitrate changes, pipeline configuration, and the accumulated result of a run. Rendition settings are represented by [`StreamingProfile`](StreamingProfile.md).

## `StreamingFormat`

Selects the output container and manifest format for a streaming pipeline.

| Member | Value | Purpose |
| --- | ---: | --- |
| `Hls` | `0` | Produces HTTP Live Streaming `.m3u8` playlists and `.ts` segments. |
| `Dash` | `1` | Produces DASH `.mpd` manifests and `.m4s` segments. |

## `PipelineState`

Reports the lifecycle state of a pipeline run.

| Member | Value | Purpose |
| --- | ---: | --- |
| `Initializing` | `0` | Output directories and playlists are being created. |
| `Running` | `1` | The pipeline is encoding and emitting segments. |
| `Completed` | `2` | All configured profiles were encoded successfully. |
| `Failed` | `3` | An unrecoverable encoding error ended the pipeline. |
| `Cancelled` | `4` | The pipeline was stopped through cancellation. |

## `StreamingSegment`

Represents one encoded media segment produced for a rendition.

| Public member | Type | Access/default | Purpose |
| --- | --- | --- | --- |
| `Id` | `string` | required, `init` | Unique segment identifier. |
| `PipelineId` | `string` | required, `init` | Identifier of the pipeline that produced the segment. |
| `Profile` | `StreamingProfile` | required, `init` | Rendition profile used to encode the segment. |
| `SequenceNumber` | `int` | required, `init` | Zero-based index within the rendition playlist. |
| `FilePath` | `string` | required, `init` | Absolute path to the segment file. |
| `DurationSeconds` | `double` | `init`; default `0` | Nominal segment duration in seconds. |
| `FileSizeBytes` | `long` | get/set; default `0` | Segment file size in bytes. |
| `EncodedAt` | `DateTimeOffset` | `init`; defaults to `DateTimeOffset.UtcNow` | UTC time at which encoding completed. |
| `ActualBitrateKbps` | `double` | get-only, computed | Achieved bitrate calculated as `(FileSizeBytes * 8) / (DurationSeconds * 1000)`, or `0` when duration is zero. |

## `BitrateSwitch`

An immutable record describing a change from one rendition profile to another.

| Public member | Type | Access/default | Purpose |
| --- | --- | --- | --- |
| `OccurredAt` | `DateTimeOffset` | `init`; defaults to `DateTimeOffset.UtcNow` | UTC time at which the switch was triggered. |
| `FromProfile` | `StreamingProfile` | required, `init` | Profile active before the switch. |
| `ToProfile` | `StreamingProfile` | required, `init` | Profile active after the switch. |
| `Reason` | `string` | required, `init` | Human-readable reason for the switch. |
| `IsUpgrade` | `bool` | get-only, computed | `true` when `ToProfile.VideoBitrateKbps` is greater than `FromProfile.VideoBitrateKbps`. |

## `StreamingPipelineSettings`

Configures one pipeline run. Call `Validate()` before starting the pipeline.

| Public member | Type | Access/default | Purpose |
| --- | --- | --- | --- |
| `InputFilePath` | `string` | required, get/set | Absolute source-media path. Assignment throws `ArgumentException` for null, empty, or whitespace input. |
| `OutputDirectory` | `string` | required, get/set | Directory for segments, rendition playlists, and the master manifest. Assignment throws `ArgumentException` for null, empty, or whitespace input. |
| `Format` | `StreamingFormat` | get/set; `Hls` | Output manifest format. |
| `Profiles` | `IList<StreamingProfile>` | get/set; copy of `StreamingProfile.DefaultLadder` | Renditions to encode. The pipeline sorts them from highest to lowest video bitrate before encoding. |
| `SegmentDurationSeconds` | `int` | get/set; `6` | Target duration from 1 through 60 seconds. Assignment outside that range throws `ArgumentOutOfRangeException`. |
| `PlaylistWindowSize` | `int` | get/set; `5` | Number of retained live-playlist segments; `0` selects VOD mode. A negative value throws `ArgumentOutOfRangeException`. |
| `EnableHardwareAcceleration` | `bool` | get/set; `false` | Whether FFmpeg hardware acceleration should be attempted. |
| `EncodeProfilesConcurrently` | `bool` | get/set; `true` | Whether renditions are encoded concurrently; when `false`, they are encoded sequentially from highest to lowest quality. |
| `Validate()` | `void` | method | Throws `FileNotFoundException` if `InputFilePath` does not exist and `InvalidOperationException` if `Profiles` is empty. |

## `StreamingPipelineResult`

Holds the live state and accumulated output of a pipeline run. Segment and switch additions use concurrent collections; their enumeration order is not guaranteed.

| Public member | Type | Access/default | Purpose |
| --- | --- | --- | --- |
| `PipelineId` | `string` | required, `init` | Globally unique identifier for the run. |
| `State` | `PipelineState` | get/set; `Initializing` | Current pipeline lifecycle state. |
| `StartedAt` | `DateTimeOffset` | `init`; defaults to `DateTimeOffset.UtcNow` | UTC start time. |
| `EndedAt` | `DateTimeOffset?` | get/set; `null` | UTC end time, or `null` while the run is active. |
| `ActiveProfile` | `StreamingProfile?` | get/set; `null` | Profile currently recommended by the adaptive-bitrate assessment. |
| `MasterPlaylistPath` | `string?` | get/set; `null` | Path to the HLS master playlist or DASH manifest. |
| `ErrorMessage` | `string?` | get/set; `null` | Error that caused a failed state. |
| `Segments` | `IReadOnlyCollection<StreamingSegment>` | get-only | Segments recorded so far; ordering is not guaranteed. |
| `BitrateSwitches` | `IReadOnlyCollection<BitrateSwitch>` | get-only | Recorded adaptive-bitrate switch events; ordering is not guaranteed by the underlying collection. |
| `Elapsed` | `TimeSpan` | get-only, computed | Time from `StartedAt` to `EndedAt`, or to the current UTC time while active. |
| `AddSegment(StreamingSegment segment)` | `void` | method | Adds a completed segment to `Segments`. |
| `RecordSwitch(BitrateSwitch bitrateSwitch)` | `void` | method | Adds a switch event to `BitrateSwitches`. |

## Usage

```csharp
using FFmpegDotnetWrapper.Models;

var settings = new StreamingPipelineSettings
{
    InputFilePath = "/media/input.mp4",
    OutputDirectory = "/media/output",
    Format = StreamingFormat.Hls,
    Profiles = [StreamingProfile.HD, StreamingProfile.SD],
    SegmentDurationSeconds = 6,
    PlaylistWindowSize = 0
};

settings.Validate();

var result = new StreamingPipelineResult
{
    PipelineId = Guid.NewGuid().ToString("N"),
    State = PipelineState.Running,
    ActiveProfile = StreamingProfile.HD
};

var segment = new StreamingSegment
{
    Id = "segment-0001",
    PipelineId = result.PipelineId,
    Profile = StreamingProfile.HD,
    SequenceNumber = 0,
    FilePath = "/media/output/720p/segment-0001.ts",
    DurationSeconds = 6,
    FileSizeBytes = 1_875_000
};

result.AddSegment(segment);
Console.WriteLine($"{segment.ActualBitrateKbps:N0} kbps");
```
