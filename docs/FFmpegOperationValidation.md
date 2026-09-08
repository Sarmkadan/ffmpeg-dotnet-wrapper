# FFmpegOperationValidation

Provides validation helpers for `FFmpegOperation` instances.

## Methods

### Validate

```csharp
public static IReadOnlyList<string> Validate(this FFmpegOperation? value)
```

Validates the specified FFmpeg operation and returns a list of human-readable problems.

- **Parameters**
  - `value`: The operation to validate.
- **Returns**
  - An empty list if valid; otherwise, a list of validation error messages.
- **Exceptions**
  - `ArgumentNullException`: Thrown if `value` is null.

#### Validation Rules

The `Validate` method checks the following:

1. **Id**
   - Cannot be null or whitespace.
   - Cannot be an empty GUID (`Guid.Empty.ToString()`).

2. **Name**
   - Cannot be null or whitespace.
   - Cannot exceed 256 characters.

3. **Type**
   - Must be a valid `FFmpegOperationType` enum value.

4. **InputFiles**
   - Cannot be null.
   - Must contain at least one item.
   - Each input file path:
     - Cannot be null or whitespace.
     - Cannot exceed 4096 characters.

5. **OutputFile**
   - Cannot be null or whitespace.
   - Cannot exceed 4096 characters.
   - The directory path (if any) must not exceed 4096 characters.
   - Must not contain invalid characters or be malformed.

6. **Arguments**
   - Cannot be null.
   - Each argument:
     - Cannot be null or whitespace.
     - Cannot exceed 1024 characters.

7. **Timeout** (if specified)
   - Must be a positive time span.
   - Cannot exceed 24 hours.

8. **Priority** (if specified)
   - Cannot be negative.
   - Cannot exceed 100.

9. **IsParallel**
   - No validation needed (boolean).

10. **CustomProperties**
    - Cannot be null.
    - Each key:
      - Cannot be null or whitespace.
      - Cannot exceed 256 characters.
    - Each value (if not null):
      - Cannot exceed 1024 characters.

11. **CreatedAt**
    - Must be set to a valid `DateTime` value.
    - Must be in UTC timezone.
    - Cannot be in the future (more than 5 minutes ahead).
    - Cannot be more than one year in the past.

12. **ExecutedAt** (if specified)
    - Must be set to a valid `DateTime` value.
    - Must be in UTC timezone.
    - Cannot be in the future (more than 5 minutes ahead).
    - Cannot be earlier than `CreatedAt`.

### IsValid

```csharp
public static bool IsValid(this FFmpegOperation? value)
```

Determines whether the specified FFmpeg operation is valid.

- **Parameters**
  - `value`: The operation to validate.
- **Returns**
  - `true` if the operation is valid; otherwise, `false`.
- **Exceptions**
  - `ArgumentNullException`: Thrown if `value` is null.

### EnsureValid

```csharp
public static void EnsureValid(this FFmpegOperation? value)
```

Validates the specified FFmpeg operation and throws an exception if invalid.

- **Parameters**
  - `value`: The operation to validate.
- **Exceptions**
  - `ArgumentNullException`: Thrown if `value` is null.
  - `ArgumentException`: Thrown if the operation is invalid, containing a list of validation errors.

## Example

```csharp
using FFmpegDotnetWrapper.Models;

// Create an operation
var operation = new FFmpegOperation
{
    Id = Guid.NewGuid().ToString(),
    Name = "Convert to MP4",
    Type = FFmpegOperationType.Conversion,
    InputFiles = new List<string> { "input.avi" },
    OutputFile = "output.mp4",
    Arguments = new List<string> { "-c:v", "libx264", "-c:a", "aac" },
    Timeout = TimeSpan.FromMinutes(30),
    Priority = 50,
    IsParallel = false,
    CustomProperties = new Dictionary<string, string>
    {
        ["preset"] = "medium",
        ["crf"] = "23"
    },
    CreatedAt = DateTime.UtcNow,
    ExecutedAt = DateTime.UtcNow.AddSeconds(10)
};

// Validate and get errors
var errors = operation.Validate();
if (errors.Count > 0)
{
    foreach (var error in errors)
    {
        Console.WriteLine($"Validation error: {error}");
    }
}

// Check if valid
bool isValid = operation.IsValid();

// Throw exception if invalid
operation.EnsureValid(); // Throws ArgumentException if invalid
```