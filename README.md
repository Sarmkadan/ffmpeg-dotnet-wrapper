// ... (rest of README.md content remains unchanged)

## TranscodeService

The `TranscodeService` class provides a set of methods for transcoding media files to various formats. It supports transcoding to web, H.265, mobile, and high-quality formats, as well as extracting audio and resizing video. 

```csharp
using FFmpegDotnetWrapper.Services;
using FFmpegDotnetWrapper.Models;

// Create TranscodeService instance
var transcodeService = new TranscodeService(new FFmpegService(), new Logger<TranscodeService>(new LoggerFactory()));

// Transcode to web format
var webResult = await transcodeService.TranscodeToWebAsync(new MediaFile { Name = "sample_video.mp4", FilePath = "/path/to/sample_video.mp4" }, "/path/to/output/web.mp4");

// Extract audio from video
var audioResult = await transcodeService.ExtractAudioAsync(new MediaFile { Name = "sample_video.mp4", FilePath = "/path/to/sample_video.mp4" }, "/path/to/output/audio.mp3");

// Resize video to specific resolution
var resizeResult = await transcodeService.ResizeVideoAsync(new MediaFile { Name = "sample_video.mp4", FilePath = "/path/to/sample_video.mp4" }, "/path/to/output/resized.mp4", 1280, 720);
```

## JsonOutputFormatter

`JsonOutputFormatter` centralises JSON serialization and deserialization for API responses, offering pretty‑printed output and custom converters for `TimeSpan` and `DateTime`. It also bundles CSV and plain‑text formatters for batch operation results, giving a consistent way to produce machine‑readable and human‑readable output.

```csharp
using System;
using System.Collections.Generic;
using FFmpegDotnetWrapper.Api.DTOs;
using FFmpegDotnetWrapper.Models;
using FFmpegDotnetWrapper.Serialization;

public class FormatterDemo
{
    public void Run()
    {
        // Initialise the JSON formatter (indented output)
        var jsonFormatter = new JsonOutputFormatter(indent: true);

        // Example API response containing a MediaFile
        var apiResponse = new ApiResponse<MediaFile>
        {
            Success = true,
            StatusCode = 200,
            Message = "File retrieved",
            Data = new MediaFile { Name = "sample.mp4", FilePath = "/videos/sample.mp4" }
        };

        // Serialize the response to JSON
        string json = jsonFormatter.Format(apiResponse);
        Console.WriteLine("JSON output:");
        Console.WriteLine(json);

        // Deserialize the JSON back to an ApiResponse<MediaFile>
        var deserialized = jsonFormatter.DeserializeApiResponse<MediaFile>(json);
        Console.WriteLine($"Deserialized success: {deserialized?.Success}");

        // Serialize an arbitrary object
        var anon = new { Greeting = "Hello", Timestamp = DateTime.UtcNow };
        string anonJson = jsonFormatter.Format(anon);
        Console.WriteLine("Anonymous object JSON:");
        Console.WriteLine(anonJson);

        // CSV formatter usage for batch conversion results
        var csvFormatter = new CsvOutputFormatter();
        var conversionResults = new List<ConversionResult>
        {
            new ConversionResult
            {
                InputFile = "video1.mp4",
                OutputFile = "video1.webm",
                Success = true,
                Duration = 12.5,
                ExecutionTime = TimeSpan.FromSeconds(13)
            },
            new ConversionResult
            {
                InputFile = "video2.mp4",
                OutputFile = "video2.webm",
                Success = false,
                ErrorMessage = "Unsupported codec"
            }
        };
        string csv = csvFormatter.FormatResults(conversionResults);
        Console.WriteLine("CSV output:");
        Console.WriteLine(csv);

        // Plain‑text formatter for a readable summary
        var textFormatter = new PlainTextFormatter();
        string plain = textFormatter.Format(apiResponse);
        Console.WriteLine("Plain‑text output:");
        Console.WriteLine(plain);
    }
}
```
