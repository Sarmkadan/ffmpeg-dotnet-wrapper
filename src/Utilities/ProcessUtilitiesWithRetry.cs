// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================
// Process execution utilities with retry policy support for FFmpeg operations.
// Provides retry logic for transient process execution failures.
// =====================================================================

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FFmpegDotnetWrapper.Policies;

namespace FFmpegDotnetWrapper.Utilities;

/// <summary>
/// Process execution utilities with retry policy support.
/// Extends ProcessUtilities with retry logic for transient failures.
/// </summary>
public static class ProcessUtilitiesWithRetry
{
    /// <summary>
    /// Executes a process with retry policy support.
    /// </summary>
    /// <param name="fileName">The executable to run.</param>
    /// <param name="arguments">Command-line arguments.</param>
    /// <param name="retryPolicy">Retry policy to use (null for no retry).</param>
    /// <param name="workingDirectory">Working directory (optional).</param>
    /// <param name="timeout">Process timeout (optional).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Process execution result with retry support.</returns>
    public static async Task<ProcessUtilities.ProcessResult> ExecuteProcessWithRetryAsync(
        string fileName,
        string arguments,
        IRetryPolicy? retryPolicy = null,
        string? workingDirectory = null,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        if (retryPolicy == null)
        {
            // No retry policy - execute directly
            return await ProcessUtilities.ExecuteProcessAsync(
                fileName, arguments, workingDirectory, timeout, cancellationToken);
        }

        return await retryPolicy.ExecuteAsync(async ct =>
        {
            return await ProcessUtilities.ExecuteProcessAsync(
                fileName, arguments, workingDirectory, timeout, ct);
        }, cancellationToken);
    }

    /// <summary>
    /// Executes a process with retry policy support and returns typed result.
    /// </summary>
    /// <typeparam name="T">Result type.</typeparam>
    /// <param name="operation">Process execution operation.</param>
    /// <param name="retryPolicy">Retry policy to use (null for no retry).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result from the operation.</returns>
    public static async Task<T> ExecuteWithRetryAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        IRetryPolicy? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        if (retryPolicy == null)
        {
            return await operation(cancellationToken);
        }

        return await retryPolicy.ExecuteAsync(operation, cancellationToken);
    }

    /// <summary>
    /// Executes a process with retry policy support.
    /// </summary>
    /// <param name="operation">Process execution operation.</param>
    /// <param name="retryPolicy">Retry policy to use (null for no retry).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the operation.</returns>
    public static async Task ExecuteWithRetryAsync(
        Func<CancellationToken, Task> operation,
        IRetryPolicy? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        if (retryPolicy == null)
        {
            await operation(cancellationToken);
            return;
        }

        await retryPolicy.ExecuteAsync(operation, cancellationToken);
    }
}