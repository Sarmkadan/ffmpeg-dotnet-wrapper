# ExtensionMethods

The `ExtensionMethods` class provides a collection of static utility and extension methods used throughout the `ffmpeg-dotnet-wrapper` project. These methods simplify common operations on strings, collections, nullable types, time parsing, file path manipulation, and formatting of media-related values such as duration, size, and bitrate. They are designed to reduce boilerplate and improve readability in argument construction, LINQ queries, and API response handling.

## API

### `AppendArgument`
```csharp
public static StringBuilder AppendArgument(this StringBuilder sb, string key, string value)
```
Appends a command-line argument pair to a `StringBuilder`. If `value` is not null or white space, the argument is added as `-key value` with a preceding space when the builder already contains text. If `value` is null or white space, nothing is appended.

**Parameters:**
- `sb` — The `StringBuilder` to append to.
- `key` — The argument name (without leading dash).
- `value` — The argument value.

**Returns:** The same `StringBuilder` instance for chaining.

**Throws:** `ArgumentNullException` if `sb` or `key` is null.

---

### `AppendArguments`
```csharp
public static StringBuilder AppendArguments(this StringBuilder sb, string key, params string[] values)
```
Appends a command-line argument with multiple values to a `StringBuilder`. Each non-null, non-white-space value is added as `-key value`. If no valid values are provided, nothing is appended.

**Parameters:**
- `sb` — The `StringBuilder` to append to.
- `key` — The argument name.
- `values` — One or more argument values.

**Returns:** The same `StringBuilder` instance.

**Throws:** `ArgumentNullException` if `sb` or `key` is null.

---

### `IsNullOrWhiteSpace`
```csharp
public static bool IsNullOrWhiteSpace(this string value)
```
Indicates whether a string is null, empty, or consists only of white-space characters.

**Parameters:**
- `value` — The string to evaluate.

**Returns:** `true` if the string is null, empty, or white space; otherwise `false`.

---

### `HasValue`
```csharp
public static bool HasValue(this string value)
```
Indicates whether a string is not null and contains at least one non-white-space character.

**Parameters:**
- `value` — The string to evaluate.

**Returns:** `true` if the string has meaningful content; otherwise `false`.

---

### `Repeat`
```csharp
public static string Repeat(this string value, int count)
```
Returns a new string consisting of the input string repeated the specified number of times.

**Parameters:**
- `value` — The string to repeat.
- `count` — The number of repetitions.

**Returns:** The concatenated result.

**Throws:** `ArgumentOutOfRangeException` if `count` is negative.

---

### `Join<T>` (with separator)
```csharp
public static string Join<T>(this IEnumerable<T> source, string separator)
```
Concatenates the string representations of the elements in a sequence, using the specified separator between each element.

**Parameters:**
- `source` — The sequence of elements.
- `separator` — The separator string.

**Returns:** The joined string.

---

### `Join<T>` (with separator and selector)
```csharp
public static string Join<T>(this IEnumerable<T> source, string separator, Func<T, string> selector)
```
Concatenates the results of applying a selector function to each element of a sequence, using the specified separator between each result.

**Parameters:**
- `source` — The sequence of elements.
- `separator` — The separator string.
- `selector` — A transform function applied to each element.

**Returns:** The joined string.

---

### `SingleOrNull<T>`
```csharp
public static T? SingleOrNull<T>(this IEnumerable<T> source) where T : struct
```
Returns the single element of a sequence, or `null` if the sequence is empty. If the sequence contains more than one element, an exception is thrown.

**Parameters:**
- `source` — The sequence of value-type elements.

**Returns:** The single element, or `null` if the sequence is empty.

**Throws:** `InvalidOperationException` if the sequence contains more than one element.

---

### `IsNullOrEmpty<T>`
```csharp
public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
```
Indicates whether a sequence is null or contains no elements.

**Parameters:**
- `source` — The sequence to evaluate.

**Returns:** `true` if the sequence is null or empty; otherwise `false`.

---

### `Batch<T>`
```csharp
public static IEnumerable<List<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
```
Partitions a sequence into batches of the specified size. The last batch may be smaller if the total number of elements is not evenly divisible.

**Parameters:**
- `source` — The sequence to partition.
- `batchSize` — The maximum size of each batch.

**Returns:** An enumerable of lists, each containing up to `batchSize` elements.

**Throws:** `ArgumentOutOfRangeException` if `batchSize` is less than or equal to zero.

---

### `ToSeconds`
```csharp
public static double ToSeconds(this TimeSpan timeSpan)
```
Converts a `TimeSpan` to its total duration in seconds as a `double`.

**Parameters:**
- `timeSpan` — The time span to convert.

**Returns:** The total number of seconds.

---

### `ToMilliseconds`
```csharp
public static long ToMilliseconds(this TimeSpan timeSpan)
```
Converts a `TimeSpan` to its total duration in whole milliseconds as a `long`.

**Parameters:**
- `timeSpan` — The time span to convert.

**Returns:** The total number of milliseconds, truncated.

---

