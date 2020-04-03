# RateLimitPolicyExtensions

The `RateLimitPolicyExtensions` static class provides a set of extension properties that expose the current state of a rate‑limit policy applied to an `ffmpeg-dotnet-wrapper` client. These members allow callers to inspect the policy’s real‑time metrics—such as the current request rate, capacity status, window progress, and time until the next reset—without modifying the policy itself. They are intended for monitoring, logging, and conditional request throttling in applications that use the wrapper’s built‑in rate limiting.

## API

### `GetCurrentRequestsPerSecond`

```csharp
public static double GetCurrentRequestsPerSecond { get; }
```

- **Purpose**: Returns the number of requests that have been counted in the current sliding window, expressed as a per‑second rate.
- **Parameters**: None (static property).
- **Return value**: A `double` representing the current request rate. A value of `0.0` indicates no requests have been recorded in the current window.
- **Throws**: This property does not throw. It returns `0.0` if no rate‑limit policy is configured or if the underlying policy has not yet recorded any requests.

### `IsAtCapacity`

```csharp
public static bool IsAtCapacity { get; }
```

- **Purpose**: Indicates whether the rate‑limit policy has reached its maximum allowed request count for the current window.
- **Parameters**: None (static property).
- **Return value**: `true` if the policy is at capacity (i.e., further requests would be denied or queued); otherwise `false`.
- **Throws**: This property does not throw. It returns `false` if no rate‑limit policy is configured.

### `GetWindowProgress`

```csharp
public static double GetWindowProgress { get; }
```

- **Purpose**: Returns the progress of the current rate‑limit window as a fraction between `0.0` and `1.0`.
- **Parameters**: None (static property).
- **Return value**: A `double` in the range `[0.0, 1.0]`. A value of `0.0` means the window has just started; `1.0` means the window is about to reset.
- **Throws**: This property does not throw. It returns `0.0` if no rate‑limit policy is configured.

### `GetTimeUntilReset`

```csharp
public static TimeSpan GetTimeUntilReset { get; }
```

- **Purpose**: Returns the amount of time remaining before the current rate‑limit window resets.
- **Parameters**: None (static property).
- **Return value**: A `TimeSpan` representing the time until the next window reset. A zero `TimeSpan` indicates the window has already expired or no policy is configured.
- **Throws**: This property does not throw. It returns `TimeSpan.Zero` if no rate‑limit policy is configured.

## Usage

### Example 1: Conditional request execution

```csharp
using FFmpeg.Wrapper;

// Before making an FFmpeg operation, check if the rate limit is near capacity.
if (!RateLimitPolicyExtensions.IsAtCapacity)
{
    // Proceed with the operation.
    FFmpegProcess.Start("input.mp4", "output.mp4");
}
else
{
    // Log or delay until the window resets.
    TimeSpan wait = RateLimitPolicyExtensions.GetTimeUntilReset;
    Console.WriteLine($"Rate limit reached. Waiting {wait.TotalSeconds:F1}s...");
    await Task.Delay(wait);
}
```

### Example 2: Monitoring request rate and window progress

```csharp
using FFmpeg.Wrapper;

// Periodically log the current request rate and window progress.
double rate = RateLimitPolicyExtensions.GetCurrentRequestsPerSecond;
double progress = RateLimitPolicyExtensions.GetWindowProgress;

Console.WriteLine($"Current request rate: {rate:F2} req/s");
Console.WriteLine($"Window progress: {progress:P1}");

if (progress > 0.8)
{
    Console.WriteLine("Approaching window reset – consider throttling.");
}
```

## Notes

- All members are static and read‑only. They reflect the state of the rate‑limit policy that was configured when the wrapper client was created. If no policy was set, the properties return sensible defaults (`0.0`, `false`, `0.0`, `TimeSpan.Zero`).
- The values are computed from the underlying policy implementation and are updated on each request. They are safe to read from multiple threads concurrently; however, the returned values represent a snapshot in time and may change immediately after being read.
- `GetWindowProgress` and `GetTimeUntilReset` are meaningful only when a sliding‑window or fixed‑window policy is active. For token‑bucket or leaky‑bucket policies, `GetWindowProgress` may always return `0.0` and `GetTimeUntilReset` may return `TimeSpan.Zero`.
- Because these properties are static, they apply to the default rate‑limit policy used by the wrapper. If your application uses multiple clients with different policies, consider accessing the policy instance directly rather than relying on these static extensions.
