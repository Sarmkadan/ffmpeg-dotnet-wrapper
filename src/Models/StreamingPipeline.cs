// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;

namespace FFmpegDotnetWrapper.Models;

/// <summary>
/// Supported output container formats for adaptive bitrate streaming.
/// </summary>
public enum StreamingFormat
{
    /// <summary>HTTP Live Streaming — produces .m3u8 playlists and .ts segment files.</summary>
    Hls,

    /// <summary>Dynamic Adaptive Streaming over HTTP — produces .mpd manifests and .m4s segment files.</summary>
    Dash
}

/// <summary>
/// Lifecycle state of a streaming pipeline run.
/// </summary>
public enum PipelineState
{
    /// <summary>Pipeline is being initialised; output directories and playlists are being created.</summary>
    Initializing,

    /// <summary>Pipeline is actively encoding and emitting segments.</summary>
    Running,

    /// <summary>All configured profiles have been encoded successfully.</summary>
    Completed,

    /// <summary>Pipeline terminated with an unrecoverable encoding error.</summary>
    Failed,

    /// <summary>Pipeline was stopped via <c>IAdaptiveBitrateService.CancelPipelineAsync</c>.</summary>
    Cancelled
}

/// <summary>
/// Defines a single quality rendition within an adaptive bitrate ladder.
/// Instances are immutable and safe to share across concurrent pipeline runs.
/// </summary>
/// <param name="Name">Human-readable rendition label, e.g. <c>"1080p"</c> or <c>"720p"</c>.</param>
/// <param name="Width">Output frame width in pixels.</param>
/// <param name="Height">Output frame height in pixels.</param>
/// <param name="VideoBitrateKbps">Target video bitrate in kilobits per second.</param>
/// <param name="AudioBitrateKbps">Target audio bitrate in kilobits per second.</param>
/// <param name="FrameRate">
/// Target output frame rate. Pass <c>0</c> to preserve the source frame rate.
/// </param>
public sealed record StreamingProfile(
    string Name,
    int Width,
    int Height,
    int VideoBitrateKbps,
    int AudioBitrateKbps,
    double FrameRate = 0)
{
    /// <summary>Gets the <c>WxH</c> resolution string (e.g., <c>"1920x1080"</c>).</summary>
    public string Resolution => $"{Width}x{Height}";

    /// <summary>Gets the combined video and audio bitrate in kbps.</summary>
    public int TotalBitrateKbps => VideoBitrateKbps + AudioBitrateKbps;

    /// <summary>Pre-built 1080p full-HD profile (4500 kbps video, 192 kbps audio).</summary>
    public static readonly StreamingProfile FullHD = new("1080p", 1920, 1080, 4500, 192);

    /// <summary>Pre-built 720p HD profile (2500 kbps video, 128 kbps audio).</summary>
    public static readonly StreamingProfile HD = new("720p", 1280, 720, 2500, 128);

    /// <summary>Pre-built 480p standard-definition profile (1000 kbps video, 96 kbps audio).</summary>
    public static readonly StreamingProfile SD = new("480p", 854, 480, 1000, 96);

    /// <summary>Pre-built 360p mobile-optimised profile (500 kbps video, 64 kbps audio).</summary>
    public static readonly StreamingProfile Mobile = new("360p", 640, 360, 500, 64);

    /// <summary>
    /// Returns the default four-rung ABR ladder ordered from highest to lowest quality:
    /// 1080p → 720p → 480p → 360p.
    /// </summary>
    public static IReadOnlyList<StreamingProfile> DefaultLadder =>
        [FullHD, HD, SD, Mobile];
}

/// <summary>
/// Represents one encoded media segment produced by the streaming pipeline.
/// </summary>
public sealed class StreamingSegment
{
    /// <summary>Gets the segment's unique identifier (generated per segment).</summary>
    public required string Id { get; init; }

    /// <summary>Gets the identifier of the pipeline that produced this segment.</summary>
    public required string PipelineId { get; init; }

