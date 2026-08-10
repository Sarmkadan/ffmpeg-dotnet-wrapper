// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using FFmpegDotnetWrapper.Models;

namespace FFmpegDotnetWrapper.Monitoring;

/// <summary>
/// Aggregates runtime telemetry emitted by the adaptive bitrate streaming pipeline.
/// </summary>
/// <remarks>
/// Registered as a singleton so that metrics accumulate across all pipeline runs for the
/// lifetime of the process. All public methods are thread-safe and lock-free where possible,
/// using <see cref="Interlocked"/> for counter updates and a lightweight lock only when
/// computing derived values that span multiple fields atomically.
/// </remarks>
public sealed class StreamingPipelineMetrics
{
    private long _totalSegmentsProduced;
    private long _totalBytesProduced;
    private long _totalBitrateSwitches;
    private long _totalUpgrades;
    private long _totalDowngrades;
    private long _completedPipelines;
    private long _failedPipelines;
    private long _cumulativeDurationTicks;

    private readonly ConcurrentDictionary<string, ProfileMetrics> _perProfile = new();

    // ── Counters ─────────────────────────────────────────────────────────────

    /// <summary>Gets the total number of segments produced across all pipeline runs.</summary>
    public long TotalSegmentsProduced => Interlocked.Read(ref _totalSegmentsProduced);

    /// <summary>Gets the cumulative bytes written across all produced segments.</summary>
    public long TotalBytesProduced => Interlocked.Read(ref _totalBytesProduced);

    /// <summary>Gets the total number of adaptive bitrate switch events recorded.</summary>
    public long TotalBitrateSwitches => Interlocked.Read(ref _totalBitrateSwitches);

    /// <summary>Gets the number of ABR switches that moved to a higher quality profile.</summary>
    public long TotalUpgrades => Interlocked.Read(ref _totalUpgrades);

    /// <summary>Gets the number of ABR switches that moved to a lower quality profile.</summary>
    public long TotalDowngrades => Interlocked.Read(ref _totalDowngrades);

    /// <summary>Gets the count of pipeline runs that reached <see cref="PipelineState.Completed"/>.</summary>
    public long CompletedPipelines => Interlocked.Read(ref _completedPipelines);

    /// <summary>Gets the count of pipeline runs that reached <see cref="PipelineState.Failed"/>.</summary>
    public long FailedPipelines => Interlocked.Read(ref _failedPipelines);

    /// <summary>
    /// Gets the average wall-clock duration of successfully completed pipeline runs.
    /// Returns <see cref="TimeSpan.Zero"/> until at least one pipeline completes.
    /// </summary>
    public TimeSpan AveragePipelineDuration
    {
        get
        {
            var completed = CompletedPipelines;
            if (completed == 0) return TimeSpan.Zero;
            var ticks = Interlocked.Read(ref _cumulativeDurationTicks);
            return TimeSpan.FromTicks(ticks / completed);
        }
    }

    // ── Recording methods ────────────────────────────────────────────────────

    /// <summary>
    /// Records that a segment was produced for a given quality profile.
    /// </summary>
    /// <param name="profile">The quality profile at which the segment was encoded.</param>
    /// <param name="fileSizeBytes">The segment's file size in bytes.</param>
    public void RecordSegmentProduced(StreamingProfile profile, long fileSizeBytes)
    {
        ArgumentNullException.ThrowIfNull(profile);
        Interlocked.Increment(ref _totalSegmentsProduced);
        Interlocked.Add(ref _totalBytesProduced, fileSizeBytes);

        _perProfile
            .GetOrAdd(profile.Name, name => new ProfileMetrics(name))
            .IncrementSegments(fileSizeBytes);
    }

    /// <summary>
    /// Records an adaptive bitrate switch event.
    /// </summary>
    /// <param name="isUpgrade">
    /// <c>true</c> when the switch moved to a higher-quality profile;
    /// <c>false</c> for a downgrade.
    /// </param>
    public void RecordBitrateSwitch(bool isUpgrade)
    {
        Interlocked.Increment(ref _totalBitrateSwitches);
        if (isUpgrade)
            Interlocked.Increment(ref _totalUpgrades);
        else
            Interlocked.Increment(ref _totalDowngrades);
    }

    /// <summary>
    /// Records that a pipeline run completed successfully.
    /// </summary>
    /// <param name="pipelineId">The pipeline identifier (used for diagnostics only).</param>
    /// <param name="elapsed">The total wall-clock duration of the completed run.</param>
    public void RecordPipelineCompleted(string pipelineId, TimeSpan elapsed)
    {
        ArgumentException.ThrowIfNullOrEmpty(pipelineId);
        Interlocked.Increment(ref _completedPipelines);
        Interlocked.Add(ref _cumulativeDurationTicks, elapsed.Ticks);
    }

