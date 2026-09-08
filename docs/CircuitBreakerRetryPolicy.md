# CircuitBreakerRetryPolicy

## Purpose

`CircuitBreakerRetryPolicy` is an `IRetryPolicy` decorator that combines an existing retry policy with circuit-breaker state. It delegates permitted operations and retry classification to the inner policy, counts failures reported after the inner policy finishes, and blocks execution while the circuit is open.

`IRetryPolicy` defines the asynchronous execution and exception-classification contract used by this decorator. `CircuitState` describes the breaker state, and `CircuitBreakerOpenException` reports an execution rejected by an open circuit.

## Public API

| Type | Member | Description |
| --- | --- | --- |
| `IRetryPolicy` | `Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default)` | Executes an asynchronous operation with a result, using the policy's retry behavior. |
| `IRetryPolicy` | `Task ExecuteAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)` | Executes an asynchronous operation without a result, using the policy's retry behavior. |
| `IRetryPolicy` | `bool ShouldRetry(Exception exception)` | Determines whether an exception should be retried. |
| `CircuitBreakerRetryPolicy` | `CircuitBreakerRetryPolicy(IRetryPolicy innerPolicy, int failureThreshold = 5, TimeSpan? breakDuration = null, int halfOpenAttempts = 2)` | Creates a decorator around `innerPolicy`. The default break duration is 30 seconds. Both integer thresholds must be at least 1. |
| `CircuitBreakerRetryPolicy` | `CircuitState State { get; }` | Gets the current stored state under a lock. Reading this property does not advance an expired open circuit to half-open; that check occurs when an operation is executed. |
| `CircuitBreakerRetryPolicy` | `int FailureCount { get; }` | Gets the current failure count under a lock. A successful delegated execution resets it to zero. |
| `CircuitBreakerRetryPolicy` | `Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default)` | Checks the circuit, delegates an allowed operation to the inner policy, and returns its result. A null operation is rejected. |
| `CircuitBreakerRetryPolicy` | `Task ExecuteAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)` | Checks the circuit and delegates an allowed operation without a result to the inner policy. A null operation is rejected. |
| `CircuitBreakerRetryPolicy` | `bool ShouldRetry(Exception exception)` | Returns the result of the inner policy's `ShouldRetry` method. |
| `CircuitBreakerRetryPolicy` | `void Reset()` | Sets the state to `Closed`, clears the failure and success counters, and clears the recorded last-failure time. |
| `CircuitBreakerOpenException` | `CircuitBreakerOpenException(string message)` | Creates the exception thrown when execution is rejected because the circuit remains open. |
| `CircuitState` | `Closed` | Operations are allowed. This is the initial state. |
| `CircuitState` | `Open` | Operations are blocked until the break duration has elapsed. |
| `CircuitState` | `HalfOpen` | Operations are allowed to test whether the underlying issue has resolved. |

## Usage example

```csharp
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FFmpegDotnetWrapper.Policies;

IRetryPolicy innerPolicy = new ExponentialBackoffRetryPolicy(maxAttempts: 2);
var policy = new CircuitBreakerRetryPolicy(
    innerPolicy,
    failureThreshold: 3,
    breakDuration: TimeSpan.FromSeconds(20),
    halfOpenAttempts: 1);

try
{
    string output = await policy.ExecuteAsync(async cancellationToken =>
    {
        await Task.Delay(10, cancellationToken);
        return await File.ReadAllTextAsync("result.txt", cancellationToken);
    }, CancellationToken.None);

    Console.WriteLine(output);
}
catch (CircuitBreakerOpenException exception)
{
    Console.WriteLine(exception.Message);
}
catch (RetryFailedException exception)
{
    // This exception can be produced by the chosen inner policy.
    Console.WriteLine(exception.Message);
}
```

## State transitions and exceptions

- The breaker starts in `Closed`. Each exception escaping the inner policy increments the failure count and records the current UTC time. Once the count reaches `failureThreshold`, the state becomes `Open`.
- A successful delegated execution resets the failure count to zero and increments an internal success counter. Consequently, opening from `Closed` requires consecutive failed executions as observed by this decorator.
- While `Open`, an execution first compares the current UTC time with the last failure time plus `breakDuration`. Before that time it throws `CircuitBreakerOpenException` without invoking the inner policy; the message includes the calculated half-open time.
- Once the duration has elapsed, an execution changes the state from `Open` to `HalfOpen`, clears both counters, and is delegated to the inner policy.
- In `HalfOpen`, failures increment the failure count. The state returns to `Open` when that count reaches `halfOpenAttempts`. Successful executions reset the failure count and increment the success counter.
- As currently implemented, success in `HalfOpen` does **not** change the state to `Closed`, and there is no separate cap on the number of operations admitted while half-open. `Reset()` is the only code path that changes `HalfOpen` to `Closed`.
- The decorator catches every `Exception` from the inner policy, including cancellation-related exceptions, records it as a breaker failure, and rethrows the same exception. Exceptions from the operation may already have been transformed by the inner policy (for example, `ExponentialBackoffRetryPolicy` can throw `RetryFailedException`).
- Construction throws `ArgumentNullException` for a null inner policy and `ArgumentOutOfRangeException` when either integer threshold is less than 1. Both execution overloads throw `ArgumentNullException` for a null operation. The constructor does not validate `breakDuration`.
