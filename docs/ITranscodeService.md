# Transcoding service interfaces

`ITranscodeService` defines high-level, asynchronous operations for common file-based transcoding tasks. Callers provide a `MediaFile`, an output path, and optionally a cancellation token; each operation returns a `ConversionResult`.

`IAdaptiveBitrateService` defines the lifecycle of adaptive bitrate streaming pipelines. It can prepare manifests, encode one or more renditions as asynchronous streams of segments, report pipeline state, and request cancellation of active pipelines.

## `ITranscodeService`

| Method | Purpose |
| --- | --- |
| `TranscodeToWebAsync(MediaFile inputMedia, string outputPath, CancellationToken cancellationToken = default)` | Produces an H.264 web-optimized output. |
| `TranscodeToH265Async(MediaFile inputMedia, string outputPath, CancellationToken cancellationToken = default)` | Produces an H.265 output for improved compression. |
| `TranscodeToMobileAsync(MediaFile inputMedia, string outputPath, CancellationToken cancellationToken = default)` | Produces a mobile-friendly output. |
| `TranscodeToHighQualityAsync(MediaFile inputMedia, string outputPath, CancellationToken cancellationToken = default)` | Produces a high-quality archival output. |
| `TranscodeWithBitrateAsync(MediaFile inputMedia, string outputPath, int videoBitrate, int audioBitrate, CancellationToken cancellationToken = default)` | Produces an output with caller-specified video and audio bitrates. |
| `ExtractAudioAsync(MediaFile inputMedia, string outputPath, AudioCodec audioCodec = AudioCodec.MP3, CancellationToken cancellationToken = default)` | Extracts the audio stream using the selected codec. |
| `ResizeVideoAsync(MediaFile inputMedia, string outputPath, int width, int height, CancellationToken cancellationToken = default)` | Resizes video to the requested dimensions. |

All methods return `Task<ConversionResult>`. Bitrate values are expressed in kilobits per second, and dimensions are expressed in pixels.

## `IAdaptiveBitrateService`

| Member | Purpose |
| --- | --- |
| `IReadOnlyCollection<string> ActivePipelineIds` | Returns a snapshot of identifiers for currently running pipelines. |
| `RunPipelineAsync(StreamingPipelineSettings settings, CancellationToken cancellationToken = default)` | Initializes and runs every configured rendition, yielding each encoded `StreamingSegment`. |
| `InitialisePipelineAsync(StreamingPipelineSettings settings, CancellationToken cancellationToken = default)` | Creates the output structure and master manifest without starting encoding; returns the manifest's absolute path. |
| `EncodeRenditionAsync(StreamingPipelineSettings settings, StreamingProfile profile, CancellationToken cancellationToken = default)` | Encodes one rendition and yields its segments as they are written. |
| `GetPipelineResultAsync(string pipelineId)` | Returns the accumulated `StreamingPipelineResult`, or `null` when the identifier is unknown. |
| `CancelPipelineAsync(string pipelineId)` | Requests cancellation and returns whether an active pipeline with that identifier was found. |

`RunPipelineAsync` and `EncodeRenditionAsync` return `IAsyncEnumerable<StreamingSegment>`. Cancelling enumeration stops in-progress work; `CancelPipelineAsync` provides identifier-based cancellation and may leave partially written segments on disk.

## Usage

The interfaces are typically resolved from dependency injection:

```csharp
using FFmpegDotnetWrapper.Models;
using FFmpegDotnetWrapper.Services;

public sealed class VideoProcessor(
    ITranscodeService transcoder,
    IAdaptiveBitrateService adaptiveBitrate)
{
    public async Task ProcessAsync(string inputPath, CancellationToken cancellationToken)
    {
        var input = new MediaFile(inputPath);
        ConversionResult result = await transcoder.TranscodeToWebAsync(
            input,
            "output/web.mp4",
            cancellationToken);

        if (!result.IsSuccess)
            throw new InvalidOperationException(result.ErrorMessage);

        var settings = new StreamingPipelineSettings
        {
            InputFilePath = inputPath,
            OutputDirectory = "output/stream",
            Profiles = [StreamingProfile.HD, StreamingProfile.Mobile]
        };

        await foreach (StreamingSegment segment in
            adaptiveBitrate.RunPipelineAsync(settings, cancellationToken))
        {
            Console.WriteLine($"Encoded {segment.Profile.Name}: {segment.FilePath}");
        }
    }
}
```

## Implementations

- [`TranscodeService`](TranscodeService.md)
- [`AdaptiveBitrateService`](AdaptiveBitrateService.md)
