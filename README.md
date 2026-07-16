// ... (rest of README.md content remains unchanged)

## WatermarkSettings

Configuration settings for applying watermarks to videos. This class allows you to specify the position, size, and animation of the watermark, as well as its duration and start time.

```csharp
// Create watermark settings for a video
var watermarkSettings = new WatermarkSettings
{
    Position = WatermarkPosition.TopRight,
    XOffset = 10,
    YOffset = 10,
    Scale = 0.2,
    PreserveAspectRatio = true,
    StartTime = TimeSpan.FromSeconds(5),
    Duration = TimeSpan.FromSeconds(10),
    AnimateIn = true,
    AnimateInDuration = TimeSpan.FromSeconds(2)
};

// Validate the watermark settings against a media file
var inputMedia = new MediaFile { Duration = TimeSpan.FromSeconds(10) };
watermarkSettings.Validate(inputMedia);

// Calculate the watermark position coordinates
var (x, y) = watermarkSettings.CalculatePosition(1920, 1080);

// Clone the watermark settings for reuse with different parameters
var clonedSettings = watermarkSettings.Clone();
clonedSettings.XOffset = 20;
clonedSettings.YOffset = 20;
```

// ... (rest of README.md content remains unchanged)
