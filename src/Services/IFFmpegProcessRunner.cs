// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

#nullable enable

using FFmpegDotnetWrapper.Models;

namespace FFmpegDotnetWrapper.Services;

/// <summary>
/// Describes a single invocation of the <c>ffmpeg</c> executable that
/// <see cref="IFFmpegProcessRunner"/> should carry out.
/// </summary>
public sealed class FFmpegProcessRequest
{
    /// <summary>
    /// Full path (or bare name, if resolvable via <c>PATH</c>) of the <c>ffmpeg</c> executable to run.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// Fully built command-line argument string (everything after the executable name).
    /// </summary>
    public required string Arguments { get; init; }

    /// <summary>
    /// Working directory for the process. When <see langword="null"/>, the current directory is used.
    /// </summary>
    public string? WorkingDirectory { get; init; }

    /// <summary>
    /// Maximum time to allow the process to run before it is treated as hung and terminated.
    /// When <see langword="null"/>, no timeout is enforced beyond the caller's cancellation token.
    /// </summary>
    public TimeSpan? Timeout { get; init; }

    /// <summary>
    /// Identifier propagated into <see cref="FFmpegProgressUpdate.OperationId"/> for progress
    /// snapshots reported while this request executes.
    /// </summary>
    public string OperationId { get; init; } = string.Empty;

    /// <summary>
    /// Total media duration used to compute completion percentage in reported progress snapshots.
    /// Pass <see cref="TimeSpan.Zero"/> when the total duration is unknown.
    /// </summary>
    public TimeSpan TotalDuration { get; init; } = TimeSpan.Zero;

    /// <summary>
    /// When <see langword="true"/>, standard output is parsed as an <c>-progress pipe:1</c> stream
    /// and reported through the <see cref="IProgress{T}"/> passed to
    /// <see cref="IFFmpegProcessRunner.RunAsync"/>. The caller is responsible for appending
    /// <c>-progress pipe:1 -nostats</c> to <see cref="Arguments"/> when this is set.
    /// </summary>
    public bool ParseProgressFromStdOut { get; init; }
}

/// <summary>
/// Outcome of an <see cref="IFFmpegProcessRunner.RunAsync"/> invocation.
/// </summary>
public sealed class FFmpegProcessResult
{
    /// <summary>
    /// Exit code reported by the process, or <c>-1</c> when the process was killed before exiting
    /// on its own (timeout or cancellation that did not finalize in time).
    /// </summary>
    public required int ExitCode { get; init; }

    /// <summary>
    /// Tail of standard error captured while the process ran, bounded to a fixed number of
    /// characters so long-running operations do not accumulate unbounded memory.
    /// </summary>
    public required string StdErrTail { get; init; }

    /// <summary>
    /// Wall-clock time the process ran for, from start to exit (or termination).
    /// </summary>
    public required TimeSpan ExecutionTime { get; init; }

    /// <summary>
    /// <see langword="true"/> when the process was terminated because <see cref="FFmpegProcessRequest.Timeout"/>
    /// elapsed before it exited.
    /// </summary>
    public bool TimedOut { get; init; }

    /// <summary>
    /// <see langword="true"/> when the process was terminated because the caller's
    /// <see cref="CancellationToken"/> was signaled.
    /// </summary>
    public bool WasCancelled { get; init; }

    /// <summary>
    /// Indicates a clean, successful run: process exited with code <c>0</c>, was not killed for
    /// timing out, and was not cancelled.
    /// </summary>
    public bool Success => ExitCode == 0 && !TimedOut && !WasCancelled;
}

/// <summary>
/// Abstraction over launching the <c>ffmpeg</c> executable as an external process. Introducing
/// this seam lets callers substitute a fake implementation in unit tests so that library behavior
/// (argument building, progress parsing, cancellation handling, result mapping) can be verified
/// without an actual <c>ffmpeg</c> binary being installed.
/// </summary>
public interface IFFmpegProcessRunner
{
    /// <summary>
    /// Runs the process described by <paramref name="request"/> to completion, optionally
    /// streaming progress snapshots, and returns its outcome.
    /// </summary>
    /// <param name="request">Describes the executable, arguments, timeout and progress-parsing options.</param>
    /// <param name="progress">
    /// Optional receiver of incremental <see cref="FFmpegProgressUpdate"/> snapshots. Only used
    /// when <see cref="FFmpegProcessRequest.ParseProgressFromStdOut"/> is <see langword="true"/>.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to request early termination. On cancellation, implementations should attempt a
    /// graceful shutdown (e.g. sending <c>q</c> to <c>ffmpeg</c>'s standard input so it finalizes
    /// the output file) before forcibly killing the process.
    /// </param>
    /// <returns>A task that resolves to the <see cref="FFmpegProcessResult"/> describing the outcome.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
    Task<FFmpegProcessResult> RunAsync(
        FFmpegProcessRequest request,
        IProgress<FFmpegProgressUpdate>? progress,
        CancellationToken cancellationToken = default);
}
