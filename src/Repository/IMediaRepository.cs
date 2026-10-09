// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Constants;
using FFmpegDotnetWrapper.Exceptions;
using FFmpegDotnetWrapper.Models;

namespace FFmpegDotnetWrapper.Repository;

/// <summary>
/// Interface for media file repository operations.
/// </summary>
public interface IMediaRepository
{
    /// <summary>
    /// Gets a media file by ID.
    /// </summary>
    /// <param name="id">The ID of the media file to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The media file with the specified ID, or <c>null</c> if no such file exists.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is null, empty, or whitespace.</exception>
    Task<MediaFile?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a media file by file path.
    /// </summary>
    /// <param name="filePath">The file path to search for. It is normalized to a full path before comparison.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The media file stored with the specified path, or <c>null</c> if no such file exists.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is null, empty, or whitespace, or is not a valid path.</exception>
    Task<MediaFile?> GetByFilePathAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all media files.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>All stored media files, or an empty sequence if the repository is empty.</returns>
    Task<IEnumerable<MediaFile>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new media file.
    /// </summary>
    /// <param name="mediaFile">The media file to add. Its <see cref="MediaFile.Id"/> must be unique within the repository.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The media file that was added.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="mediaFile"/> is null.</exception>
    /// <exception cref="RepositoryException">Thrown when a media file with the same ID already exists.</exception>
    Task<MediaFile> AddAsync(MediaFile mediaFile, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing media file.
    /// </summary>
    /// <param name="mediaFile">The media file with updated values. Its <see cref="MediaFile.Id"/> identifies the stored file to replace.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The updated media file, with <see cref="MediaFile.ModifiedAt"/> set to the current UTC time.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="mediaFile"/> is null.</exception>
    /// <exception cref="RepositoryException">Thrown when no media file with the same ID exists.</exception>
    Task<MediaFile> UpdateAsync(MediaFile mediaFile, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a media file by ID.
    /// </summary>
    /// <param name="id">The ID of the media file to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><c>true</c> if the media file was found and removed; <c>false</c> if no file with the specified ID exists.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is null, empty, or whitespace.</exception>
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches media files by name.
    /// </summary>
    /// <param name="name">The text to search for. Matching is a case-insensitive substring match against <see cref="MediaFile.Name"/>.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The media files whose names contain the search text, or an empty sequence if none match.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null, empty, or whitespace.</exception>
    Task<IEnumerable<MediaFile>> SearchByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets media files by container format.
    /// </summary>
    /// <param name="format">The container format to filter by. Files are matched on the file extension associated with this format.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The media files whose extension matches the format, or an empty sequence if none match.</returns>
    Task<IEnumerable<MediaFile>> GetByFormatAsync(ContainerFormat format, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets video files only.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The stored media files that are classified as video, or an empty sequence if there are none.</returns>
    Task<IEnumerable<MediaFile>> GetVideoFilesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audio files only.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The stored media files that are classified as audio, or an empty sequence if there are none.</returns>
    Task<IEnumerable<MediaFile>> GetAudioFilesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a media file exists.
    /// </summary>
    /// <param name="id">The ID of the media file to check.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><c>true</c> if a media file with the specified ID is stored; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is null, empty, or whitespace.</exception>
    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of media files.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of media files in the repository.</returns>
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
}
