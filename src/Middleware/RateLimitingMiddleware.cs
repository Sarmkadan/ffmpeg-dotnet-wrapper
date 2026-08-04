// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace FFmpegDotnetWrapper.Middleware
{
    /// <summary>
    /// Rate limiting policy configuration defining limits for different operation types.
    /// Supports per-user and global rate limiting with sliding window tracking.
    /// </summary>
    public class RateLimitPolicy
    {
        /// <summary>Maximum number of operations allowed in the time window.</summary>
        public int MaxRequests { get; set; } = 10;

        /// <summary>Time window in seconds for counting requests.</summary>
        public int WindowSeconds { get; set; } = 60;

        /// <summary>Whether to apply per-user limits in addition to global limits.</summary>
        public bool PerUserLimit { get; set; } = true;

        /// <summary>Identifier for this policy (e.g., "transcode", "watermark").</summary>
        public string PolicyName { get; set; } = "default";
    }

    /// <summary>
    /// Rate limiter that tracks requests using a sliding window algorithm.
    /// Prevents abuse by limiting the number of operations per time window.
    /// Supports both global and per-tenant rate limits.
    /// </summary>
    public interface IRateLimiter
    {
        bool AllowRequest(string identifier, string policyName = "default");
        bool AllowRequest(string? userId, string? tenantId, string policyName = "default");
        RateLimitStatus GetStatus(string identifier, string policyName = "default");
        void Reset(string identifier, string policyName = "default");
        void ResetAll();
    }

    /// <summary>
    /// Represents the current status of rate limiting for a specific identifier.
    /// Includes remaining requests and reset time.
    /// </summary>
    public class RateLimitStatus
    {
        public bool IsAllowed { get; set; }
        public int RequestsMade { get; set; }
        public int MaxRequests { get; set; }
        public int RemainingRequests => Math.Max(0, MaxRequests - RequestsMade);
        public DateTime ResetTime { get; set; }
        public double SecondsUntilReset => Math.Max(0, (ResetTime - DateTime.UtcNow).TotalSeconds);
    }

    /// <summary>
    /// In-memory sliding window rate limiter implementation.
    /// Maintains request history per identifier and enforces rate limit policies.
    /// </summary>
    public class SlidingWindowRateLimiter : IRateLimiter
    {
        private class RequestWindow
        {
            public Queue<DateTime> Timestamps { get; set; } = new();
            public DateTime WindowStart { get; set; }
        }

        private readonly Dictionary<string, RateLimitPolicy> _policies = new();
        private readonly Dictionary<string, RequestWindow> _windows = new();
        private readonly ILogger<SlidingWindowRateLimiter> _logger;
        private readonly object _lockObject = new();

        public SlidingWindowRateLimiter(ILogger<SlidingWindowRateLimiter> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Register default policies
            RegisterPolicy(new RateLimitPolicy { PolicyName = "default", MaxRequests = 100, WindowSeconds = 60 });
            RegisterPolicy(new RateLimitPolicy { PolicyName = "transcode", MaxRequests = 5, WindowSeconds = 3600 });
            RegisterPolicy(new RateLimitPolicy { PolicyName = "watermark", MaxRequests = 20, WindowSeconds = 3600 });
            RegisterPolicy(new RateLimitPolicy { PolicyName = "merge", MaxRequests = 10, WindowSeconds = 3600 });
        }

        /// <summary>
        /// Registers a rate limit policy that can be applied to requests.
        /// Policies define max requests per time window.
        /// </summary>
        public void RegisterPolicy(RateLimitPolicy policy)
        {
            if (policy == null)
                throw new ArgumentNullException(nameof(policy));

            lock (_lockObject) // atomic update
            {
                _policies[policy.PolicyName] = policy;
            }
        }

        /// <summary>
        /// Checks if a request is allowed under the specified policy.
        /// Returns true if within rate limit, false if exceeded.
        /// </summary>
        public bool AllowRequest(string identifier, string policyName = "default")
        {
            if (string.IsNullOrEmpty(identifier))
                throw new ArgumentException("Identifier cannot be empty", nameof(identifier));

            lock (_lockObject) // atomic update
            {
                if (!_policies.TryGetValue(policyName, out var policy))
                {
                    _logger.LogWarning("Unknown rate limit policy: {PolicyName}", policyName);
                    return true; // Allow if policy not found
                }

                var windowKey = $"{identifier}:{policyName}";

                if (!_windows.TryGetValue(windowKey, out var window))
                {
                    window = new RequestWindow { WindowStart = DateTime.UtcNow };
                    _windows[windowKey] = window;
                }

                var now = DateTime.UtcNow;
                var windowSeconds = policy.WindowSeconds;

                // Remove timestamps outside the sliding window
                while (window.Timestamps.Count > 0 &&
                       (now - window.Timestamps.Peek()).TotalSeconds > windowSeconds)
                {
                    window.Timestamps.Dequeue();
                }

                // Check if we're under the limit
                if (window.Timestamps.Count < policy.MaxRequests)
                {
                    window.Timestamps.Enqueue(now);
                    return true;
                }

                _logger.LogWarning(
                    "Rate limit exceeded for {Identifier} on policy {PolicyName} ({Count}/{Max})",
                    identifier,
                    policyName,
                    window.Timestamps.Count,
                    policy.MaxRequests);

                return false;
            }
        }

        /// <summary>
        /// Checks rate limit for a user/tenant combination.
        /// Applies both global and per-user limits if configured.
        /// </summary>
        public bool AllowRequest(string? userId, string? tenantId, string policyName = "default")
        {
            if (!_policies.TryGetValue(policyName, out var policy))
                return true;

            // Check tenant-level limit (global)
            var tenantId_safe = tenantId ?? "default";
            if (!AllowRequest(tenantId_safe, policyName))
                return false;

            // Check per-user limit if enabled
            if (policy.PerUserLimit && !string.IsNullOrEmpty(userId))
            {
                var userKey = $"{tenantId_safe}:{userId}";
                if (!AllowRequest(userKey, $"{policyName}:user"))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the current rate limit status for an identifier.
        /// Includes remaining requests and reset time.
        /// </summary>
        public RateLimitStatus GetStatus(string identifier, string policyName = "default")
        {
            lock (_lockObject) // atomic update
            {
                if (!_policies.TryGetValue(policyName, out var policy))
                {
                    return new RateLimitStatus { IsAllowed = true };
                }

                var windowKey = $"{identifier}:{policyName}";
                var now = DateTime.UtcNow;

                RequestWindow window;
                if (!_windows.TryGetValue(windowKey, out window!))
                {
                    window = new RequestWindow { WindowStart = now };
                }

                // Clean up expired timestamps
                while (window.Timestamps.Count > 0 &&
                       (now - window.Timestamps.Peek()).TotalSeconds > policy.WindowSeconds)
                {
                    window.Timestamps.Dequeue();
                }

                var resetTime = window.Timestamps.Count > 0
                    ? window.Timestamps.Peek().AddSeconds(policy.WindowSeconds)
                    : now;

                return new RateLimitStatus
                {
                    IsAllowed = window.Timestamps.Count < policy.MaxRequests,
                    RequestsMade = window.Timestamps.Count,
                    MaxRequests = policy.MaxRequests,
                    ResetTime = resetTime
                };
            }
        }

        /// <summary>
        /// Resets the rate limit counter for a specific identifier.
        /// Used for administrative purposes (e.g., quota resets).
        /// </summary>
        public void Reset(string identifier, string policyName = "default")
        {
            lock (_lockObject) // atomic update
            {
                var windowKey = $"{identifier}:{policyName}";
                _windows.Remove(windowKey);
                _logger.LogInformation("Rate limit reset for {Identifier} on policy {PolicyName}", identifier, policyName);
            }
        }

        /// <summary>
        /// Clears all rate limiting windows.
        /// Useful for testing or system maintenance.
        /// </summary>
        public void ResetAll()
        {
            lock (_lockObject) // atomic update
            {
                var count = _windows.Count;
                _windows.Clear();
                _logger.LogInformation("Cleared all rate limiting windows ({Count} entries)", count);
            }
        }
    }
}
