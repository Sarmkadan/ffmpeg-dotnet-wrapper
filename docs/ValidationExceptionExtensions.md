# ValidationExceptionExtensions

Provides extension methods for `ValidationException` to facilitate error message aggregation, field-specific error checking, error augmentation, and detailed string representation. These methods simplify common validation exception handling patterns in the context of FFmpeg command processing workflows.

## API

### GetAllErrorMessages

```csharp
public static IEnumerable<string> GetAllErrorMessages(this ValidationException exception)
```

Retrieves all error messages contained within the `ValidationException` and its inner exceptions. Flattened into a single enumerable sequence for easy iteration.

**Parameters**
- `exception`: The source `ValidationException` instance.

**Returns**
- `IEnumerable<string>`: A flattened sequence of all error messages from the exception hierarchy.

**Exceptions**
- `ArgumentNullException`: Thrown when `exception` is null.

---

### HasErrorForField

```csharp
public static bool HasErrorForField(this ValidationException exception, string fieldName)
```

Determines whether the `ValidationException` contains an error associated with a specific field name.

**Parameters**
- `exception`: The source `ValidationException` instance.
- `fieldName`: The name of the field to check for errors.

**Returns**
- `bool`: `true` if an error exists for the specified field; otherwise, `false`.

**Exceptions**
- `ArgumentNullException`: Thrown when `exception` or `fieldName` is null.

---

### WithAddedError

```csharp
public static ValidationException WithAddedError(this ValidationException exception, string errorMessage)
```

Creates a new `ValidationException` with an additional error message appended to the original exception's errors. Preserves the original exception as inner exception.

**Parameters**
- `exception`: The source `ValidationException` instance.
- `errorMessage`: The error message to add.

**Returns**
- `ValidationException`: A new exception instance containing the original errors plus the new message.

**Exceptions**
- `ArgumentNullException`: Thrown when `exception` or `errorMessage` is null.

---

### ToDetailedString

```csharp
public static string ToDetailedString(this ValidationException exception)
```

Generates a detailed multi-line string representation of the `ValidationException`, including all error messages and field-specific details.

**Parameters**
- `exception`: The source `ValidationException` instance.

**Returns**
- `string`: A formatted string containing all validation error details.

**Exceptions**
- `ArgumentNullException`: Thrown when `exception` is null.

---

## Usage

### Aggregating All Errors

```csharp
try
{
    var result = FFmpegConverter.Convert(inputFile, outputFile, settings);
}
catch (ValidationException ex)
{
    var allErrors = ex.GetAllErrorMessages();
    foreach (var error in allErrors)
    {
        Console.WriteLine($"Validation error: {error}");
    }
}
```

### Checking Field Errors and Augmenting

```csharp
ValidationException ex = ValidateFFmpegSettings(settings);

if (ex.HasErrorForField("Codec"))
{
    ex = ex.WithAddedError("Unsupported codec specified for target format");
}

throw ex;
```

---

## Notes

- **Null Handling**: All methods throw `ArgumentNullException` for null inputs. Callers must ensure non-null `ValidationException` and parameter values.
- **Immutability**: `WithAddedError` returns a new exception instance rather than modifying the original, preserving immutability semantics.
- **Thread Safety**: These methods are thread-safe for read operations on the source exception. However, concurrent calls to `WithAddedError` on the same exception instance may produce inconsistent results if the underlying exception structure is modified externally.
- **Empty Exceptions**: `GetAllErrorMessages` returns an empty enumerable if the exception contains no errors. `HasErrorForField` returns `false` for non-existent fields.
