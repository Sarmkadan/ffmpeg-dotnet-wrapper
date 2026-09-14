# InfrastructureJsonExtensions

Provides JSON serialization extension methods for various infrastructure classes in the FFmpeg Dotnet Wrapper library.

## API

### ApiRequestJsonExtensions

Provides System.Text.Json serialization and deserialization extensions for `ApiRequest` types.

#### ToJson

```csharp
public static string ToJson(this ApiRequest value, bool indented = false)
```

**Purpose:** Serializes the specified `ApiRequest` object to a JSON string.

**Parameters:**
- `value` — The API request to serialize. Must not be `null`.
- `indented` — Whether to format the JSON with indentation for readability.

**Return value:** A JSON string representation of the API request.

**Throws:** `ArgumentNullException` when `value` is `null`.

#### FromJson

```csharp
public static ApiRequest? FromJson(string json)
```

**Purpose:** Deserializes a JSON string to an `ApiRequest` object.

**Parameters:**
- `json` — The JSON string to deserialize.

**Return value:** An `ApiRequest` object if deserialization succeeds; otherwise, `null`.

**Throws:** `ArgumentException` when `json` is `null`, empty, or whitespace.

#### TryFromJson

```csharp
public static bool TryFromJson(string json, out ApiRequest? value)
```

**Purpose:** Attempts to deserialize a JSON string to an `ApiRequest` object.

**Parameters:**
- `json` — The JSON string to deserialize.
- `value` — Receives the deserialized object if successful; otherwise, `null`.

**Return value:** `true` if deserialization succeeds; otherwise, `false`.

**Throws:** `ArgumentException` when `json` is `null`, empty, or whitespace.

---

### FFmpegExceptionJsonExtensions

Provides System.Text.Json serialization and deserialization extensions for FFmpegException and its derived types.

#### ToJson

```csharp
public static string ToJson(this FFmpegException value, bool indented = false)
```

**Purpose:** Serializes the FFmpegException to a JSON string.

**Parameters:**
- `value` — The exception to serialize.
- `indented` — Whether to format the JSON with indentation for readability.

**Return value:** A JSON string representation of the exception.

**Throws:** `ArgumentNullException` when `value` is `null`.

#### FromJson

```csharp
public static FFmpegException? FromJson(string json)
```

**Purpose:** Deserializes a JSON string to an FFmpegException instance.

**Parameters:**
- `json` — The JSON string to deserialize.

**Return value:** The deserialized FFmpegException, or `null` if the JSON is `null`, empty, or whitespace.

**Throws:** `ArgumentException` when `json` is `null`.

#### TryFromJson

```csharp
public static bool TryFromJson(string json, out FFmpegException? value)
```

**Purpose:** Attempts to deserialize a JSON string to an FFmpegException instance.

**Parameters:**
- `json` — The JSON string to deserialize.
- `value` — Receives the deserialized exception if successful.

**Return value:** `true` if deserialization succeeded; otherwise, `false`.

**Throws:** `ArgumentException` when `json` is `null`.

---

### BackgroundJobJsonExtensions

Provides JSON serialization extensions for `BackgroundJob` instances.

#### ToJson

```csharp
public static string ToJson(this BackgroundJob job, bool indented = false)
```

**Purpose:** Serializes the data properties of a background job to JSON.

**Parameters:**
- `job` — The background job to serialize.
- `indented` — `true` to format the JSON with indentation; otherwise, `false`.

**Return value:** A JSON string containing the background job data.

**Throws:** `ArgumentNullException` when `job` is `null`.

---

### QueuedJobJsonExtensions

Provides JSON serialization and deserialization extension methods for `QueuedJob` instances.

#### ToJson

```csharp
public static string ToJson(this QueuedJob value, bool indented = false)
```

**Purpose:** Serializes a `QueuedJob` instance into a JSON string.

**Parameters:**
- `value` — The `QueuedJob` to serialize.
- `indented` — Whether the resulting JSON string should be formatted with indentation.

**Return value:** A JSON string representation of the job.

**Throws:** `ArgumentNullException` when `value` is null.

#### FromJson

```csharp
public static QueuedJob? FromJson(string json)
```

**Purpose:** Deserializes a JSON string into a `QueuedJob` instance.

**Parameters:**
- `json` — The JSON string to deserialize.

**Return value:** The deserialized `QueuedJob` instance.

