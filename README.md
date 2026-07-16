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

## MergeSettings

Configuration settings for merging multiple media files into a single output file. Controls whether to preserve audio/video streams, apply crossfades between clips, and configure transcoding behavior for compatibility.

```csharp
// Create merge settings for combining multiple video files with crossfade transitions
var mergeSettings = new MergeSettings
{
    PreserveAudio = true,
    PreserveVideo = true,
    Crossfade = true,
    CrossfadeDuration = 2.5,
    TranscodeOnMerge = true
};

// Add input files to merge
mergeSettings.AddInputFile("input1.mp4");
mergeSettings.AddInputFile("input2.mp4");
mergeSettings.AddInputFile("input3.mp4");

// Configure transcoding settings for compatibility
mergeSettings.TranscodeSettings = new TranscodeSettings
{
    VideoCodec = "libx264",
    AudioCodec = "aac",
    VideoBitrate = "4000k",
    AudioBitrate = "192k"
};

// Validate settings before merging
mergeSettings.Validate();

// Get information about the merge configuration
Console.WriteLine($"Input files: {mergeSettings.GetInputFileCount()}");
Console.WriteLine($"Crossfade enabled: {mergeSettings.Crossfade}");

// Clone settings for reuse with different files
var clonedMergeSettings = mergeSettings.Clone();
clonedMergeSettings.CrossfadeDuration = 1.5;
```