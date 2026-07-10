# ValidationMiddleware

`ValidationMiddleware` provides a set of static methods for validating API request objects and file paths before they are processed by the FFmpeg wrapper. It ensures that incoming data meets structural and semantic constraints, returning standardized `ApiResponse` objects that indicate success or failure without throwing exceptions for routine validation errors.

## API

### `ValidateRequest<T>`

```csharp
public static ApiResponse<T>? ValidateRequest<T>(T request)
```

Validates a single request object of type `T`. Checks for null references, required fields, and type-specific constraints defined by the request model.

- **Parameters**: `request` — the object to validate.
- **Returns**: An `ApiResponse<T>` containing the original request and a success flag if validation passes; an `ApiResponse<T>` with error details if validation fails; `null` only when the request itself is `null` and the method cannot construct a meaningful response.
- **Throws**: Does not throw. All validation outcomes are communicated through the return value.

### `ValidateRequestList<T>`

```csharp
public static ApiResponse<List<T>>? ValidateRequestList<T>(List<T> requestList)
```

Validates a list of request objects. Each element is individually validated, and aggregate errors are collected.

- **Parameters**: `requestList` — the list of objects to validate.
- **Returns**: An `ApiResponse<List<T>>` with the original list if all elements pass; an `ApiResponse<List<T>>` with accumulated error messages if any element fails; `null` if the list itself is `null`.
- **Throws**: Does not throw.

### `ValidateFilePaths<T>`

```csharp
public static ApiResponse<T>? ValidateFilePaths<T>(T request)
```

Validates that all file paths referenced within the request object exist on disk and are accessible. This method inspects path properties on the request model and verifies them against the file system.

- **Parameters**: `request` — the object whose file path properties are to be checked.
- **Returns**: An `ApiResponse<T>` indicating success or listing the invalid paths; `null` if the request is `null`.
- **Throws**: Does not throw. File system exceptions (e.g., permission errors) are caught and surfaced as error messages in the response.

### `ValidateMergeRequest`

```csharp
public static ApiResponse<MergeRequest>? ValidateMergeRequest(MergeRequest request)
```

Performs specialized validation for merge operations. Ensures that the `MergeRequest` contains at least two input files, that all input paths are valid, and that the output path is writable.

- **Parameters**: `request` — the `MergeRequest` instance to validate.
- **Returns**: An `ApiResponse<MergeRequest>` with validation result; `null` if the request is `null`.
- **Throws**: Does not throw.

## Usage

### Example 1: Validating a Single Transcode Request

```csharp
var request = new TranscodeRequest
{
    InputPath = "/videos/input.mp4",
    OutputPath = "/videos/output.avi",
    Codec = "h264"
};

ApiResponse<TranscodeRequest>? response = ValidationMiddleware.ValidateRequest(request);

if (response == null)
{
    Console.WriteLine("Request was null.");
    return;
}

if (!response.IsSuccess)
{
    Console.WriteLine($"Validation failed: {response.ErrorMessage}");
    return;
}

// Proceed with transcode operation using response.Data
```

### Example 2: Validating a Batch Request with File Path Checks

```csharp
var batchRequests = new List<TranscodeRequest>
{
    new TranscodeRequest { InputPath = "/videos/a.mp4", OutputPath = "/videos/a_out.mkv" },
    new TranscodeRequest { InputPath = "/videos/b.mp4", OutputPath = "/videos/b_out.mkv" }
};

ApiResponse<List<TranscodeRequest>>? listResponse = ValidationMiddleware.ValidateRequestList(batchRequests);

if (listResponse == null || !listResponse.IsSuccess)
{
    Console.WriteLine($"Batch validation failed: {listResponse?.ErrorMessage}");
    return;
}

foreach (var req in listResponse.Data)
{
    ApiResponse<TranscodeRequest>? pathResponse = ValidationMiddleware.ValidateFilePaths(req);
    if (pathResponse == null || !pathResponse.IsSuccess)
    {
        Console.WriteLine($"File path invalid for {req.InputPath}: {pathResponse?.ErrorMessage}");
        continue;
    }

    // Execute transcode for this request
}
```

## Notes

- **Null returns**: All methods return `null` when the primary argument is `null`. Callers must perform a null check on the returned `ApiResponse` before accessing its properties.
- **File path validation**: `ValidateFilePaths<T>` relies on real file system access. It will report paths as invalid if they do not exist at the moment of validation, even if they are created later. No caching is performed.
- **Thread safety**: All methods are static and stateless. They are safe to call concurrently from multiple threads, though `ValidateFilePaths<T>` may produce transient failures if file system state changes between validation and actual operation execution.
- **Error accumulation**: `ValidateRequestList<T>` collects all per-item errors into a single response. Partial success is not supported — the entire list is marked as failed if any element fails.
- **Merge-specific rules**: `ValidateMergeRequest` enforces a minimum of two input files. Requests with zero or one input are rejected regardless of path validity.
