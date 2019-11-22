// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Constants;
using FFmpegDotnetWrapper.Exceptions;
using FFmpegDotnetWrapper.Models;

namespace FFmpegDotnetWrapper.Repository;

/// <summary>
/// In-memory implementation of the media repository.
/// </summary>
public class MediaRepository : IMediaRepository
{
    private readonly Dictionary<string, MediaFile> _mediaFiles = new();
    private readonly object _lockObject = new();

    public Task<MediaFile?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            _mediaFiles.TryGetValue(id, out var mediaFile);
            return Task.FromResult(mediaFile);
        }
    }

    public Task<MediaFile?> GetByFilePathAsync(string filePath, CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            var normalizedPath = Path.GetFullPath(filePath);
            var mediaFile = _mediaFiles.Values.FirstOrDefault(m => m.FilePath == normalizedPath);
            return Task.FromResult(mediaFile);
        }
    }

    public Task<IEnumerable<MediaFile>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            return Task.FromResult(_mediaFiles.Values.AsEnumerable());
        }
    }

    public Task<MediaFile> AddAsync(MediaFile mediaFile, CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            if (mediaFile == null)
                throw new ArgumentNullException(nameof(mediaFile));

            if (_mediaFiles.ContainsKey(mediaFile.Id))
                throw new RepositoryException($"Media file with ID {mediaFile.Id} already exists", "MediaRepository");

            _mediaFiles[mediaFile.Id] = mediaFile;
            return Task.FromResult(mediaFile);
        }
    }

    public Task<MediaFile> UpdateAsync(MediaFile mediaFile, CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            if (mediaFile == null)
                throw new ArgumentNullException(nameof(mediaFile));

            if (!_mediaFiles.ContainsKey(mediaFile.Id))
                throw new RepositoryException($"Media file with ID {mediaFile.Id} not found", "MediaRepository");

            mediaFile.ModifiedAt = DateTime.UtcNow;
            _mediaFiles[mediaFile.Id] = mediaFile;
            return Task.FromResult(mediaFile);
        }
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            return Task.FromResult(_mediaFiles.Remove(id));
        }
    }

    public Task<IEnumerable<MediaFile>> SearchByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            var results = _mediaFiles.Values
                .Where(m => m.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
                .AsEnumerable();

            return Task.FromResult(results);
        }
    }

    public Task<IEnumerable<MediaFile>> GetByFormatAsync(ContainerFormat format, CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            var results = _mediaFiles.Values
                .Where(m => m.Extension == GetExtensionForFormat(format))
                .AsEnumerable();

            return Task.FromResult(results);
        }
    }

    public Task<IEnumerable<MediaFile>> GetVideoFilesAsync(CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            var results = _mediaFiles.Values
                .Where(m => m.IsVideo())
                .AsEnumerable();

            return Task.FromResult(results);
        }
    }

    public Task<IEnumerable<MediaFile>> GetAudioFilesAsync(CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            var results = _mediaFiles.Values
                .Where(m => m.IsAudio())
                .AsEnumerable();

            return Task.FromResult(results);
        }
    }

    public Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            return Task.FromResult(_mediaFiles.ContainsKey(id));
        }
    }

    public Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            return Task.FromResult(_mediaFiles.Count);
        }
    }

    /// <summary>
    /// Gets the file extension for a container format.
    /// </summary>
    private static string GetExtensionForFormat(ContainerFormat format)
    {
        return format switch
        {
            ContainerFormat.MP4 => ".mp4",
            ContainerFormat.Matroska => ".mkv",
            ContainerFormat.AVI => ".avi",
            ContainerFormat.QuickTime => ".mov",
            ContainerFormat.WebM => ".webm",
            ContainerFormat.FLV => ".flv",
            ContainerFormat.WAV => ".wav",
            ContainerFormat.MP3 => ".mp3",
            ContainerFormat.AAC => ".aac",
            ContainerFormat.FLAC => ".flac",
            _ => ""
        };
    }
}
