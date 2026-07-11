# WatermarkSettingsExtensions

The `WatermarkSettingsExtensions` class provides a set of static extension methods designed to configure `WatermarkSettings` instances using a fluent API pattern. These methods allow developers to immutably modify watermark properties such as position, scale, animation, timing, and opacity, returning a new configured instance suitable for chaining within the `ffmpeg-dotnet-wrapper` pipeline.

## API

### WithTopLeftPosition
Configures the watermark to be positioned at the top-left corner of the video frame.
- **Parameters**: None.
- **Return Value**: A new `WatermarkSettings` instance with the position set to top-left.
- **Exceptions**: Throws `ArgumentNullException` if the source `WatermarkSettings` instance is null.

### WithCenterPosition
Configures the watermark to be centered horizontally and vertically within the video frame.
- **Parameters**: None.
- **Return Value**: A new `WatermarkSettings` instance with the position set to center.
- **Exceptions**: Throws `ArgumentNullException` if the source `WatermarkSettings` instance is null.

### WithScale
Sets the scaling factor for the watermark image or video relative to its original dimensions.
- **Parameters**: 
  - `double scale`: The multiplier for the scale (e.g., 0.5 for 50%, 2.0 for 200%). Must be greater than 0.
- **Return Value**: A new `WatermarkSettings` instance with the updated scale factor.
- **Exceptions**: Throws `ArgumentNullException` if the source instance is null. Throws `ArgumentOutOfRangeException` if `scale` is less than or equal to zero.

### WithAnimation
Applies an animation effect to the watermark, such as fading in or sliding.
- **Parameters**: 
  - `WatermarkAnimation animation`: The specific animation configuration to apply.
- **Return Value**: A new `WatermarkSettings` instance with the specified animation.
- **Exceptions**: Throws `ArgumentNullException` if the source instance or the `animation` parameter is null.

### WithTimeConstraints
Defines the start time and duration for which the watermark should be visible during playback.
- **Parameters**: 
  - `TimeSpan? startTime`: The optional start time offset. If null, the watermark appears from the beginning.
  - `TimeSpan? duration`: The optional duration of visibility. If null, the watermark remains until the end of the input.
- **Return Value**: A new `WatermarkSettings` instance with the applied time constraints.
- **Exceptions**: Throws `ArgumentNullException` if the source instance is null. Throws `ArgumentException` if `duration` is provided but is less than or equal to zero.

### WithOpacity
Sets the transparency level of the watermark.
- **Parameters**: 
  - `double opacity`: A value between 0.0 (fully transparent) and 1.0 (fully opaque).
- **Return Value**: A new `WatermarkSettings` instance with the updated opacity.
- **Exceptions**: Throws `ArgumentNullException` if the source instance is null. Throws `ArgumentOutOfRangeException` if `opacity` is outside the range [0.0, 1.0].

## Usage

The following example demonstrates chaining multiple extension methods to create a watermark that is centered, scaled to 50% size, and fades in over the first two seconds of the video.

```csharp
using FFMpegWrapper.Settings;
using FFMpegWrapper.Enums;

var settings = new WatermarkSettings("logo.png")
    .WithCenterPosition()
    .WithScale(0.5)
    .WithAnimation(new WatermarkAnimation { Type = AnimationType.FadeIn, Duration = TimeSpan.FromSeconds(2) })
    .WithOpacity(0.8);
```

This example illustrates setting a watermark in the top-left corner that only appears between the 10-second and 20-second marks of the output video, with reduced opacity.

```csharp
using FFMpegWrapper.Settings;

var settings = new WatermarkSettings("overlay.jpg")
    .WithTopLeftPosition()
    .WithTimeConstraints(startTime: TimeSpan.FromSeconds(10), duration: TimeSpan.FromSeconds(10))
    .WithOpacity(0.6);
```

## Notes

- **Immutability**: All extension methods return a new `WatermarkSettings` instance rather than modifying the existing one. This ensures thread safety when sharing configuration objects across different encoding tasks, as no internal state is mutated during configuration.
- **Null Safety**: Since these are extension methods operating on an instance, passing a `null` reference as the source `WatermarkSettings` will result in a `ArgumentNullException`. Callers must ensure the base object is instantiated before applying extensions.
- **Validation**: Parameters such as `scale` and `opacity` are strictly validated. Providing invalid ranges (e.g., negative scale or opacity > 1.0) will halt execution immediately via `ArgumentOutOfRangeException`, preventing invalid FFmpeg command generation.
- **Order of Operations**: While the fluent API allows methods to be called in any order, logical conflicts (such as setting two different positions sequentially) will result in the last called method taking precedence.
