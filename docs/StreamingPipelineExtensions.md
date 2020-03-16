# StreamingPipelineExtensions

Extension methods for registering adaptive bitrate streaming pipelines in the Microsoft.Extensions.DependencyInjection container.

## API

### `AddAdaptiveBitrateStreaming(IServiceCollection services, Action<AdaptiveBitrateStreamingOptions> configureOptions)`

Registers services required for adaptive bitrate streaming using FFmpeg.

**Parameters:**
- `services`: The `IServiceCollection` instance to register services with.
- `configureOptions`: An optional action to configure `AdaptiveBitrateStreamingOptions`.

**Return value:**
- Returns the `IServiceCollection` for method chaining.

**Exceptions:**
- Throws `ArgumentNullException` if `services` is `null`.

---

### `AddAdaptiveBitrateStreaming(IServiceCollection services, string ffmpegPath, Action<AdaptiveBitrateStreamingOptions> configureOptions)`

Registers services required for adaptive bitrate streaming using FFmpeg with a specified binary path.

**Parameters:**
- `services`: The `IServiceCollection` instance to register services with.
- `ffmpegPath`: The file system path to the FFmpeg executable.
- `configureOptions`: An optional action to configure `AdaptiveBitrateStreamingOptions`.

**Return value:**
- Returns the `IServiceCollection` for method chaining.

**Exceptions:**
- Throws `ArgumentNullException` if `services` is `null`.
- Throws `ArgumentNullException` if `ffmpegPath` is `null` or empty.

---

### `AddAdaptiveBitrateStreaming(IServiceCollection services, Func<string> ffmpegPathProvider, Action<AdaptiveBitrateStreamingOptions> configureOptions)`

Registers services required for adaptive bitrate streaming using FFmpeg with a dynamic binary path provider.

**Parameters:**
- `services`: The `IServiceCollection` instance to register services with.
- `ffmpegPathProvider`: A function that returns the file system path to the FFmpeg executable.
- `configureOptions`: An optional action to configure `AdaptiveBitrateStreamingOptions`.

**Return value:**
- Returns the `IServiceCollection` for method chaining.

**Exceptions:**
- Throws `ArgumentNullException` if `services` is `null`.
- Throws `ArgumentNullException` if `ffmpegPathProvider` is `null`.

---
### `AddAdaptiveBitrateStreaming(IServiceCollection services, string ffmpegPath, string ffprobePath, Action<AdaptiveBitrateStreamingOptions> configureOptions)`

Registers services required for adaptive bitrate streaming using FFmpeg with explicit paths for both FFmpeg and FFprobe binaries.

**Parameters:**
- `services`: The `IServiceCollection` instance to register services with.
- `ffmpegPath`: The file system path to the FFmpeg executable.
- `ffprobePath`: The file system path to the FFprobe executable.
- `configureOptions`: An optional action to configure `AdaptiveBitrateStreamingOptions`.

**Return value:**
- Returns the `IServiceCollection` for method chaining.

**Exceptions:**
- Throws `ArgumentNullException` if `services` is `null`.
- Throws `ArgumentNullException` if `ffmpegPath` is `null` or empty.
- Throws `ArgumentNullException` if `ffprobePath` is `null` or empty.

## Usage

### Basic registration
```csharp
var services = new ServiceCollection();
services.AddAdaptiveBitrateStreaming(options =>
{
    options.InputFilePath = "input.mp4";
    options.OutputTemplates = new[] { "output_{bitrate}.mp4" };
});
```

### Registration with explicit FFmpeg paths
```csharp
var services = new ServiceCollection();
services.AddAdaptiveBitrateStreaming(
    ffmpegPath: "/usr/bin/ffmpeg",
    ffprobePath: "/usr/bin/ffprobe",
    configureOptions: options =>
    {
        options.InputFilePath = "input.mp4";
        options.OutputTemplates = new[] { "output_{bitrate}.mp4" };
    });
```

## Notes

- All overloads are thread-safe for concurrent calls to `AddAdaptiveBitrateStreaming`.
- The `ffmpegPathProvider` overload defers path resolution until the first pipeline activation; ensure the provider remains valid for the application lifetime.
- Path validation occurs during registration, not during pipeline activation; invalid paths will cause failures at service resolution time rather than registration time.
- The `AdaptiveBitrateStreamingOptions` instance is registered as a singleton; modifications after registration do not affect resolved pipelines.
