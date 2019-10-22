// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Exceptions;

namespace FFmpegDotnetWrapper.Models;

/// <summary>
/// Represents a media file with metadata and analysis information.
/// </summary>
public class MediaFile
{
    private string _filePath = string.Empty;
    private long _fileSize;

    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; private set; } = string.Empty;

    public string FilePath
    {
        get => _filePath;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidMediaFileException("File path cannot be null or empty");

            if (!File.Exists(value))
                throw new InvalidMediaFileException($"File does not exist: {value}", value);

            _filePath = Path.GetFullPath(value);
            Name = Path.GetFileNameWithoutExtension(value);
            _fileSize = new FileInfo(value).Length;
        }
    }

    public long FileSize
    {
        get => _fileSize;
        private set => _fileSize = value;
    }

    public string Extension => Path.GetExtension(FilePath);

    public TimeSpan? Duration { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public double? FrameRate { get; set; }
    public long? Bitrate { get; set; }
    public string? VideoCodec { get; set; }
    public string? AudioCodec { get; set; }
    public int? AudioSampleRate { get; set; }
    public int? AudioChannels { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; set; }

    public string? Description { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();

    public MediaFile()
    {
    }

    public MediaFile(string filePath)
    {
        FilePath = filePath;
    }

    /// <summary>
    /// Validates that the media file has required video properties.
    /// </summary>
    public void ValidateAsVideo()
    {
        if (!Width.HasValue || !Height.HasValue)
            throw new InvalidMediaFileException($"Invalid video file: missing dimensions. File: {FilePath}");

        if (!Duration.HasValue || Duration.Value.TotalSeconds <= 0)
            throw new InvalidMediaFileException($"Invalid video file: invalid duration. File: {FilePath}");
    }

    /// <summary>
    /// Validates that the media file has required audio properties.
    /// </summary>
    public void ValidateAsAudio()
    {
        if (!AudioSampleRate.HasValue || AudioSampleRate <= 0)
            throw new InvalidMediaFileException($"Invalid audio file: missing sample rate. File: {FilePath}");

        if (!Duration.HasValue || Duration.Value.TotalSeconds <= 0)
            throw new InvalidMediaFileException($"Invalid audio file: invalid duration. File: {FilePath}");
    }

    /// <summary>
    /// Gets the file size in megabytes.
    /// </summary>
    public double GetFileSizeInMegabytes() => Math.Round(FileSize / 1024d / 1024d, 2);

    /// <summary>
    /// Gets the file size in gigabytes.
    /// </summary>
    public double GetFileSizeInGigabytes() => Math.Round(FileSize / 1024d / 1024d / 1024d, 2);

    /// <summary>
    /// Checks if the media file is a video file.
    /// </summary>
    public bool IsVideo() => Width.HasValue && Height.HasValue;

    /// <summary>
    /// Checks if the media file is an audio file.
    /// </summary>
    public bool IsAudio() => AudioSampleRate.HasValue && !IsVideo();

    /// <summary>
    /// Calculates the approximate bitrate from file size and duration.
    /// </summary>
    public long? CalculateApproximateBitrate()
    {
        if (!Duration.HasValue || Duration.Value.TotalSeconds <= 0)
            return null;

        return (long)(FileSize * 8 / Duration.Value.TotalSeconds / 1000); // in kbps
    }

    /// <summary>
    /// Clones the media file object.
    /// </summary>
    public MediaFile Clone()
    {
        return new MediaFile
        {
            Id = Id,
            _filePath = _filePath,
            Name = Name,
            _fileSize = _fileSize,
            Duration = Duration,
            Width = Width,
            Height = Height,
            FrameRate = FrameRate,
            Bitrate = Bitrate,
            VideoCodec = VideoCodec,
            AudioCodec = AudioCodec,
            AudioSampleRate = AudioSampleRate,
            AudioChannels = AudioChannels,
            CreatedAt = CreatedAt,
            ModifiedAt = ModifiedAt,
            Description = Description,
            Metadata = new Dictionary<string, string>(Metadata)
        };
    }
}
