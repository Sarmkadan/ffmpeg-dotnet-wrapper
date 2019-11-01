// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Constants;
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
    Task<MediaFile?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a media file by file path.
    /// </summary>
    Task<MediaFile?> GetByFilePathAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all media files.
    /// </summary>
    Task<IEnumerable<MediaFile>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new media file.
    /// </summary>
    Task<MediaFile> AddAsync(MediaFile mediaFile, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing media file.
    /// </summary>
    Task<MediaFile> UpdateAsync(MediaFile mediaFile, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a media file by ID.
    /// </summary>
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches media files by name.
    /// </summary>
    Task<IEnumerable<MediaFile>> SearchByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets media files by container format.
    /// </summary>
    Task<IEnumerable<MediaFile>> GetByFormatAsync(ContainerFormat format, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets video files only.
    /// </summary>
    Task<IEnumerable<MediaFile>> GetVideoFilesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audio files only.
    /// </summary>
    Task<IEnumerable<MediaFile>> GetAudioFilesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a media file exists.
    /// </summary>
    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of media files.
    /// </summary>
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
}
