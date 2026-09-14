# ExceptionExtensionHelpers

Provides static extension methods for `ConfigurationException` and `ServiceException` objects, enabling enhanced error handling, context enrichment, and diagnostic capabilities for FFmpeg wrapper exceptions.

## API

### ConfigurationExceptionExtensions

#### HasConfigurationKey

```csharp
public static bool HasConfigurationKey(this ConfigurationException exception, string key)
```

**Purpose:** Checks if the exception was caused by a specific configuration key.

**Parameters:**
- `exception` — The `ConfigurationException` instance to check. Must not be `null`.
- `key` — The configuration key to compare against. Must not be `null` or empty.

**Return value:** `True` if the exception's configuration key matches the specified key (case-insensitive); otherwise, `false`.

**Throws:** `ArgumentNullException` when `exception` is `null`. `ArgumentException` when `key` is `null` or empty.

---

#### WithContext

```csharp
public static ConfigurationException WithContext(this ConfigurationException exception, string key, string value)
```

**Purpose:** Adds additional context to the exception's Context dictionary.

**Parameters:**
- `exception` — The `ConfigurationException` instance to update. Must not be `null`.
- `key` — The context key to add. Must not be `null` or empty.
- `value` — The context value to add. Must not be `null` or empty.

**Return value:** The same exception instance for fluent chaining.

**Throws:** `ArgumentNullException` when `exception` is `null`. `ArgumentException` when `key` or `value` is `null` or empty.

---

#### WithAdditionalContext

```csharp
public static ConfigurationException WithAdditionalContext(this ConfigurationException exception, string context)
```

**Purpose:** Creates a new exception with additional context while preserving original state.

**Parameters:**
- `exception` — The original `ConfigurationException`. Must not be `null`.
- `context` — Additional context information to include in the error message. Must not be `null`.

**Return value:** A new `ConfigurationException` with the additional context appended to the message.

**Throws:** `ArgumentNullException` when `exception` or `context` is `null`.

---

#### GetMessageWithKey

```csharp
public static string GetMessageWithKey(this ConfigurationException exception)
```

**Purpose:** Gets a formatted message including configuration key if present.

**Parameters:**
- `exception` — The `ConfigurationException` instance to format. Must not be `null`.

**Return value:** A formatted message string that includes the configuration key if it exists; otherwise, the original message.

**Throws:** `ArgumentNullException` when `exception` is `null`.

---

#### GetConfigurationKey

```csharp
public static string? GetConfigurationKey(this ConfigurationException exception)
```

**Purpose:** Gets the configuration key from the exception's Context dictionary.

**Parameters:**
- `exception` — The `ConfigurationException` instance to check. Must not be `null`.

**Return value:** The configuration key if present; otherwise, `null`.

**Throws:** `ArgumentNullException` when `exception` is `null`.

### ServiceExceptionExtensions

#### WithServiceName

```csharp
public static ServiceException WithServiceName(this ServiceException exception, string newServiceName)
```

**Purpose:** Creates a new ServiceException with the same message but a different service name.

**Parameters:**
- `exception` — The original exception containing the message and inner exception. Must not be `null`.
- `newServiceName` — The new service name to use for the exception. Must not be `null`.

**Return value:** A new `ServiceException` instance with the specified service name.

**Throws:** `ArgumentNullException` when `exception` or `newServiceName` is `null`.

---

#### WithContext

```csharp
public static ServiceException WithContext(this ServiceException exception, string key, string value)
```

**Purpose:** Adds additional context to the exception's Context dictionary.

**Parameters:**
- `exception` — The `ServiceException` instance to update. Must not be `null`.
- `key` — The context key to add. Must not be `null` or empty.
- `value` — The context value to add. Must not be `null` or empty.

**Return value:** The same exception instance for fluent chaining.

**Throws:** `ArgumentNullException` when `exception` is `null`. `ArgumentException` when `key` or `value` is `null` or empty.

---

#### GetMessageWithService

```csharp
public static string GetMessageWithService(this ServiceException exception)
```

**Purpose:** Returns a formatted string containing both service name (if present) and message.

**Parameters:**
- `exception` — The exception to format. Must not be `null`.

**Return value:** A formatted string containing service name and message, or just the message if no service name is set.

**Throws:** `ArgumentNullException` when `exception` is `null`.

---

#### HasServiceContext

```csharp
public static bool HasServiceContext(this ServiceException exception)
```

**Purpose:** Checks if the exception has service context (service name is set).

**Parameters:**
- `exception` — The exception to check. Must not be `null`.

**Return value:** `true` if the exception has a service name; otherwise, `false`.

**Throws:** `ArgumentNullException` when `exception` is `null`.

---

#### GetServiceName

```csharp
public static string? GetServiceName(this ServiceException exception)
```

**Purpose:** Gets the service name from the exception's Context dictionary.

**Parameters:**
- `exception` — The `ServiceException` instance to check. Must not be `null`.

**Return value:** The service name if present; otherwise, `null`.

**Throws:** `ArgumentNullException` when `exception` is `null`.

## Usage

### Example 1: Working with ConfigurationException

```csharp
try
{
    // Some operation that might throw ConfigurationException
    var config = LoadConfiguration("invalid-settings.json");
}
catch (ConfigurationException ex)
{
    // Check if it's related to a specific configuration key
    if (ex.HasConfigurationKey("FFmpegPath"))
    {
        logger.Error("FFmpeg path configuration is invalid: {Message}", 
                    ex.GetMessageWithKey());
        // Provide guidance to user
    }
    else
    {
        logger.Error("Configuration error: {Message}", ex.Message);
    }
    
    // Add context for better diagnostics
    ex.WithContext("Operation", "Loading user settings")
      .WithContext("Timestamp", DateTime.UtcNow.ToString());
}
```

### Example 2: Enhancing ServiceException with service name

```csharp
public async Task ProcessMediaFile(string inputPath)
{
    try
    {
        await _ffmpegService.ProcessAsync(inputPath);
    }
    catch (ServiceException ex)
    {
        // Enhance exception with service context
        var enhancedEx = ex.WithServiceName("MediaProcessor")
                          .WithContext("InputFile", inputPath)
                          .WithContext("Operation", "ProcessMediaFile");
        
        logger.Error(enhancedEx, "Media processing failed for {InputFile}", inputPath);
        throw enhancedEx; // Re-throw with enhanced context
    }
}
```

### Example 3: Fluent context building

```csharp
try
{
    ValidateSettings(userSettings);
}
catch (ConfigurationException ex)
{
    // Build rich context using fluent interface
    var detailedEx = ex
        .WithContext("UserId", currentUser.Id.ToString())
        .WithContext("SettingsVersion", userSettings.Version.ToString())
        .WithAdditionalContext("Validation occurred during startup sequence");
    
    // The detailed message will include all added context
    logger.Error(detailedEx, "Configuration validation failed");
    throw detailedEx;
}
```

## Notes

- All methods throw `ArgumentNullException` if a `null` exception reference is passed. Guard calls accordingly when exceptions may originate from contexts where the exception object itself could be absent.
- The `WithContext` methods modify the exception instance in-place and return it for fluent chaining.
- The `WithAdditionalContext` and `WithServiceName` methods create new exception instances, preserving the original exception's state while adding new information.
- Context dictionaries are particularly useful when logging exceptions with structured logging systems that can render key-value pairs.
- These extension methods are pure functions that inspect or modify exception data and do not have external side effects beyond the exception instance itself.
- The methods are safe to call from any thread without synchronization, though care should be taken when modifying exception instances that may be shared across threads.