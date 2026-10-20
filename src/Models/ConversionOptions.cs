// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Constants;
using FFmpegDotnetWrapper.Exceptions;

namespace FFmpegDotnetWrapper.Models;

/// <summary>
/// Options describing a single file conversion (input, output, encoding, and optional time range).
/// Call <see cref="Validate"/> before running the conversion.
/// </summary>
public class ConversionOptions
{
    /// <summary>
    /// Lowest accepted Constant Rate Factor (lossless for x264/x265).
    /// </summary>
    public const int MinCrf = 0;

    /// <summary>
    /// Highest accepted Constant Rate Factor (x264/x265 scale).
    /// </summary>
    public const int MaxCrf = 51;

    /// <summary>
    /// Path to the existing input media file. Required.
    /// </summary>
    public string? InputPath { get; set; }

    /// <summary>
    /// Path of the output file. Required; its directory must exist or be creatable.
    /// </summary>
    public string? OutputPath { get; set; }

    /// <summary>
    /// Target video bitrate in kbps. Mutually exclusive with <see cref="Crf"/>.
    /// Must be between <see cref="FFmpegConstants.MinBitrate"/> and <see cref="FFmpegConstants.MaxBitrate"/>.
    /// </summary>
    public int? VideoBitrateKbps { get; set; }

    /// <summary>
    /// Constant Rate Factor for quality-based encoding. Mutually exclusive with <see cref="VideoBitrateKbps"/>.
    /// Must be between <see cref="MinCrf"/> and <see cref="MaxCrf"/>.
    /// </summary>
    public int? Crf { get; set; }

    /// <summary>
    /// Output width in pixels. Must be positive and even when set.
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// Output height in pixels. Must be positive and even when set.
    /// </summary>
    public int? Height { get; set; }

    /// <summary>
    /// Output frame rate in frames per second. Must be greater than zero when set.
    /// </summary>
    public double? FrameRate { get; set; }

    /// <summary>
    /// Target audio bitrate in kbps. Must be positive when set.
    /// </summary>
    public int? AudioBitrateKbps { get; set; }

    /// <summary>
    /// Audio sample rate in Hz. Must be positive when set.
    /// </summary>
    public int? AudioSampleRate { get; set; }

    /// <summary>
    /// Number of output audio channels. Must be positive when set.
    /// </summary>
    public int? AudioChannels { get; set; }

    /// <summary>
    /// FFmpeg video codec name (for example <c>libx264</c>). Must not be empty when set.
    /// </summary>
    public string? VideoCodec { get; set; }

    /// <summary>
    /// FFmpeg audio codec name (for example <c>aac</c>). Must not be empty when set.
    /// </summary>
    public string? AudioCodec { get; set; }

    /// <summary>
    /// Start of the converted range. Defaults to the beginning of the input when unset; must not be negative.
    /// </summary>
    public TimeSpan? StartTime { get; set; }

    /// <summary>
    /// End of the converted range. Must be after the start time (zero when <see cref="StartTime"/> is unset).
    /// </summary>
    public TimeSpan? EndTime { get; set; }

    /// <summary>
    /// Known duration of the input media. When set, the time range must lie within it.
    /// </summary>
    public TimeSpan? SourceDuration { get; set; }

    /// <summary>
    /// Checks every rule and throws once with all violations, rather than stopping at the first.
    /// </summary>
    /// <exception cref="OptionsValidationException">Thrown when one or more rules are violated.
    /// <see cref="OptionsValidationException.Violations"/> lists each one.</exception>
    public void Validate()
    {
        var violations = new List<string>();

        var inputFullPath = ValidateInputPath(violations);
        var outputFullPath = ValidateOutputPath(violations);
        if (inputFullPath is not null && outputFullPath is not null &&
            string.Equals(inputFullPath, outputFullPath, PathComparison))
        {
            violations.Add($"OutputPath must differ from InputPath: {OutputPath}");
        }

        ValidateVideo(violations);
        ValidateAudio(violations);
        ValidateCodecs(violations);
        ValidateTimeRange(violations);

        if (violations.Count > 0)
            throw new OptionsValidationException(violations);
    }

    private static StringComparison PathComparison =>
        OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

    private string? ValidateInputPath(List<string> violations)
    {
        if (string.IsNullOrWhiteSpace(InputPath))
        {
            violations.Add("InputPath must be set");
            return null;
        }

        if (!File.Exists(InputPath))
            violations.Add($"InputPath does not exist: {InputPath}");

        return TryGetFullPath(InputPath);
    }

