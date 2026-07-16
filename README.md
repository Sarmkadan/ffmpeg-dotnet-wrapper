// ... (rest of README.md content remains unchanged)

## MediaRepository

The `MediaRepository` class provides an in-memory implementation of a media repository, allowing you to manage media files with various operations. It supports CRUD (Create, Read, Update, Delete) operations, as well as querying for media files by ID, file path, name, format, and more.

```csharp
using FFmpegDotnetWrapper.Repository;

// Create a media repository instance
var repository = new MediaRepository();

// Add a new media file
var mediaFile = new MediaFile
{
    Id = Guid.NewGuid().ToString(),
    Name = "sample_video.mp4",
    FilePath = "/path/to/sample_video.mp4"
};
await repository.AddAsync(mediaFile);

// Get a media file by ID
var retrievedMediaFile = await repository.GetByIdAsync(mediaFile.Id);
Console.WriteLine(retrievedMediaFile?.Name);

// Get all media files
var allMediaFiles = await repository.GetAllAsync();
Console.WriteLine($"Total media files: {allMediaFiles.Count()}");

// Update an existing media file
mediaFile.Name = "updated_sample_video.mp4";
await repository.UpdateAsync(mediaFile);

// Delete a media file by ID
await repository.DeleteAsync(mediaFile.Id);

// Search for media files by name
var searchedMediaFiles = await repository.SearchByNameAsync("sample_video");
Console.WriteLine($"Found {searchedMediaFiles.Count()} media files with name 'sample_video'");

// Get media files by format
var videoFiles = await repository.GetByFormatAsync(ContainerFormat.MP4);
Console.WriteLine($"Found {videoFiles.Count()} video files with format MP4");

// Get video files
var videoFilesList = await repository.GetVideoFilesAsync();
Console.WriteLine($"Found {videoFilesList.Count()} video files");

// Get audio files
var audioFilesList = await repository.GetAudioFilesAsync();
Console.WriteLine($"Found {audioFilesList.Count()} audio files");

// Check if a media file exists by ID
var exists = await repository.ExistsAsync(mediaFile.Id);
Console.WriteLine($"Media file with ID {mediaFile.Id} exists: {exists}");

// Get the total count of media files
var totalCount = await repository.GetCountAsync();
Console.WriteLine($"Total media files: {totalCount}");
```

// ... (rest of README.md content remains unchanged)

## RequestLoggingOptions

The `RequestLoggingOptions` class provides configuration for controlling what information is logged by the `RequestLoggingMiddleware`. It allows you to customize logging behavior based on your requirements for detail level, performance, and security constraints.



```csharp
using FFmpegDotnetWrapper.Middleware;

// Create default options (logs arguments and stack traces, includes performance metrics)
var defaultOptions = new RequestLoggingOptions();

// Create custom options for production (disable sensitive data logging)
var productionOptions = new RequestLoggingOptions
{
    LogArguments = true,           // Log request parameters
    LogResponseData = false,       // Don't log response payloads in production
    LogStackTrace = true,          // Include stack traces for errors
    MaxLogValueLength = 500,      // Truncate long values
    LogPerformanceMetrics = true   // Track execution time and resource usage
};

// Example usage with middleware
services.AddSingleton(productionOptions);
services.AddScoped<RequestLoggingMiddleware>();
```

## RateLimitPolicy

The `RateLimitPolicy` class defines rate limiting rules for API endpoints using either fixed window or sliding window algorithms. It tracks request counts within configurable time windows and enforces maximum request limits per user or globally.




```csharp
using FFmpegDotnetWrapper.Middleware;

// Create a rate limit policy with fixed window algorithm
var fixedWindowPolicy = new RateLimitPolicy
{
    MaxRequests = 100,
    WindowSeconds = 60,
    PerUserLimit = true,
    PolicyName = "api-rate-limit"
};

// Create a rate limit policy with sliding window algorithm
var slidingWindowPolicy = new RateLimitPolicy
{
    MaxRequests = 50,
    WindowSeconds = 30,
    PerUserLimit = false,
    PolicyName = "download-rate-limit"
};

// Register policies with dependency injection
services.AddSingleton(fixedWindowPolicy);
services.AddSingleton(slidingWindowPolicy);

// Check if a request is allowed (returns true if within limit, false if exceeded)
var isAllowed = fixedWindowPolicy.AllowRequest("user123");

// Get current rate limit status
var status = fixedWindowPolicy.GetStatus("user123");
Console.WriteLine($"Requests made: {status.RequestsMade}, Max allowed: {status.MaxRequests}");

// Reset the rate limiter for a specific user
fixedWindowPolicy.Reset("user123");

// Reset all rate limiters
slidingWindowPolicy.ResetAll();
```
