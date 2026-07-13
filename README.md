// ... (rest of README.md content remains unchanged)

## ServiceException

The `ServiceException` class represents an exception that occurs when a service-related error is encountered. It provides information about the service name and the error message.

```csharp
try
{
    // Service code
}
catch (ServiceException ex)
{
    Console.WriteLine($"Service Error: {ex.Message}");
    Console.WriteLine($"Service Name: {ex.ServiceName}");
}
```
// ... (rest of README.md content remains unchanged)
