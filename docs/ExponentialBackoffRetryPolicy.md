# ExponentialBackoffRetryPolicy

## Purpose

`ExponentialBackoffRetryPolicy` implements `IRetryPolicy` and runs asynchronous operations with exponential backoff and random jitter. Its default predicate retries selected transient failures, while allowing a caller-supplied predicate to replace that behavior. When every permitted attempt fails with a retryable exception, the policy throws `RetryFailedException` with the last exception as its inner exception.

`RetryFailedException` represents exhaustion of the configured attempts. Its public `Attempts` property exists but is not assigned by its constructor, so it currently has the default value `0`; the generated exception message states the configured attempt count.

## Public API

| Member | Description |
| --- | --- |
| `ExponentialBackoffRetryPolicy(int maxAttempts = 3, int initialDelayMilliseconds = 100, double backoffFactor = 2.0, double jitterFactor = 0.5, Func<Exception, bool>? shouldRetryPredicate = null)` | Creates a policy. `maxAttempts` must be at least 1, the initial delay must be positive, the backoff factor must be greater than 1.0, and the jitter factor must be from 0.0 through 1.0. The optional predicate replaces the default retry classification. |
| `int MaxAttempts { get; }` | Gets the maximum number of total operation attempts. A value of `1` means that no retry follows the initial attempt. |
| `int InitialDelayMilliseconds { get; }` | Gets the base delay before the first retry. |
| `Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default)` | Executes an asynchronous operation and returns its result. Retryable failures are retried until the maximum is reached; exhausted retries produce `RetryFailedException`. |
| `Task ExecuteAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)` | Executes an asynchronous operation without a result, using the same retry behavior. |
| `bool ShouldRetry(Exception exception)` | Evaluates the configured retry predicate for an exception. |
| `protected virtual TimeSpan CalculateDelay(int attemptNumber)` | Calculates a retry delay as `initialDelay * backoffFactor^(attemptNumber - 1)`, plus random jitter from zero up to `jitterFactor * baseDelay`. Derived policies can override it. |
| `protected virtual bool DefaultShouldRetryPredicate(Exception exception)` | Implements the default exception classification. Derived policies can override it. |
| `RetryFailedException(string message, Exception? innerException = null)` | Creates the exception raised after retry attempts are exhausted. |
| `int RetryFailedException.Attempts { get; }` | Gets the attempt count property. The current constructor does not assign it, so its value is `0`. |

## Usage example

```csharp
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FFmpegDotnetWrapper.Policies;

var policy = new ExponentialBackoffRetryPolicy(
    maxAttempts: 4,
    initialDelayMilliseconds: 200,
    backoffFactor: 2.0,
    jitterFactor: 0.25);

var calls = 0;

try
{
    string value = await policy.ExecuteAsync(async cancellationToken =>
    {
        await Task.Delay(10, cancellationToken);
        calls++;

        if (calls < 3)
        {
            throw new IOException("Temporary I/O failure.");
        }

        return "completed";
    }, CancellationToken.None);

    Console.WriteLine(value);
}
catch (RetryFailedException exception)
{
    Console.WriteLine(exception.Message);
}
```

## Non-retryable exit codes

For `ProcessExecutionException`, the default predicate does not retry these exit codes:

| Exit code | Meaning shown in the source |
| ---: | --- |
| `1` | Generic error; may indicate a bad argument. |
| `2` | Generic error. |
| `126` | Command cannot execute. |
| `127` | Command not found. |
| `130` | Process terminated by a signal (`Ctrl+C`). |

Other `ProcessExecutionException` exit codes are retryable under the default predicate. `ConfigurationException`, `ValidationException`, `OperationCanceledException`, `UnauthorizedAccessException`, and unknown exception types are also non-retryable; `TimeoutException` and `IOException` are retryable. An `AggregateException` is classified by recursively checking its inner exception.
