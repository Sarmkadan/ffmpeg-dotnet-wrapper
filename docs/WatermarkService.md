# WatermarkService

Convenience service for common watermark scenarios. Wraps `IFFmpegService` with pre-configured `WatermarkSettings`.

## Purpose
Provides simplified methods for applying watermarks to videos using predefined positions or custom settings. The service handles validation, logging, and delegates the actual watermarking operation to `IFFmpegService`.

## Public API

### IWatermarkService Interface

| Method | Description |
|--------|-------------|
| `Task<ConversionResult> ApplyWatermarkAsync(MediaFile inputMedia, string outputPath, string watermarkPath, WatermarkPosition position = WatermarkPosition.TopRight, int margin = 10, double opacity = 0.8, double scale = 0.2, CancellationToken cancellationToken = default)` | Applies a watermark to a video at the specified corner position. |
| `Task<ConversionResult> ApplyWatermarkAsync(MediaFile inputMedia, string outputPath, WatermarkSettings settings, CancellationToken cancellationToken = default)` | Applies a watermark to a video with custom watermark settings. |
| `WatermarkSettings CreateSettings(WatermarkPosition position = WatermarkPosition.TopRight, int margin = 10, double opacity = 0.8, double scale = 0.2)` | Creates a watermark settings object configured for the specified corner position. |

### WatermarkService Class

| Member | Description |
|--------|-------------|
| `WatermarkService(IFFmpegService ffmpegService, ILogger<WatermarkService> logger)` | Constructor that initializes the service with required dependencies. |
| `ApplyWatermarkAsync(...)` | Implements both interface methods with argument validation, logging, and error handling. |
| `CreateSettings(...)` | Creates and returns a configured `WatermarkSettings` instance. |

## Exceptions

- `ArgumentNullException`: Thrown when `ffmpegService` or `logger` is null in the constructor; or when `inputMedia`, `outputPath`, or `watermarkPath` (first overload) is null; or when `settings` is null (second overload).
- `ArgumentException`: Thrown when `outputPath` or `watermarkPath` (first overload) is empty or whitespace.
- `ArgumentOutOfRangeException`: Thrown when `margin` is negative; `opacity` is outside [0.0, 1.0]; or `scale` is outside (0.0, 1.0].
- `InvalidOperationException`: May be thrown by `settings.Validate(inputMedia)` in the second overload (see [WatermarkSettings.md](./WatermarkSettings.md) for validation rules).

## Usage Example

### Basic Watermark Application

```csharp
// Dependencies (typically injected via DI)
var ffmpegService = new FFmpegService(...);
var logger = new Logger<WatermarkService>(...);

var watermarkService = new WatermarkService(ffmpegService, logger);

// Apply watermark with default settings (TopRight position)
var result = await watermarkService.ApplyWatermarkAsync(
    inputMedia: new MediaFile("input.mp4"),
    outputPath: "output.mp4",
    watermarkPath: "watermark.png"
);

// Apply watermark with custom position and values
var result = await watermarkService.ApplyWatermarkAsync(
    inputMedia: new MediaFile("input.mp4"),
    outputPath: "output.mp4",
    watermarkPath: "watermark.png",
    position: WatermarkPosition.BottomLeft,
    margin: 20,
    opacity: 0.9,
    scale: 0.15
);
```

### Using Custom WatermarkSettings

```csharp
var watermarkService = new WatermarkService(ffmpegService, logger);

// Create base settings using helper method
var settings = watermarkService.CreateSettings(
    position: WatermarkPosition.TopCenter,
    margin: 15,
    opacity: 0.7,
    scale: 0.2
);

// Set the watermark image path (CreateSettings does not set this)
settings.WatermarkPath = "watermark.png";

// Apply watermark with custom settings
var result = await watermarkService.ApplyWatermarkAsync(
    inputMedia: new MediaFile("input.mp4"),
    outputPath: "output.mp4",
    settings: settings
);
```

## See Also
- [WatermarkSettings](./WatermarkSettings.md) - Detailed documentation of watermark configuration options