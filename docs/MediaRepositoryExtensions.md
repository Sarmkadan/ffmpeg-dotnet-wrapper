# MediaRepositoryExtensions
The `MediaRepositoryExtensions` class provides a set of extension methods for managing media files in a repository. It offers functionality for retrieving media files, counting media items, adding or updating media files by file path, and searching for media files by name. These methods are designed to be used asynchronously, allowing for non-blocking operations.

## API
* `GetAllMediaFilesAsync`: Retrieves a list of all media files in the repository. Returns an `IReadOnlyList<MediaFile>` containing all media files. Throws an exception if an error occurs during the retrieval process.
* `GetMediaCountAsync`: Retrieves the total count of media items in the repository. Returns an `int` representing the count of media items. Throws an exception if an error occurs during the retrieval process.
* `AddOrUpdateByFilePathAsync`: Adds or updates a media file in the repository based on the provided file path. Returns the added or updated `MediaFile` object. Throws an exception if an error occurs during the addition or update process.
* `SearchByNameOrEmptyAsync`: Searches for media files in the repository by name, returning all files if no name is provided. Returns an `IReadOnlyList<MediaFile>` containing the search results. Throws an exception if an error occurs during the search process.

## Usage
```csharp
// Example 1: Retrieving all media files and counting media items
var mediaFiles = await MediaRepositoryExtensions.GetAllMediaFilesAsync();
var mediaCount = await MediaRepositoryExtensions.GetMediaCountAsync();
Console.WriteLine($"Total media files: {mediaCount}, Retrieved media files: {mediaFiles.Count}");

// Example 2: Adding a new media file and searching for media files by name
var newMediaFile = await MediaRepositoryExtensions.AddOrUpdateByFilePathAsync("path/to/new/media/file.mp4");
var searchResults = await MediaRepositoryExtensions.SearchByNameOrEmptyAsync("example");
Console.WriteLine($"Added media file: {newMediaFile.Name}, Search results: {searchResults.Count}");
```

## Notes
The `MediaRepositoryExtensions` class is designed to be used in a multi-threaded environment, and its methods are thread-safe. However, the underlying repository implementation may have its own thread-safety considerations. When using these extension methods, be aware of potential edge cases such as duplicate file paths, empty search queries, or repository connectivity issues. Additionally, error handling and logging mechanisms should be implemented to handle exceptions thrown by these methods.
