// ... (rest of README.md content remains unchanged)

## ServiceCollectionExtensions

The `ServiceCollectionExtensions` class provides extension methods for registering FFmpeg wrapper services in the Microsoft.Extensions.DependencyInjection container. These methods allow you to easily integrate FFmpeg functionality into your .NET applications using dependency injection patterns.

```csharp
using FFmpegDotnetWrapper.Configuration;
using Microsoft.Extensions.DependencyInjection;

// Example: register FFmpeg wrapper services with default configuration
var services = new ServiceCollection();
services.AddFFmpegWrapper();

// Example: register with custom configuration
services.AddFFmpegWrapper(opts =>
{
    opts.DefaultTimeout = TimeSpan.FromSeconds(600);
    opts.FFmpegPath = "/usr/local/bin/ffmpeg";
    opts.FFprobePath = "/usr/local/bin/ffprobe";
    opts.LogLevel = LogLevel.Debug;
    opts.EnableOperationCaching = true;
    opts.MaxCachedOperations = 500;
    opts.EnableDetailedLogging = true;
});

// Example: register with pre-configured options
var options = new FFmpegWrapperOptions
{
    DefaultTimeout = TimeSpan.FromSeconds(300),
    FFmpegPath = "/usr/bin/ffmpeg",
    FFprobePath = "/usr/bin/ffprobe",
    LogLevel = LogLevel.Warning,
    EnableOperationCaching = false,
    MaxCachedOperations = 100,
    EnableDetailedLogging = false
};
services.AddFFmpegWrapper(options);
```

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

