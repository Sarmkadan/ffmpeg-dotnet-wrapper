# Repository Interfaces

This document outlines the Repository Interfaces in the **FFmpegDotnetWrapper** project: `IMediaRepository` and `IOperationRepository`. These interfaces define the contracts for managing media metadata and tracking FFmpeg operations.

For information on the concrete implementations, see:
- [MediaRepository](MediaRepository.md)
- [OperationRepository](OperationRepository.md)

---

## 1. IMediaRepository

The `IMediaRepository` interface defines the contract for asynchronously managing and querying media file metadata records (represented by `MediaFile` instances) within a storage system.

### Purpose
- Exposes standard CRUD operations for `MediaFile` objects.
- Provides search and filtering capabilities (e.g., searching by name, filtering by format, retrieving video or audio files exclusively).
- Supports checking for existence and counting records.

### Method Table

| Method | Return Type | Description |
| :--- | :--- | :--- |
| `GetByIdAsync(string id, CancellationToken cancellationToken)` | `Task<MediaFile?>` | Gets a media file by its unique identifier. |
| `GetByFilePathAsync(string filePath, CancellationToken cancellationToken)` | `Task<MediaFile?>` | Gets a media file by its file system path. |
| `GetAllAsync(CancellationToken cancellationToken)` | `Task<IEnumerable<MediaFile>>` | Gets all media files registered in the repository. |
| `AddAsync(MediaFile mediaFile, CancellationToken cancellationToken)` | `Task<MediaFile>` | Adds a new media file record to the repository. |
| `UpdateAsync(MediaFile mediaFile, CancellationToken cancellationToken)` | `Task<MediaFile>` | Updates an existing media file record in the repository. |
| `DeleteAsync(string id, CancellationToken cancellationToken)` | `Task<bool>` | Deletes a media file record by its identifier. |
| `SearchByNameAsync(string name, CancellationToken cancellationToken)` | `Task<IEnumerable<MediaFile>>` | Searches for media files matching the given name. |
| `GetByFormatAsync(ContainerFormat format, CancellationToken cancellationToken)` | `Task<IEnumerable<MediaFile>>` | Gets media files filtered by their container format. |
| `GetVideoFilesAsync(CancellationToken cancellationToken)` | `Task<IEnumerable<MediaFile>>` | Retrieves only video files. |
| `GetAudioFilesAsync(CancellationToken cancellationToken)` | `Task<IEnumerable<MediaFile>>` | Retrieves only audio files. |
| `ExistsAsync(string id, CancellationToken cancellationToken)` | `Task<bool>` | Checks whether a media file with the given identifier exists. |
| `GetCountAsync(CancellationToken cancellationToken)` | `Task<int>` | Gets the total count of media files in the repository. |

---

## 2. IOperationRepository

The `IOperationRepository` interface defines the contract for asynchronously managing, querying, and auditing records of FFmpeg operations (represented by `FFmpegOperation` instances).

### Purpose
- Exposes standard CRUD operations for tracking FFmpeg operation history.
- Supports querying operations by operational type, recency, or specific date ranges.
- Allows for cleanup/maintenance operations such as purging old history entries.

### Method Table

| Method | Return Type | Description |
| :--- | :--- | :--- |
| `GetByIdAsync(string id, CancellationToken cancellationToken)` | `Task<FFmpegOperation?>` | Gets an operation record by its unique identifier. |
| `GetAllAsync(CancellationToken cancellationToken)` | `Task<IEnumerable<FFmpegOperation>>` | Gets all operations registered in the repository. |
| `AddAsync(FFmpegOperation operation, CancellationToken cancellationToken)` | `Task<FFmpegOperation>` | Registers a new FFmpeg operation in the repository. |
| `UpdateAsync(FFmpegOperation operation, CancellationToken cancellationToken)` | `Task<FFmpegOperation>` | Updates an existing FFmpeg operation record. |
| `DeleteAsync(string id, CancellationToken cancellationToken)` | `Task<bool>` | Deletes an operation record by its identifier. |
| `GetByTypeAsync(FFmpegOperationType type, CancellationToken cancellationToken)` | `Task<IEnumerable<FFmpegOperation>>` | Retrieves operations matching a specific type. |
| `GetRecentAsync(int count, CancellationToken cancellationToken)` | `Task<IEnumerable<FFmpegOperation>>` | Retrieves the most recently executed operations, up to the specified count. |
| `GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken)` | `Task<IEnumerable<FFmpegOperation>>` | Retrieves operations executed within the specified date range. |
| `ClearOldAsync(int olderThanDays, CancellationToken cancellationToken)` | `Task<int>` | Purges operation history records older than the specified number of days and returns the count of deleted records. |
| `GetCountAsync(CancellationToken cancellationToken)` | `Task<int>` | Gets the total count of registered operations. |

---

## Usage Example

Below is a short usage example demonstrating how to register and use both `IMediaRepository` and `IOperationRepository` via Dependency Injection in a C# application.

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using FFmpegDotnetWrapper.Models;
using FFmpegDotnetWrapper.Repository;

namespace FFmpegDotnetWrapper.Examples
{
    public class MediaProcessingService
    {
        private readonly IMediaRepository _mediaRepository;
        private readonly IOperationRepository _operationRepository;

        public MediaProcessingService(
            IMediaRepository mediaRepository,
            IOperationRepository operationRepository)
        {
            _mediaRepository = mediaRepository ?? throw new ArgumentNullException(nameof(mediaRepository));
            _operationRepository = operationRepository ?? throw new ArgumentNullException(nameof(operationRepository));
        }

        public async Task ProcessAndTrackMediaAsync(string fileId, CancellationToken cancellationToken = default)
        {
            // 1. Fetch media information
            MediaFile? mediaFile = await _mediaRepository.GetByIdAsync(fileId, cancellationToken);
            if (mediaFile == null)
            {
                Console.WriteLine($"Media file with ID '{fileId}' not found.");
                return;
            }

            Console.WriteLine($"Processing video file: {mediaFile.FilePath}");

            // 2. Track the transcoding operation
            var operation = new FFmpegOperation
            {
                Id = Guid.NewGuid().ToString(),
                Type = FFmpegOperationType.Transcode,
                Status = FFmpegOperationStatus.Started,
                InputPath = mediaFile.FilePath,
                OutputPath = mediaFile.FilePath + ".mp4",
                CreatedAt = DateTime.UtcNow
            };

            await _operationRepository.AddAsync(operation, cancellationToken);

            try
            {
                // Perform transcoding logic ...
                
                // 3. Complete and update operation tracking
                operation.Status = FFmpegOperationStatus.Completed;
                operation.CompletedAt = DateTime.UtcNow;
                await _operationRepository.UpdateAsync(operation, cancellationToken);
                Console.WriteLine("Transcode operation recorded successfully.");
            }
            catch (Exception ex)
            {
                // 4. Update status in case of failure
                operation.Status = FFmpegOperationStatus.Failed;
                operation.ErrorMessage = ex.Message;
                operation.CompletedAt = DateTime.UtcNow;
                await _operationRepository.UpdateAsync(operation, cancellationToken);
                Console.WriteLine($"Transcode operation failed: {ex.Message}");
            }
        }
    }
}
```