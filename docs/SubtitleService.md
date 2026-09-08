# SubtitleService

`SubtitleService` is a convenience wrapper around `IFFmpegService` for adding subtitles to media. It creates the appropriate [`SubtitleSettings`](./SubtitleSettings.md), logs the operation, and delegates processing to `IFFmpegService.EmbedSubtitlesAsync`.

## Purpose

Use this service when subtitles should either remain a selectable stream in the output container or be rendered permanently into the video frames.

## Public API

| Member | Description |
| --- | --- |
| `SubtitleService(IFFmpegService ffmpegService, ILogger<SubtitleService> logger)` | Creates the service with the FFmpeg service and logger dependencies. Throws `ArgumentNullException` if either dependency is `null`. |
| `Task<ConversionResult> EmbedSoftSubtitlesAsync(MediaFile inputMedia, string subtitlePath, string outputPath, string? language = null, CancellationToken cancellationToken = default)` | Adds the subtitle file as a selectable subtitle stream and optionally stores an ISO 639-1 language code in its metadata. |
| `Task<ConversionResult> BurnSubtitlesAsync(MediaFile inputMedia, string subtitlePath, string outputPath, string fontName = "Arial", int fontSize = 24, CancellationToken cancellationToken = default)` | Renders subtitles permanently into the video using the requested font name and size. |

## Soft and burned-in subtitles

`EmbedSoftSubtitlesAsync` creates `SubtitleSettings` with `SubtitlePath` set to the supplied file, `HardEmbed` set to `false`, and `Language` set to the optional language argument. The implementation delegates those settings to `IFFmpegService.EmbedSubtitlesAsync`; the video and audio streams are copied without re-encoding, and the resulting subtitle stream can be selected or disabled by the player. The method logs both the start and the success or failure of the operation.

`BurnSubtitlesAsync` creates `SubtitleSettings` with `SubtitlePath` set to the supplied file, `HardEmbed` set to `true`, and `FontName` and `FontSize` set from the method arguments. It then delegates to the same `IFFmpegService.EmbedSubtitlesAsync` operation. Because the subtitles are rendered into the video frames, they are always visible and cannot be disabled as a separate stream. This method logs when the operation starts.

See [`SubtitleSettings`](./SubtitleSettings.md) for the settings type and its other configuration members.

## Usage

```csharp
using FFmpegDotnetWrapper.Models;
using FFmpegDotnetWrapper.Services;
using Microsoft.Extensions.Logging;

// These dependencies are normally supplied by dependency injection.
IFFmpegService ffmpegService = GetFFmpegService();
ILogger<SubtitleService> logger = GetSubtitleLogger();

var subtitles = new SubtitleService(ffmpegService, logger);
var input = new MediaFile("input.mp4");

ConversionResult softResult = await subtitles.EmbedSoftSubtitlesAsync(
    input,
    subtitlePath: "captions.srt",
    outputPath: "output-with-captions.mkv",
    language: "en");

ConversionResult burnedResult = await subtitles.BurnSubtitlesAsync(
    input,
    subtitlePath: "captions.srt",
    outputPath: "output-burned.mp4",
    fontName: "Arial",
    fontSize: 24);
```
