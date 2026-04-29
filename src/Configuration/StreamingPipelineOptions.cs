// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Models;

namespace FFmpegDotnetWrapper.Configuration;

/// <summary>
/// Application-level configuration for the adaptive bitrate streaming pipeline.
/// Bind from the <c>FFmpeg:Streaming</c> section of <c>appsettings.json</c>.
/// </summary>
/// <example>
/// <code>
/// "FFmpeg": {
///   "Streaming": {
///     "DefaultSegmentDurationSeconds": 6,
///     "MaxConcurrentPipelines": 3,
///     "DowngradeSpeedThreshold": 0.9,
///     "UpgradeSpeedThreshold": 1.5
///   }
/// }
/// </code>
/// </example>
public sealed class StreamingPipelineOptions
{
    /// <summary>The <c>appsettings.json</c> section path for this options class.</summary>
    public const string Section = "FFmpeg:Streaming";

    /// <summary>
    /// Gets or sets whether the adaptive bitrate streaming pipeline feature is enabled.
    /// When <c>false</c>, <c>IAdaptiveBitrateService</c> is still registered but will
    /// throw <see cref="InvalidOperationException"/> on any pipeline start.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the default HLS or DASH segment duration in seconds applied to
    /// pipelines that do not explicitly set <c>StreamingPipelineSettings.SegmentDurationSeconds</c>.
    /// </summary>
    public int DefaultSegmentDurationSeconds { get; set; } = 6;

    /// <summary>
    /// Gets or sets the default sliding-window size for live playlists.
    /// <c>0</c> produces a VOD playlist that retains all segments.
    /// </summary>
    public int DefaultPlaylistWindowSize { get; set; } = 5;

    /// <summary>
    /// Gets or sets the default output format used when a pipeline does not specify one.
    /// Defaults to <see cref="StreamingFormat.Hls"/>.
    /// </summary>
    public StreamingFormat DefaultFormat { get; set; } = StreamingFormat.Hls;

    /// <summary>
    /// Gets or sets whether profiles within a single pipeline are encoded in parallel by default.
    /// Individual pipelines may override this via <c>StreamingPipelineSettings.EncodeProfilesConcurrently</c>.
    /// </summary>
    public bool DefaultEncodeProfilesConcurrently { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of simultaneously active pipelines across the entire process.
    /// Requests that would exceed this limit receive <see cref="InvalidOperationException"/>.
    /// </summary>
    public int MaxConcurrentPipelines { get; set; } = 3;

    /// <summary>
    /// Gets or sets the maximum number of quality profiles encoded in parallel within a single pipeline.
    /// Governs the degree of parallelism when <c>EncodeProfilesConcurrently</c> is <c>true</c>.
    /// </summary>
    public int MaxConcurrentRenditionsPerPipeline { get; set; } = 2;

    /// <summary>
    /// Gets or sets the segment bitrate ratio threshold below which the adaptive bitrate logic
    /// begins accumulating consecutive "slow" observations before recommending a quality downgrade.
    /// </summary>
    /// <remarks>
    /// A ratio of <c>1.0</c> means the actual segment bitrate exactly matches the target.
    /// Values below 1.0 indicate under-utilisation; the default <c>0.9</c> allows a 10% tolerance.
    /// </remarks>
    public double DowngradeSpeedThreshold { get; set; } = 0.9;

    /// <summary>
    /// Gets or sets the segment bitrate ratio threshold above which the adaptive bitrate logic
    /// begins accumulating consecutive "fast" observations before recommending a quality upgrade.
    /// </summary>
    public double UpgradeSpeedThreshold { get; set; } = 1.5;

    /// <summary>
    /// Gets or sets the number of consecutive segments whose bitrate ratio must exceed the
    /// up/down threshold before an adaptive bitrate switch is committed. A larger window
    /// reduces switch frequency at the cost of slower adaptation.
    /// </summary>
    public int BitrateDecisionWindowSegments { get; set; } = 3;

    /// <summary>
    /// Gets or sets the base directory under which per-pipeline output sub-folders are created.
    /// When <c>null</c>, the caller must supply an explicit output directory for each run via
    /// <c>StreamingPipelineSettings.OutputDirectory</c>.
    /// </summary>
    public string? DefaultOutputBaseDirectory { get; set; }

    /// <summary>Gets or sets whether hardware acceleration is attempted by default.</summary>
    public bool DefaultEnableHardwareAcceleration { get; set; }

    /// <summary>
    /// Gets or sets the quality profile definitions loaded from configuration.
    /// When this list is empty, <see cref="StreamingProfile.DefaultLadder"/> is used.
    /// </summary>
    public IList<StreamingProfileOptions> DefaultProfiles { get; set; } = [];
}

/// <summary>
/// JSON-serialisable representation of a <see cref="StreamingProfile"/>,
/// used to populate profiles from <c>appsettings.json</c>.
/// Convert to the strongly-typed record via <see cref="ToProfile"/>.
/// </summary>
public sealed class StreamingProfileOptions
{
    /// <summary>Gets or sets the rendition label (e.g., <c>"720p"</c>).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the output frame width in pixels.</summary>
    public int Width { get; set; }

    /// <summary>Gets or sets the output frame height in pixels.</summary>
    public int Height { get; set; }

    /// <summary>Gets or sets the target video bitrate in kilobits per second.</summary>
    public int VideoBitrateKbps { get; set; }

    /// <summary>Gets or sets the target audio bitrate in kilobits per second.</summary>
    public int AudioBitrateKbps { get; set; }

    /// <summary>Gets or sets the target frame rate. <c>0</c> preserves the source frame rate.</summary>
    public double FrameRate { get; set; }

    /// <summary>
    /// Converts this configuration object to the strongly-typed <see cref="StreamingProfile"/> record.
    /// </summary>
    /// <returns>A new <see cref="StreamingProfile"/> with the values from this options object.</returns>
    public StreamingProfile ToProfile() =>
        new(Name, Width, Height, VideoBitrateKbps, AudioBitrateKbps, FrameRate);
}