    /// <summary>Gets the quality profile at which this segment was encoded.</summary>
    public required StreamingProfile Profile { get; init; }

    /// <summary>Gets the zero-based sequence index within this rendition's playlist.</summary>
    public required int SequenceNumber { get; init; }

    /// <summary>Gets the absolute path to the segment file on disk.</summary>
    public required string FilePath { get; init; }

    /// <summary>Gets the nominal content duration of this segment in seconds.</summary>
    public double DurationSeconds { get; init; }

    /// <summary>Gets or sets the file size in bytes (populated once the file is fully written).</summary>
    public long FileSizeBytes { get; set; }

    /// <summary>Gets the UTC timestamp at which encoding of this segment completed.</summary>
    public DateTimeOffset EncodedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the actual achieved bitrate for this segment in kbps,
    /// derived from <see cref="FileSizeBytes"/> and <see cref="DurationSeconds"/>.
    /// Returns <c>0</c> when <see cref="DurationSeconds"/> is zero.
    /// </summary>
    public double ActualBitrateKbps =>
        DurationSeconds > 0 ? (FileSizeBytes * 8d) / (DurationSeconds * 1000d) : 0;
}

/// <summary>
/// Immutable record describing a single adaptive bitrate switch event recorded
/// during a pipeline run.
/// </summary>
public sealed record BitrateSwitch
{
    /// <summary>Gets the UTC time at which the switch was triggered.</summary>
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>Gets the quality profile that was active before this switch.</summary>
    public required StreamingProfile FromProfile { get; init; }

    /// <summary>Gets the quality profile that became active after this switch.</summary>
    public required StreamingProfile ToProfile { get; init; }

    /// <summary>Gets a human-readable explanation of why the switch was triggered.</summary>
    public required string Reason { get; init; }

    /// <summary>
    /// Gets <c>true</c> when the switch moved to a higher-quality (higher bitrate) profile.
    /// </summary>
    public bool IsUpgrade => ToProfile.VideoBitrateKbps > FromProfile.VideoBitrateKbps;
}

/// <summary>
/// Encapsulates all configuration for a single streaming pipeline run.
/// Call <see cref="Validate"/> before passing this to the pipeline service.
/// </summary>
public sealed class StreamingPipelineSettings
{
    private string _inputFilePath = string.Empty;
    private string _outputDirectory = string.Empty;
    private int _segmentDurationSeconds = 6;
    private int _playlistWindowSize = 5;

    /// <summary>Gets or sets the absolute path of the source media file to encode.</summary>
    /// <exception cref="ArgumentException">Thrown when set to a null or whitespace value.</exception>
    public required string InputFilePath
    {
        get => _inputFilePath;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Input file path cannot be empty.", nameof(value));
            _inputFilePath = value;
        }
    }

    /// <summary>
    /// Gets or sets the directory where all segment files, per-rendition playlists,
    /// and the master manifest are written.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when set to a null or whitespace value.</exception>
    public required string OutputDirectory
    {
        get => _outputDirectory;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Output directory cannot be empty.", nameof(value));
            _outputDirectory = value;
        }
    }

    /// <summary>Gets or sets the output manifest format. Defaults to <see cref="StreamingFormat.Hls"/>.</summary>
    public StreamingFormat Format { get; set; } = StreamingFormat.Hls;

    /// <summary>
    /// Gets or sets the ordered list of quality profiles to encode.
    /// Profiles are automatically sorted highest-to-lowest by video bitrate before encoding.
    /// </summary>
    public IList<StreamingProfile> Profiles { get; set; } = [.. StreamingProfile.DefaultLadder];

    /// <summary>
    /// Gets or sets the target segment duration in seconds.
    /// Must be between 1 and 60; 2–10 is recommended for HLS.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when outside the valid range.</exception>
    public int SegmentDurationSeconds
    {
        get => _segmentDurationSeconds;
        set
        {
            if (value is < 1 or > 60)
                throw new ArgumentOutOfRangeException(nameof(value), "Segment duration must be between 1 and 60 seconds.");
            _segmentDurationSeconds = value;
        }
    }

    /// <summary>
    /// Gets or sets the sliding-window size for live playlists (number of segments to retain).
    /// Set to <c>0</c> for VOD mode, which keeps all segments in the playlist.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when set to a negative value.</exception>
    public int PlaylistWindowSize
    {
        get => _playlistWindowSize;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Playlist window size cannot be negative.");
            _playlistWindowSize = value;
        }
    }

    /// <summary>Gets or sets whether to attempt hardware-accelerated encoding via FFmpeg's <c>-hwaccel</c>.</summary>
    public bool EnableHardwareAcceleration { get; set; }

    /// <summary>
    /// Gets or sets whether all profiles are encoded concurrently.
    /// When <c>false</c>, profiles are encoded sequentially highest-to-lowest quality.
    /// </summary>
    public bool EncodeProfilesConcurrently { get; set; } = true;

    /// <summary>
    /// Validates that all required fields are set and internally consistent.
    /// </summary>
    /// <exception cref="FileNotFoundException">Thrown when <see cref="InputFilePath"/> does not exist on disk.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no profiles are configured.</exception>
    public void Validate()
    {
        if (!File.Exists(InputFilePath))
            throw new FileNotFoundException($"Input file not found: {InputFilePath}", InputFilePath);

        if (Profiles.Count == 0)
            throw new InvalidOperationException("At least one streaming profile must be specified.");
    }
}

