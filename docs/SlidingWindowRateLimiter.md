# SlidingWindowRateLimiter

`IRateLimiter` defines the request-checking and reset contract used by the rate-limiting middleware. `SlidingWindowRateLimiter` is its in-memory, thread-safe implementation: it stores request timestamps for each identifier and policy, removes timestamps older than the configured window, and admits a request only while the number of retained timestamps is below the policy limit.

Policies are represented by [`RateLimitPolicy`](RateLimitPolicy.md). The limiter's state is process-local and is not persisted or shared between application instances.

## Default policies

The `SlidingWindowRateLimiter` constructor requires an `ILogger<SlidingWindowRateLimiter>` and registers these policies:

| Policy name | Maximum requests | Window | Per-user limit |
| --- | ---: | ---: | --- |
| `default` | 100 | 60 seconds | Enabled |
| `transcode` | 5 | 3,600 seconds | Enabled |
| `watermark` | 20 | 3,600 seconds | Enabled |
| `merge` | 10 | 3,600 seconds | Enabled |

`PerUserLimit` is enabled because these registrations use the `RateLimitPolicy` default value. Registering another policy with the same name replaces the existing policy configuration without clearing its recorded request window.

## Public API

### `IRateLimiter` and `SlidingWindowRateLimiter`

| Member | Available on | Description |
| --- | --- | --- |
| `SlidingWindowRateLimiter(ILogger<SlidingWindowRateLimiter> logger)` | Class | Creates the limiter and registers the default policies. Throws `ArgumentNullException` when `logger` is `null`. |
| `void RegisterPolicy(RateLimitPolicy policy)` | Class | Adds or replaces a policy by `PolicyName`. Throws `ArgumentNullException` when `policy` is `null`. |
| `bool AllowRequest(string identifier, string policyName = "default")` | Interface and class | Checks and records a request for an identifier. Returns `false` when the active window is full. Throws `ArgumentException` for an empty identifier. An unknown policy is logged and allowed without recording a request. |
| `bool AllowRequest(string? userId, string? tenantId, string policyName = "default")` | Interface and class | Checks the tenant identifier first, using `"default"` when `tenantId` is `null`. If the policy enables per-user limiting and `userId` is non-empty, it also checks the combined `tenantId:userId` identifier against the derived `policyName:user` policy. An unknown requested policy is allowed. |
| `RateLimitStatus GetStatus(string identifier, string policyName = "default")` | Interface and class | Returns a snapshot after discarding expired timestamps. For an unknown policy, returns a status with `IsAllowed` set to `true` and the other properties at their defaults. This method does not record a request. |
| `void Reset(string identifier, string policyName = "default")` | Interface and class | Removes the recorded window for one identifier and policy. It does not remove or modify the policy. |
| `void ResetAll()` | Interface and class | Removes every recorded request window. Registered policies remain available. |

All mutations of policies and request windows performed by the class are synchronized within the limiter instance.

### `RateLimitStatus`

| Property | Type | Description |
| --- | --- | --- |
| `IsAllowed` | `bool` | Whether another request is currently below the configured maximum. |
| `RequestsMade` | `int` | Number of unexpired requests currently recorded. |
| `MaxRequests` | `int` | Maximum requests configured for the policy. |
| `RemainingRequests` | `int` | Computed as `MaxRequests - RequestsMade`, with a minimum of zero. |
| `ResetTime` | `DateTime` | UTC time when the oldest recorded request leaves the window; if no requests exist, it is the time the status was calculated. |
| `SecondsUntilReset` | `double` | Remaining seconds until `ResetTime`, computed against `DateTime.UtcNow` with a minimum of zero. |

## Usage

```csharp
using FFmpegDotnetWrapper.Middleware;
using Microsoft.Extensions.Logging;

using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
    builder.AddConsole());

var limiter = new SlidingWindowRateLimiter(
    loggerFactory.CreateLogger<SlidingWindowRateLimiter>());

limiter.RegisterPolicy(new RateLimitPolicy
{
    PolicyName = "preview",
    MaxRequests = 10,
    WindowSeconds = 60,
    PerUserLimit = false
});

const string clientId = "client-42";

if (limiter.AllowRequest(clientId, "preview"))
{
    // Start the preview operation.
}
else
{
    RateLimitStatus status = limiter.GetStatus(clientId, "preview");
    Console.WriteLine($"Try again in {status.SecondsUntilReset:F0} seconds.");
}
```
