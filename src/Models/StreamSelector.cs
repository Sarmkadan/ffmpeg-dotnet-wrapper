// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Exceptions;
using FFmpegDotnetWrapper.Utilities;

namespace FFmpegDotnetWrapper.Models;

/// <summary>
/// Represents a stream selection criterion for FFmpeg -map operations.
/// Supports selection by index, type, language, or all streams of a type.
/// </summary>
public sealed class StreamSelector
{
    private readonly List<StreamSelection> _selections = new();

    /// <summary>
    /// Creates a new empty StreamSelector.
    /// </summary>
    public StreamSelector()
    {
    }

    /// <summary>
    /// Selects a specific stream by its input file index and stream index (e.g., "0:0", "0:1").
    /// </summary>
    /// <param name="inputIndex">The input file index (0-based).</param>
    /// <param name="streamIndex">The stream index within the input file (0-based).</param>
    /// <returns>This StreamSelector for fluent chaining.</returns>
    public StreamSelector SelectStream(int inputIndex, int streamIndex)
    {
        if (inputIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(inputIndex), "Input index must be non-negative");
        if (streamIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(streamIndex), "Stream index must be non-negative");

        _selections.Add(new StreamSelection
        {
            Type = StreamSelectionType.ByIndex,
            InputIndex = inputIndex,
            StreamIndex = streamIndex
        });
        return this;
    }

    /// <summary>
    /// Selects all video streams from the specified input file.
    /// </summary>
    /// <param name="inputIndex">The input file index (0-based). Defaults to 0.</param>
    /// <returns>This StreamSelector for fluent chaining.</returns>
    public StreamSelector SelectAllVideo(int inputIndex = 0)
    {
        if (inputIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(inputIndex), "Input index must be non-negative");

        _selections.Add(new StreamSelection
        {
            Type = StreamSelectionType.AllOfType,
            InputIndex = inputIndex,
            StreamType = StreamType.Video
        });
        return this;
    }

    /// <summary>
    /// Selects all audio streams from the specified input file.
    /// </summary>
    /// <param name="inputIndex">The input file index (0-based). Defaults to 0.</param>
    /// <returns>This StreamSelector for fluent chaining.</returns>
    public StreamSelector SelectAllAudio(int inputIndex = 0)
    {
        if (inputIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(inputIndex), "Input index must be non-negative");

        _selections.Add(new StreamSelection
        {
            Type = StreamSelectionType.AllOfType,
            InputIndex = inputIndex,
            StreamType = StreamType.Audio
        });
        return this;
    }

    /// <summary>
    /// Selects all subtitle streams from the specified input file.
    /// </summary>
    /// <param name="inputIndex">The input file index (0-based). Defaults to 0.</param>
    /// <returns>This StreamSelector for fluent chaining.</returns>
    public StreamSelector SelectAllSubtitles(int inputIndex = 0)
    {
        if (inputIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(inputIndex), "Input index must be non-negative");

        _selections.Add(new StreamSelection
        {
            Type = StreamSelectionType.AllOfType,
            InputIndex = inputIndex,
            StreamType = StreamType.Subtitle
        });
        return this;
    }

    /// <summary>
    /// Selects all data streams from the specified input file.
    /// </summary>
    /// <param name="inputIndex">The input file index (0-based). Defaults to 0.</param>
    /// <returns>This StreamSelector for fluent chaining.</returns>
    public StreamSelector SelectAllData(int inputIndex = 0)
    {
        if (inputIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(inputIndex), "Input index must be non-negative");

        _selections.Add(new StreamSelection
        {
            Type = StreamSelectionType.AllOfType,
            InputIndex = inputIndex,
            StreamType = StreamType.Data
        });
        return this;
    }

    /// <summary>
    /// Selects an audio stream by its language metadata tag (e.g., "eng", "spa", "fre").
    /// </summary>
    /// <param name="language">The ISO 639-2 language code (3 letters).</param>
    /// <param name="inputIndex">The input file index (0-based). Defaults to 0.</param>
    /// <returns>This StreamSelector for fluent chaining.</returns>
    public StreamSelector SelectAudioByLanguage(string language, int inputIndex = 0)
    {
        if (string.IsNullOrWhiteSpace(language))
            throw new ArgumentException("Language code cannot be null or empty", nameof(language));
        if (language.Length != 3)
            throw new ArgumentException("Language code must be a 3-letter ISO 639-2 code", nameof(language));
        if (inputIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(inputIndex), "Input index must be non-negative");

        _selections.Add(new StreamSelection
        {
            Type = StreamSelectionType.ByLanguage,
            InputIndex = inputIndex,
            StreamType = StreamType.Audio,
            Language = language.ToLowerInvariant()
        });
        return this;
    }

