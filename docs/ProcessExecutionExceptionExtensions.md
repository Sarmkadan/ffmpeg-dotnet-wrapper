# ProcessExecutionExceptionExtensions

Static utility members that provide information about the outcome of a process execution.

## API

### IsSuccessful
- **Purpose:** Indicates whether the process execution was successful.  
- **Parameters:** None.  
- **Return value:** `true` if the execution succeeded; otherwise `false`.  
- **Exceptions:** Does not throw any exceptions under normal usage.

### GetErrorMessage
- **Purpose:** Retrieves a concise error message associated with the process execution failure.  
- **Parameters:** None.  
- **Return value:** A string containing the error message; returns an empty string if no error information is available.  
- **Exceptions:** Does not throw any exceptions under normal usage.

### GetDetailedErrorMessage
- **Purpose:** Retrieves a detailed error message, which may include standard error output from the process.  
- **Parameters:** None.  
- **Return value:** A string containing detailed error information; returns an empty string if unavailable.  
- **Exceptions:** Does not throw any exceptions under normal usage.

### GetFullExceptionDetails
- **Purpose:** Returns a full description of the exception, including stack trace and process‑specific data.  
- **Parameters:** None.  
- **Return value:** A string with complete exception details.  
- **Exceptions:** Does not throw any exceptions under normal usage.

## Usage

```csharp
// Example 1: Check success and obtain a brief error message
bool succeeded = ProcessExecutionExceptionExtensions.IsSuccessful();
if (!succeeded)
{
    string brief = ProcessExecutionExceptionExtensions.GetErrorMessage();
    Console.WriteLine($"Execution failed: {brief}");
}
```

```csharp
// Example 2: Get detailed diagnostics and full exception info
string detailed = ProcessExecutionExceptionExtensions.GetDetailedErrorMessage();
string full = ProcessExecutionExceptionExtensions.GetFullExceptionDetails();
Debug.WriteLine($"Detailed: {detailed}");
Debug.WriteLine($"Full: {full}");
```

## Notes

- These members are stateless and thread‑safe; they only read internal data and do not modify shared state.  
- If the underlying process execution context does not contain error information, `GetErrorMessage` and `GetDetailedErrorMessage` will return `string.Empty`.  
- `GetFullExceptionDetails` may produce a large string because it includes the complete exception representation.  
- Invoking these members on a null reference is not applicable, as they are static methods; however, any internal state they rely on must be properly initialized by the caller before use.
