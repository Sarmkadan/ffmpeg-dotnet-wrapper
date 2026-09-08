# ConcatenationBuilder

## Purpose
The `ConcatenationBuilder` class provides a fluent API for constructing video concatenation pipelines. It allows you to define a sequence of video/audio segments, apply optional trimming, set transitions between segments, configure re-encoding options, and build a `MergeSettings` object ready for use with `IFFmpegService.MergeAsync`.

## Fluent API

| Method | Description |
|--------|-------------|
| `Add(string filePath)` | Adds a video segment at the end of the concatenation sequence. |
| `Add(string filePath, TimeSpan? trimStart, TimeSpan? trimEnd, TimeSpan? trimDuration)` | Adds a trimmed video segment at the end of the sequence. Specify either `trimEnd` or `trimDuration`, not both. |
| `Insert(int index, string filePath)` | Inserts a segment at a specific zero-based position in the sequence. |
| `Remove(string filePath)` | Removes all segments matching the specified file path (case-insensitive). |
| `WithTransition(ConcatTransition transition, double duration = 1.0)` | Sets the transition type applied between every pair of consecutive segments. Duration must be greater than zero. |
| `WithReencode(bool reencode = true)` | Controls whether all segments are re-encoded before concatenation. Required when segments differ in codec, resolution, or frame rate. |
| `WithTranscodeSettings(TranscodeSettings settings)` | Supplies custom transcode settings applied during re-encoding. Implies `WithReencode(true)`. |
| `Build()` | Returns a `MergeSettings` object from the accumulated configuration. Requires at least two segments. |
| `Reset()` | Clears all segments and resets options, allowing the builder to be reused. |

## Properties
- `SegmentCount`: Returns the number of segments currently registered.
- `Segments`: Returns a read-only view of the current segment list.

## Example Usage
```csharp
var settings = new ConcatenationBuilder()
    .Add("intro.mp4")
    .Add("main.mp4", trimStart: TimeSpan.FromSeconds(5), trimDuration: TimeSpan.FromMinutes(2))
    .Add("outro.mp4")
    .WithTransition(ConcatTransition.Crossfade, duration: 0.5)
    .WithReencode(true)
    .Build();

await ffmpegService.MergeAsync(settings.InputFiles, "output.mp4", settings);
```

## Related Documentation
- [ConcatenationSegment](ConcatenationSegment.md)
- [ConcatenationBuilderTests](ConcatenationBuilderTests.md)