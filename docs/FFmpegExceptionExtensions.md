# FFmpegExceptionExtensions

Provides static extension methods for `Exception` objects thrown by the FFmpeg wrapper, enabling callers to inspect failure reasons, extract detailed diagnostic messages, and classify errors into well-defined categories such as process failures, invalid media files, configuration problems, or unsupported operations.

## API

### ToDetailedErrorMessage

```csharp
public static string ToDetailedErrorMessage(this Exception exception)
```

**Purpose:** Recursively traverses the exception and its inner exceptions to build a single, human-readable string containing all available error messages, typically including FFmpeg stderr output when present.

**Parameters:**
- `exception` — The `Exception` instance to inspect. Must not be `null`.

**Return value:** A `string` composed of concatenated messages from the exception hierarchy, separated by newlines or other delimiters as appropriate.

**Throws:** `ArgumentNullException` when `exception` is `null`.

---

### IsProcessFailure

```csharp
public static bool IsProcessFailure(this Exception exception)
```

**Purpose:** Determines whether the exception originated from a failure of the underlying FFmpeg process itself (e.g., non-zero exit code, crash, or unexpected termination), as opposed to a purely managed-side error.

**Parameters:**
- `exception` — The `Exception` instance to evaluate. Must not be `null`.

**Return value:** `true` if the root cause is an FFmpeg process-level failure; otherwise `false`.

**Throws:** `ArgumentNullException` when `exception` is `null`.

---

### IsInvalidMediaFileError

```csharp
public static bool IsInvalidMediaFileError(this Exception exception)
```

**Purpose:** Checks whether the exception indicates that one or more input media files are invalid, corrupted, unsupported, or otherwise unprocessable by FFmpeg.

**Parameters:**
- `exception` — The `Exception` instance to evaluate. Must not be `null`.

**Return value:** `true` if the error is attributable to invalid media file content; otherwise `false`.

**Throws:** `ArgumentNullException` when `exception` is `null`.

---

### IsInvalidConfigurationError

```csharp
public static bool IsInvalidConfigurationError(this Exception exception)
```

**Purpose:** Identifies errors caused by invalid or contradictory FFmpeg argument configurations, filter graph definitions, or parameter combinations supplied by the caller.

**Parameters:**
- `exception` — The `Exception` instance to evaluate. Must not be `null`.

**Return value:** `true` if the error stems from invalid configuration; otherwise `false`.

**Throws:** `ArgumentNullException` when `exception` is `null`.

---

### IsUnsupportedOperationError

```csharp
public static bool IsUnsupportedOperationError(this Exception exception)
```

**Purpose:** Detects whether the exception signals that the requested operation is not supported by the current FFmpeg build, codec capabilities, or format constraints.

**Parameters:**
- `exception` — The `Exception` instance to evaluate. Must not be `null`.

**Return value:** `true` if the operation is unsupported; otherwise `false`.

**Throws:** `ArgumentNullException` when `exception` is `null`.

## Usage

### Example 1: Categorizing a failure and logging details

```csharp
try
{
    await FFmpegWrapper.ConvertAsync(inputPath, outputPath, options);
}
catch (Exception ex)
{
    string details = ex.ToDetailedErrorMessage();

    if (ex.IsInvalidMediaFileError())
    {
        logger.Error("Input file is invalid or corrupted: {Details}", details);
        // Notify user to check the source file
    }
    else if (ex.IsInvalidConfigurationError())
    {
        logger.Error("FFmpeg configuration is invalid: {Details}", details);
        // Adjust parameters and retry
    }
    else if (ex.IsProcessFailure())
    {
        logger.Error("FFmpeg process failed unexpectedly: {Details}", details);
        // Escalate for investigation
    }
    else
    {
        logger.Error("Unhandled FFmpeg error: {Details}", details);
    }
}
```

### Example 2: Validating media before processing

```csharp
public async Task<bool> TryTranscode(string mediaPath, string outputPath)
{
    try
    {
        await FFmpegWrapper.TranscodeAsync(mediaPath, outputPath, preset);
        return true;
    }
    catch (Exception ex) when (ex.IsUnsupportedOperationError())
    {
        Console.WriteLine($"Operation not supported: {ex.ToDetailedErrorMessage()}");
        return false;
    }
    catch (Exception ex) when (ex.IsInvalidMediaFileError())
    {
        Console.WriteLine($"Cannot process file: {ex.ToDetailedErrorMessage()}");
        return false;
    }
}
```

## Notes

- All methods throw `ArgumentNullException` if a `null` exception reference is passed. Guard calls accordingly when exceptions may originate from contexts where the exception object itself could be absent.
- The classification methods (`IsProcessFailure`, `IsInvalidMediaFileError`, `IsInvalidConfigurationError`, `IsUnsupportedOperationError`) are mutually exclusive only for the root cause; an exception hierarchy may contain multiple inner exceptions of different types. The methods evaluate the entire chain and return `true` if any level matches the category.
- `ToDetailedErrorMessage` aggregates messages from all inner exceptions. The resulting string may contain raw FFmpeg stderr output, which can be large. Avoid logging it indiscriminately in high-throughput pipelines without truncation or sampling.
- These methods are pure functions that inspect exception data and do not modify state. They are safe to call from any thread without synchronization.
- The accuracy of error classification depends on how the wrapper maps FFmpeg exit reasons to exception types. Custom exception types thrown by the library are expected; generic `Exception` instances not originating from the wrapper may yield `false` for all category checks.