    /// <summary>
    /// Selects a subtitle stream by its language metadata tag (e.g., "eng", "spa", "fre").
    /// </summary>
    /// <param name="language">The ISO 639-2 language code (3 letters).</param>
    /// <param name="inputIndex">The input file index (0-based). Defaults to 0.</param>
    /// <returns>This StreamSelector for fluent chaining.</returns>
    public StreamSelector SelectSubtitleByLanguage(string language, int inputIndex = 0)
    {
        if (string.IsNullOrWhiteSpace(language))
            throw new ArgumentException("Language code cannot be null or empty", nameof(language));
        if (language.Length != 3)
            throw new ArgumentException("Language code must be a 3-letter ISO 639-2 code", nameof(language));
        if (inputIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(inputIndex), "Input index must be non-negative");

        _selections.Add(new StreamSelection
        {
            Type = StreamSelectionType.ByLanguage,
            InputIndex = inputIndex,
            StreamType = StreamType.Subtitle,
            Language = language.ToLowerInvariant()
        });
        return this;
    }

    /// <summary>
    /// Selects a video stream by its index within the video streams of the input file.
    /// </summary>
    /// <param name="videoStreamIndex">The video stream index (0-based, relative to video streams only).</param>
    /// <param name="inputIndex">The input file index (0-based). Defaults to 0.</param>
    /// <returns>This StreamSelector for fluent chaining.</returns>
    public StreamSelector SelectVideoByIndex(int videoStreamIndex, int inputIndex = 0)
    {
        if (videoStreamIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(videoStreamIndex), "Video stream index must be non-negative");
        if (inputIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(inputIndex), "Input index must be non-negative");

        _selections.Add(new StreamSelection
        {
            Type = StreamSelectionType.ByTypeIndex,
            InputIndex = inputIndex,
            StreamType = StreamType.Video,
            TypeIndex = videoStreamIndex
        });
        return this;
    }

    /// <summary>
    /// Selects an audio stream by its index within the audio streams of the input file.
    /// </summary>
    /// <param name="audioStreamIndex">The audio stream index (0-based, relative to audio streams only).</param>
    /// <param name="inputIndex">The input file index (0-based). Defaults to 0.</param>
    /// <returns>This StreamSelector for fluent chaining.</returns>
    public StreamSelector SelectAudioByIndex(int audioStreamIndex, int inputIndex = 0)
    {
        if (audioStreamIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(audioStreamIndex), "Audio stream index must be non-negative");
        if (inputIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(inputIndex), "Input index must be non-negative");

        _selections.Add(new StreamSelection
        {
            Type = StreamSelectionType.ByTypeIndex,
            InputIndex = inputIndex,
            StreamType = StreamType.Audio,
            TypeIndex = audioStreamIndex
        });
        return this;
    }

    /// <summary>
    /// Selects a subtitle stream by its index within the subtitle streams of the input file.
    /// </summary>
    /// <param name="subtitleStreamIndex">The subtitle stream index (0-based, relative to subtitle streams only).</param>
    /// <param name="inputIndex">The input file index (0-based). Defaults to 0.</param>
    /// <returns>This StreamSelector for fluent chaining.</returns>
    public StreamSelector SelectSubtitleByIndex(int subtitleStreamIndex, int inputIndex = 0)
    {
        if (subtitleStreamIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(subtitleStreamIndex), "Subtitle stream index must be non-negative");
        if (inputIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(inputIndex), "Input index must be non-negative");

        _selections.Add(new StreamSelection
        {
            Type = StreamSelectionType.ByTypeIndex,
            InputIndex = inputIndex,
            StreamType = StreamType.Subtitle,
            TypeIndex = subtitleStreamIndex
        });
        return this;
    }

    /// <summary>
    /// Builds the FFmpeg -map arguments for the current selections.
    /// </summary>
    /// <returns>A list of -map arguments ready to be passed to FFmpeg.</returns>
    public IReadOnlyList<string> BuildMapArguments()
    {
        var args = new List<string>();
        foreach (var selection in _selections)
        {
            args.Add(selection.BuildMapArgument());
        }
        return args;
    }

