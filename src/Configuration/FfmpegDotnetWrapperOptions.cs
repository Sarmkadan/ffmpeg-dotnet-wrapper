// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;

using FFmpegDotnetWrapper.Constants;

namespace FFmpegDotnetWrapper.Configuration;

/// <summary>
/// Options for the FFmpeg wrapper, bindable from the <see cref="SectionName"/> section of
/// <c>appsettings.json</c>, environment variables, or user secrets. Register them with
/// <c>AddFfmpegDotnetWrapperOptions</c>; <see cref="FfmpegDotnetWrapperOptionsValidator"/> rejects values
/// that break the data annotations or the log level and working directory rules.
/// </summary>
/// <remarks>
/// Nothing in this class holds a secret. Every default is either a neutral value or <see langword="null"/>,
/// so executable locations are resolved from <c>PATH</c> on each machine rather than baked into the library.
/// </remarks>
public sealed class FfmpegDotnetWrapperOptions
{
    /// <summary>The configuration section name that <see cref="FfmpegDotnetWrapperOptions"/> binds to.</summary>
    public const string SectionName = "FfmpegDotnetWrapper";

    /// <summary>
    /// Path to the <c>ffmpeg</c> executable. When <see langword="null"/> or whitespace, <c>ffmpeg</c> is resolved from <c>PATH</c>.
    /// </summary>
    public string? FfmpegPath { get; set; }

    /// <summary>
    /// Path to the <c>ffprobe</c> executable. When <see langword="null"/> or whitespace, <c>ffprobe</c> is resolved from <c>PATH</c>.
    /// </summary>
    public string? FfprobePath { get; set; }

    /// <summary>
    /// Maximum time in seconds that a single FFmpeg process may run.
    /// </summary>
    [Range(1, FFmpegConstants.MaxTimeoutSeconds)]
    public int TimeoutSeconds { get; set; } = FFmpegConstants.DefaultTimeoutSeconds;

    /// <summary>
    /// The value passed to <c>ffmpeg -loglevel</c>. Must be one of <see cref="FfmpegRunnerOptions.AllowedLogLevels"/>.
    /// </summary>
    [Required]
    public string LogLevel { get; set; } = "info";

    /// <summary>
    /// Directory that FFmpeg processes run in. When <see langword="null"/>, the current process directory is used;
    /// when set, the directory must exist.
    /// </summary>
    public string? WorkingDirectory { get; set; }

    /// <summary>
    /// Converts these options into the <see cref="FfmpegRunnerOptions"/> used to launch FFmpeg and FFprobe.
    /// The result is not validated; call <see cref="FfmpegRunnerOptions.EnsureValid"/> or use <c>AddFfmpeg</c> to do that.
    /// </summary>
    /// <returns>A new <see cref="FfmpegRunnerOptions"/> carrying the same values.</returns>
    public FfmpegRunnerOptions ToRunnerOptions()
    {
        return new FfmpegRunnerOptions
        {
            FfmpegPath = FfmpegPath,
            FfprobePath = FfprobePath,
            Timeout = TimeSpan.FromSeconds(TimeoutSeconds),
            LogLevel = LogLevel,
            WorkingDirectory = WorkingDirectory
        };
    }

    /// <summary>
    /// Returns a string that represents the current options.
    /// </summary>
    public override string ToString()
    {
        return $"FfmpegDotnetWrapperOptions {{ FfmpegPath = {FfmpegPath}, FfprobePath = {FfprobePath}, TimeoutSeconds = {TimeoutSeconds}, LogLevel = {LogLevel}, WorkingDirectory = {WorkingDirectory} }}";
    }
}
