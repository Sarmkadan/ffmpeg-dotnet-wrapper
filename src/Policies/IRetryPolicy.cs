// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================
// Interface for retry policies that can execute operations with retry logic.
// Supports both synchronous and asynchronous operations with cancellation.
// =============================================================================

using System;
using System.Threading;
using System.Threading.Tasks;

namespace FFmpegDotnetWrapper.Policies;

/// <summary>
/// Represents a retry policy that can execute operations with retry logic.
/// </summary>
public interface IRetryPolicy
{
    /// <summary>
    /// Executes the specified operation with retry logic.
    /// </summary>
    /// <typeparam name="T">The type of result returned by the operation.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    /// <exception cref="Exception">Thrown if the operation fails after all retry attempts.</exception>
    Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the specified operation with retry logic.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the operation.</returns>
    /// <exception cref="Exception">Thrown if the operation fails after all retry attempts.</exception>
    Task ExecuteAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines if an exception should be retried.
    /// </summary>
    /// <param name="exception">The exception to check.</param>
    /// <returns><c>true</c> if the exception should be retried; otherwise <c>false</c>.</returns>
    bool ShouldRetry(Exception exception);
}
