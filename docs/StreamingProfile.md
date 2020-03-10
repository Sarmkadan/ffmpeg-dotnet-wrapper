# StreamingProfile

`StreamingProfile` is an immutable record that represents a single encoding or transmuxing operation within a streaming pipeline. It captures the source profile, the target profile, the reason for the transition, the resulting file metadata, and optional hardware-acceleration settings. Instances are typically produced by pipeline orchestration logic and consumed by monitoring, logging, or downstream processing components.

## API

### Properties

- **`Id`** `string` (required)  
  Uniquely identifies this profile instance across the system. Must be non‑null and non‑empty; assigned at creation and used for correlation in logs and state tracking.

- **`PipelineId`** `string` (required)  
  Identifies the parent pipeline that owns this profile. Must match the `Id` of an active `Pipeline`. Duplicated in the member list intentionally — the property is declared once and is required.

- **`Profile`** `StreamingProfile` (required)  
  The concrete profile definition (resolution, bitrate, codec parameters) applied for this operation. Must not be `null`.

- **`SequenceNumber`** `int` (required)  
  Monotonically increasing ordinal within the pipeline, used to order profiles chronologically and detect gaps.

- **`FilePath`** `string` (required)  
  Absolute or relative path to the output media file produced by this profile. The path is set before encoding begins; the file may not exist until the operation completes successfully.

- **`DurationSeconds`** `double`  
  Duration of the output media in seconds. Defaults to `0` when the profile has not yet completed or when duration extraction fails.

- **`FileSizeBytes`** `long`  
  Size of the output file in bytes. Defaults to `0` before the file is written or when the file is absent.

- **`EncodedAt`** `DateTimeOffset`  
  Timestamp when encoding finished successfully. Set to `default(DateTimeOffset)` if encoding has not completed.

- **`OccurredAt`** `DateTimeOffset`  
  Timestamp when this profile transition was requested or triggered. Always populated at creation.

- **`FromProfile`** `StreamingProfile` (required)  
  The source profile from which this operation originates. Must not be `null`. In a chain, this points to the previous profile; for the initial profile it may reference a sentinel “source” profile.

- **`ToProfile`** `StreamingProfile` (required)  
  The target profile that this operation produces. Must not be `null`. Represents the desired output specification.

- **`Reason`** `string` (required)  
  Human-readable or machine‑readable cause for the transition (e.g. `"quality_adaptation"`, `"manual_override"`, `"failover"`). Must not be `null`; empty string is permitted but discouraged.

- **`Format`** `StreamingFormat`  
  The container or streaming format (e.g. HLS, DASH, MP4) applied to the output. May be `null` when the format is inherited from the pipeline defaults.

- **`Profiles`** `IList<StreamingProfile>`  
  An ordered collection of sub‑profiles or alternative renditions generated alongside this profile. May be empty but never `null`. Modifications to the list after the record is constructed are discouraged because the record itself is immutable; consumers should treat the list as read‑only.

- **`EnableHardwareAcceleration`** `bool`  
  When `true`, instructs the encoder to use available GPU‑based acceleration (e.g. NVENC, VAAPI). Defaults to `false`. Requires compatible hardware and drivers; otherwise encoding falls back to software.

- **`EncodeProfilesConcurrently`** `bool`  
  When `true`, allows the pipeline to process multiple profiles in `Profiles` simultaneously. Defaults to `false`. Ignored when `Profiles` contains fewer than two items.

- **`State`** `PipelineState`  
  Current lifecycle state of this profile (e.g. `Pending`, `Running`, `Completed`, `Failed`). Consumers must check this before relying on `DurationSeconds`, `FileSizeBytes`, or `EncodedAt`.

### Methods

- **`void Validate()`**  
  Performs an immediate, synchronous validation of required fields and cross‑field consistency.  
  **Throws** `ArgumentException` or a derived exception when:
  - `Id` or `PipelineId` is `null` or whitespace.
  - `Profile`, `FromProfile`, or `ToProfile` is `null`.
  - `Reason` is `null`.
  - `SequenceNumber` is negative.
  - `FilePath` is `null` or contains invalid characters for the target OS.
  - `Profiles` is `null`.
  - `State` is an unrecognised value.  
  Does **not** validate hardware‑acceleration availability or file existence — those are runtime concerns.

## Usage

### Example 1: Creating and validating a profile

```csharp
var sourceProfile = new StreamingProfile
{
    Id = "src-001",
    PipelineId = "pipe-42",
    Profile = baselineProfile,
    SequenceNumber = 0,
    FilePath = "/media/source.mp4",
    FromProfile = baselineProfile,   // self-referencing for origin
    ToProfile = baselineProfile,
    Reason = "source",
    State = PipelineState.Completed
};

var encodeProfile = new StreamingProfile
{
    Id = "enc-001",
    PipelineId = "pipe-42",
    Profile = hdProfile,
    SequenceNumber = 1,
    FilePath = "/media/output_hd.mkv",
    FromProfile = sourceProfile,
    ToProfile = hdProfile,
    Reason = "quality_ladder",
    Format = StreamingFormat.MpegTs,
    EnableHardwareAcceleration = true,
    EncodeProfilesConcurrently = false,
    State = PipelineState.Pending,
    Profiles = new List<StreamingProfile> { sdProfile, mobileProfile }
};

encodeProfile.Validate();   // throws if any required field is missing
```

### Example 2: Monitoring completion and extracting metadata

```csharp
if (encodeProfile.State == PipelineState.Completed)
{
    Console.WriteLine(
        $"Profile '{encodeProfile.Id}' finished at {encodeProfile.EncodedAt:O}, " +
        $"duration: {encodeProfile.DurationSeconds:F2}s, " +
        $"size: {encodeProfile.FileSizeBytes} bytes");
}
else if (encodeProfile.State == PipelineState.Failed)
{
    Console.WriteLine(
        $"Profile '{encodeProfile.Id}' failed. Reason: {encodeProfile.Reason}");
}
```

## Notes

- **Immutability**: `StreamingProfile` is a `record`; once constructed, its property values should not change. The `Profiles` list is a reference type — consumers should avoid mutating it to preserve logical immutability.
- **Thread safety**: Read operations on a fully constructed instance are safe across threads. Concurrent writes to the `Profiles` list or mutation through reflection break this guarantee. `Validate()` is not thread‑safe with respect to the instance being mutated concurrently.
- **Validation scope**: `Validate()` checks structural integrity only. It does not verify that `FromProfile` and `ToProfile` form a valid DAG, that `FilePath` is writable, or that hardware acceleration is actually available. Those checks belong to the pipeline executor.
- **Default timestamps**: `EncodedAt` remains `default(DateTimeOffset)` until encoding succeeds. Do not interpret `0001-01-01T00:00:00+00:00` as a real timestamp.
- **Hardware acceleration**: Setting `EnableHardwareAcceleration = true` without a usable GPU causes the encoding step to log a warning and fall back to software; it does not throw during `Validate()`.
- **Concurrent encoding**: `EncodeProfilesConcurrently` is a hint to the scheduler. Resource limits (CPU, GPU, memory) may cause the scheduler to serialise work regardless of this flag.
