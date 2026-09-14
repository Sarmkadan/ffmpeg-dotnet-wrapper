# ModelJsonExtensions

Provides JSON serialization extension methods for various model classes in the FFmpeg Dotnet Wrapper library.

## API

### MediaFileJsonExtensions

Provides JSON serialization extension methods for `MediaFile`.

#### ToJson

```csharp
public static string ToJson(this MediaFile mediaFile, bool indented = false)
```

**Purpose:** Serializes the `MediaFile` instance to a JSON string.

**Parameters:**
- `mediaFile` — The `MediaFile` to serialize. Must not be `null`.
- `indented` — Whether to write indented JSON.

**Return value:** A JSON string representing the `MediaFile`.

**Throws:** `ArgumentNullException` when `mediaFile` is `null`.

---

### ConversionResultJsonExtensions

Provides JSON serialization extension methods for `ConversionResult`.

#### ToJson

```csharp
public static string ToJson(this ConversionResult result, bool indented = false)
```

**Purpose:** Serializes the specified `ConversionResult` to a JSON string.

**Parameters:**
- `result` — The conversion result to serialize. Must not be `null`.
- `indented` — `true` to format the JSON with indentation; otherwise, `false`.

**Return value:** A JSON string representation of `result`.

**Throws:** `ArgumentNullException` when `result` is `null`.

---

### FFmpegOperationJsonExtensions

Provides JSON serialization extension methods for `FFmpegOperation`.

#### ToJson

```csharp
public static string ToJson(this FFmpegOperation operation, bool indented = false)
```

**Purpose:** Serializes the specified `FFmpegOperation` to a JSON string.

**Parameters:**
- `operation` — The operation to serialize. Must not be `null`.
- `indented` — `true` to format the JSON with indentation; otherwise, `false`.

**Return value:** A JSON string representation of `operation`.

**Throws:** `ArgumentNullException` when `operation` is `null`.

---

### FFmpegProgressUpdateJsonExtensions

Provides JSON serialization extension methods for `FFmpegProgressUpdate`.

#### ToJson

```csharp
public static string ToJson(this FFmpegProgressUpdate update, bool indented = false)
```

**Purpose:** Serializes the `FFmpegProgressUpdate` instance to a JSON string.

**Parameters:**
- `update` — The progress update to serialize. Must not be `null`.
- `indented` — If set to `true`, the JSON is pretty-printed with indentation. Default is `false`.

**Return value:** A JSON string representing the `FFmpegProgressUpdate`.

**Throws:** `ArgumentNullException` when `update` is `null`.

---

### ThumbnailResultJsonExtensions

Provides System.Text.Json serialization and deserialization helpers for `ThumbnailResult`.

#### ToJson

```csharp
public static string ToJson(this ThumbnailResult value, bool indented = false)
```

**Purpose:** Serializes the `ThumbnailResult` to a JSON string.

**Parameters:**
- `value` — The thumbnail result to serialize. Must not be `null`.
- `indented` — Whether to format the JSON with indentation for readability.

**Return value:** A JSON string representation of the thumbnail result.

**Throws:** `ArgumentNullException` when `value` is `null`.

#### FromJson

```csharp
public static ThumbnailResult? FromJson(string json)
```

**Purpose:** Deserializes a JSON string into a `ThumbnailResult` instance.

**Parameters:**
- `json` — The JSON string to deserialize.

**Return value:** The deserialized `ThumbnailResult` instance, or `null` if the JSON is empty or whitespace.

**Throws:** `JsonException` when the JSON is invalid or cannot be deserialized.

#### TryFromJson

```csharp
public static bool TryFromJson(string json, out ThumbnailResult? value)
```

**Purpose:** Attempts to deserialize a JSON string into a `ThumbnailResult` instance.

**Parameters:**
- `json` — The JSON string to deserialize.
- `value` — Receives the deserialized instance if successful.

**Return value:** `true` if deserialization succeeded; otherwise, `false`.

**Throws:** `ArgumentNullException` when `json` is `null`.

---

### ThumbnailSettingsJsonExtensions

Provides extension methods for serializing and deserializing `ThumbnailSettings` instances to and from JSON.

#### ToJson

```csharp
public static string ToJson(this ThumbnailSettings value, bool indented = false)
```

**Purpose:** Serializes the specified `ThumbnailSettings` value to a JSON string.

**Parameters:**
- `value` — The value to serialize. Must not be `null`.
- `indented` — Whether to indent the JSON for readability.

**Return value:** A JSON string representation of the value.

**Throws:** `ArgumentNullException` when `value` is `null`.

#### FromJson

```csharp
public static ThumbnailSettings? FromJson(string json)
```

**Purpose:** Deserializes a JSON string to a `ThumbnailSettings` instance.

**Parameters:**
- `json` — The JSON string to deserialize. Must not be `null` or empty/whitespace.

**Return value:** The deserialized `ThumbnailSettings` instance, or `null` if the JSON is empty or whitespace.

**Throws:** 
- `ArgumentNullException` when `json` is `null`.
- `ArgumentException` when `json` is empty or whitespace.
- `JsonException` when the JSON is invalid or cannot be deserialized.

#### TryFromJson

```csharp
public static bool TryFromJson(string json, out ThumbnailSettings? value)
```

