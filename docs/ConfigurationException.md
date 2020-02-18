# ConfigurationException

Represents an exception that occurs when a required configuration value is missing, invalid, or cannot be resolved during FFmpeg wrapper initialization or operation. This exception captures the specific configuration key involved to facilitate targeted diagnostics and recovery.

## API

### ConfigurationKey
```csharp
public string? ConfigurationKey { get; }
```
Gets the name of the configuration key that caused the exception, if available. Returns `null` when the exception was constructed without a configuration key.

### ConfigurationException(string message)
```csharp
public ConfigurationException(string message) : base(message)
```
Initializes a new instance with a descriptive error message.

**Parameters:**
- `message`: The error message explaining the configuration failure.

**Throws:** None directly; propagates any exception from the base `Exception` constructor.

### ConfigurationException(string message, string configurationKey)
```csharp
public ConfigurationException(string message, string configurationKey) : base(message)
```
Initializes a new instance with a descriptive error message and the associated configuration key.

**Parameters:**
- `message`: The error message explaining the configuration failure.
- `configurationKey`: The name of the configuration key that triggered the exception. Stored in `ConfigurationKey`.

**Throws:** None directly; propagates any exception from the base `Exception` constructor.

### ConfigurationException(string message, Exception innerException)
```csharp
public ConfigurationException(string message, Exception innerException) : base(message, innerException)
```
Initializes a new instance with a descriptive error message and a reference to the inner exception that caused this exception.

**Parameters:**
- `message`: The error message explaining the configuration failure.
- `innerException`: The exception that is the cause of the current exception.

**Throws:** None directly; propagates any exception from the base `Exception` constructor.

### ConfigurationException(string message, string configurationKey, Exception innerException)
```csharp
public ConfigurationException(string message, string configurationKey, Exception innerException) : base(message, innerException)
```
Initializes a new instance with a descriptive error message, the associated configuration key, and a reference to the inner exception.

**Parameters:**
- `message`: The error message explaining the configuration failure.
- `configurationKey`: The name of the configuration key that triggered the exception. Stored in `ConfigurationKey`.
- `innerException`: The exception that is the cause of the current exception.

**Throws:** None directly; propagates any exception from the base `Exception` constructor.

## Usage

### Validating required configuration at startup
```csharp
public void InitializeFfmpeg(IConfiguration config)
{
    var ffmpegPath = config["FFmpeg:Path"];
    if (string.IsNullOrWhiteSpace(ffmpegPath))
    {
        throw new ConfigurationException(
            "FFmpeg executable path is not configured.",
            "FFmpeg:Path");
    }

    if (!File.Exists(ffmpegPath))
    {
        throw new ConfigurationException(
            $"FFmpeg executable not found at configured path: {ffmpegPath}",
            "FFmpeg:Path",
            new FileNotFoundException("FFmpeg binary missing", ffmpegPath));
    }

    FfmpegWrapper.Configure(ffmpegPath);
}
```

### Wrapping configuration binding failures
```csharp
public FfmpegOptions LoadOptions(IConfigurationSection section)
{
    try
    {
        return section.Get<FfmpegOptions>() 
            ?? throw new ConfigurationException(
                "FFmpeg options section is missing or empty.",
                "FFmpeg:Options");
    }
    catch (Exception ex) when (ex is not ConfigurationException)
    {
        throw new ConfigurationException(
            "Failed to bind FFmpeg options from configuration.",
            "FFmpeg:Options",
            ex);
    }
}
```

## Notes

- **Immutability**: The `ConfigurationKey` property is read-only and set only during construction. Instances are effectively immutable after creation.
- **Thread safety**: This type is thread-safe for read-only access. Multiple threads may simultaneously read `ConfigurationKey` and standard `Exception` properties without synchronization.
- **Null configuration key**: When constructed without a `configurationKey` parameter, `ConfigurationKey` returns `null`. Consumers should guard against null when logging or displaying the key.
- **Inheritance**: As a standard `Exception` derivative, it participates in normal exception filtering and catching. Catch `ConfigurationException` specifically to handle configuration errors distinctly from other runtime failures.
- **Serialization**: Supports standard .NET exception serialization. The `ConfigurationKey` value is preserved across serialization boundaries when using `ISerializable` or binary serialization formats.
