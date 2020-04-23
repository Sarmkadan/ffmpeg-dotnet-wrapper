// ... (rest of README.md content remains unchanged)

## ProcessExecutionException

The `ProcessExecutionException` class represents an exception that occurs when a process execution fails, including FFmpeg and FFprobe operations. It provides information about the process exit code and error output.

```csharp
try
{
    // Process execution code
}
catch (ProcessExecutionException ex)
{
    Console.WriteLine($"Process Error: {ex.Message}");
    if (ex.ExitCode.HasValue)
    {
        Console.WriteLine($"Exit Code: {ex.ExitCode}");
    }
    if (ex.ErrorOutput != null)
    {
        Console.WriteLine($"Error Output: {ex.ErrorOutput}");
    }
}
```

// ... (rest of README.md content remains unchanged)
