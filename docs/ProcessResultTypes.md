# Process Result Types Documentation

This document describes the three process result types in the FFmpeg.NET Wrapper library and provides guidance on when to use each.

## Overview

The library contains three similarly named result types that serve different purposes:

1. `ProcessResult` in `src/Abstractions/ProcessResult.cs` - Abstract base result type
2. `ProcessResult` nested in `src/Utilities/ProcessUtilities.cs` - Detailed execution result
3. `FFmpegProcessResult` in `src/Services/IFFmpegProcessRunner.cs` - FFmpeg-specific result type

## Type Comparison

| Property | Abstractions.ProcessResult | Utilities.ProcessResult | Services.FFmpegProcessResult |
|----------|----------------------------|-------------------------|------------------------------|
| **Namespace** | `FFmpegDotnetWrapper.Abstraction` | `FFmpegDotnetWrapper.Utilities` | `FFmpegDotnetWrapper.Services` |
| **Exit Code** | ✅ `ExitCode` (int) | ✅ `ExitCode` (int) | ✅ `ExitCode` (int) |
| **Standard Output** | ❌ | ✅ `StandardOutput` (string) | ❌ |
| **Standard Error** | ✅ `StdErrTail` (string, truncated) | ✅ `StandardError` (string) | ✅ `StdErrTail` (string, truncated) |
| **Execution Time** | ❌ | ✅ `ExecutionTime` (TimeSpan) | ✅ `ExecutionTime` (TimeSpan) |
| **Timeout Flag** | ✅ `TimedOut` (bool) | ✅ `TimedOut` (bool) | ✅ `TimedOut` (bool) |
| **Cancellation Flag** | ✅ `WasCancelled` (bool) | ❌ | ✅ `WasCancelled` (bool) |
| **Success Indicator** | ❌ | ✅ `Success` (computed) | ✅ `Success` (computed) |
| **ToString()** | ✅ Culture-invariant format | ✅ Detailed format | ✅ Culture-invariant format |
| **Returned By** | Abstract interfaces | `ProcessUtilities.ExecuteProcess*` | `IFFmpegProcessRunner.RunAsync` |

## Detailed Descriptions

### 1. Abstractions.ProcessResult

**File:** `src/Abstractions/ProcessResult.cs`  
**Namespace:** `FFmpegDotnetWrapper.Abstraction`

A simplified result type used in abstract interfaces and base classes. Contains only the essential information needed for basic process outcome reporting.

**Properties:**
- `ExitCode` (int): The process exit code
- `StdErrTail` (string): Truncated standard error output (max 100 chars)
- `TimedOut` (bool): Whether the process timed out
- `WasCancelled` (bool): Whether the process was cancelled

**Usage:** Used in abstract definitions where minimal process information is sufficient.

### 2. Utilities.ProcessResult

**File:** `src/Utilities/ProcessUtilities.cs` (nested class)  
**Namespace:** `FFmpegDotnetWrapper.Utilities`

A comprehensive result type returned by the low-level process execution utilities. Contains full details about process execution including output streams and timing.

**Properties:**
- `ExitCode` (int): The process exit code (0 = success)
- `StandardOutput` (string): Complete standard output
- `StandardError` (string): Complete standard error output
- `ExecutionTime` (TimeSpan): Total execution time
- `TimedOut` (bool): Whether the process timed out
- `Success` (bool, computed): True if exit code is 0 and not timed out

**Usage:** Returned by `ProcessUtilities.ExecuteProcess()` and `ProcessUtilities.ExecuteProcessAsync()` methods for detailed process execution information.

### 3. Services.FFmpegProcessResult

**File:** `src/Services/IFFmpegProcessRunner.cs`  
**Namespace:** `FFmpegDotnetWrapper.Services`

FFmpeg-specific result type used by the FFmpeg process runner abstraction. Balances detail with FFmpeg-specific concerns like bounded error tails.

**Properties:**
- `ExitCode` (int): Exit code reported by the process (-1 if killed before exiting)
- `StdErrTail` (string): Bounded standard error tail (prevents unbounded memory growth)
- `ExecutionTime` (TimeSpan): Wall-clock time the process ran
- `TimedOut` (bool): True if terminated due to timeout
- `WasCancelled` (bool): True if terminated due to cancellation token
- `Success` (bool, computed): True if exit code is 0, not timed out, and not cancelled

**Usage:** Returned by `IFFmpegProcessRunner.RunAsync()` method for FFmpeg-specific process execution results.

## Guidance on Which to Use

### Use Abstractions.ProcessResult when:
- Defining abstract interfaces or base classes
- Minimal process information is sufficient
- Working with abstraction layers that don't need output details
- Implementing generic process handling logic

### Use Utilities.ProcessResult when:
- You need complete process output (stdout/stderr) for debugging or logging
- Detailed execution timing information is required
- Working directly with process execution utilities
- Building diagnostic or monitoring features that need full output access

### Use Services.FFmpegProcessResult when:
- Working with FFmpeg-specific operations through the IFFmpegProcessRunner interface
- You need FFmpeg-tailored result information
- Building FFmpeg wrapper functionality that requires bounded error tails
- Implementing FFmpeg progress reporting or cancellation handling

## Key Differences

1. **Output Handling:** 
   - Utilities version provides full StandardOutput and StandardError
   - Abstraction and FFmpeg versions provide only StdErrTail (bounded/truncated)

2. **Success Calculation:**
   - Utilities: `Success => ExitCode == 0 && !TimedOut`
   - FFmpeg: `Success => ExitCode == 0 && !TimedOut && !WasCancelled`
   - Abstraction: No success property

3. **Cancellation Tracking:**
   - Abstraction and FFmpeg versions track WasCancelled
   - Utilities version does not have explicit cancellation flag

4. **Memory Considerations:**
   - FFmpegProcessResult uses bounded StdErrTail to prevent memory issues in long-running operations
   - Utilities.ProcessResult can contain unlimited output (use with caution for long processes)

## Migration Notes

When moving between these types:
- From Utilities to Abstraction/FFmpeg: You lose access to StandardOutput and full StandardError
- From Abstraction to Utilities: You gain full output access but lose the abstraction boundary
- From FFmpeg to Utilities: You gain StandardOutput but lose the FFmpeg-specific bounded error tail design

Choose the type that best matches your layer's responsibilities and information needs.