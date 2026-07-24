// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

#nullable enable

using FFmpegDotnetWrapper.Models;

namespace FFmpegDotnetWrapper.Services;

/// <summary>
/// In-memory <see cref="IFFmpegProcessRunner"/> test double that never spawns a real process.
/// Lets callers verify service-level behavior (argument construction, result mapping, cancellation
/// handling) without requiring the <c>ffmpeg</c> binary to be installed on the test machine.
/// </summary>
public sealed class FakeFFmpegProcessRunner : IFFmpegProcessRunner
{
    private readonly List<FFmpegProcessRequest> _requests = new();

    /// <summary>
    /// Every request passed to <see cref="RunAsync"/> so far, in call order.
    /// </summary>
    public IReadOnlyList<FFmpegProcessRequest> Requests => _requests;

    /// <summary>
    /// The <see cref="FFmpegProcessResult"/> returned by every call to <see cref="RunAsync"/>.
    /// Defaults to a successful, zero-duration result.
    /// </summary>
    public FFmpegProcessResult ResultToReturn { get; set; } = new()
    {
        ExitCode = 0,
        StdErrTail = string.Empty,
        ExecutionTime = TimeSpan.Zero
    };

    /// <summary>
    /// Progress snapshots reported to the caller's <see cref="IProgress{T}"/> before
    /// <see cref="ResultToReturn"/> is returned, in order, when the request opts into progress
    /// parsing. Defaults to no snapshots.
    /// </summary>
    public IReadOnlyList<FFmpegProgressUpdate> ProgressUpdatesToReport { get; set; } = Array.Empty<FFmpegProgressUpdate>();

    /// <summary>
    /// Optional callback invoked synchronously with each incoming request, before
    /// <see cref="ResultToReturn"/> is produced. Useful for asserting on arguments or throwing to
    /// simulate a hard failure.
    /// </summary>
    public Action<FFmpegProcessRequest>? OnRun { get; set; }

    /// <summary>
    /// Records the request, optionally replays configured progress updates, and returns
    /// <see cref="ResultToReturn"/>. Honors <paramref name="cancellationToken"/> by throwing
    /// <see cref="OperationCanceledException"/> if it is already signaled, mirroring how the real
    /// runner would abandon a launch that was cancelled before starting.
    /// </summary>
    /// <param name="request">The simulated ffmpeg invocation.</param>
    /// <param name="progress">Optional receiver of the configured progress snapshots.</param>
    /// <param name="cancellationToken">Token checked once before returning the configured result.</param>
    /// <returns>A task that resolves to <see cref="ResultToReturn"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is already signaled.</exception>
    public Task<FFmpegProcessResult> RunAsync(
        FFmpegProcessRequest request,
        IProgress<FFmpegProgressUpdate>? progress,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        cancellationToken.ThrowIfCancellationRequested();

        _requests.Add(request);
        OnRun?.Invoke(request);

        if (request.ParseProgressFromStdOut && progress is not null)
        {
            foreach (var update in ProgressUpdatesToReport)
                progress.Report(update);
        }

        return Task.FromResult(ResultToReturn);
    }
}