/// <summary>
/// Holds the live state and accumulated output of a streaming pipeline run.
/// All collection mutations are thread-safe and may be called from concurrent tasks.
/// </summary>
public sealed class StreamingPipelineResult
{
    private readonly ConcurrentBag<StreamingSegment> _segments = [];
    private readonly ConcurrentBag<BitrateSwitch> _bitrateSwitches = [];

    /// <summary>Gets the globally unique identifier for this pipeline run.</summary>
    public required string PipelineId { get; init; }

    /// <summary>Gets or sets the current lifecycle state of the pipeline.</summary>
    public PipelineState State { get; set; } = PipelineState.Initializing;

    /// <summary>Gets the UTC timestamp at which the pipeline was started.</summary>
    public DateTimeOffset StartedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>Gets or sets the UTC timestamp at which the pipeline ended. <c>null</c> while still running.</summary>
    public DateTimeOffset? EndedAt { get; set; }

    /// <summary>
    /// Gets or sets the quality profile currently recommended for viewers based on
    /// the encoder's adaptive bitrate assessment. Updated in real time as the pipeline runs.
    /// </summary>
    public StreamingProfile? ActiveProfile { get; set; }

    /// <summary>Gets or sets the path to the master playlist file (HLS .m3u8 or DASH .mpd).</summary>
    public string? MasterPlaylistPath { get; set; }

    /// <summary>Gets or sets the error message that caused a <see cref="PipelineState.Failed"/> state.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Gets all segments produced so far. Thread-safe; order not guaranteed.</summary>
    public IReadOnlyCollection<StreamingSegment> Segments => _segments;

    /// <summary>Gets the chronological log of adaptive bitrate switch events.</summary>
    public IReadOnlyCollection<BitrateSwitch> BitrateSwitches => _bitrateSwitches;

    /// <summary>Gets the wall-clock time elapsed since the pipeline started.</summary>
    public TimeSpan Elapsed => (EndedAt ?? DateTimeOffset.UtcNow) - StartedAt;

    /// <summary>Appends a completed segment to the result collection.</summary>
    /// <param name="segment">The segment to record.</param>
    public void AddSegment(StreamingSegment segment) => _segments.Add(segment);

    /// <summary>Records that the pipeline switched to a different quality profile.</summary>
    /// <param name="bitrateSwitch">The switch event to log.</param>
    public void RecordSwitch(BitrateSwitch bitrateSwitch) => _bitrateSwitches.Add(bitrateSwitch);
}
