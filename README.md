// ... (rest of README.md content remains unchanged)

## ThumbnailSettings

Configuration settings for extracting thumbnail images from a video file. Supports extracting frames at specific timestamps or evenly distributed across the video.

```csharp
// Create thumbnail settings for extracting 3 thumbnails evenly spaced across a 2-minute video
var thumbnailSettings = new ThumbnailSettings
{
    Count = 3,
    Format = ThumbnailFormat.Jpeg,
    Width = 1280,
    Height = 720,
    JpegQuality = 5
};

// Validate the settings against a media file before extracting thumbnails
var inputMedia = new MediaFile { Duration = TimeSpan.FromMinutes(2) };
thumbnailSettings.Validate(inputMedia);

// Create thumbnail settings for extracting at specific timestamps
var timestampedThumbnailSettings = new ThumbnailSettings
{
    Times = new List<TimeSpan> { TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(45) },
    Format = ThumbnailFormat.Png
};

// Clone settings for reuse with different parameters
var clonedSettings = thumbnailSettings.Clone();
clonedSettings.Width = 1920;
clonedSettings.Height = 1080;
```