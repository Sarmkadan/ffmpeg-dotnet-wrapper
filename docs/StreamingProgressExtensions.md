# StreamingProgressExtensions

Extension methods for registering streaming progress services in the Microsoft.Extensions.DependencyInjection container.

## API

### `AddStreamingProgress(IServiceCollection services)`

Adds <see cref="IStreamingProgressService"/> to the DI container as a singleton.
The service holds no per-operation state; all processing state is stack-local within each
<see cref="IStreamingProgressService.StreamProgressAsync"/> call, making it safe for concurrent use.

**Parameters:**
- `services`: The <see cref="IServiceCollection"/> instance to register services with.

**Return value:**
- Returns the <see cref="IServiceCollection"/> for method chaining.

**Exceptions:**
- Throws <see cref="ArgumentNullException"/> if `services` is `null`.

---

### `AddFFmpegWrapperWithStreaming(IServiceCollection services, Action<FFmpegWrapperOptions>? configureOptions = null)`

Adds FFmpeg wrapper services and <see cref="IStreamingProgressService"/> in a single call.
Convenience method for applications that require both FFmpeg execution and progress tracking.

**Parameters:**
- `services`: The <see cref="IServiceCollection"/> instance to register services with.
- `configureOptions`: An optional delegate to configure <see cref="FFmpegWrapperOptions"/>.

**Return value:**
- Returns the <see cref="IServiceCollection"/> for method chaining.

**Exceptions:**
- Throws <see cref="ArgumentNullException"/> if `services` is `null`.

## Usage

### Basic registration of streaming progress service
```csharp
var services = new ServiceCollection();
services.AddStreamingProgress();
```

### Registration with FFmpeg wrapper and streaming progress
```csharp
var services = new ServiceCollection();
services.AddFFmpegWrapperWithStreaming(options =>
{
    options.FfmpegBinaryPath = "/usr/bin/ffmpeg";
    options.FfprobeBinaryPath = "/usr/bin/ffprobe";
});
```

## Notes

- The `AddStreamingProgress` method registers <see cref="IStreamingProgressService"/> as a singleton; the service is stateless and safe for concurrent use.
- The `AddFFmpegWrapperWithStreaming` method combines FFmpeg wrapper registration (via <see cref="FFmpegWrapperExtensions.AddFFmpegWrapper"/>) with streaming progress registration.
- All extension methods are thread-safe for concurrent calls.
- Exceptions for null arguments are thrown during registration to fail fast.