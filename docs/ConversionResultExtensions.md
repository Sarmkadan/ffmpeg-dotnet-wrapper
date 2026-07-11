# ConversionResultExtensions

The `ConversionResultExtensions` class provides a set of static extension methods designed to augment `ConversionResult` instances within the `ffmpeg-dotnet-wrapper` library. These utilities facilitate the extraction of performance metrics, resource consumption data, and status indicators from completed or ongoing FFmpeg conversion processes, enabling detailed post-processing analysis and logging without modifying the core result object structure.

## API

### GetProcessingSpeedFps
Retrieves the average processing speed of the conversion operation measured in frames per second.
- **Parameters**: `this ConversionResult result` – The extension target instance.
- **Returns**: `double?` – The calculated FPS value if available in the process output; otherwise, `null`.
- **Throws**: No exceptions are thrown by this method; it returns `null` if data is missing or unparseable.

### GetOutputFileSizeMb
Calculates and returns the size of the generated output file in megabytes.
- **Parameters**: `this ConversionResult result` – The extension target instance.
- **Returns**: `double?` – The file size in MB if the output file exists and can be accessed; otherwise, `null`.
- **Throws**: No exceptions are thrown; file access errors result in a `null` return value.

### HasWarnings
Determines whether the conversion process logged any non-fatal warnings during execution.
- **Parameters**: `this ConversionResult result` – The extension target instance.
- **Returns**: `bool` – `true` if warning patterns were detected in the standard error output; otherwise, `false`.
- **Throws**: No exceptions are thrown.

### GetFormattedDuration
Returns the total duration of the processed media as a formatted time string (e.g., "HH:mm:ss").
- **Parameters**: `this ConversionResult result` – The extension target instance.
- **Returns**: `string` – The formatted duration string. If duration data is unavailable, returns a default representation (e.g., "00:00:00") or an empty string depending on implementation specifics.
- **Throws**: No exceptions are thrown.

### AddPerformanceMetrics
Appends detailed performance statistics to the metadata or log collection associated with the conversion result.
- **Parameters**: 
  - `this ConversionResult result` – The extension target instance.
  - `IDictionary<string, object> metrics` – (Optional) An external dictionary to merge with internal metrics.
- **Returns**: `void`.
- **Throws**: May throw `ArgumentNullException` if the result instance is null (standard extension behavior) or if internal state is corrupted.

### GetCpuUsage
Extracts the estimated CPU usage percentage recorded during the conversion process.
- **Parameters**: `this ConversionResult result` – The extension target instance.
- **Returns**: `double?` – The CPU usage percentage if captured; otherwise, `null`.
- **Throws**: No exceptions are thrown.

### GetMemoryUsageMb
Retrieves the peak or average memory consumption of the FFmpeg process in megabytes.
- **Parameters**: `this ConversionResult result` – The extension target instance.
- **Returns**: `double?` – The memory usage in MB if available; otherwise, `null`.
- **Throws**: No exceptions are thrown.

### CompletedWithinThreshold
Evaluates whether the conversion completed within a specified time threshold.
- **Parameters**: 
  - `this ConversionResult result` – The extension target instance.
  - `TimeSpan threshold` – The maximum allowable duration.
- **Returns**: `bool` – `true` if the actual duration is less than or equal to the threshold; otherwise, `false`. Returns `false` if duration data is missing.
- **Throws**: No exceptions are thrown.

### GetMetricsSummary
Generates a consolidated string summary containing all available performance and resource metrics.
- **Parameters**: `this ConversionResult result` – The extension target instance.
- **Returns**: `string` – A formatted string containing key-value pairs of available metrics.
- **Throws**: No exceptions are thrown.

## Usage

### Example 1: Basic Metrics Extraction
This example demonstrates how to retrieve fundamental performance data and check for warnings after a conversion completes.

```csharp
using FFMpegDotNetWrapper;
using FFMpegDotNetWrapper.Extensions;

// Assume 'result' is a completed ConversionResult instance
var result = await FFMpegConverter.ConvertAsync(inputPath, outputPath, settings);

if (result.HasWarnings())
{
    Console.WriteLine("Conversion completed with warnings.");
}

var speed = result.GetProcessingSpeedFps();
var size = result.GetOutputFileSizeMb();

Console.WriteLine($"Speed: {speed?.ToString("F2")} fps");
Console.WriteLine($"Output Size: {size?.ToString("F2")} MB");
Console.WriteLine($"Duration: {result.GetFormattedDuration()}");
```

### Example 2: Performance Threshold Validation
This example validates whether a conversion met specific performance criteria and logs a detailed summary if it failed.

```csharp
using FFMpegDotNetWrapper;
using FFMpegDotNetWrapper.Extensions;

var threshold = TimeSpan.FromMinutes(5);
var result = await FFMpegConverter.ConvertAsync(inputPath, outputPath, settings);

if (!result.CompletedWithinThreshold(threshold))
{
    var cpu = result.GetCpuUsage();
    var memory = result.GetMemoryUsageMb();
    
    Console.WriteLine("Performance threshold exceeded.");
    Console.WriteLine($"CPU Usage: {cpu?.ToString("F1")}%");
    Console.WriteLine($"Memory Usage: {memory?.ToString("F2")} MB");
    Console.WriteLine("Full Metrics Summary:");
    Console.WriteLine(result.GetMetricsSummary());
}
else
{
    Console.WriteLine("Conversion completed within acceptable time limits.");
}
```

## Notes

- **Null Safety**: All numeric retrieval methods (`GetProcessingSpeedFps`, `GetOutputFileSizeMb`, `GetCpuUsage`, `GetMemoryUsageMb`) return nullable types (`double?`). Callers must handle `null` values gracefully, as they indicate that the specific metric was not captured or could not be parsed from the FFmpeg output.
- **Data Availability**: Metrics such as CPU and memory usage depend on the underlying process monitoring capabilities enabled during the conversion. If the converter was initialized without monitoring hooks, these methods will consistently return `null`.
- **Thread Safety**: As this class consists entirely of static methods operating on immutable or snapshot data provided by the `ConversionResult` instance, it is thread-safe for read operations. However, the `ConversionResult` object itself should not be modified concurrently while these extensions are accessing its properties.
- **File Access**: `GetOutputFileSizeMb` relies on file system access at the time of invocation. If the output file is deleted or locked by another process between conversion completion and method invocation, the method will return `null` rather than throwing an I/O exception.
