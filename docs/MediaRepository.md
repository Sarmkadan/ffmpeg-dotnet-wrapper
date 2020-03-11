# MediaRepository

A repository implementation that provides asynchronous access to media file metadata stored in a backing store. It exposes CRUD operations and specialized queries for media files, returning strongly-typed `MediaFile` objects or collections thereof.

## API

### `Task<MediaFile?> GetByIdAsync(int id)`

Retrieves a single media file by its unique identifier.
- **Parameters**: `id` – The integer identifier of the media file.
- **Returns**: A `Task` resolving to the matching `MediaFile` if found; otherwise `null`.
- **Exceptions**: Throws `ArgumentOutOfRangeException` if `id` is negative.

---

### `Task<MediaFile?> GetByFilePathAsync(string filePath)`

Retrieves a single media file by its absolute file system path.
- **Parameters**: `filePath` – The absolute path to the media file.
- **Returns**: A `Task` resolving to the matching `MediaFile` if found; otherwise `null`.
- **Exceptions**: Throws `ArgumentException` if `filePath` is `null`, empty, or not an absolute path.

---

### `Task<IEnumerable<MediaFile>> GetAllAsync()`

Retrieves all media files stored in the repository.
- **Returns**: A `Task` resolving to an `IEnumerable<MediaFile>` containing every media file.
- **Exceptions**: None.

---

### `Task<MediaFile> AddAsync(MediaFile mediaFile)`

Adds a new media file entry to the repository.
- **Parameters**: `mediaFile` – The `MediaFile` instance to add.
- **Returns**: A `Task` resolving to the added `MediaFile` (including any auto-generated fields).
- **Exceptions**: Throws `ArgumentNullException` if `mediaFile` is `null`; throws `InvalidOperationException` if a file with the same path already exists.

---

### `Task<MediaFile> UpdateAsync(MediaFile mediaFile)`

Updates an existing media file entry in the repository.
- **Parameters**: `mediaFile` – The `MediaFile` instance containing updated data.
- **Returns**: A `Task` resolving to the updated `MediaFile`.
- **Exceptions**: Throws `ArgumentNullException` if `mediaFile` is `null`; throws `KeyNotFoundException` if no entry with the same identifier exists.

---
### `Task<bool> DeleteAsync(int id)`

Removes a media file entry from the repository by its identifier.
- **Parameters**: `id` – The integer identifier of the media file to remove.
- **Returns**: A `Task` resolving to `true` if the entry was found and removed; otherwise `false`.
- **Exceptions**: Throws `ArgumentOutOfRangeException` if `id` is negative.

---
### `Task<IEnumerable<MediaFile>> SearchByNameAsync(string name, StringComparison comparison = StringComparison.OrdinalIgnoreCase)`

Searches media files whose names contain the specified substring.
- **Parameters**:
  - `name` – The substring to search for.
  - `comparison` – The string comparison method to use (default: `StringComparison.OrdinalIgnoreCase`).
- **Returns**: A `Task` resolving to an `IEnumerable<MediaFile>` of matching files.
- **Exceptions**: Throws `ArgumentException` if `name` is `null` or empty.

---
### `Task<IEnumerable<MediaFile>> GetByFormatAsync(string format)`

Retrieves media files matching the specified media format (e.g., "mp4", "mp3").
- **Parameters**: `format` – The media format to filter by (case-insensitive).
- **Returns**: A `Task` resolving to an `IEnumerable<MediaFile>` of matching files.
- **Exceptions**: Throws `ArgumentException` if `format` is `null` or empty.

---
### `Task<IEnumerable<MediaFile>> GetVideoFilesAsync()`

Retrieves all media files whose format is classified as video.
- **Returns**: A `Task` resolving to an `IEnumerable<MediaFile>` of video files.
- **Exceptions**: None.

---
### `Task<IEnumerable<MediaFile>> GetAudioFilesAsync()`

Retrieves all media files whose format is classified as audio.
- **Returns**: A `Task` resolving to an `IEnumerable<MediaFile>` of audio files.
- **Exceptions**: None.

---
### `Task<bool> ExistsAsync(int id)`

Checks whether a media file with the specified identifier exists.
- **Parameters**: `id` – The integer identifier to check.
- **Returns**: A `Task` resolving to `true` if the identifier exists; otherwise `false`.
- **Exceptions**: Throws `ArgumentOutOfRangeException` if `id` is negative.

---
### `Task<int> GetCountAsync()`

Returns the total number of media files stored in the repository.
- **Returns**: A `Task` resolving to the count of media files.
- **Exceptions**: None.

## Usage