### `FormatAsTime`
```csharp
public static string FormatAsTime(this TimeSpan timeSpan)
```
Formats a `TimeSpan` as a time string in `HH:mm:ss.fff` format, omitting the fractional seconds if they are zero.

**Parameters:**
- `timeSpan` — The time span to format.

**Returns:** The formatted time string.

---

### `TryParseTime`
```csharp
public static double? TryParseTime(this string value)
```
Attempts to parse a string as a time duration and returns the total seconds if successful. Supports formats such as `HH:mm:ss`, `HH:mm:ss.fff`, and plain seconds as a number.

**Parameters:**
- `value` — The string to parse.

**Returns:** The parsed duration in seconds, or `null` if parsing fails.

---

### `FormatAsSize`
```csharp
public static string FormatAsSize(this long bytes)
```
Formats a byte count as a human-readable file size string (e.g., `1.5 MB`). Uses binary prefixes (KiB, MiB, GiB) with one decimal place when appropriate.

**Parameters:**
- `bytes` — The number of bytes.

**Returns:** The formatted size string.

---

### `FormatAsBitrate`
```csharp
public static string FormatAsBitrate(this long bitsPerSecond)
```
Formats a bitrate value in bits per second as a human-readable string (e.g., `1.5 Mbps`). Uses metric prefixes (kbps, Mbps, Gbps).

**Parameters:**
- `bitsPerSecond` — The bitrate in bits per second.

**Returns:** The formatted bitrate string.

---

### `GetFileName`
```csharp
public static string GetFileName(this string path)
```
Extracts the file name with extension from a file path.

**Parameters:**
- `path` — The file path.

**Returns:** The file name, or an empty string if the path ends with a directory separator.

---

### `GetDirectoryPath`
```csharp
public static string GetDirectoryPath(this string path)
```
Extracts the directory portion of a file path, excluding the file name.

**Parameters:**
- `path` — The file path.

**Returns:** The directory path, or an empty string if no directory component is present.

---

### `GetFileExtension`
```csharp
public static string GetFileExtension(this string path)
```
Extracts the file extension from a file path, including the leading period. Returns an empty string if no extension is present.

**Parameters:**
- `path` — The file path.

**Returns:** The file extension (e.g., `.mp4`), or an empty string.

---

### `WithRequestId<T>`
```csharp
public static ApiResponse<T> WithRequestId<T>(this ApiResponse<T> response, string requestId)
```
Associates a request identifier with an `ApiResponse<T>` instance by setting its `RequestId` property and returning the response for chaining.

**Parameters:**
- `response` — The API response object.
- `requestId` — The request identifier to assign.

**Returns:** The same `ApiResponse<T>` instance.

**Throws:** `ArgumentNullException` if `response` is null.

## Usage

### Example 1: Building FFmpeg Arguments
```csharp
var args = new StringBuilder()
    .AppendArgument("i", inputFile)
    .AppendArgument("c:v", "libx264")
    .AppendArguments("vf", "scale=1280:720", "fps=30")
    .AppendArgument("b:v", bitrate.FormatAsBitrate())
    .AppendArgument("t", duration.FormatAsTime());

Console.WriteLine(args.ToString());
// Output: -i video.mp4 -c:v libx264 -vf scale=1280:720 -vf fps=30 -b:v 2.5 Mbps -t 00:01:30.500
```

### Example 2: Batching and Processing Media Files
```csharp
var files = Directory.GetFiles(@"C:\Media", "*.mp4");
var batches = files.Batch(10);

foreach (var batch in batches)
{
    var fileList = batch.Join(", ", f => f.GetFileName());
    Console.WriteLine($"Processing batch: {fileList}");

    foreach (var file in batch)
    {
        var dir = file.GetDirectoryPath();
        var ext = file.GetFileExtension();
        var durationStr = "00:02:15.000";
        var seconds = durationStr.TryParseTime();

        if (seconds.HasValue)
        {
            Console.WriteLine($"  {file.GetFileName()} in {dir}, duration: {seconds.Value} s");
        }
    }
}
```

## Notes

- **Argument methods:** `AppendArgument` and `AppendArguments` skip null or white-space values entirely. This prevents malformed command lines but means callers must ensure meaningful values are passed when required.
- **`SingleOrNull<T>`:** Only works with value types (`struct` constraint). For reference types, a different approach is needed. Throws on sequences with more than one element, matching `Single` semantics.
- **`TryParseTime`:** Returns `null` for any unparseable input, including null strings. Callers should check the return value before use.
- **`Batch<T>`:** The source sequence is enumerated lazily. Each batch is materialized as a `List<T>`. Modifying the original collection during enumeration leads to undefined behavior.
- **Thread safety:** All methods are static and operate on their arguments without shared mutable state. They are safe to call concurrently provided the arguments themselves are not mutated by other threads during execution. `StringBuilder` parameters in `AppendArgument` and `AppendArguments` are not internally synchronized; concurrent calls on the same `StringBuilder` instance require external synchronization.
- **Path methods:** `GetFileName`, `GetDirectoryPath`, and `GetFileExtension` perform string-based path manipulation and do not validate whether the path actually exists on the file system.
