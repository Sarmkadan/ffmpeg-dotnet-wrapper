using System;
using System.Linq;

public static class RateLimitPolicyExtensions
{
    /// <summary>
    /// Gets the current request rate as requests per second within the current window.
    /// </summary>
    /// <param name="policy">The rate limit policy instance.</param>
    /// <returns>Current requests per second rate, or 0 if no requests have been made.</returns>
    public static double GetCurrentRequestsPerSecond(this RateLimitPolicy policy)
    {
        if (policy.Timestamps.Count == 0)
        {
            return 0;
        }

        var windowDuration = DateTime.UtcNow - policy.WindowStart;
        var seconds = windowDuration.TotalSeconds;

        if (seconds <= 0)
        {
            return policy.RequestsMade;
        }

        return policy.RequestsMade / seconds;
    }

    /// <summary>
    /// Checks if the rate limit policy has reached its maximum capacity.
    /// </summary>
    /// <param name="policy">The rate limit policy instance.</param>
    /// <returns>True if the policy is at or over capacity; otherwise false.</returns>
    public static bool IsAtCapacity(this RateLimitPolicy policy)
    {
        return policy.RequestsMade >= policy.MaxRequests;
    }

    /// <summary>
    /// Gets the percentage of the rate limit window that has elapsed.
    /// </summary>
    /// <param name="policy">The rate limit policy instance.</param>
    /// <returns>Percentage (0-100) of the window that has elapsed.</returns>
    public static double GetWindowProgress(this RateLimitPolicy policy)
    {
        var elapsed = DateTime.UtcNow - policy.WindowStart;
        var totalWindow = TimeSpan.FromSeconds(policy.WindowSeconds);

        if (elapsed >= totalWindow)
        {
            return 100;
        }

        return (elapsed.TotalSeconds / policy.WindowSeconds) * 100;
    }

    /// <summary>
    /// Gets the estimated time remaining until the rate limit resets.
    /// </summary>
    /// <param name="policy">The rate limit policy instance.</param>
    /// <returns>TimeSpan representing how long until the window resets.</returns>
    public static TimeSpan GetTimeUntilReset(this RateLimitPolicy policy)
    {
        var now = DateTime.UtcNow;
        var windowEnd = policy.WindowStart.AddSeconds(policy.WindowSeconds);

        if (now >= windowEnd)
        {
            return TimeSpan.Zero;
        }

        return windowEnd - now;
    }
}
