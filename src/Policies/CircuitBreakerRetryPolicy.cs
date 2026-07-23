// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================
// Circuit breaker pattern implementation that works with IRetryPolicy.
// Opens circuit after N consecutive failures, closes after cooldown period.
// =====================================================================

using System;
using System.Threading;
using System.Threading.Tasks;

namespace FFmpegDotnetWrapper.Policies;

/// <summary>
/// Circuit breaker decorator for retry policies that opens after consecutive failures
/// and enters a half-open state after a cooldown period to test if the issue is resolved.
/// </summary>
public class CircuitBreakerRetryPolicy : IRetryPolicy
{
    private readonly IRetryPolicy _innerPolicy;
    private readonly int _failureThreshold;
    private readonly TimeSpan _breakDuration;
    private readonly int _halfOpenAttempts;

    private int _failureCount;
    private int _successCount;
    private CircuitState _state = CircuitState.Closed;
    private DateTime _lastFailureTime = DateTime.MinValue;
    private readonly object _stateLock = new object();

    /// <summary>
    /// Creates a new circuit breaker retry policy.
    /// </summary>
    /// <param name="innerPolicy">The underlying retry policy to use when circuit is closed.</param>
    /// <param name="failureThreshold">Number of consecutive failures before opening the circuit.</param>
    /// <param name="breakDuration">Duration to wait before attempting to close the circuit (half-open state).</param>
    /// <param name="halfOpenAttempts">Number of attempts to make in half-open state before deciding circuit state.</param>
    public CircuitBreakerRetryPolicy(
        IRetryPolicy innerPolicy,
        int failureThreshold = 5,
        TimeSpan? breakDuration = null,
        int halfOpenAttempts = 2)
    {
        ArgumentNullException.ThrowIfNull(innerPolicy);

        if (failureThreshold < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(failureThreshold),
                "Failure threshold must be at least 1");
        }

        if (halfOpenAttempts < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(halfOpenAttempts),
                "Half-open attempts must be at least 1");
        }

        _innerPolicy = innerPolicy;
        _failureThreshold = failureThreshold;
        _breakDuration = breakDuration ?? TimeSpan.FromSeconds(30);
        _halfOpenAttempts = halfOpenAttempts;
    }

    /// <summary>
    /// Gets the current circuit state.
    /// </summary>
    public CircuitState State
    {
        get
        {
            lock (_stateLock)
            {
                return _state;
            }
        }
    }

    /// <summary>
    /// Gets the number of consecutive failures.
    /// </summary>
    public int FailureCount
    {
        get
        {
            lock (_stateLock)
            {
                return _failureCount;
            }
        }
    }

    /// <summary>
    /// Executes the specified operation with circuit breaker and retry logic.
    /// </summary>
    /// <typeparam name="T">The type of result returned by the operation.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    /// <exception cref="CircuitBreakerOpenException">Thrown when circuit is open.</exception>
    /// <exception cref="RetryFailedException">Thrown when operation fails after all retry attempts.</exception>
    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        // Check circuit state before attempting execution
        var circuitState = CheckCircuitState();

        if (circuitState == CircuitState.Open)
        {
            throw new CircuitBreakerOpenException(
                $"Circuit breaker is open. Circuit will be half-open at {_lastFailureTime + _breakDuration:O}");
        }

        try
        {
            var result = await _innerPolicy.ExecuteAsync(operation, cancellationToken);

            // Success - reset failure count
            lock (_stateLock)
            {
                _failureCount = 0;
                _successCount++;
            }

            return result;
        }
        catch (Exception ex)
        {
            HandleFailure(ex);
            throw;
        }
    }

    /// <summary>
    /// Executes the specified operation with circuit breaker and retry logic.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the operation.</returns>
    /// <exception cref="CircuitBreakerOpenException">Thrown when circuit is open.</exception>
    /// <exception cref="RetryFailedException">Thrown when operation fails after all retry attempts.</exception>
    public async Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        // Check circuit state before attempting execution
        var circuitState = CheckCircuitState();

        if (circuitState == CircuitState.Open)
        {
            throw new CircuitBreakerOpenException(
                $"Circuit breaker is open. Circuit will be half-open at {_lastFailureTime + _breakDuration:O}");
        }

        try
        {
            await _innerPolicy.ExecuteAsync(operation, cancellationToken);

            // Success - reset failure count
            lock (_stateLock)
            {
                _failureCount = 0;
                _successCount++;
            }
        }
        catch (Exception ex)
        {
            HandleFailure(ex);
            throw;
        }
    }

    /// <summary>
    /// Determines if an exception should be retried (delegates to inner policy).
    /// </summary>
    /// <param name="exception">The exception to check.</param>
    /// <returns><c>true</c> if the exception should be retried; otherwise <c>false</c>.</returns>
    public bool ShouldRetry(Exception exception)
    {
        return _innerPolicy.ShouldRetry(exception);
    }

    private CircuitState CheckCircuitState()
    {
        lock (_stateLock)
        {
            if (_state == CircuitState.HalfOpen)
            {
                // In half-open state, allow limited attempts
                return CircuitState.HalfOpen;
            }

            if (_state == CircuitState.Open && DateTime.UtcNow >= _lastFailureTime + _breakDuration)
            {
                // Time to try closing the circuit
                _state = CircuitState.HalfOpen;
                _failureCount = 0;
                _successCount = 0;
                return CircuitState.HalfOpen;
            }

            return _state;
        }
    }

    private void HandleFailure(Exception exception)
    {
        lock (_stateLock)
        {
            _failureCount++;
            _lastFailureTime = DateTime.UtcNow;

            // Check if we should open the circuit
            if (_state == CircuitState.Closed && _failureCount >= _failureThreshold)
            {
                _state = CircuitState.Open;
            }
            else if (_state == CircuitState.HalfOpen)
            {
                // In half-open state, failure opens the circuit again
                if (_failureCount >= _halfOpenAttempts)
                {
                    _state = CircuitState.Open;
                }
            }
        }
    }

    /// <summary>
    /// Resets the circuit breaker to the closed state.
    /// Useful for testing or after manual intervention to fix underlying issues.
    /// </summary>
    public void Reset()
    {
        lock (_stateLock)
        {
            _state = CircuitState.Closed;
            _failureCount = 0;
            _successCount = 0;
            _lastFailureTime = DateTime.MinValue;
        }
    }
}

/// <summary>
/// Exception thrown when the circuit breaker is in the Open state.
/// </summary>
public class CircuitBreakerOpenException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CircuitBreakerOpenException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public CircuitBreakerOpenException(string message)
        : base(message)
    {
    }
}

/// <summary>
/// Represents the state of a circuit breaker.
/// </summary>
public enum CircuitState
{
    /// <summary>
    /// The circuit is closed and operations are allowed.
    /// </summary>
    Closed,

    /// <summary>
    /// The circuit is open and operations are blocked.
    /// </summary>
    Open,

    /// <summary>
    /// The circuit is half-open, allowing limited operations to test if the issue is resolved.
    /// </summary>
    HalfOpen
}
