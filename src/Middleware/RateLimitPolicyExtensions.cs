using System;

public static class RateLimitPolicyExtensions
{
    /// <summary>
    /// Gets the current request rate as requests per second within the current window.
    /// </summary>
    /// <param name="policy">The rate limit policy instance.</param>
    /// <returns>Current requests per second rate, or 0 if no requests have been made.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="policy"/> is null.</exception>
    public static double GetCurrentRequestsPerSecond(this RateLimitPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        if (policy.Timestamps.Count == 0)
        {
            return 0;
        }

        var windowDuration = DateTime.UtcNow - policy.WindowStart;
        var seconds = windowDuration.TotalSeconds;

        return seconds > 0
            ? policy.RequestsMade / seconds
            : policy.RequestsMade;
    }

    /// <summary>
    /// Checks if the rate limit policy has reached its maximum capacity.
    /// </summary>
    /// <param name="policy">The rate limit policy instance.</param>
    /// <returns>True if the policy is at or over capacity; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="policy"/> is null.</exception>
    public static bool IsAtCapacity(this RateLimitPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);
        return policy.RequestsMade >= policy.MaxRequests;
    }

    /// <summary>
    /// Gets the percentage of the rate limit window that has elapsed.
    /// </summary>
    /// <param name="policy">The rate limit policy instance.</param>
    /// <returns>Percentage (0-100) of the window that has elapsed.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="policy"/> is null.</exception>
    public static double GetWindowProgress(this RateLimitPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        var elapsed = DateTime.UtcNow - policy.WindowStart;
        var totalWindow = TimeSpan.FromSeconds(policy.WindowSeconds);

        return elapsed >= totalWindow
            ? 100
            : (elapsed.TotalSeconds / policy.WindowSeconds) * 100;
    }

    /// <summary>
    /// Gets the estimated time remaining until the rate limit resets.
    /// </summary>
    /// <param name="policy">The rate limit policy instance.</param>
    /// <returns>TimeSpan representing how long until the window resets.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="policy"/> is null.</exception>
    public static TimeSpan GetTimeUntilReset(this RateLimitPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        var now = DateTime.UtcNow;
        var windowEnd = policy.WindowStart.AddSeconds(policy.WindowSeconds);

        return now >= windowEnd
            ? TimeSpan.Zero
            : windowEnd - now;
    }
}
