# IMediaRepository Interface

Defines the contract for media file repository operations in the FFmpegDotnetWrapper library.

## Methods

### GetByIdAsync

```csharp
Task<MediaFile?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
```

Retrieves a media file by its unique identifier.

- **Parameters**:
  - `id`: The unique identifier of the media file to retrieve.
  - `cancellationToken`: Optional token to cancel the operation. Defaults to `CancellationToken.None`.
- **Returns**: A task that represents the asynchronous operation. The task result contains the media file if found, otherwise `null`.
- **Exceptions**: May throw `RepositoryException` if an error occurs during the repository operation.
- **Async**: Yes, returns a `Task<MediaFile?>`.

### GetByFilePathAsync

```csharp
Task<MediaFile?> GetByFilePathAsync(string filePath, CancellationToken cancellationToken = default)
```

Retrieves a media file by its file path.

- **Parameters**:
  - `filePath`: The file path of the media file to retrieve.
  - `cancellationToken`: Optional token to cancel the operation. Defaults to `CancellationToken.None`.
- **Returns**: A task that represents the asynchronous operation. The task result contains the media file if found, otherwise `null`.
- **Exceptions**: May throw `RepositoryException` if an error occurs during the repository operation.
- **Async**: Yes, returns a `Task<MediaFile?>`.

### GetAllAsync

```csharp
Task<IEnumerable<MediaFile>> GetAllAsync(CancellationToken cancellationToken = default)
```

Retrieves all media files from the repository.

- **Parameters**:
  - `cancellationToken`: Optional token to cancel the operation. Defaults to `CancellationToken.None`.
- **Returns**: A task that represents the asynchronous operation. The task result contains an enumerable collection of all media files.
- **Exceptions**: May throw `RepositoryException` if an error occurs during the repository operation.
- **Async**: Yes, returns a `Task<IEnumerable<MediaFile>>`.

### AddAsync

```csharp
Task<MediaFile> AddAsync(MediaFile mediaFile, CancellationToken cancellationToken = default)
```

Adds a new media file to the repository.

- **Parameters**:
  - `mediaFile`: The media file to add.
  - `cancellationToken`: Optional token to cancel the operation. Defaults to `CancellationToken.None`.
- **Returns**: A task that represents the asynchronous operation. The task result contains the added media file (may include generated properties like ID).
- **Exceptions**: May throw `RepositoryException` if an error occurs during the repository operation (e.g., duplicate ID, validation failure).
- **Async**: Yes, returns a `Task<MediaFile>`.

### UpdateAsync

```csharp
Task<MediaFile> UpdateAsync(MediaFile mediaFile, CancellationToken cancellationToken = default)
```

Updates an existing media file in the repository.

- **Parameters**:
  - `mediaFile`: The media file with updated information. Must include a valid ID.
  - `cancellationToken`: Optional token to cancel the operation. Defaults to `CancellationToken.None`.
- **Returns**: A task that represents the asynchronous operation. The task result contains the updated media file.
- **Exceptions**: May throw `RepositoryException` if an error occurs during the repository operation (e.g., media file not found, validation failure).
- **Async**: Yes, returns a `Task<MediaFile>`.

### DeleteAsync

```csharp
Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
```

Deletes a media file by its identifier.

- **Parameters**:
  - `id`: The unique identifier of the media file to delete.
  - `cancellationToken`: Optional token to cancel the operation. Defaults to `CancellationToken.None`.
- **Returns**: A task that represents the asynchronous operation. The task result is `true` if the media file was deleted, `false` if not found.
- **Exceptions**: May throw `RepositoryException` if an error occurs during the repository operation.
- **Async**: Yes, returns a `Task<bool>`.

### SearchByNameAsync

```csharp
Task<IEnumerable<MediaFile>> SearchByNameAsync(string name, CancellationToken cancellationToken = default)
```

Searches for media files by name (partial match).

- **Parameters**:
  - `name`: The name to search for.
  - `cancellationToken`: Optional token to cancel the operation. Defaults to `CancellationToken.None`.
- **Returns**: A task that represents the asynchronous operation. The task result contains an enumerable collection of media files matching the name.
- **Exceptions**: May throw `RepositoryException` if an error occurs during the repository operation.
- **Async**: Yes, returns a `Task<IEnumerable<MediaFile>>`.

### GetByFormatAsync

```csharp
Task<IEnumerable<MediaFile>> GetByFormatAsync(ContainerFormat format, CancellationToken cancellationToken = default)
```

Retrieves media files by container format.

- **Parameters**:
  - `format`: The container format to filter by.
  - `cancellationToken`: Optional token to cancel the operation. Defaults to `CancellationToken.None`.
- **Returns**: A task that represents the asynchronous operation. The task result contains an enumerable collection of media files with the specified container format.
- **Exceptions**: May throw `RepositoryException` if an error occurs during the repository operation.
- **Async**: Yes, returns a `Task<IEnumerable<MediaFile>>`.

### GetVideoFilesAsync

```csharp
Task<IEnumerable<MediaFile>> GetVideoFilesAsync(CancellationToken cancellationToken = default)
```

Retrieves all video files.

- **Parameters**:
  - `cancellationToken`: Optional token to cancel the operation. Defaults to `CancellationToken.None`.
- **Returns**: A task that represents the asynchronous operation. The task result contains an enumerable collection of video media files.
- **Exceptions**: May throw `RepositoryException` if an error occurs during the repository operation.
- **Async**: Yes, returns a `Task<IEnumerable<MediaFile>>`.

### GetAudioFilesAsync

```csharp
Task<IEnumerable<MediaFile>> GetAudioFilesAsync(CancellationToken cancellationToken = default)
```

