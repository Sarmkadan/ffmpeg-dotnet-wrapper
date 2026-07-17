# ApplicationStartupJsonExtensions

Provides JSON serialization and deserialization utilities for `ApplicationStartup` objects, enabling configuration to be stored or transmitted as JSON strings.

## API

### `ToJson`
Serializes an `ApplicationStartup` instance into a JSON string.

- **Parameters**
  - `startup` The `ApplicationStartup` instance to serialize.
- **Returns**
  - A JSON string representation of the `ApplicationStartup` object.
- **Throws**
  - `ArgumentNullException` if `startup` is `null`.

### `FromJson`
Deserializes a JSON string into an `ApplicationStartup` instance.

- **Parameters**
  - `json` The JSON string to deserialize.
- **Returns**
  - An `ApplicationStartup` instance populated from the JSON data.
- **Throws**
  - `ArgumentNullException` if `json` is `null`.
  - `JsonException` if the JSON is malformed or incompatible with the `ApplicationStartup` type.

### `TryFromJson`
Attempts to deserialize a JSON string into an `ApplicationStartup` instance without throwing exceptions.

- **Parameters**
  - `json` The JSON string to deserialize.
  - `result` Output parameter receiving the deserialized `ApplicationStartup` instance if successful.
- **Returns**
  - `true` if deserialization succeeds; otherwise, `false`.
- **Throws**
  - None. Errors are returned via the `result` parameter.

### `FFmpegPath`
Gets or sets the path to the FFmpeg executable.

- **Type:** `string?`
- **Remarks**
  - If `null`, the system will attempt to locate FFmpeg in the system `PATH`.

### `FFprobePath`
Gets or sets the path to the FFprobe executable.

- **Type:** `string?`
- **Remarks**
  - If `null`, the system will attempt to locate FFprobe in the system `PATH`.

### `EnableHardwareAcceleration`
Gets or sets a value indicating whether hardware acceleration should be enabled for encoding operations.

- **Type:** `bool`
- **Default:** `false`

### `EncodingPreset`
Gets or sets the encoding preset to use.

- **Type:** `string?`
- **Remarks**
  - Common values include `"ultrafast"`, `"superfast"`, `"veryfast"`, `"faster"`, `"fast"`, `"medium"`, `"slow"`, `"slower"`, `"veryslow"`.
  - If `null`, the default preset will be used.

### `VerboseLogging`
Gets or sets a value indicating whether verbose logging should be enabled.

- **Type:** `bool`
- **Default:** `false`

### `AllowConcurrentOperations`
Gets or sets a value indicating whether multiple FFmpeg operations can run concurrently.

- **Type:** `bool`
- **Default:** `false`

### `MaxConcurrentOperations`
Gets or sets the maximum number of concurrent FFmpeg operations allowed when `AllowConcurrentOperations` is `true`.

- **Type:** `int`
- **Default:** `4`
- **Remarks**
  - Must be a positive integer.

### `OperationTimeoutSeconds`
Gets or sets the timeout duration (in seconds) for FFmpeg operations.

- **Type:** `int`
- **Default:** `300`

### `MaxFileSizeBytes`
Gets or sets the maximum allowed file size (in bytes) for operations.

- **Type:** `long`
- **Default:** `10737418240` (10 GiB)
- **Remarks**
  - A value of `0` indicates no limit.

### `KeepTemporaryFiles`
Gets or sets a value indicating whether temporary files should be retained after operations complete.

- **Type:** `bool`
- **Default:** `false`

### `TemporaryDirectory`
Gets or sets the directory where temporary files should be stored.

- **Type:** `string?`
- **Remarks**
  - If `null`, the system default temporary directory will be used.

### `SupportedFormats`
Gets or sets the list of supported media formats.

- **Type:** `string[]`
- **Default:** Empty array
- **Remarks**
  - Each entry should be a valid FFmpeg format identifier (e.g., `"mp4"`, `"avi"`).

### `RetryAttempts`
Gets or sets the number of retry attempts for failed operations.

- **Type:** `int`
- **Default:** `3`

### `RetryDelayMs`
Gets or sets the delay (in milliseconds) between retry attempts.

- **Type:** `int`
- **Default:** `1000`

## Usage

### Example 1: Serializing and Deserializing Configuration
```csharp
using var startup = new ApplicationStartup
{
    FFmpegPath = "/usr/bin/ffmpeg",
    FFprobePath = "/usr/bin/ffprobe",
    EnableHardwareAcceleration = true,
    EncodingPreset = "fast",
    VerboseLogging = true,
    MaxConcurrentOperations = 2
};

string json = ApplicationStartupJsonExtensions.ToJson(startup);
Console.WriteLine(json);

ApplicationStartup? deserialized = ApplicationStartupJsonExtensions.FromJson(json);
Console.WriteLine($"Deserialized preset: {deserialized?.EncodingPreset}");
```

### Example 2: Safe Deserialization with Error Handling
```csharp
string json = File.ReadAllText("config.json");
if (ApplicationStartupJsonExtensions.TryFromJson(json, out var startup))
{
    Console.WriteLine($"Loaded config with {startup.SupportedFormats.Length} supported formats.");
}
else
{
    Console.WriteLine("Failed to parse configuration.");
}
```

## Notes

- Thread safety: The `ToJson`, `FromJson`, and `TryFromJson` methods are thread-safe. Instance properties of `ApplicationStartup` are not inherently thread-safe; concurrent reads and writes should be synchronized if shared across threads.
- Edge cases: Deserialization will succeed even if unknown properties are present in the JSON, but they will be ignored. Malformed JSON will cause `FromJson` to throw; `TryFromJson` will return `false`.
- Default values: When serializing, only non-default values are included in the JSON output. Deserialization will apply defaults to any missing properties.
