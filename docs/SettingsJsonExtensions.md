# SettingsJsonExtensions

Provides JSON serialization extension methods for various settings model classes in the FFmpeg Dotnet Wrapper library.

## API

### TrimSettingsJsonExtensions

Provides JSON serialization extension methods for `TrimSettings`.

#### ToJson

```csharp
public static string ToJson(this TrimSettings settings, bool indented = false)
```

**Purpose:** Serializes the `TrimSettings` instance to a JSON string.

**Parameters:**
- `settings` — The trim settings to serialize. Must not be `null`.
- `indented` — If set to `true`, the JSON is pretty-printed with indentation. Default is `false`.

**Return value:** A JSON string representing the `TrimSettings`.

**Throws:** `ArgumentNullException` when `settings` is `null`.

---

### MergeSettingsJsonExtensions

Provides extension methods for `MergeSettings`.

#### ToJson

```csharp
public static string ToJson(this MergeSettings settings, bool indented = false)
```

**Purpose:** Converts the `MergeSettings` instance to a JSON string.

**Parameters:**
- `settings` — The settings to convert.
- `indented` — Whether to format the JSON with indentation.

**Return value:** A JSON string representation of the settings.

**Throws:** `ArgumentNullException` when `settings` is `null`.

---

### SubtitleSettingsJsonExtensions

Provides JSON serialization extensions for `SubtitleSettings`.

#### ToJson

```csharp
public static string ToJson(this SubtitleSettings settings, bool indented = false)
```

**Purpose:** Serializes the specified subtitle settings to a JSON string.

**Parameters:**
- `settings` — The subtitle settings to serialize.
- `indented` — `true` to format the JSON with indentation; otherwise, `false`.

**Return value:** A JSON string representation of `settings`.

**Throws:** `ArgumentNullException` when `settings` is `null`.

---

### WatermarkSettingsJsonExtensions

Provides JSON serialization extensions for `WatermarkSettings`.

#### ToJson

```csharp
public static string ToJson(this WatermarkSettings settings, bool indented = false)
```

**Purpose:** Serializes the specified watermark settings to a JSON string.

**Parameters:**
- `settings` — The watermark settings to serialize.
- `indented` — `true` to format the JSON with indentation; otherwise, `false`.

**Return value:** A JSON string representation of `settings`.

**Throws:** `ArgumentNullException` when `settings` is `null`.

---

### GifExportSettingsJsonExtensions

Provides JSON serialization extensions for `GifExportSettings`.

#### ToJson

```csharp
public static string ToJson(this GifExportSettings settings, bool indented = false)
```

**Purpose:** Serializes the specified GIF export settings to a JSON string.

**Parameters:**
- `settings` — The GIF export settings to serialize.
- `indented` — `true` to format the JSON with indentation; otherwise, `false`.

**Return value:** A JSON string representation of `settings`.

**Throws:** `ArgumentNullException` when `settings` is `null`.

---

### ConcatenationSegmentJsonExtensions

Provides JSON serialization and deserialization extensions for `ConcatenationSegment`.

#### ToJson

```csharp
public static string ToJson(this ConcatenationSegment value, bool indented = false)
```

**Purpose:** Serializes a `ConcatenationSegment` to a JSON string.

**Parameters:**
- `value` — The segment to serialize.
- `indented` — Whether to format the JSON with indentation for readability.

**Return value:** A JSON string representing the segment.

**Throws:** `ArgumentNullException` when `value` is `null`.

#### FromJson

```csharp
public static ConcatenationSegment? FromJson(string json)
```

**Purpose:** Deserializes a JSON string to a `ConcatenationSegment` instance.

**Parameters:**
- `json` — The JSON string to deserialize.

**Return value:** The deserialized segment, or `null` if the JSON is empty or whitespace.

**Throws:**
- `ArgumentException` when `json` is null or empty.
- `JsonException` when the JSON is invalid or cannot be deserialized.

#### TryFromJson

```csharp
public static bool TryFromJson(string json, out ConcatenationSegment? value)
```

**Purpose:** Attempts to deserialize a JSON string to a `ConcatenationSegment` instance.

**Parameters:**
- `json` — The JSON string to deserialize.
- `value` — Receives the deserialized segment if successful, otherwise null.

**Return value:** `True` if deserialization succeeded; otherwise false.

**Throws:** `ArgumentException` when `json` is null or empty.

## Summary Table

| Class | Methods |
|-------|---------|
| `TrimSettingsJsonExtensions` | `ToJson` |
| `MergeSettingsJsonExtensions` | `ToJson` |
| `SubtitleSettingsJsonExtensions` | `ToJson` |
| `WatermarkSettingsJsonExtensions` | `ToJson` |
| `GifExportSettingsJsonExtensions` | `ToJson` |
| `ConcatenationSegmentJsonExtensions` | `ToJson`, `FromJson`, `TryFromJson` |

## Usage

### Example: Round-trip JSON serialization for ConcatenationSegment

```csharp
using FFmpegDotnetWrapper.Models;

// Create a concatenation segment
var segment = new ConcatenationSegment
{
    FilePath = "/path/to/video1.mp4",
    SeekPosition = TimeSpan.FromSeconds(5),
    Duration = TimeSpan.FromSeconds(10)
};

// Serialize to JSON (with indentation for readability)
string json = segment.ToJson(indented: true);
Console.WriteLine("Serialized JSON:");
Console.WriteLine(json);

// Deserialize back from JSON
ConcatenationSegment? deserialized = ConcatenationSegmentJsonExtensions.FromJson(json);
if (deserialized != null)
{
    Console.WriteLine($"Deserialized file: {deserialized.FilePath}");
    Console.WriteLine($"Seek position: {deserialized.SeekPosition}");
    Console.WriteLine($"Duration: {deserialized.Duration}");
}

// Example with TryFromJson
string invalidJson = "{ invalid json";
bool success = ConcatenationSegmentJsonExtensions.TryFromJson(invalidJson, out ConcatenationSegment? result);
Console.WriteLine($"TryFromJson succeeded: {success}"); // False
```