**Throws:** `ArgumentException` when `json` is null or empty.

#### TryFromJson

```csharp
public static bool TryFromJson(string json, out QueuedJob? value)
```

**Purpose:** Attempts to deserialize a JSON string into a `QueuedJob` instance.

**Parameters:**
- `json` — The JSON string to deserialize.
- `value` — When this method returns, contains the deserialized `QueuedJob` if successful, or null if it fails.

**Return value:** True if deserialization succeeded; otherwise, false.

**Throws:** `ArgumentException` when `json` is null or empty.

---

### CliCommandJsonExtensions

Provides JSON serialization extensions for `CliCommand` instances.

#### ToJson

```csharp
public static string ToJson(this CliCommand command, bool indented = false)
```

**Purpose:** Serializes the specified `CliCommand` to a JSON string.

**Parameters:**
- `command` — The command to serialize.
- `indented` — `true` to format the JSON with indentation; otherwise, `false`.

**Return value:** A JSON string representation of `command`.

**Throws:** `ArgumentNullException` when `command` is `null`.

---

### RateLimitStatusJsonExtensions

Provides JSON serialization extensions for `RateLimitStatus`.

#### ToJson

```csharp
public static string ToJson(this RateLimitStatus status, bool indented = false)
```

**Purpose:** Serializes the specified rate limit status to JSON.

**Parameters:**
- `status` — The rate limit status to serialize.
- `indented` — `true` to format the JSON with indentation; otherwise, `false`.

**Return value:** A JSON string representing the rate limit status.

**Throws:** `ArgumentNullException` when `status` is `null`.

---

### RequestLoggingOptionsJsonExtensions

Provides System.Text.Json serialization and deserialization extensions for RequestLoggingOptions.

#### ToJson

```csharp
public static string ToJson(this RequestLoggingOptions value, bool indented = false)
```

**Purpose:** Serializes the RequestLoggingOptions instance to a JSON string.

**Parameters:**
- `value` — The RequestLoggingOptions instance to serialize.
- `indented` — Whether to format the JSON with indentation for readability.

**Return value:** A JSON string representation of the RequestLoggingOptions.

**Throws:** `ArgumentNullException` when `value` is `null`.

#### FromJson

```csharp
public static RequestLoggingOptions? FromJson(string json)
```

**Purpose:** Deserializes a JSON string into a RequestLoggingOptions instance.

**Parameters:**
- `json` — The JSON string to deserialize.

**Return value:** A RequestLoggingOptions instance populated from the JSON, or `null` if parsing fails.

**Throws:** 
- `ArgumentNullException` when `json` is `null`.
- `ArgumentException` when `json` is empty or consists only of whitespace.

#### TryFromJson

```csharp
public static bool TryFromJson(string json, out RequestLoggingOptions? value)
```

**Purpose:** Attempts to deserialize a JSON string into a RequestLoggingOptions instance.

**Parameters:**
- `json` — The JSON string to deserialize.
- `value` — Receives the deserialized RequestLoggingOptions if successful, otherwise `null`.

**Return value:** `true` if deserialization succeeded; `false` if an exception occurred.

**Throws:** `ArgumentNullException` when `json` is `null`.

---

### WebhookEndpointJsonExtensions

Provides JSON serialization extensions for `WebhookEndpoint` instances.

#### ToJson

```csharp
public static string ToJson(this WebhookEndpoint endpoint, bool indented = false)
```

**Purpose:** Serializes the specified `WebhookEndpoint` to a JSON string.

**Parameters:**
- `endpoint` — The webhook endpoint to serialize.
- `indented` — Whether to format the JSON with indentation.

**Return value:** A JSON string representation of `endpoint`.

**Throws:** `ArgumentNullException` when `endpoint` is `null`.

---

### HttpClientFactoryExtensionsJsonExtensions

Provides System.Text.Json serialization extensions for `HttpClientConfig`.

#### ToJson

```csharp
public static string ToJson(this HttpClientConfig value, bool indented = false)
```

**Purpose:** Serializes an `HttpClientConfig` instance to a JSON string.

**Parameters:**
- `value` — The HttpClientConfig instance to serialize.
- `indented` — Whether to format the JSON with indentation for readability.

**Return value:** A JSON string representation of the HttpClientConfig instance.

**Throws:** `ArgumentNullException` when `value` is null.

#### FromJson

```csharp
public static HttpClientConfig? FromJson(string json)
```