**Purpose:** Attempts to deserialize a JSON string to a `ThumbnailSettings` instance.

**Parameters:**
- `json` — The JSON string to deserialize. Must not be `null` or empty/whitespace.
- `value` — Receives the deserialized value if successful.

**Return value:** `true` if deserialization succeeded; otherwise, `false`.

**Throws:**
- `ArgumentNullException` when `json` is `null`.
- `ArgumentException` when `json` is empty or whitespace.

---

### TranscodeSettingsJsonExtensions

Provides System.Text.Json serialization and deserialization extensions for `TranscodeSettings`.

#### ToJson

```csharp
public static string ToJson(this TranscodeSettings value, bool indented = false)
```

**Purpose:** Serializes the `TranscodeSettings` instance to a JSON string using camelCase property naming.

**Parameters:**
- `value` — The transcode settings to serialize. Must not be `null`.
- `indented` — Whether to format the JSON with indentation for readability.

**Return value:** A JSON string representation of the transcode settings.

**Throws:** `ArgumentNullException` when `value` is `null`.

#### FromJson

```csharp
public static TranscodeSettings? FromJson(string json)
```

**Purpose:** Deserializes a JSON string to a `TranscodeSettings` instance.

**Parameters:**
- `json` — The JSON string to deserialize. Must not be `null`, empty, or whitespace.

**Return value:** The deserialized `TranscodeSettings` instance, or `null` if the JSON is empty.

**Throws:**
- `ArgumentException` when `json` is `null`, empty, or whitespace.
- `JsonException` when the JSON is invalid or cannot be deserialized.

#### TryFromJson

```csharp
public static bool TryFromJson(string json, out TranscodeSettings? value)
```

**Purpose:** Attempts to deserialize a JSON string to a `TranscodeSettings` instance.

**Parameters:**
- `json` — The JSON string to deserialize. Must not be `null`, empty, or whitespace.
- `value` — Receives the deserialized `TranscodeSettings` instance if successful; otherwise, `null`.

**Return value:** `true` if deserialization succeeds; otherwise, `false`.

**Throws:**
- `ArgumentException` when `json` is `null`, empty, or whitespace.

---

### StreamingPipelineResultJsonExtensions

Provides JSON serialization extensions for `StreamingPipelineResult`.

#### ToJson

```csharp
public static string ToJson(this StreamingPipelineResult result, bool indented = false)
```

**Purpose:** Serializes the specified `StreamingPipelineResult` to a JSON string.

**Parameters:**
- `result` — The streaming pipeline result to serialize. Must not be `null`.
- `indented` — `true` to format the JSON with indentation; otherwise, `false`.

**Return value:** A JSON string representation of `result`.

**Throws:** `ArgumentNullException` when `result` is `null`.

---

### StreamingProfileJsonExtensions

Provides JSON serialization extensions for `StreamingProfile`.

#### ToJson

```csharp
public static string ToJson(this StreamingProfile profile, bool indented = false)
```

**Purpose:** Converts a `StreamingProfile` to its JSON representation.

**Parameters:**
- `profile` — The profile to convert. Must not be `null`.
- `indented` — Whether to format the JSON with indentation.

**Return value:** A JSON string representing the profile.

**Throws:** `ArgumentNullException` when `profile` is `null`.

## Usage

### Example: Round-trip JSON serialization

```csharp
using FFmpegDotnetWrapper.Models;
using System.Text.Json;

// Create a media file
var mediaFile = new MediaFile
{
    FilePath = "/path/to/video.mp4",
    Duration = TimeSpan.FromSeconds(120),
    Width = 1920,
    Height = 1080,
    FrameRate = 30,
    BitRate = 5000000
};

// Serialize to JSON
string json = mediaFile.ToJson(indented: true);
Console.WriteLine(json);

// Deserialize back from JSON
MediaFile? deserialized = MediaFileJsonExtensions.FromJson(json);
if (deserialized != null)
{
    Console.WriteLine($"Deserialized file: {deserialized.FilePath}");
    Console.WriteLine($"Duration: {deserialized.Duration}");
}

// Example with ThumbnailSettings
var thumbnailSettings = new ThumbnailSettings
{
    Format = ThumbnailFormat.Png,
    Width = 320,
    Height = 240,
    SeekPosition = TimeSpan.FromSeconds(30)
};

// Serialize with indentation
string thumbnailJson = thumbnailSettings.ToJson(indented: true);
Console.WriteLine(thumbnailJson);

// Deserialize
ThumbnailSettings? settings = ThumbnailSettingsJsonExtensions.FromJson(thumbnailJson);
if (settings != null)
{
    Console.WriteLine($"Thumbnail format: {settings.Format}");
    Console.WriteLine($"Size: {settings.Width}x{settings.Height}");
}
```

## Notes

- All serialization methods throw `ArgumentNullException` if a `null` instance is passed.
- Methods with `indented` parameter allow pretty-printing JSON for readability.
- Several extensions include deserialization helpers (`FromJson` and `TryFromJson`) for round-trip JSON operations.
- The extensions use camelCase property naming by default for JSON output.
- Null values are ignored during serialization when appropriate.
- These extensions are pure functions that do not have external side effects.