    /// <summary>
    /// Records that a pipeline run ended with an error.
    /// </summary>
    /// <param name="pipelineId">The pipeline identifier (used for diagnostics only).</param>
    public void RecordPipelineFailed(string pipelineId)
    {
        ArgumentException.ThrowIfNullOrEmpty(pipelineId);
        Interlocked.Increment(ref _failedPipelines);
    }

    // ── Reporting ────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns a read-only snapshot of per-profile segment production metrics,
    /// keyed by profile name (e.g., <c>"1080p"</c>, <c>"720p"</c>).
    /// </summary>
    public IReadOnlyDictionary<string, ProfileMetrics> GetProfileBreakdown() => _perProfile;

    /// <summary>
    /// Generates a human-readable summary report of all accumulated streaming metrics.
    /// </summary>
    /// <returns>A multi-line string suitable for logging or health-check endpoints.</returns>
    public string GetSummaryReport()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Adaptive Bitrate Streaming — Metrics Report");
        sb.AppendLine("============================================");
        sb.AppendLine($"  Segments produced     : {TotalSegmentsProduced:N0}");
        sb.AppendLine($"  Data produced         : {TotalBytesProduced / 1_048_576.0:F1} MB");
        sb.AppendLine($"  ABR switches total    : {TotalBitrateSwitches:N0}  " +
                      $"({TotalUpgrades} up, {TotalDowngrades} down)");
        sb.AppendLine($"  Completed pipelines   : {CompletedPipelines:N0}");
        sb.AppendLine($"  Failed pipelines      : {FailedPipelines:N0}");
        sb.AppendLine($"  Avg pipeline duration : {AveragePipelineDuration:c}");
        sb.AppendLine();
        sb.AppendLine("  Profile breakdown:");

        foreach (var (name, pm) in _perProfile.OrderBy(kv => kv.Key))
        {
            sb.AppendLine($"    {name,-10}: {pm.TotalSegments,7:N0} segments  " +
                          $"{pm.TotalBytes / 1_048_576.0,8:F1} MB  " +
                          $"avg {(pm.TotalSegments > 0 ? pm.TotalBytes / pm.TotalSegments / 1024.0 : 0):F0} KB/seg");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Exports per-profile metrics as a CSV string for offline analysis.
    /// </summary>
    /// <returns>CSV text with a header row followed by one row per profile.</returns>
    public string ExportProfilesAsCsv()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("ProfileName,TotalSegments,TotalBytes,AvgSegmentBytes");

        foreach (var (_, pm) in _perProfile.OrderBy(kv => kv.Key))
        {
            var avgBytes = pm.TotalSegments > 0 ? pm.TotalBytes / pm.TotalSegments : 0;
            sb.AppendLine($"{pm.ProfileName},{pm.TotalSegments},{pm.TotalBytes},{avgBytes}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Resets all accumulated metrics counters to zero.
    /// Intended for use in testing or at the start of a new measurement period.
    /// </summary>
    public void Reset()
    {
        Interlocked.Exchange(ref _totalSegmentsProduced, 0);
        Interlocked.Exchange(ref _totalBytesProduced, 0);
        Interlocked.Exchange(ref _totalBitrateSwitches, 0);
        Interlocked.Exchange(ref _totalUpgrades, 0);
        Interlocked.Exchange(ref _totalDowngrades, 0);
        Interlocked.Exchange(ref _completedPipelines, 0);
        Interlocked.Exchange(ref _failedPipelines, 0);
        Interlocked.Exchange(ref _cumulativeDurationTicks, 0);
        _perProfile.Clear();
    }
}

/// <summary>
/// Per-profile segment production telemetry for a single quality rendition.
/// All access is lock-free via <see cref="Interlocked"/>.
/// </summary>
public sealed class ProfileMetrics
{
    private long _totalSegments;
    private long _totalBytes;

    internal ProfileMetrics(string profileName)
    {
        ProfileName = profileName;
    }

    /// <summary>Gets the name of the quality profile (e.g., <c>"720p"</c>).</summary>
    public string ProfileName { get; }

    /// <summary>Gets the total number of segments produced for this profile.</summary>
    public long TotalSegments => Interlocked.Read(ref _totalSegments);

    /// <summary>Gets the total bytes written across all segments for this profile.</summary>
    public long TotalBytes => Interlocked.Read(ref _totalBytes);

    /// <summary>Increments the segment counter and adds the given byte count atomically.</summary>
    /// <param name="bytes">The file size of the newly produced segment.</param>
    internal void IncrementSegments(long bytes)
    {
        Interlocked.Increment(ref _totalSegments);
        Interlocked.Add(ref _totalBytes, bytes);
    }
}
