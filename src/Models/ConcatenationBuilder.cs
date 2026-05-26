// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Exceptions;

namespace FFmpegDotnetWrapper.Models;

/// <summary>
/// Describes the transition effect applied between two consecutive segments in a concatenation.
/// </summary>
public enum ConcatTransition
{
    /// <summary>No transition — segments are cut directly one after the other.</summary>
    None,
    /// <summary>Crossfade audio and video between segments.</summary>
    Crossfade
}

/// <summary>
/// Represents a single video segment to include in a concatenation.
/// An optional trim range can be applied before the segment is joined.
/// </summary>
public class ConcatenationSegment
{
    /// <summary>Absolute path to the video or audio file.</summary>
    public string FilePath { get; }

    /// <summary>
    /// Optional trim start within the segment. When set, the segment begins playback at this offset.
    /// </summary>
    public TimeSpan? TrimStart { get; init; }

    /// <summary>
    /// Optional trim end within the segment. When set, the segment ends at this position.
    /// Either set <see cref="TrimEnd"/> or <see cref="TrimDuration"/>, not both.
    /// </summary>
    public TimeSpan? TrimEnd { get; init; }

    /// <summary>
    /// Optional duration to keep from <see cref="TrimStart"/>. Mutually exclusive with <see cref="TrimEnd"/>.
    /// </summary>
    public TimeSpan? TrimDuration { get; init; }

    /// <summary>Label displayed in log messages for this segment.</summary>
    public string Label { get; init; }

    public ConcatenationSegment(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new InvalidOperationConfigurationException("Segment file path cannot be null or empty");

        if (!File.Exists(filePath))
            throw new InvalidOperationConfigurationException($"Segment file does not exist: {filePath}");

        FilePath = Path.GetFullPath(filePath);
        Label = Path.GetFileNameWithoutExtension(filePath);
    }

    /// <summary>Checks whether any trim parameters have been configured for this segment.</summary>
    public bool HasTrim => TrimStart.HasValue || TrimEnd.HasValue || TrimDuration.HasValue;
}

/// <summary>
/// Fluent builder for composing a video concatenation pipeline from multiple segments.
/// Construct a sequence of <see cref="ConcatenationSegment"/> objects, configure global
/// options, and call <see cref="Build"/> to obtain a <see cref="MergeSettings"/> object
/// ready for <c>IFFmpegService.MergeAsync</c>.
/// </summary>
/// <example>
/// <code>
/// var settings = new ConcatenationBuilder()
///     .Add("intro.mp4")
///     .Add("main.mp4", trimStart: TimeSpan.FromSeconds(5), trimDuration: TimeSpan.FromMinutes(2))
///     .Add("outro.mp4")
///     .WithTransition(ConcatTransition.Crossfade, duration: 0.5)
///     .WithReencode(true)
///     .Build();
///
/// await ffmpegService.MergeAsync(settings.InputFiles, "output.mp4", settings);
/// </code>
/// </example>
public class ConcatenationBuilder
{
    private readonly List<ConcatenationSegment> _segments = [];
    private ConcatTransition _transition = ConcatTransition.None;
    private double _transitionDuration = 1.0;
    private bool _reencode = false;
    private TranscodeSettings? _transcodeSettings;

    /// <summary>
    /// Adds a video segment at the end of the concatenation sequence.
    /// </summary>
    /// <param name="filePath">Path to the video file.</param>
    /// <returns>This builder instance for chaining.</returns>
    public ConcatenationBuilder Add(string filePath)
    {
        _segments.Add(new ConcatenationSegment(filePath));
        return this;
    }

    /// <summary>
    /// Adds a trimmed video segment at the end of the concatenation sequence.
    /// </summary>
    /// <param name="filePath">Path to the video file.</param>
    /// <param name="trimStart">Position within the file to begin reading. Defaults to the beginning.</param>
    /// <param name="trimEnd">Position within the file to stop reading. Mutually exclusive with <paramref name="trimDuration"/>.</param>
    /// <param name="trimDuration">How long to read from <paramref name="trimStart"/>. Mutually exclusive with <paramref name="trimEnd"/>.</param>
    /// <returns>This builder instance for chaining.</returns>
    public ConcatenationBuilder Add(
        string filePath,
        TimeSpan? trimStart = null,
        TimeSpan? trimEnd = null,
        TimeSpan? trimDuration = null)
    {
        if (trimEnd.HasValue && trimDuration.HasValue)
            throw new InvalidOperationConfigurationException(
                "Specify either TrimEnd or TrimDuration for a segment, not both");

        _segments.Add(new ConcatenationSegment(filePath)
        {
            TrimStart = trimStart,
            TrimEnd = trimEnd,
            TrimDuration = trimDuration
        });

        return this;
    }

