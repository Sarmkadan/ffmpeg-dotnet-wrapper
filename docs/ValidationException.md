# ValidationException

Represents an exception thrown when validation of input parameters or configuration fails, encapsulating structured validation errors alongside a descriptive message.

## API

### ValidationErrors
```csharp
public Dictionary<string, string[]>? ValidationErrors { get; }
```
Gets the collection of validation errors, where each key is a field or property name and each value is an array of error messages for that field. Returns `null` if no structured errors were provided.

### ValidationException(string message)
```csharp
public ValidationException(string message) : base(message)
```
Initializes a new instance with a descriptive error message.

**Parameters**
- `message`: The error message describing the validation failure.

### ValidationException(string message, Dictionary<string, string[]> validationErrors)
```csharp
public ValidationException(string message, Dictionary<string, string[]> validationErrors) : base(message)
```
Initializes a new instance with a descriptive error message and structured validation errors.

**Parameters**
- `message`: The error message describing the validation failure.
- `validationErrors`: A dictionary mapping field names to arrays of validation error messages.

### ValidationException(string message, Exception innerException)
```csharp
public ValidationException(string message, Exception innerException) : base(message, innerException)
```
Initializes a new instance with a descriptive error message and a reference to the inner exception that caused this exception.

**Parameters**
- `message`: The error message describing the validation failure.
- `innerException`: The exception that is the cause of the current exception.

### ValidationException(string message, Dictionary<string, string[]> validationErrors, Exception innerException)
```csharp
public ValidationException(string message, Dictionary<string, string[]> validationErrors, Exception innerException) : base(message, innerException)
```
Initializes a new instance with a descriptive error message, structured validation errors, and a reference to the inner exception.

**Parameters**
- `message`: The error message describing the validation failure.
- `validationErrors`: A dictionary mapping field names to arrays of validation error messages.
- `innerException`: The exception that is the cause of the current exception.

### FromDictionary
```csharp
public static ValidationException FromDictionary(string message, Dictionary<string, string[]> validationErrors)
```
Creates a `ValidationException` from a message and a validation errors dictionary.

**Parameters**
- `message`: The error message describing the validation failure.
- `validationErrors`: A dictionary mapping field names to arrays of validation error messages.

**Returns**
A new `ValidationException` instance populated with the provided message and validation errors.

## Usage

### Validating FFmpeg input parameters
```csharp
public void ConfigureEncoding(EncodingOptions options)
{
    var errors = new Dictionary<string, string[]>();

    if (string.IsNullOrWhiteSpace(options.InputPath))
        errors["InputPath"] = new[] { "Input path is required." };

    if (options.VideoBitrate <= 0)
        errors["VideoBitrate"] = new[] { "Video bitrate must be greater than zero." };

    if (options.AudioSampleRate != 44100 && options.AudioSampleRate != 48000)
        errors["AudioSampleRate"] = new[] { "Audio sample rate must be 44100 or 48000 Hz." };

    if (errors.Count > 0)
        throw ValidationException.FromDictionary("Encoding configuration is invalid.", errors);
}
```

### Catching and reporting validation failures
```csharp
try
{
    var job = await _ffmpegService.StartConversionAsync(request);
}
catch (ValidationException ex)
{
    _logger.LogWarning(ex, "Validation failed for conversion request {RequestId}", request.Id);

    if (ex.ValidationErrors != null)
    {
        foreach (var (field, messages) in ex.ValidationErrors)
        {
            foreach (var msg in messages)
            {
                ModelState.AddModelError(field, msg);
            }
        }
    }

    return BadRequest(ModelState);
}
```

## Notes

- The `ValidationErrors` dictionary is immutable after construction; callers should not modify the returned reference.
- When `ValidationErrors` is `null`, the exception carries only the base message. Always check for `null` before iterating.
- The static `FromDictionary` factory is a convenience method equivalent to calling the two-argument constructor directly.
- Instances are immutable and thread-safe for read access once constructed. No synchronization is required when sharing a caught exception across threads.
- The exception does not implement `ISerializable`; serialization behavior follows the base `Exception` class.
