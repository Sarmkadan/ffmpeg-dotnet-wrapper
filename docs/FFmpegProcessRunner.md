# FFmpegProcessRunner Documentation

## Purpose
The `FFmpegProcessRunner` module provides a robust abstraction for executing the `ffmpeg` executable as an external process. It centralizes process lifecycle management, bounded standard error capture, graceful shutdown on cancellation, and parsing of `ffmpeg`'s `-progress pipe:1` output for real-time progress reporting. 

The `IFFmpegProcessRunner` interface introduces a clean seam for dependency injection, allowing callers to substitute a fake implementation (`FakeFFmpegProcessRunner`) in unit tests. This enables verification of library behavior (argument construction, result mapping, cancellation handling) without requiring the `ffmpeg` binary to be installed on the test machine.

## API Reference

| Type | Description |
|------|-------------|
| `IFFmpegProcessRunner` | Interface defining the contract for running `ffmpeg` processes asynchronously. |
| `FFmpegProcessRequest` | Configuration object describing a single `ffmpeg` invocation (executable path, arguments, timeout, progress options). |
| `FFmpegProcessResult` | Outcome object containing the exit code, bounded stderr tail, execution time, and cancellation/timeout status. |
| `FFmpegProcessRunner` | Production implementation of `IFFmpegProcessRunner` that spawns real `ffmpeg` processes via `System.Diagnostics.Process`. |
| `FakeFFmpegProcessRunner` | In-memory test double for `IFFmpegProcessRunner` that simulates process execution and progress reporting. |

### FFmpegProcessRequest
| Property | Type | Description |
|----------|------|-------------|
| `FileName` | `string` | Full path or bare name of the `ffmpeg` executable. |
| `Arguments` | `string` | Fully built command-line argument string. |
| `WorkingDirectory` | `string?` | Working directory for the process. Defaults to current directory if null. |
| `Timeout` | `TimeSpan?` | Maximum execution time before the process is treated as hung. |
| `OperationId` | `string` | Identifier propagated to progress updates. |
| `TotalDuration` | `TimeSpan` | Total media duration for calculating completion percentage. |
| `ParseProgressFromStdOut` | `bool` | When `true`, enables parsing of `-progress pipe:1` output. |

### FFmpegProcessResult
| Property | Type | Description |
|----------|------|-------------|
| `ExitCode` | `int` | Process exit code, or `-1` if killed before natural exit. |
| `StdErrTail` | `string` | Tail of standard error captured during execution. |
| `ExecutionTime` | `TimeSpan` | Wall-clock time from start to exit/termination. |
| `TimedOut` | `bool` | `true` if terminated due to `Timeout` elapsing. |
| `WasCancelled` | `bool` | `true` if terminated due to `CancellationToken` signaling. |
| `Success` | `bool` | Computed property: `ExitCode == 0 && !TimedOut && !WasCancelled`. |

## Stderr Retention Limit
To prevent memory exhaustion during long-running or error-prone operations, the runner automatically bounds the captured standard error output. Only the **last 64 KB (65,536 characters)** of stderr are retained in `FFmpegProcessResult.StdErrTail`. Older output is automatically discarded as new lines are read.

## Usage Example

### Production Usage
