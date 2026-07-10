# ApiResponse
The `ApiResponse` type is a generic class used to represent the response from an API, encapsulating information about the success or failure of the request, along with any data or errors that may have occurred. It provides a standardized way to handle API responses, making it easier to manage and process the results of API calls.

## API
The `ApiResponse` class has several public members:
- `Success`: A boolean indicating whether the API request was successful.
- `StatusCode`: An integer representing the HTTP status code of the response.
- `Message`: A string containing a message describing the response.
- `Data`: A generic property of type `T` that contains the data returned by the API, if any.
- `Errors`: A list of `ApiError` objects that contain information about any errors that occurred during the request.
- `Timestamp`: A `DateTime` object representing the time at which the response was received.
- `RequestId`: A string that uniquely identifies the request, if available.
- `StackTrace`: A string containing the stack trace of any exception that occurred during the request, if available.
- `Code`: A string representing an error code, if applicable.
- `Field`: A string indicating the field related to an error, if applicable.
- `Suggestion`: A string providing a suggestion for how to resolve an error, if applicable.
The class also includes several static factory methods:
- `Ok`: Creates a successful `ApiResponse` instance with the provided data.
- `Failure`: Creates a failed `ApiResponse` instance with the provided error information.

## Usage
Here are two examples of using the `ApiResponse` class:
```csharp
// Example 1: Handling a successful API response
ApiResponse<string> response = ApiResponse<string>.Ok("Hello, World!");
if (response.Success)
{
    Console.WriteLine(response.Data); // Outputs: Hello, World!
}

// Example 2: Handling a failed API response
ApiResponse<string> failedResponse = ApiResponse<string>.Failure("Invalid request");
if (!failedResponse.Success)
{
    Console.WriteLine(failedResponse.Message); // Outputs: Invalid request
    foreach (var error in failedResponse.Errors)
    {
        Console.WriteLine(error.Message);
    }
}
```

## Notes
When using the `ApiResponse` class, it's essential to check the `Success` property to determine whether the API request was successful. If `Success` is `false`, you should inspect the `Errors` list to retrieve information about the errors that occurred. The `Data` property should only be accessed when `Success` is `true`. The `ApiResponse` class is designed to be thread-safe, allowing it to be safely used in concurrent environments. However, the thread-safety of the `Data` and `Errors` properties depends on the thread-safety of the objects they contain.
