// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Models;

namespace FFmpegDotnetWrapper.Services;

/// <summary>
/// Orchestrates real-time adaptive bitrate streaming pipelines.
/// </summary>
/// <remarks>
/// A streaming pipeline encodes one source file into multiple quality renditions
/// simultaneously (or sequentially) and writes HLS or DASH segments and manifests
/// to a configured output directory.
///
/// The service monitors the ratio between each segment's actual and target bitrate
/// in a sliding window and recommends quality switches via <see cref="StreamingPipelineResult.ActiveProfile"/>
/// whenever the encoder consistently falls below or exceeds the configured thresholds.
/// These recommendations are recorded as <see cref="BitrateSwitch"/> events and can be
/// used by a downstream playlist server to steer clients towards the appropriate rendition.
/// </remarks>
public interface IAdaptiveBitrateService
{
    /// <summary>
    /// Gets the identifiers of all currently active (running) pipelines.
    /// </summary>
    IReadOnlyCollection<string> ActivePipelineIds { get; }

    /// <summary>
    /// Starts a new streaming pipeline and asynchronously yields every
    /// <see cref="StreamingSegment"/> as it is encoded.
    /// </summary>
    /// <remarks>
    /// The master playlist is written to disk before the first segment is yielded.
    /// Cancelling <paramref name="cancellationToken"/> transitions the pipeline to
    /// <see cref="PipelineState.Cancelled"/> and terminates all in-progress FFmpeg processes.
    /// </remarks>
    /// <param name="settings">
    /// Pipeline configuration, including the source file, quality profiles, output directory,
    /// segment duration, and concurrency preferences.
    /// </param>
    /// <param name="cancellationToken">Token to abort the entire pipeline.</param>
    /// <returns>
    /// An async sequence of <see cref="StreamingSegment"/> objects, one per encoded segment,
    /// across all configured quality profiles.
    /// </returns>
    IAsyncEnumerable<StreamingSegment> RunPipelineAsync(
        StreamingPipelineSettings settings,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates the output directory structure and writes the master playlist or manifest
    /// without starting any FFmpeg encoding process.
    /// </summary>
    /// <remarks>
    /// Useful for pre-staging the output hierarchy so that a CDN origin can begin
    /// serving the manifest endpoint before segments arrive.
    /// </remarks>
    /// <param name="settings">Pipeline configuration.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The absolute path to the master playlist file that was written.</returns>
    Task<string> InitialisePipelineAsync(
        StreamingPipelineSettings settings,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Encodes a single quality rendition and asynchronously yields each segment as it is
    /// written to disk.
    /// </summary>
    /// <remarks>
    /// This method can be called independently of <see cref="RunPipelineAsync"/> when only
    /// a single rendition is required, or when callers need fine-grained control over
    /// per-profile encoding (e.g., prioritising a specific quality level first).
    /// </remarks>
    /// <param name="settings">
    /// Pipeline configuration; only <see cref="StreamingPipelineSettings.InputFilePath"/>,
    /// <see cref="StreamingPipelineSettings.OutputDirectory"/>,
    /// <see cref="StreamingPipelineSettings.SegmentDurationSeconds"/>,
    /// <see cref="StreamingPipelineSettings.Format"/>, and
    /// <see cref="StreamingPipelineSettings.EnableHardwareAcceleration"/> are used.
    /// </param>
    /// <param name="profile">The quality profile to encode.</param>
    /// <param name="cancellationToken">Token to abort encoding of this rendition.</param>
    /// <returns>Async sequence of <see cref="StreamingSegment"/> objects for this rendition.</returns>
    IAsyncEnumerable<StreamingSegment> EncodeRenditionAsync(
        StreamingPipelineSettings settings,
        StreamingProfile profile,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a snapshot of the accumulated state and results for a pipeline.
    /// </summary>
    /// <param name="pipelineId">
    /// The pipeline identifier that was assigned when <see cref="RunPipelineAsync"/> started.
    /// </param>
    /// <returns>
    /// The <see cref="StreamingPipelineResult"/> for the requested pipeline,
    /// or <c>null</c> if no pipeline with that identifier is found.
    /// </returns>
    Task<StreamingPipelineResult?> GetPipelineResultAsync(string pipelineId);

    /// <summary>
    /// Requests cancellation of an active pipeline.
    /// </summary>
    /// <remarks>
    /// This method signals the pipeline's cancellation token; the pipeline will stop encoding
    /// and transition to <see cref="PipelineState.Cancelled"/> asynchronously.
    /// Any partially written segments remain on disk.
    /// </remarks>
    /// <param name="pipelineId">The identifier of the pipeline to cancel.</param>
    /// <returns>
    /// <c>true</c> if the pipeline was found and cancellation was requested;
    /// <c>false</c> if no pipeline with that identifier is currently active.
    /// </returns>
    Task<bool> CancelPipelineAsync(string pipelineId);
}