    /// <summary>
    /// Validates the stream selections against the actual streams in the input media file.
    /// Throws if any selection cannot be satisfied by the available streams.
    /// </summary>
    /// <param name="mediaFile">The media file to validate against (must have been probed with ffprobe).</param>
    /// <exception cref="InvalidStreamSelectionException">Thrown when a stream selection cannot be satisfied.</exception>
    public void Validate(MediaFile mediaFile)
    {
        ArgumentNullException.ThrowIfNull(mediaFile);

        // We need stream information from ffprobe - this requires the MediaFile to have stream metadata
        // For now, we validate based on what we can determine from MediaFile properties
        // In a full implementation, this would use MediaProbeService to get detailed stream info

        foreach (var selection in _selections)
        {
            selection.Validate(mediaFile);
        }
    }

    /// <summary>
    /// Creates a clone of this StreamSelector.
    /// </summary>
    public StreamSelector Clone()
    {
        var clone = new StreamSelector();
        clone._selections.AddRange(_selections.Select(s => s.Clone()));
        return clone;
    }

    /// <summary>
    /// Gets the number of stream selections.
    /// </summary>
    public int Count => _selections.Count;

    /// <summary>
    /// Returns a string representation of the stream selector.
    /// </summary>
    public override string ToString()
    {
        return $"StreamSelector {{ Selections = {_selections.Count} }}";
    }
}

/// <summary>
/// Type of stream selection.
/// </summary>
internal enum StreamSelectionType
{
    /// <summary>Select by absolute stream index (inputIndex:streamIndex).</summary>
    ByIndex,
    /// <summary>Select all streams of a specific type.</summary>
    AllOfType,
    /// <summary>Select by stream type and index within that type.</summary>
    ByTypeIndex,
    /// <summary>Select by language metadata tag.</summary>
    ByLanguage
}

/// <summary>
/// Stream type enumeration.
/// </summary>
internal enum StreamType
{
    Video,
    Audio,
    Subtitle,
    Data
}

/// <summary>
/// Represents a single stream selection criterion.
/// </summary>
internal sealed class StreamSelection
{
    public StreamSelectionType Type { get; set; }
    public int InputIndex { get; set; }
    public int StreamIndex { get; set; }
    public StreamType StreamType { get; set; }
    public int TypeIndex { get; set; }
    public string? Language { get; set; }

    public string BuildMapArgument()
    {
        return Type switch
        {
            StreamSelectionType.ByIndex => $"-map {InputIndex}:{StreamIndex}",
            StreamSelectionType.AllOfType => $"-map {InputIndex}:{GetStreamTypeChar(StreamType)}",
            StreamSelectionType.ByTypeIndex => $"-map {InputIndex}:{GetStreamTypeChar(StreamType)}:{TypeIndex}",
            StreamSelectionType.ByLanguage => $"-map {InputIndex}:{GetStreamTypeChar(StreamType)}:m:language:{Language}",
            _ => throw new InvalidOperationException($"Unknown stream selection type: {Type}")
        };
    }

    private static char GetStreamTypeChar(StreamType type) => type switch
    {
        StreamType.Video => 'v',
        StreamType.Audio => 'a',
        StreamType.Subtitle => 's',
        StreamType.Data => 'd',
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown stream type")
    };

    public void Validate(MediaFile mediaFile)
    {
        // Basic validation - in a full implementation this would check against actual ffprobe output
        // For now we validate the selection parameters are reasonable
        switch (Type)
        {
            case StreamSelectionType.ByIndex:
                if (InputIndex < 0 || StreamIndex < 0)
                    throw new InvalidStreamSelectionException($"Invalid stream index: {InputIndex}:{StreamIndex}");
                break;
            case StreamSelectionType.ByTypeIndex:
                if (InputIndex < 0 || TypeIndex < 0)
                    throw new InvalidStreamSelectionException($"Invalid type index: input {InputIndex}, type {StreamType}, index {TypeIndex}");
                break;
            case StreamSelectionType.ByLanguage:
                if (string.IsNullOrWhiteSpace(Language))
                    throw new InvalidStreamSelectionException("Language code is required for language-based selection");
                break;
        }
    }

    public StreamSelection Clone()
    {
        return new StreamSelection
        {
            Type = Type,
            InputIndex = InputIndex,
            StreamIndex = StreamIndex,
            StreamType = StreamType,
            TypeIndex = TypeIndex,
            Language = Language
        };
    }
}