Retrieves all audio files.

- **Parameters**:
  - `cancellationToken`: Optional token to cancel the operation. Defaults to `CancellationToken.None`.
- **Returns**: A task that represents the asynchronous operation. The task result contains an enumerable collection of audio media files.
- **Exceptions**: May throw `RepositoryException` if an error occurs during the repository operation.
- **Async**: Yes, returns a `Task<IEnumerable<MediaFile>>`.

### ExistsAsync

```csharp
Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
```

Checks if a media file exists by its identifier.

- **Parameters**:
  - `id`: The unique identifier to check.
  - `cancellationToken`: Optional token to cancel the operation. Defaults to `CancellationToken.None`.
- **Returns**: A task that represents the asynchronous operation. The task result is `true` if the media file exists, otherwise `false`.
- **Exceptions**: May throw `RepositoryException` if an error occurs during the repository operation.
- **Async**: Yes, returns a `Task<bool>`.

### GetCountAsync

```csharp
Task<int> GetCountAsync(CancellationToken cancellationToken = default)
```

Gets the total count of media files in the repository.

- **Parameters**:
  - `cancellationToken`: Optional token to cancel the operation. Defaults to `CancellationToken.None`.
- **Returns**: A task that represents the asynchronous operation. The task result contains the total number of media files.
- **Exceptions**: May throw `RepositoryException` if an error occurs during the repository operation.
- **Async**: Yes, returns a `Task<int>`.

## Async and Cancellation Semantics

All methods in this interface are asynchronous and return a `Task` or `Task<T>`. Each method accepts an optional `CancellationToken` parameter with a default value of `CancellationToken.None`, allowing callers to cancel long-running operations.

## Expected Exceptions

Implementations of this interface may throw `RepositoryException` (or a derived type) to indicate repository-specific errors such as:
- Database connection failures
- Constraint violations (e.g., duplicate keys)
- Validation errors
- Other unexpected repository errors

Consult the specific implementation documentation for details on when these exceptions are thrown.

## Custom Implementation Example

Below is an example of a simple in-memory implementation of `IMediaRepository` for demonstration or testing purposes:

```csharp
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FFmpegDotnetWrapper.Models;
using FFmpegDotnetWrapper.Repository;

public class InMemoryMediaRepository : IMediaRepository
{
    // Thread-safe dictionary for storage
    private readonly ConcurrentDictionary<string, MediaFile> _mediaFiles = new();

    public Task<MediaFile?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _mediaFiles.TryGetValue(id, out var mediaFile);
        return Task.FromResult<MediaFile?>(mediaFile);
    }

    public Task<MediaFile?> GetByFilePathAsync(string filePath, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var mediaFile = _mediaFiles.Values.FirstOrDefault(m => m.FilePath == filePath);
        return Task.FromResult<MediaFile?>(mediaFile);
    }

    public Task<IEnumerable<MediaFile>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IEnumerable<MediaFile>>(_mediaFiles.Values);
    }

    public Task<MediaFile> AddAsync(MediaFile mediaFile, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (mediaFile == null) throw new ArgumentNullException(nameof(mediaFile));
        if (string.IsNullOrEmpty(mediaFile.Id))
            throw new ArgumentException("MediaFile must have an Id set", nameof(mediaFile));

        // Overwrite if exists (or implement your own logic)
        _mediaFiles[mediaFile.Id] = mediaFile;
        return Task.FromResult(mediaFile);
    }

    public Task<MediaFile> UpdateAsync(MediaFile mediaFile, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (mediaFile == null) throw new ArgumentNullException(nameof(mediaFile));
        if (string.IsNullOrEmpty(mediaFile.Id))
            throw new ArgumentException("MediaFile must have an Id set", nameof(mediaFile));

        if (!_mediaFiles.ContainsKey(mediaFile.Id))
            throw new RepositoryException($"MediaFile with Id '{mediaFile.Id}' not found.");

        _mediaFiles[mediaFile.Id] = mediaFile;
        return Task.FromResult(mediaFile);
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrEmpty(id)) throw new ArgumentException("Id cannot be null or empty", nameof(id));

        var removed = _mediaFiles.TryRemove(id, out _);
        return Task.FromResult(removed);
    }

    public Task<IEnumerable<MediaFile>> SearchByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrEmpty(name))
            return Task.FromResult<IEnumerable<MediaFile>>(Enumerable.Empty<MediaFile>());

        var result = _mediaFiles.Values.Where(m => m.FileName.Contains(name, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult<IEnumerable<MediaFile>>(result);
    }

    public Task<IEnumerable<MediaFile>> GetByFormatAsync(ContainerFormat format, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = _mediaFiles.Values.Where(m => m.ContainerFormat == format);
        return Task.FromResult<IEnumerable<MediaFile>>(result);
    }

    public Task<IEnumerable<MediaFile>> GetVideoFilesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = _mediaFiles.Values.Where(m => m.IsVideo);
        return Task.FromResult<IEnumerable<MediaFile>>(result);
    }

    public Task<IEnumerable<MediaFile>> GetAudioFilesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = _mediaFiles.Values.Where(m => m.IsAudio);
        return Task.FromResult<IEnumerable<MediaFile>>(result);
    }

    public Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrEmpty(id)) throw new ArgumentException("Id cannot be null or empty", nameof(id));
        return Task.FromResult(_mediaFiles.ContainsKey(id));
    }

    public Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_mediaFiles.Count);
    }
}
```

**Note**: This example is for illustrative purposes only. A production implementation would typically use a persistent storage mechanism (e.g., Entity Framework Core, Dapper with SQL Server, etc.) and handle exceptions appropriately.