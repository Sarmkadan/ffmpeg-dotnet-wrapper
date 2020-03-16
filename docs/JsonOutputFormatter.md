# JsonOutputFormatter

`JsonOutputFormatter` is a specialized output formatter within the `ffmpeg-dotnet-wrapper` project that handles JSON serialization and deserialization of FFmpeg command results. It provides generic and non-generic methods to format output objects into JSON strings, parse JSON back into typed objects or `ApiResponse<T>` wrappers, and override base class read/write operations for `TimeSpan` and `DateTime` values to ensure consistent JSON representation.

## API

### Constructors

- **`public JsonOutputFormatter()`**
  Initializes a new instance of the `JsonOutputFormatter` class. No parameters are required; the formatter uses default internal JSON serialization settings appropriate for FFmpeg output structures.

### Methods

- **`public string Format<T>(T value)`**
  Serializes an object of type `T` to its JSON string representation.
  - **Parameters:** `value` — the object to serialize.
  - **Returns:** A JSON string representing `value`.
  - **Throws:** May throw `JsonSerializationException` or other serialization-related exceptions if `value` contains data that cannot be serialized.

- **`public string Format(object value)`**
  Non-generic overload that serializes an object to a JSON string. The runtime type of `value` determines the serialization behavior.
  - **Parameters:** `value` — the object to serialize.
  - **Returns:** A JSON string.
  - **Throws:** Same serialization exceptions as the generic overload.

- **`public string FormatResult(object value)`**
  Formats a single result object into a JSON string, typically used for individual FFmpeg operation outcomes.
  - **Parameters:** `value` — the result object.
  - **Returns:** JSON string representation.
  - **Throws:** Serialization exceptions if `value` is not serializable.

- **`public string FormatResults(object value)`**
  Formats a collection or composite result object into a JSON string, intended for batch or multi-result FFmpeg outputs.
  - **Parameters:** `value` — the results object (often an enumerable or aggregate).
  - **Returns:** JSON string representation.
  - **Throws:** Serialization exceptions if `value` cannot be serialized.

- **`public ApiResponse<T>? DeserializeApiResponse<T>(string json)`**
  Deserializes a JSON string into an `ApiResponse<T>` wrapper, which typically includes status information and a payload of type `T`.
  - **Parameters:** `json` — the JSON string to parse.
  - **Returns:** An `ApiResponse<T>` instance, or `null` if deserialization yields a null result.
  - **Throws:** `JsonSerializationException` or `JsonReaderException` if the JSON is malformed or does not match the expected `ApiResponse<T>` structure.

- **`public T? Deserialize<T>(string json)`**
  Deserializes a JSON string directly into an instance of type `T`.
  - **Parameters:** `json` — the JSON string to parse.
  - **Returns:** An object of type `T`, or `null` if the JSON represents a null value.
  - **Throws:** `JsonSerializationException` or `JsonReaderException` if the JSON is invalid or incompatible with type `T`.

- **`public override TimeSpan Read(string json)`**
  Overrides a base class method to read and parse a JSON string specifically into a `TimeSpan` value. Expects the JSON to contain a string or numeric representation compatible with `TimeSpan` parsing.
  - **Parameters:** `json` — the JSON string representing a `TimeSpan`.
  - **Returns:** The parsed `TimeSpan`.
  - **Throws:** `FormatException` or `JsonReaderException` if the JSON does not represent a valid `TimeSpan`.

- **`public override void Write(TimeSpan value)`**
  Overrides a base class method to write a `TimeSpan` value to JSON output. The exact output destination is managed by the underlying formatter infrastructure.
  - **Parameters:** `value` — the `TimeSpan` to write.
  - **Throws:** May throw `IOException` or serialization-related exceptions if the write operation fails.

- **`public override DateTime Read(string json)`**
  Overrides a base class method to read and parse a JSON string into a `DateTime` value. Expects the JSON to contain a string conforming to a recognized date/time format.
  - **Parameters:** `json` — the JSON string representing a `DateTime`.
  - **Returns:** The parsed `DateTime`.
  - **Throws:** `FormatException` or `JsonReaderException` if the JSON does not represent a valid `DateTime`.

- **`public override void Write(DateTime value)`**
  Overrides a base class method to write a `DateTime` value to JSON output.
  - **Parameters:** `value` — the `DateTime` to write.
  - **Throws:** May throw `IOException` or serialization-related exceptions if the write operation fails.

## Usage

### Example 1: Serializing and Deserializing an FFmpeg Progress Result

```csharp
var formatter = new JsonOutputFormatter();

// Serialize a progress object
var progress = new { Frame = 150, Fps = 29.97, Time = TimeSpan.FromSeconds(5) };
string json = formatter.Format(progress);
Console.WriteLine(json);

// Deserialize back into a strongly-typed ApiResponse
string responseJson = "{\"Status\":\"Success\",\"Data\":{\"Frame\":150,\"Fps\":29.97,\"Time\":\"00:00:05\"}}";
ApiResponse<ProgressData>? response = formatter.DeserializeApiResponse<ProgressData>(responseJson);
if (response?.Status == "Success")
{
    Console.WriteLine($"Frame: {response.Data.Frame}");
}
```

### Example 2: Overriding TimeSpan and DateTime Read/Write

```csharp
var formatter = new JsonOutputFormatter();

// Write a TimeSpan to JSON (output destination depends on context)
TimeSpan duration = TimeSpan.FromMinutes(2.5);
formatter.Write(duration);

// Read a TimeSpan from a JSON string
TimeSpan parsedDuration = formatter.Read("\"00:02:30\"");

// Write and read DateTime values
DateTime timestamp = DateTime.UtcNow;
formatter.Write(timestamp);
DateTime parsedTimestamp = formatter.Read("\"2025-03-15T10:30:00Z\"");
```

## Notes

- The `Format`, `FormatResult`, and `FormatResults` methods differ in their intended use: `Format` is the general-purpose serializer, `FormatResult` targets single operation outcomes, and `FormatResults` handles aggregated or batch results. Their underlying serialization logic may be identical, but they provide semantic clarity in FFmpeg output processing pipelines.
- `DeserializeApiResponse<T>` returns `null` when the JSON input represents a JSON null literal or when deserialization produces a null reference. Callers should perform null checks before accessing the response payload.
- The `Read` and `Write` overrides for `TimeSpan` and `DateTime` assume specific JSON formats (e.g., ISO 8601 strings for `DateTime`, and `"hh:mm:ss"` or numeric ticks for `TimeSpan`). Providing malformed JSON to `Read` will result in `FormatException` or `JsonReaderException`.
- Thread safety: `JsonOutputFormatter` does not maintain mutable instance state beyond its serialization settings. If the underlying JSON serializer is configured with static default settings, instances are safe for concurrent use across multiple threads. If custom settings are applied externally, callers must ensure those settings are immutable or synchronize access accordingly.
- Edge case: When `Deserialize<T>` is called with a JSON string that represents a JSON null value, the method returns `default(T)` (which is `null` for reference types). This is distinct from a malformed JSON string, which throws an exception.
