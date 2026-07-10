# SubtitleSettings
The `SubtitleSettings` type in the `ffmpeg-dotnet-wrapper` project is designed to configure subtitle settings for FFmpeg operations. It provides properties to control the embedding of subtitles, font settings, and stream index, as well as a method to validate the settings.

## API
* `public bool HardEmbed`: A boolean property indicating whether subtitles should be hard-embedded into the video stream.
* `public string? FontName`: A nullable string property specifying the name of the font to use for subtitles.
* `public int FontSize`: An integer property setting the font size for subtitles.
* `public int SubtitleStreamIndex`: An integer property specifying the index of the subtitle stream.
* `public string? Language`: A nullable string property indicating the language of the subtitles.
* `public void Validate()`: A method that validates the subtitle settings. It does not return a value but may throw an exception if the settings are invalid.
* `public SubtitleSettings Clone()`: A method that creates and returns a clone of the current `SubtitleSettings` instance.

## Usage
The following examples demonstrate how to use the `SubtitleSettings` type:
```csharp
// Example 1: Basic subtitle settings configuration
var settings = new SubtitleSettings
{
    HardEmbed = true,
    FontName = "Arial",
    FontSize = 24,
    SubtitleStreamIndex = 0,
    Language = "en"
};
settings.Validate();

// Example 2: Cloning and modifying subtitle settings
var originalSettings = new SubtitleSettings
{
    FontSize = 18,
    SubtitleStreamIndex = 1
};
var clonedSettings = originalSettings.Clone();
clonedSettings.FontSize = 20;
clonedSettings.Validate();
```

## Notes
When using the `SubtitleSettings` type, consider the following:
* The `Validate` method should be called after configuring the settings to ensure they are valid. Failure to do so may result in unexpected behavior or errors during FFmpeg operations.
* The `Clone` method creates a deep copy of the `SubtitleSettings` instance, allowing for independent modification of the cloned instance.
* The `SubtitleSettings` type is not thread-safe by default. If accessing or modifying instances from multiple threads, proper synchronization mechanisms should be employed to avoid data corruption or other concurrency issues.
* The `FontName` and `Language` properties are nullable, allowing for cases where these settings are not applicable or unknown. However, the `Validate` method may still throw an exception if these properties are null when required by the FFmpeg operation.
