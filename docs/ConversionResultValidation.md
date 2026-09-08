# ConversionResultValidation

`ConversionResultValidation` provides extension methods for validating a [`ConversionResult`](ConversionResult.md). Validation reports all detected problems rather than stopping at the first one.

## API

### `Validate`

```csharp
IReadOnlyList<string> problems = result.Validate();
```

Returns a read-only list of human-readable validation problems. An empty list means the result is valid. Passing `null` throws `ArgumentNullException`.

### `IsValid`

```csharp
bool valid = result.IsValid();
```

Returns `true` when `Validate` returns no problems. Passing `null` throws `ArgumentNullException` through `Validate`.

### `EnsureValid`

```csharp
result.EnsureValid();
```

Returns normally for a valid result. For an invalid result, it throws `ArgumentException` with all problems joined into the exception message. Passing `null` throws `ArgumentNullException`.

## Validation rules

| Property | Requirement |
| --- | --- |
| `Id` | Must not be null, empty, or whitespace and must parse as a GUID. |
| `OutputFilePath` | Must not be null or whitespace when `IsSuccess` is `true`. Whenever it is non-empty, it must be an absolute path. The validator does not require this path to exist. |
| `OutputMedia` | Must not be null when `IsSuccess` is `true`. When present, its `FilePath` must not be null or whitespace and must reference an existing file; `FileSize` must be greater than zero; and an assigned `Duration` must be greater than zero seconds. |
| `Duration` | Must be zero or positive. |
| `ErrorMessage` | Must be null or empty when `IsSuccess` is `true`, and must be non-null and non-empty when `IsSuccess` is `false`. Whitespace-only text counts as present. A successful result with error text produces two consistency problems. |
| `Metrics` | Must not be null and may contain at most 1,000 entries. Metric keys and values are not inspected. |
| `CreatedAt` | Must not be the default `DateTime` and must not be more than five minutes later than the current UTC time at validation. |
| `CompletedAt` | Must not be the default `DateTime` for either successful or failed results. When both timestamps are set, it must not be earlier than `CreatedAt`. |
| `FFmpegOutput` | Must not be null, empty, or whitespace when `IsSuccess` is `true`. |

`WarningMessage` and `IsSuccess` need no independent validation. The validator also does not inspect `ExitCode` or `ErrorOutput`, and it does not require `FFmpegOutput` for a failed result.

## Example

This example creates a non-empty file because the validator checks that `OutputMedia.FilePath` exists and that its file size is positive.

```csharp
using FFmpegDotnetWrapper.Models;

var outputPath = Path.Combine(AppContext.BaseDirectory, "converted.mp4");
File.WriteAllBytes(outputPath, [0x00]);

try
{
    var createdAt = DateTime.UtcNow;
    var result = new ConversionResult
    {
        Id = Guid.NewGuid().ToString(),
        IsSuccess = true,
        OutputFilePath = outputPath,
        OutputMedia = new MediaFile(outputPath)
        {
            Duration = TimeSpan.FromSeconds(1)
        },
        Duration = TimeSpan.FromSeconds(1),
        CreatedAt = createdAt,
        CompletedAt = createdAt.AddSeconds(1),
        FFmpegOutput = "Conversion completed."
    };

    IReadOnlyList<string> problems = result.Validate();
    if (problems.Count == 0)
    {
        result.EnsureValid();
    }
}
finally
{
    File.Delete(outputPath);
}
```
