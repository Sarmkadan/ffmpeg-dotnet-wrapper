// ... (rest of README.md content remains unchanged)

## FFmpegOptions

The `FFmpegOptions` class holds configuration for how the FFmpeg wrapper interacts with the FFmpeg binary and its environment. It allows you to specify paths, timeouts, quality settings, and runtime behaviour such as hardware acceleration and concurrency limits.

```csharp
using FFmpegDotnetWrapper.Configuration;
using System.Collections.Generic;

// Example: configure FFmpeg options and register the wrapper
var services = new ServiceCollection();
services.AddFFmpegWrapperWithConfiguration(
    ffmpegOptions: opts =>
    {
        opts.FFmpegPath = "/usr/bin/ffmpeg";
        opts.FFprobePath = "/usr/bin/ffprobe";
        opts.OperationTimeoutSeconds = 1200;
        opts.MaxFileSizeBytes = 10L * 1024 * 1024 * 1024; // 10 GB
        opts.EnableHardwareAcceleration = true;
        opts.EncodingPreset = "fast";
        opts.KeepTemporaryFiles = false;
        opts.TemporaryDirectory = "/tmp/ffmpeg";
        opts.VerboseLogging = true;
        opts.DefaultQuality = 22;
        opts.DefaultAudioBitrate = 128;
        opts.DefaultVideoBitrate = 2000;
        opts.AllowConcurrentOperations = true;
        opts.MaxConcurrentOperations = 4;
        opts.SupportedFormats = new List<string> { "mp4", "mkv" };
        opts.ValidatePaths = true;
        opts.ValidateOutputPath = true;
        opts.RetryAttempts = 3;
        opts.RetryDelayMs = 2000;
        opts.Enabled = true;
    },
    cachingOptions: null,
    rateLimitingOptions: null);
```

