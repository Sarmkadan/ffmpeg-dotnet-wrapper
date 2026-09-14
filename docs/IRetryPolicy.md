# IRetryPolicy

## Purpose

Defines the contract for retry policies that can execute operations with retry logic. Supports both synchronous and asynchronous operations with cancellation tokens. Implementations determine when to retry based on exception types and control retry timing.

## Members

| Member | Description |
| --- | --- |
| `Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default)` | Executes the specified asynchronous operation that returns a result, applying retry logic. Returns the operation result on success. Throws an exception if the operation fails after all retry attempts. |
| `Task ExecuteAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)` | Executes the specified asynchronous operation that does not return a result, applying retry logic. Completes when the operation succeeds. Throws an exception if the operation fails after all retry attempts. |
| `bool ShouldRetry(Exception exception)` | Determines whether the specified exception should trigger a retry. Implementations should return `true` for exceptions that are considered transient or recoverable, and `false` for fatal errors. |

## Contract Expectations

Implementations of `IRetryPolicy` must adhere to the following contracts:

1. **Thread Safety**: Unless otherwise documented, implementations should be safe for concurrent use by multiple threads.
2. **Exception Handling**: The `ExecuteAsync` methods should catch all exceptions thrown by the operation, pass them to `ShouldRetry`, and retry if it returns `true`. If `ShouldRetry` returns `false` or the maximum retry attempts are exhausted, the last exception is propagated.
3. **Cancellation**: The provided `cancellationToken` should be respected. If cancellation is requested, the operation should be cancelled and no further retries attempted.
4. **No Side Effects on Success**: On successful execution, the implementation should not modify the operation's result or state beyond executing it.
5. **Deterministic Retry Logic**: Given the same sequence of exceptions, the policy should make the same retry decisions.

## Implementations

The following policies implement `IRetryPolicy`:

- [ExponentialBackoffRetryPolicy](ExponentialBackoffRetryPolicy.md): Retries operations with exponentially increasing delays between attempts, optionally with jitter.
- [CircuitBreakerRetryPolicy](CircuitBreakerRetryPolicy.md): Combines retry logic with a circuit breaker that temporarily stops attempts when failures exceed a threshold.

## Custom Implementation Example usage of selecting a policy:
 ```csharp
 IRetryPolicy policy = new ExponentialBackoffRetryPolicy(maxAttempts: 5);
 // or
 IRetryPolicy policy = new CircuitBreakerRetryPolicy(failureThreshold: 3, retryTimeoutSeconds: 30);
 ```

## Custom Implementation Example

To create a custom retry policy, implement the `IRetryPolicy` interface. Below is an example of a fixed-interval retry policy:

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using FFmpegDotnetWrapper.Policies;

/// <summary>
/// Retries operations with a fixed delay between attempts.
/// </summary>
public class FixedIntervalRetryPolicy : IRetryPolicy
{
    private readonly int _maxAttempts;
    private readonly TimeSpan _delay;
    private readonly Func<Exception, bool> _shouldRetry;

    /// <summary>
    /// Initializes a new instance of the <see cref="FixedIntervalRetryPolicy"/> class.
    /// </summary>
    /// <param name="maxAttempts">Maximum number of execution attempts. Must be at least 1.</param>
    /// <param name="delay">Fixed delay between retry attempts.</param>
    /// <param name="shouldRetry">Optional predicate to determine if an exception should be retried. If null, all exceptions are retried.</param>
    public FixedIntervalRetryPolicy(int maxAttempts = 3, TimeSpan delay = default, Func<Exception, bool>? shouldRetry = null)
    {
        if (maxAttempts < 1)
            throw new ArgumentOutOfRangeException(nameof(maxAttempts), "Maximum attempts must be at least 1.");
        if (delay < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(delay), "Delay cannot be negative.");

        _maxAttempts = maxAttempts;
        _delay = delay;
        _shouldRetry = shouldRetry ?? (_ => true);
    }

    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default)
    {
        int attempt = 0;
        while (true)
        {
            attempt++;
            try
            {
                return await operation(cancellationToken);
            }
            catch (Exception ex) when (attempt < _maxAttempts && _shouldRetry(ex))
            {
                if (_delay > TimeSpan.Zero)
                    await Task.Delay(_delay, cancellationToken);
            }
        }
    }

    public async Task ExecuteAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)
    {
        int attempt = 0;
        while (true)
        {
            attempt++;
            try
            {
                await operation(cancellationToken);
                return;
            }
            catch (Exception ex) when (attempt < _maxAttempts && _shouldRetry(ex))
            {
                if (_delay > TimeSpan.Zero)
                    await Task.Delay(_delay, cancellationToken);
            }
        }
    }

    public bool ShouldRetry(Exception exception)
    {
        return _shouldRetry(exception);
    }
}
```

Usage example:
```csharp
var policy = new FixedIntervalRetryPolicy(
    maxAttempts: 4,
    delay: TimeSpan.FromSeconds(2),
    shouldRetry: ex => ex is IOException || ex is TimeoutException);

string result = await policy.ExecuteAsync(async ct =>
{
    // Operation that might fail transiently
    await SomeUnreliableOperationAsync(ct);
    return "Success";
});
```