**Purpose:** Deserializes a JSON string to an `HttpClientConfig` instance.

**Parameters:**
- `json` — The JSON string to deserialize.

**Return value:** The deserialized HttpClientConfig instance, or null if deserialization fails.

**Throws:** 
- `ArgumentNullException` when `json` is null.
- `ArgumentException` when `json` is empty or whitespace.
- `JsonException` when the JSON is invalid or cannot be deserialized.

#### TryFromJson

```csharp
public static bool TryFromJson(string json, out HttpClientConfig? value)
```

**Purpose:** Attempts to deserialize a JSON string to an `HttpClientConfig` instance.

**Parameters:**
- `json` — The JSON string to deserialize.
- `value` — Receives the deserialized HttpClientConfig instance if successful.

**Return value:** True if deserialization succeeded; otherwise, false.

**Throws:** 
- `ArgumentNullException` when `json` is null.
- `ArgumentException` when `json` is empty or whitespace.

---

### ProcessUtilitiesJsonExtensions

Provides System.Text.Json serialization and deserialization extensions for ProcessUtilities types.

#### ToJson

```csharp
public static string ToJson(this ProcessUtilities.ProcessResult value, bool indented = false)
```

**Purpose:** Serializes a `ProcessUtilities.ProcessResult` instance to a JSON string.

**Parameters:**
- `value` — The ProcessResult instance to serialize.
- `indented` — Whether to format the JSON with indentation for readability.

**Return value:** A JSON string representation of the ProcessResult instance.

**Throws:** `ArgumentNullException` when `value` is `null`.

#### FromJson

```csharp
public static ProcessUtilities.ProcessResult? FromJson(string json)
```

**Purpose:** Deserializes a JSON string to a `ProcessUtilities.ProcessResult` instance.

**Parameters:**
- `json` — The JSON string to deserialize.

**Return value:** A ProcessResult instance, or `null` if deserialization fails.

**Throws:** `ArgumentException` when `json` is `null` or empty.

#### TryFromJson

```csharp
public static bool TryFromJson(string json, out ProcessUtilities.ProcessResult? value)
```

**Purpose:** Attempts to deserialize a JSON string to a `ProcessUtilities.ProcessResult` instance.

**Parameters:**
- `json` — The JSON string to deserialize.
- `value` — Receives the deserialized ProcessResult instance if successful.

**Return value:** `true` if deserialization succeeded; otherwise, `false`.

**Throws:** `ArgumentException` when `json` is `null` or empty.

---

### OperationStatsJsonExtensions

Extension methods for `OperationStats`.

#### ToJson

```csharp
public static string ToJson(this OperationStats stats, bool indented = false)
```

**Purpose:** Converts the `OperationStats` to a JSON string.

**Parameters:**
- `stats` — The operation statistics to convert.
- `indented` — Whether to format the JSON with indentation.

**Return value:** A JSON string representation of the statistics.

**Throws:** `ArgumentNullException` when `stats` is null.

---

### AdaptiveBitrateServiceJsonExtensions

Provides JSON serialization extensions for `AdaptiveBitrateService`.

#### ToJson

```csharp
public static string ToJson(this AdaptiveBitrateService value, bool indented = false)
```

**Purpose:** Serializes the `AdaptiveBitrateService` instance to a JSON string.

**Parameters:**
- `value` — The service instance to serialize.
- `indented` — Whether to format the JSON with indentation for readability.

**Return value:** A JSON string representation of the service.

**Throws:** `ArgumentNullException` when `value` is null.

#### FromJson

```csharp
public static AdaptiveBitrateService? FromJson(string json)
```

**Purpose:** Deserializes a JSON string to an `AdaptiveBitrateService` instance.

**Parameters:**
- `json` — The JSON string to deserialize.

**Return value:** The deserialized service instance, or null if the JSON is empty or whitespace.

**Throws:** 
- `ArgumentNullException` when `json` is null.
- `JsonException` when the JSON is invalid or cannot be deserialized.

#### TryFromJson

```csharp
public static bool TryFromJson(string json, out AdaptiveBitrateService? value)
```

**Purpose:** Attempts to deserialize a JSON string to an `AdaptiveBitrateService` instance.

**Parameters:**
- `json` — The JSON string to deserialize.
- `value` — Receives the deserialized service instance if successful.

**Return value:** True if deserialization succeeded; otherwise, false.

