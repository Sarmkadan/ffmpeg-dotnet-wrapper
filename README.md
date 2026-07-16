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
