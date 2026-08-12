// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Exceptions;
using FFmpegDotnetWrapper.Utilities;

namespace FFmpegDotnetWrapper.Models;

/// <summary>
/// Configuration settings for embedding subtitles into video files.
/// Supports both soft embedding (as a subtitle stream) and hard embedding
/// (burning subtitles directly into the video frames).
/// </summary>
public class SubtitleSettings
{
    private static readonly HashSet<string> SupportedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".srt", ".ass", ".ssa", ".vtt", ".sub" };

    private string _subtitlePath = string.Empty;
    private string _charEncoding = "UTF-8";

    /// <summary>
    /// Path to the subtitle file (.srt, .ass, .ssa, .vtt, or .sub).
    /// The file must exist at the time the property is set.
    /// </summary>
    public string SubtitlePath
    {
        get => _subtitlePath;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationConfigurationException("Subtitle path cannot be null or empty");

            if (!File.Exists(value))
                throw new InvalidOperationConfigurationException($"Subtitle file does not exist: {value}");

            var ext = Path.GetExtension(value);
            if (!SupportedExtensions.Contains(ext))
                throw new InvalidOperationConfigurationException(
                    $"Unsupported subtitle format '{ext}'. Supported: {string.Join(", ", SupportedExtensions)}");

            // Validate that the subtitle path stays within the current directory
                // Use the executable's directory as a safe base directory
                var baseDirectory = AppContext.BaseDirectory;
                _subtitlePath = PathValidation.ValidateExistingFileWithinBaseDirectory(value, baseDirectory, nameof(SubtitlePath));
        }
    }

    /// <summary>
    /// When <c>true</c>, the subtitles are burned directly into the video frames (hard embed),
    /// making them visible on all players without subtitle support.
    /// When <c>false</c> (default), the subtitle track is added as a soft-coded stream
    /// that can be toggled by the viewer.
    /// </summary>
    public bool HardEmbed { get; set; } = false;

    /// <summary>
    /// Character encoding of the subtitle file. Defaults to <c>UTF-8</c>.
    /// Common alternatives include <c>latin1</c>, <c>cp1252</c>, and <c>ISO-8859-1</c>.
    /// </summary>
    public string CharEncoding
    {
        get => _charEncoding;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationConfigurationException("Character encoding cannot be null or empty");
            _charEncoding = value;
        }
    }

    /// <summary>
    /// Font name used when hard-embedding subtitles. Only applies when <see cref="HardEmbed"/> is <c>true</c>.
    /// Defaults to <c>Arial</c>.
    /// </summary>
    public string? FontName { get; set; } = "Arial";

    /// <summary>
    /// Font size in points used when hard-embedding subtitles.
    /// Only applies when <see cref="HardEmbed"/> is <c>true</c>.
    /// Must be between 6 and 120.
    /// </summary>
    public int FontSize { get; set; } = 24;

    /// <summary>
    /// Zero-based index of the subtitle stream to embed when the input file contains
    /// multiple subtitle tracks. Defaults to <c>0</c>.
    /// </summary>
    public int SubtitleStreamIndex { get; set; } = 0;

    /// <summary>
    /// Language code (ISO 639-1, e.g. <c>en</c>, <c>fr</c>) stored in the output stream metadata.
    /// Optional; leave <c>null</c> to omit the language tag.
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// Validates all subtitle settings before an operation is executed.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(_subtitlePath))
            throw new InvalidOperationConfigurationException("Subtitle path must be specified");

        if (!File.Exists(_subtitlePath))
            throw new InvalidOperationConfigurationException($"Subtitle file no longer exists: {_subtitlePath}");

        if (FontSize < 6 || FontSize > 120)
            throw new InvalidOperationConfigurationException("FontSize must be between 6 and 120");

        if (SubtitleStreamIndex < 0)
            throw new InvalidOperationConfigurationException("SubtitleStreamIndex must be non-negative");
    }

    /// <summary>
    /// Creates a deep copy of the current settings.
    /// </summary>
    public SubtitleSettings Clone() =>
        new()
        {
            _subtitlePath = _subtitlePath,
            HardEmbed = HardEmbed,
            _charEncoding = _charEncoding,
            FontName = FontName,
            FontSize = FontSize,
            SubtitleStreamIndex = SubtitleStreamIndex,
            Language = Language
        };

    public override string ToString() => $"SubtitleSettings {{ HardEmbed = {HardEmbed}, FontName = {FontName}, FontSize = {FontSize}, SubtitleStreamIndex = {SubtitleStreamIndex}, Language = {Language} }}";
}