    private string? ValidateOutputPath(List<string> violations)
    {
        if (string.IsNullOrWhiteSpace(OutputPath))
        {
            violations.Add("OutputPath must be set");
            return null;
        }

        var fullPath = TryGetFullPath(OutputPath);
        if (fullPath is null)
        {
            violations.Add($"OutputPath is not a valid path: {OutputPath}");
            return null;
        }

        if (Directory.Exists(fullPath))
        {
            violations.Add($"OutputPath is an existing directory: {OutputPath}");
            return fullPath;
        }

        var directory = Path.GetDirectoryName(fullPath);
        if (directory is null || !CanExistAsDirectory(directory))
            violations.Add($"Output directory does not exist and cannot be created: {directory}");

        return fullPath;
    }

    private void ValidateVideo(List<string> violations)
    {
        if (VideoBitrateKbps is not null && Crf is not null)
            violations.Add("VideoBitrateKbps and Crf are mutually exclusive; set only one");

        if (VideoBitrateKbps is { } bitrate &&
            (bitrate < FFmpegConstants.MinBitrate || bitrate > FFmpegConstants.MaxBitrate))
        {
            violations.Add(
                $"VideoBitrateKbps must be between {FFmpegConstants.MinBitrate} and {FFmpegConstants.MaxBitrate}, but was {bitrate}");
        }

        if (Crf is { } crf && (crf < MinCrf || crf > MaxCrf))
            violations.Add($"Crf must be between {MinCrf} and {MaxCrf}, but was {crf}");

        if (Width is { } width && (width <= 0 || width % 2 != 0))
            violations.Add($"Width must be a positive even number, but was {width}");

        if (Height is { } height && (height <= 0 || height % 2 != 0))
            violations.Add($"Height must be a positive even number, but was {height}");

        if (FrameRate is { } frameRate && (!double.IsFinite(frameRate) || frameRate <= 0))
            violations.Add($"FrameRate must be greater than zero, but was {frameRate}");
    }

    private void ValidateAudio(List<string> violations)
    {
        if (AudioBitrateKbps is { } bitrate && bitrate <= 0)
            violations.Add($"AudioBitrateKbps must be positive, but was {bitrate}");

        if (AudioSampleRate is { } sampleRate && sampleRate <= 0)
            violations.Add($"AudioSampleRate must be positive, but was {sampleRate}");

        if (AudioChannels is { } channels && channels <= 0)
            violations.Add($"AudioChannels must be positive, but was {channels}");
    }

    private void ValidateCodecs(List<string> violations)
    {
        if (VideoCodec is not null && string.IsNullOrWhiteSpace(VideoCodec))
            violations.Add("VideoCodec must not be empty when specified");

        if (AudioCodec is not null && string.IsNullOrWhiteSpace(AudioCodec))
            violations.Add("AudioCodec must not be empty when specified");
    }

    private void ValidateTimeRange(List<string> violations)
    {
        if (StartTime is { } start && start < TimeSpan.Zero)
            violations.Add($"StartTime must not be negative, but was {start}");

        if (SourceDuration is { } duration && duration <= TimeSpan.Zero)
            violations.Add($"SourceDuration must be greater than zero, but was {duration}");

        var effectiveStart = StartTime ?? TimeSpan.Zero;
        if (EndTime is { } end && end <= effectiveStart)
            violations.Add($"EndTime ({end}) must be after StartTime ({effectiveStart})");

        if (SourceDuration is { } sourceDuration && sourceDuration > TimeSpan.Zero)
        {
            if (StartTime is { } s && s >= sourceDuration)
                violations.Add($"StartTime ({s}) exceeds the source duration ({sourceDuration})");

            if (EndTime is { } e && e > sourceDuration)
                violations.Add($"EndTime ({e}) exceeds the source duration ({sourceDuration})");
        }
    }

    private static string? TryGetFullPath(string path)
    {
        try
        {
            return Path.GetFullPath(path);
        }
        catch (ArgumentException)
        {
            return null;
        }
        catch (NotSupportedException)
        {
            return null;
        }
    }

    // Walks up to the nearest existing ancestor; the directory is creatable only if that ancestor is a directory.
    private static bool CanExistAsDirectory(string directory)
    {
        var existing = directory;
        while (existing is not null && !Directory.Exists(existing))
        {
            if (File.Exists(existing))
                return false;

            existing = Path.GetDirectoryName(existing);
        }

        return existing is not null;
    }
}
