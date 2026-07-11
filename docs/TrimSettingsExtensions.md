# TrimSettingsExtensions

The `TrimSettingsExtensions` class provides a set of static extension methods and helper properties designed to facilitate the configuration and analysis of `TrimSettings` objects within the FFmpeg .NET wrapper. It enables fluent modification of trim parameters, such as start time offsets and duration adjustments, while offering utility functions to calculate derived values like end times and effective durations. Additionally, it includes boolean predicates to determine stream preservation strategies and keyframe requirements, ensuring precise control over media trimming operations without altering the underlying `TrimSettings` state directly unless explicitly configured via the provided methods.

## API

### `WithStartTimeOffset`
```csharp
public static TrimSettings WithStartTimeOffset(this TrimSettings settings, TimeSpan offset)
```
Creates a new `TrimSettings` instance with the start time adjusted by the specified offset. This method is immutable; it does not modify the original `settings` object.
*   **Parameters**:
    *   `settings`: The source `TrimSettings` instance.
    *   `offset`: The `TimeSpan` to add to the current start time.
*   **Returns**: A new `TrimSettings` object with the updated start time.
*   **Throws**: Throws `ArgumentNullException` if `settings` is null.

### `WithDurationAdjustment`
```csharp
public static TrimSettings WithDurationAdjustment(this TrimSettings settings, TimeSpan adjustment)
```
Creates a new `TrimSettings` instance with the duration modified by the specified adjustment value. Positive values extend the duration, while negative values shorten it.
*   **Parameters**:
    *   `settings`: The source `TrimSettings` instance.
    *   `adjustment`: The `TimeSpan` representing the change in duration.
*   **Returns**: A new `TrimSettings` object with the adjusted duration.
*   **Throws**: Throws `ArgumentNullException` if `settings` is null.

### `PreservesBothStreams`
```csharp
public static bool PreservesBothStreams(this TrimSettings settings)
```
Determines if the current trim configuration results in both audio and video streams being preserved in the output.
*   **Parameters**:
    *   `settings`: The `TrimSettings` instance to evaluate.
*   **Returns**: `true` if both streams are preserved; otherwise, `false`.
*   **Throws**: Throws `ArgumentNullException` if `settings` is null.

### `PreservesOnlyAudio`
```csharp
public static bool PreservesOnlyAudio(this TrimSettings settings)
```
Checks whether the trim settings are configured to preserve only the audio stream, effectively dropping the video stream.
*   **Parameters**:
    *   `settings`: The `TrimSettings` instance to evaluate.
*   **Returns**: `true` if only audio is preserved; otherwise, `false`.
*   **Throws**: Throws `ArgumentNullException` if `settings` is null.

### `PreservesOnlyVideo`
```csharp
public static bool PreservesOnlyVideo(this TrimSettings settings)
```
Checks whether the trim settings are configured to preserve only the video stream, effectively dropping the audio stream.
*   **Parameters**:
    *   `settings`: The `TrimSettings` instance to evaluate.
*   **Returns**: `true` if only video is preserved; otherwise, `false`.
*   **Throws**: Throws `ArgumentNullException` if `settings` is null.

### `GetEndTime`
```csharp
public static TimeSpan GetEndTime(this TrimSettings settings)
```
Calculates the absolute end time of the trim segment based on the start time and duration defined in the settings.
*   **Parameters**:
    *   `settings`: The `TrimSettings` instance.
*   **Returns**: A `TimeSpan` representing the calculated end time. If duration is undefined or infinite, behavior depends on the underlying `TrimSettings` implementation logic.
*   **Throws**: Throws `ArgumentNullException` if `settings` is null.

### `GetTrimmedDurationOrZero`
```csharp
public static TimeSpan GetTrimmedDurationOrZero(this TrimSettings settings)
```
Retrieves the effective duration of the trim. If the duration is null, negative, or undefined, this method returns `TimeSpan.Zero` instead of throwing an exception or returning a nullable value.
*   **Parameters**:
    *   `settings`: The `TrimSettings` instance.
*   **Returns**: The duration as a `TimeSpan`, or `TimeSpan.Zero` if no valid duration is set.
*   **Throws**: Throws `ArgumentNullException` if `settings` is null.

