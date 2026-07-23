// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================
// Implementation of IRetryPolicy with exponential backoff strategy.
// Handles transient failures with configurable retry parameters.
// =====================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using FFmpegDotnetWrapper.Exceptions;

namespace FFmpegDotnetWrapper.Policies;

/// <summary>
/// Retry policy that implements exponential backoff with jitter.
/// Retries transient failures (network issues, timeouts, process crashes) but not
/// permanent failures (bad arguments, configuration errors, validation errors).
/// </summary>
public class ExponentialBackoffRetryPolicy : IRetryPolicy
{
    private readonly int _maxAttempts;
    private readonly int _initialDelayMilliseconds;
    private readonly double _backoffFactor;
    private readonly double _jitterFactor;
    private readonly Func<Exception, bool> _shouldRetryPredicate;

    /// <summary>
    /// Creates a new instance of ExponentialBackoffRetryPolicy.
    /// </summary>
    /// <param name="maxAttempts">Maximum number of retry attempts (1 = no retry).</param>
    /// <param name="initialDelayMilliseconds">Initial delay in milliseconds before first retry.</param>
    /// <param name="backoffFactor">Multiplier for delay between retries (e.g., 2.0 for exponential).</param>
    /// <param name="jitterFactor">Random factor to add jitter to delays (0.0-1.0).</param>
    /// <param name="shouldRetryPredicate">Optional predicate to determine if an exception should be retried.
    /// If null, uses default logic for transient failures.</param>
    public ExponentialBackoffRetryPolicy(
        int maxAttempts = 3,
        int initialDelayMilliseconds = 100,
        double backoffFactor = 2.0,
        double jitterFactor = 0.5,
        Func<Exception, bool>? shouldRetryPredicate = null)
    {
        if (maxAttempts < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxAttempts),
                "Max attempts must be at least 1");
        }

        if (initialDelayMilliseconds <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(initialDelayMilliseconds),
                "Initial delay must be positive");
        }

        if (backoffFactor <= 1.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(backoffFactor),
                "Backoff factor must be greater than 1.0");
        }

        if (jitterFactor < 0.0 || jitterFactor > 1.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(jitterFactor),
                "Jitter factor must be between 0.0 and 1.0");
        }

        _maxAttempts = maxAttempts;
        _initialDelayMilliseconds = initialDelayMilliseconds;
        _backoffFactor = backoffFactor;
        _jitterFactor = jitterFactor;
        _shouldRetryPredicate = shouldRetryPredicate ?? DefaultShouldRetryPredicate;
    }

    /// <summary>
    /// Gets the maximum number of retry attempts.
    /// </summary>
    public int MaxAttempts => _maxAttempts;

    /// <summary>
    /// Gets the initial delay in milliseconds.
    /// </summary>
    public int InitialDelayMilliseconds => _initialDelayMilliseconds;

    /// <summary>
    /// Executes the specified operation with retry logic.
    /// </summary>
    /// <typeparam name="T">The type of result returned by the operation.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        var attempts = 0;
        Exception? lastException = null;

        while (attempts < _maxAttempts)
        {
            attempts++;

            try
            {
                return await operation(cancellationToken);
            }
            catch (Exception ex) when (ShouldRetry(ex))
            {
                lastException = ex;

                // Don't retry on first attempt
                if (attempts >= _maxAttempts)
                {
                    break;
                }

                var delay = CalculateDelay(attempts);

                try
                {
                    await Task.Delay(delay, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // Re-throw cancellation if it occurred during delay
                    throw;
                }
            }
        }

        // All attempts failed - throw the last exception
        throw new RetryFailedException(
            $"Operation failed after {_maxAttempts} attempt(s). Last error: {lastException?.Message}",
            lastException);
    }

    /// <summary>
    /// Executes the specified operation with retry logic.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the operation.</returns>
    public async Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        var attempts = 0;
        Exception? lastException = null;

        while (attempts < _maxAttempts)
        {
            attempts++;

            try
            {
                await operation(cancellationToken);
                return; // Success - exit the retry loop
            }
            catch (Exception ex) when (ShouldRetry(ex))
            {
                lastException = ex;

                // Don't retry on first attempt
                if (attempts >= _maxAttempts)
                {
                    break;
                }

                var delay = CalculateDelay(attempts);

                try
                {
                    await Task.Delay(delay, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // Re-throw cancellation if it occurred during delay
                    throw;
                }
            }
        }

        // All attempts failed - throw the last exception
        throw new RetryFailedException(
            $"Operation failed after {_maxAttempts} attempt(s). Last error: {lastException?.Message}",
            lastException);
    }

    /// <summary>
    /// Determines if an exception should be retried.
    /// </summary>
    /// <param name="exception">The exception to check.</param>
    /// <returns><c>true</c> if the exception should be retried; otherwise <c>false</c>.</returns>
    public bool ShouldRetry(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return _shouldRetryPredicate(exception);
    }

    /// <summary>
    /// Calculates the delay before the next retry attempt.
    /// Uses exponential backoff with jitter to prevent thundering herd problems.
    /// </summary>
    /// <param name="attemptNumber">The current attempt number (1-based).</param>
    /// <returns>The delay before the next retry.</returns>
    protected virtual TimeSpan CalculateDelay(int attemptNumber)
    {
        // Base delay: initial * factor^(attempt-1)
        var baseDelayMs = _initialDelayMilliseconds * Math.Pow(_backoffFactor, attemptNumber - 1);

        // Add jitter: random factor between 0 and jitterFactor * baseDelay
        var jitterRange = _jitterFactor * baseDelayMs;
        var jitterMs = Random.Shared.NextDouble() * jitterRange;

        var totalDelayMs = baseDelayMs + jitterMs;

        return TimeSpan.FromMilliseconds(totalDelayMs);
    }

    /// <summary>
    /// Default predicate for determining if an exception should be retried.
    /// Retries transient failures but not permanent configuration or validation errors.
    /// </summary>
    protected virtual bool DefaultShouldRetryPredicate(Exception exception)
    {
        // Don't retry ConfigurationException - these are permanent setup issues
        if (exception is ConfigurationException)
        {
            return false;
        }

        // Don't retry ValidationException - these are permanent input validation issues
        if (exception is ValidationException)
        {
            return false;
        }

        // Retry ProcessExecutionException with non-zero exit codes that indicate transient issues
        if (exception is ProcessExecutionException processEx)
        {
            // Retry only if exit code suggests transient failure (not bad arguments)
            // Common transient exit codes: network issues, resource exhaustion, crashes
            // Bad argument exit codes: typically 1 or other specific codes
            return processEx.ExitCode switch
            {
                // Retry these common transient failures
                1 => false, // Generic error - check if it's a bad argument
                2 => false, // Generic error
                126 => false, // Command cannot execute
                127 => false, // Command not found
                130 => false, // Process terminated by signal (Ctrl+C)
                _ => true // Retry other exit codes (transient issues)
            };
        }

        // Retry standard transient exceptions
        return exception switch
        {
            TimeoutException => true,
            OperationCanceledException => false, // Cancellation is not a retryable failure
            IOException => true, // IO errors are often transient
            UnauthorizedAccessException => false, // Permission issues are not transient
            AggregateException aggEx => ShouldRetry(aggEx.InnerException),
            _ => false // Default: don't retry unknown exceptions
        };
    }
}

/// <summary>
/// Exception thrown when an operation fails after all retry attempts.
/// </summary>
public class RetryFailedException : Exception
{
    /// <summary>
    /// Gets the number of attempts made before failure.
    /// </summary>
    public int Attempts { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RetryFailedException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public RetryFailedException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
        // Attempts can be inferred from message
    }
}