using System;
using System.Collections.Generic;

namespace FFmpegDotnetWrapper.Middleware
{
    /// <summary>
    /// Extension methods for <see cref="RateLimitStatus"/>.
    /// </summary>
    public static class RateLimitStatusExtensions
    {
        /// <summary>
        /// Determines whether the rate limit is exhausted.
        /// </summary>
        /// <param name="status">The rate limit status.</param>
        /// <returns>True if no requests remain; otherwise, false.</returns>
        public static bool IsExhausted(this RateLimitStatus status)
        {
            if (status == null)
                throw new ArgumentNullException(nameof(status));

            return status.RemainingRequests == 0;
        }

        /// <summary>
        /// Gets the remaining requests as a percentage of the limit.
        /// </summary>
        /// <param name="status">The rate limit status.</param>
        /// <returns>The remaining percentage (0-100).</returns>
        public static double GetRemainingPercentage(this RateLimitStatus status)
        {
            if (status == null)
                throw new ArgumentNullException(nameof(status));

            if (status.MaxRequests == 0)
                return 0;

            return (double)status.RemainingRequests / status.MaxRequests * 100;
        }

        /// <summary>
        /// Gets the time to wait before retrying a request.
        /// </summary>
        /// <param name="status">The rate limit status.</param>
        /// <returns>The time span to wait, or zero if not limited.</returns>
        public static TimeSpan GetRetryAfter(this RateLimitStatus status)
        {
            if (status == null)
                throw new ArgumentNullException(nameof(status));

            return status.IsAllowed ? TimeSpan.Zero : TimeSpan.FromSeconds(status.SecondsUntilReset);
        }

        /// <summary>
        /// Converts the rate limit status to a dictionary of HTTP headers.
        /// </summary>
        /// <param name="status">The rate limit status.</param>
        /// <returns>A dictionary containing the rate limit headers.</returns>
        public static Dictionary<string, string> ToHeaderDictionary(this RateLimitStatus status)
        {
            if (status == null)
                throw new ArgumentNullException(nameof(status));

            var headers = new Dictionary<string, string>
            {
                { "X-RateLimit-Limit", status.MaxRequests.ToString() },
                { "X-RateLimit-Remaining", status.RemainingRequests.ToString() }
            };

            // Calculate the reset time as Unix timestamp (seconds)
            var resetEpoch = (int)status.ResetTime.ToUniversalTime()
                                            .Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))
                                            .TotalSeconds;

            headers.Add("X-RateLimit-Reset", resetEpoch.ToString());

            return headers;
        }
    }
}