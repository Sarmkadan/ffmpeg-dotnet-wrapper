# IFFmpegProcessRunner

There are two interfaces named `IFFmpegProcessRunner` in different namespaces, serving different purposes in the FFmpeg wrapper library.

## FFmpegDotnetWrapper.Abstraction.IFFmpegProcessRunner

Located in `src/Abstractions/IFFmpegProcessRunner.cs`

This interface is part of the abstraction layer and defines a simpler contract for executing FFmpeg commands using the `CliCommand` type.

```csharp
namespace FFmpegDotnetWrapper.Abstraction
{
    public interface IFFmpegProcessRunner
    {
        /// <summary>
        /// Executes the specified FFmpeg command.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="progress">Optional progress reporter.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The result of the process execution.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="command"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="command"/><see cref="CliCommand.FileName"/> is <see langword="null"/> or empty.</exception>
        Task<ProcessResult> RunAsync(
            CliCommand command,
            IProgress<FFmpegProgressUpdate>? progress,
            CancellationToken cancellationToken);
    }
}
```

### Key Points:
- Uses `CliCommand` from `FFmpegDotnetWrapper.Cli` namespace
- Returns `ProcessResult` from `FFmpegDotnetWrapper.Models` namespace
- Designed for basic FFmpeg command execution
- **Not implemented by any concrete class in the current codebase** - appears to be an alternative abstraction that is not currently used

## FFmpegDotnetWrapper.Services.IFFmpegProcessRunner

Located in `src/Services/IFFmpegProcessRunner.cs`

This is the primary interface used throughout the FFmpeg service layer. It defines a more detailed contract for FFmpeg process execution and includes the request/result models.

```csharp
namespace FFmpegDotnetWrapper.Services
{
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

        /// <summary>
        /// Returns a culture-invariant string representation of the FFmpegProcessRequest.
        /// </summary>
        public override string ToString()
        {
            string args = Arguments;
            if (args.Length > 120)
            {
                args = args.Substring(0, 120) + "...";
            }
            return string.Format(System.Globalization.CultureInfo.InvariantCulture, "FileName={0}, Arguments={1}, Timeout={2}", FileName, args, Timeout);
        }
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
        /// </>
        public bool WasCancelled { get; init; }

        /// <summary>
        /// Indicates a clean, successful run: process exited with code <c>0</c>, was not killed for
        /// timing out, and was not cancelled.
        /// </summary>
        public bool Success => ExitCode == 0 && !TimedOut && !WasCancelled;

        /// <summary>
        /// Returns a culture-invariant string representation of the FFmpegProcessResult.
        /// </summary>
        public override string ToString()
        {
            return string.Format(System.Globalization.CultureInfo.InvariantCulture, "ExitCode={0}, Success={1}, ExecutionTime={2}, TimedOut={3}", ExitCode, Success, ExecutionTime, TimedOut);
        }
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
}
```

### FFmpegProcessRequest Properties:
- **FileName**: Full path or executable name of ffmpeg
- **Arguments**: Command-line arguments for ffmpeg
- **WorkingDirectory**: Working directory for the process (null = current directory)
- **Timeout**: Maximum execution time (null = no timeout)
- **OperationId**: Identifier for progress tracking
- **TotalDuration**: Media duration for progress percentage calculation
- **ParseProgressFromStdOut**: Whether to parse progress from stdout

### FFmpegProcessResult Properties:
- **ExitCode**: Process exit code (-1 if killed)
- **StdErrTail**: Bounded stderr output for diagnostics
- **ExecutionTime**: Actual wall-clock execution time
- **TimedOut**: True if terminated by timeout
- **WasCancelled**: True if terminated by cancellation
- **Success**: Computed property (ExitCode == 0 && !TimedOut && !WasCancelled)

## Implementations

### FFmpegDotnetWrapper.Services.FFmpegProcessRunner
Located in `src/Services/FFmpegProcessRunner.cs`

This is the concrete implementation that launches the actual ffmpeg process. It implements `FFmpegDotnetWrapper.Services.IFFmpegProcessRunner`.

### FFmpegDotnetWrapper.Services.FakeFFmpegProcessRunner
Located in `src/Services/FakeFFmpegProcessRunner.cs`

This is an in-memory test double that implements `FFmpegDotnetWrapper.Services.IFFmpegProcessRunner` for unit testing. It never spawns a real process and allows configuring:
- Requests received (via `Requests` property)
- Result to return (via `ResultToReturn` property)
- Progress updates to report (via `ProgressUpdatesToReport` property)
- Optional callback for request validation (via `OnRun` property)

### Usage in FFmpegService
The `FFmpegDotnetWrapper.Services.FFmpegService` class depends on `IFFmpegProcessRunner` (from the Services namespace) via constructor injection. By default, it uses `FFmpegProcessRunner`, but can be substituted with `FakeFFmpegProcessRunner` for testing.

```csharp
public FFmpegService(
    IMediaRepository mediaRepository,
    IOperationRepository operationRepository,
    ILogger<FFmpegService> logger,
    IRetryPolicy? retryPolicy = null,
    IFFmpegProcessRunner? processRunner = null)
{
    // ...
    _processRunner = processRunner ?? new FFmpegProcessRunner();
}
```

## Namespace Summary
- **FFmpegDotnetWrapper.Abstraction.IFFmpegProcessRunner**: Simpler abstraction using `CliCommand` (not currently used)
- **FFmpegDotnetWrapper.Services.IFFmpegProcessRunner**: Feature-rich abstraction with detailed request/models (used by FFmpegService)