**Throws:** `ArgumentNullException` when `json` is null.

---

### TranscodeServiceJsonExtensions

Provides System.Text.Json serialization and deserialization extensions for `TranscodeService`.

#### ToJson

```csharp
public static string ToJson(this TranscodeService value, bool indented = false)
```

**Purpose:** Serializes the `TranscodeService` instance to a JSON string.

**Parameters:**
- `value` — The service instance to serialize.
- `indented` — Whether to format the JSON with indentation for readability.

**Return value:** A JSON string representation of the service.

**Throws:** `ArgumentNullException` when `value` is `null`.

#### FromJson

```csharp
public static TranscodeService? FromJson(string json)
```

**Purpose:** Deserializes a JSON string to a `TranscodeService` instance.

**Parameters:**
- `json` — The JSON string to deserialize.

**Return value:** The deserialized `TranscodeService` instance, or `null` if the JSON represents a null value.

**Throws:** 
- `ArgumentNullException` when `json` is `null`.
- `JsonException` when the JSON is invalid or cannot be deserialized.

#### TryFromJson

```csharp
public static bool TryFromJson(string json, out TranscodeService? value)
```

**Purpose:** Attempts to deserialize a JSON string to a `TranscodeService` instance.

**Parameters:**
- `json` — The JSON string to deserialize.
- `value` — Receives the deserialized instance if successful.

**Return value:** `true` if deserialization succeeds; otherwise, `false`.

**Throws:** `ArgumentNullException` when `json` is `null`.

---

### MediaProbeResultJsonExtensions

Provides JSON serialization extensions for `MediaProbeResult`.

#### ToJson

```csharp
public static string ToJson(this MediaProbeResult result, bool indented = false)
```

**Purpose:** Converts the `MediaProbeResult` to a JSON string.

**Parameters:**
- `result` — The media probe result to convert.
- `indented` — If set to `true` the JSON is indented; otherwise, it's compact.

**Return value:** A JSON string representing the media probe result.

**Throws:** `ArgumentNullException` when `result` is `null`.

---

### OperationRepositoryJsonExtensions

Provides System.Text.Json serialization extensions for `OperationRepository`.

#### ToJson

```csharp
public static string ToJson(this OperationRepository value, bool indented = false)
```

**Purpose:** Serializes the `OperationRepository` to a JSON string.

**Parameters:**
- `value` — The repository to serialize.
- `indented` — Whether to format the JSON with indentation for readability.

**Return value:** A JSON string representation of the repository.

**Throws:** `ArgumentNullException` when `value` is `null`.

#### FromJson

```csharp
public static OperationRepository? FromJson(string json)
```

**Purpose:** Deserializes a JSON string to an `OperationRepository` instance.

**Parameters:**
- `json` — The JSON string to deserialize.

**Return value:** An `OperationRepository` instance, or `null` if the JSON is empty or whitespace.

**Throws:** 
- `ArgumentException` when `json` is `null`, empty, or whitespace.
- `JsonException` when the JSON is invalid or cannot be deserialized.

#### TryFromJson

```csharp
public static bool TryFromJson(string json, out OperationRepository? value)
```

**Purpose:** Attempts to deserialize a JSON string to an `OperationRepository` instance.

**Parameters:**
- `json` — The JSON string to deserialize.
- `value` — Receives the deserialized repository, or `null` if deserialization fails.

**Return value:** `true` if deserialization succeeds; otherwise, `false`.

**Throws:** `ArgumentException` when `json` is `null`, empty, or whitespace.

## Usage

### Example: Round-trip JSON serialization with ApiRequest

```csharp
using FFmpegDotnetWrapper.Api.DTOs;
using System.Text.Json;

// Create an API request
var apiRequest = new ApiRequest
{
    // Assuming ApiRequest has properties like Endpoint, Method, etc.
    // Adjust based on actual ApiRequest class definition
};

// Serialize to JSON
string json = apiRequest.ToJson(indented: true);
Console.WriteLine(json);

// Deserialize back from object
ApiRequest? deserialized = ApiRequestJsonExtensions.FromJson(json);
if (deserialized != null)
{
    Console.WriteLine($"Deserialized API request: {deserialized}");
}

// Example with TryFromJson
if (ApiRequestJsonExtensions.TryFromJson(json, out var tryResult))
{
    Console.WriteLine($"TryFromJson successful: {tryResult != null}");
}
```