    /// <summary>
    /// Inserts a segment at a specific position in the sequence.
    /// </summary>
    /// <param name="index">Zero-based index at which to insert.</param>
    /// <param name="filePath">Path to the video file.</param>
    /// <returns>This builder instance for chaining.</returns>
    public ConcatenationBuilder Insert(int index, string filePath)
    {
        if (index < 0 || index > _segments.Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _segments.Insert(index, new ConcatenationSegment(filePath));
        return this;
    }

    /// <summary>
    /// Removes all segments whose file path equals <paramref name="filePath"/>.
    /// </summary>
    /// <returns>This builder instance for chaining.</returns>
    public ConcatenationBuilder Remove(string filePath)
    {
        _segments.RemoveAll(s => string.Equals(s.FilePath, Path.GetFullPath(filePath), StringComparison.OrdinalIgnoreCase));
        return this;
    }

    /// <summary>
    /// Sets the transition applied between every pair of consecutive segments.
    /// </summary>
    /// <param name="transition">Transition type.</param>
    /// <param name="duration">Transition duration in seconds. Must be greater than zero.</param>
    /// <returns>This builder instance for chaining.</returns>
    public ConcatenationBuilder WithTransition(ConcatTransition transition, double duration = 1.0)
    {
        if (duration <= 0)
            throw new InvalidOperationConfigurationException("Transition duration must be greater than zero");

        _transition = transition;
        _transitionDuration = duration;
        return this;
    }

    /// <summary>
    /// Controls whether all segments are re-encoded before concatenation.
    /// Re-encoding is required when segments differ in codec, resolution, or frame rate.
    /// When <c>false</c> (default), stream-copy is used (faster but requires compatible streams).
    /// </summary>
    /// <returns>This builder instance for chaining.</returns>
    public ConcatenationBuilder WithReencode(bool reencode = true)
    {
        _reencode = reencode;
        return this;
    }

    /// <summary>
    /// Supplies custom transcode settings applied during re-encoding.
    /// Implies <see cref="WithReencode(bool)"/> = <c>true</c>.
    /// </summary>
    /// <returns>This builder instance for chaining.</returns>
    public ConcatenationBuilder WithTranscodeSettings(TranscodeSettings settings)
    {
        _transcodeSettings = settings ?? throw new ArgumentNullException(nameof(settings));
        _reencode = true;
        return this;
    }

    /// <summary>
    /// Returns the number of segments currently registered with this builder.
    /// </summary>
    public int SegmentCount => _segments.Count;

    /// <summary>
    /// Returns a read-only view of the current segment list.
    /// </summary>
    public IReadOnlyList<ConcatenationSegment> Segments => _segments.AsReadOnly();

    /// <summary>
    /// Builds a <see cref="MergeSettings"/> object from the accumulated configuration.
    /// Requires at least two segments.
    /// </summary>
    /// <returns>A fully configured <see cref="MergeSettings"/> ready for <c>IFFmpegService.MergeAsync</c>.</returns>
    /// <exception cref="InvalidOperationConfigurationException">
    /// Thrown when fewer than two segments are registered.
    /// </exception>
    public MergeSettings Build()
    {
        if (_segments.Count < 2)
            throw new InvalidOperationConfigurationException(
                "At least two segments are required to build a concatenation");

        var settings = new MergeSettings
        {
            InputFiles = _segments.Select(s => s.FilePath).ToList(),
            TranscodeOnMerge = _reencode,
            TranscodeSettings = _transcodeSettings,
            Crossfade = _transition == ConcatTransition.Crossfade,
            CrossfadeDuration = _transitionDuration,
            PreserveAudio = true,
            PreserveVideo = true
        };

        return settings;
    }

    /// <summary>
    /// Clears all segments and resets options, allowing the builder to be reused.
    /// </summary>
    /// <returns>This builder instance for chaining.</returns>
    public ConcatenationBuilder Reset()
    {
        _segments.Clear();
        _transition = ConcatTransition.None;
        _transitionDuration = 1.0;
        _reencode = false;
        _transcodeSettings = null;
        return this;
    }
}
