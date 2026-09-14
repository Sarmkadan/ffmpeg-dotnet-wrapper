# EnumerableExtensions

The `MediaFileEnumerableExtensions` and `ConversionResultEnumerableExtensions` static classes provide aggregate operations for sequences of media files and conversion results in the ffmpeg-dotnet-wrapper library. These extension methods operate on `IEnumerable<T>` sequences to offer convenient ways to calculate totals, success rates, and other aggregate values.

## API

### MediaFileEnumerableExtensions

All methods in this class are extension methods on `IEnumerable<MediaFile>`. Unless otherwise noted, they throw an `ArgumentNullException` if the source argument is `null`.

#### `GetTotalFileSize`

```csharp
public static long GetTotalFileSize(this IEnumerable<MediaFile> files)
```

Calculates the total file size of the media files in a sequence, skipping null elements.

**Parameters**
- `files`: The media files whose sizes are summed.

**Returns**
The sum of the `MediaFile.FileSize` values for all non-null elements, or `0` when the sequence is empty or contains only null elements.

**Exceptions**
- `ArgumentNullException`: `<paramref name="files"/>` is `<see langword="null"/>`.

### ConversionResultEnumerableExtensions

All methods in this class are extension methods on `IEnumerable<ConversionResult>`. Unless otherwise noted, they throw an `ArgumentNullException` if the source argument is `null`.

#### `GetSuccessRate`

```csharp
public static double GetSuccessRate(this IEnumerable<ConversionResult> results)
```

Calculates the percentage of conversion results that completed successfully.

**Parameters**
- `results`: The conversion results to evaluate.

**Returns**
The percentage of results whose `ConversionResult.IsSuccess` property is `<see langword="true"/>`, or `0` when the sequence is empty.

**Exceptions**
- `ArgumentNullException`: `<paramref name="results"/>` is `<see langword="null"/>`.

## Usage

### Example 1: Calculating total file size of media files

```csharp
using FFmpegDotnetWrapper.Models;
using System.Collections.Generic;

var mediaFiles = new List<MediaFile>
{
    new MediaFile { FileSize = 1024 },
    new MediaFile { FileSize = 2048 },
    null, // Null elements are skipped
    new MediaFile { FileSize = 512 }
};

long totalSize = mediaFiles.GetTotalFileSize();
// Output: 3584 (1024 + 2048 + 512)
```

### Example 2: Calculating conversion success rate

```csharp
using FFmpegDotnetWrapper.Models;
using System.Collections.Generic;

var conversionResults = new List<ConversionResult>
{
    new ConversionResult { IsSuccess = true },
    new ConversionResult { IsSuccess = false },
    new ConversionResult { IsSuccess = true },
    new ConversionResult { IsSuccess = true }
};

double successRate = conversionResults.GetSuccessRate();
// Output: 75.0 (3 out of 4 successful)
```

### Example 3: Handling empty sequences

```csharp
using FFmpegDotnetWrapper.Models;
using System.Collections.Generic;

var emptyMediaFiles = new List<MediaFile>();
var emptyConversionResults = new List<ConversionResult>();

long emptyTotalSize = emptyMediaFiles.GetTotalFileSize(); // Output: 0
double emptySuccessRate = emptyConversionResults.GetSuccessRate(); // Output: 0
```

## Notes

- All methods assume the source sequence is not `null`; passing `null` will result in an `ArgumentNullException`.
- For `GetTotalFileSize`, null elements in the sequence are skipped and do not contribute to the total.
- For `GetSuccessRate`, an empty sequence returns `0` to avoid division by zero.
- These methods are implemented using simple iteration for performance and do not rely on LINQ operators to avoid unnecessary allocations.
- When working with large collections, these methods process elements in a single pass and use minimal memory overhead.