### `TrimToEnd`
```csharp
public static TrimSettings TrimToEnd(this TrimSettings settings, TimeSpan? startTime = null)
```
Configures the settings to trim from a specific start time (or the existing start time) to the very end of the source media.
*   **Parameters**:
    *   `settings`: The source `TrimSettings` instance.
    *   `startTime`: An optional `TimeSpan` to set as the new start time. If null, the existing start time is used.
*   **Returns**: A new `TrimSettings` object configured to extend to the end of the media.
*   **Throws**: Throws `ArgumentNullException` if `settings` is null.

### `RequiresKeyframes`
```csharp
public static bool RequiresKeyframes(this TrimSettings settings)
```
Evaluates whether the current trim configuration necessitates cutting at keyframes to ensure stream integrity. This is typically true when precise timestamp cutting is disabled or when specific codec constraints apply.
*   **Parameters**:
    *   `settings`: The `TrimSettings` instance to evaluate.
*   **Returns**: `true` if keyframe alignment is required; otherwise, `false`.
*   **Throws**: Throws `ArgumentNullException` if `settings` is null.

## Usage

### Example 1: Fluent Configuration and Duration Calculation
This example demonstrates how to chain extension methods to configure a trim operation starting 10 seconds in, lasting for 30 seconds, and then calculating the precise end time.

```csharp
using FFmpeg.Wrapper;
using System;

public class TrimExample
{
    public void ConfigureTrim()
    {
        var baseSettings = new TrimSettings();
        
        // Apply start offset and specific duration
        var configured = baseSettings
            .WithStartTimeOffset(TimeSpan.FromSeconds(10))
            .WithDurationAdjustment(TimeSpan.FromSeconds(30));

        // Calculate derived values
        TimeSpan endTime = configured.GetEndTime();
        TimeSpan duration = configured.GetTrimmedDurationOrZero();

        Console.WriteLine($"Trim End: {endTime}, Duration: {duration}");
        
        if (configured.RequiresKeyframes())
        {
            Console.WriteLine("Warning: Cut points must align with keyframes.");
        }
    }
}
```

### Example 2: Stream Preservation and End-of-File Trimming
This example illustrates checking stream preservation flags and configuring a trim that extends from a specific point to the end of the file.

```csharp
using FFmpeg.Wrapper;
using System;

public class StreamStrategyExample
{
    public void AnalyzeAndTrimToEnd(TrimSettings currentSettings)
    {
        if (currentSettings.PreservesOnlyAudio())
        {
            Console.WriteLine("Processing audio-only extraction.");
        }
        else if (currentSettings.PreservesOnlyVideo())
        {
            Console.WriteLine("Processing video-only extraction.");
        }
        else if (currentSettings.PreservesBothStreams())
        {
            Console.WriteLine("Processing full A/V trim.");
        }

        // Modify settings to trim from 5 seconds until the end of the source
        var trimToEndSettings = currentSettings.TrimToEnd(TimeSpan.FromSeconds(5));
        
        // Verify the new configuration
        if (!trimToEndSettings.GetTrimmedDurationOrZero().Equals(TimeSpan.Zero))
        {
            Console.WriteLine($"Ready to process until {trimToEndSettings.GetEndTime()}");
        }
    }
}
```

## Notes

*   **Immutability**: The methods `WithStartTimeOffset`, `WithDurationAdjustment`, and `TrimToEnd` follow an immutable pattern. They return a new `TrimSettings` instance and do not modify the input `settings` object. The caller must capture the return value to apply changes.
*   **Null Safety**: All extension methods will throw `ArgumentNullException` if the `settings` argument passed to them is `null`. Callers should ensure valid instances are provided.
*   **Duration Logic**: `GetTrimmedDurationOrZero` provides a safe access pattern for duration, preventing errors related to null or negative durations by normalizing them to `TimeSpan.Zero`. However, `GetEndTime` relies on the internal logic of `TrimSettings` for handling undefined durations, which may result in unexpected values if the duration is not explicitly set.
*   **Thread Safety**: As this class consists entirely of static methods that operate on passed-in instances without maintaining internal static state, it is thread-safe. However, the thread safety of the `TrimSettings` objects themselves depends on their implementation; if a single `TrimSettings` instance is mutated concurrently by other means while these extensions are reading it, external synchronization is required.
*   **Keyframe Dependency**: The `RequiresKeyframes` predicate is a logical evaluation based on the current settings. If this returns `true`, attempting to trim at non-keyframe intervals may result in seeking to the nearest preceding keyframe, potentially altering the precise start time requested.
