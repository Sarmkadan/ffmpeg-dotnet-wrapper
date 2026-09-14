# Rate Limiting Middleware

This document describes the rate limiting middleware implementation in `src/Middleware/RateLimitingMiddleware.cs`. For detailed information about specific components, refer to the linked documentation:

- [`IRateLimiter` interface and `SlidingWindowRateLimiter` class](SlidingWindowRateLimiter.md)
- [`RateLimitPolicy` class](RateLimitPolicy.md)
- [`RateLimitStatus` class](SlidingWindowRateLimiter.md#ratelimitstatus)

## Overview

The rate limiting middleware provides a sliding window rate limiter implementation that protects FFmpeg operations from abuse by limiting the number of requests per time window. It supports both global and per-user/tenant rate limiting with configurable policies.

## Components

### Core Interfaces and Classes

1. **`IRateLimiter`** - Defines the contract for rate limiting operations:
   - `AllowRequest(string identifier, string policyName)` - Checks and records a request
   - `AllowRequest(string? userId, string? tenantId, string policyName)` - Checks rate limit for user/tenant combinations
   - `GetStatus(string identifier, string policyName)` - Gets current rate limit status
   - `Reset(string identifier, string policyName)` - Resets rate limit counter for identifier
   - `ResetAll()` - Clears all rate limiting windows

2. **`SlidingWindowRateLimiter`** - Thread-safe in-memory implementation of `IRateLimiter`:
   - Uses sliding window algorithm to track request timestamps
   - Maintains separate windows per identifier and policy
   - Automatically registers default policies for common operations
   - Supports policy registration at runtime

3. **`RateLimitPolicy`** - Configuration class for rate limiting rules:
   - Defines maximum requests, window duration, and per-user limiting
   - Includes properties for policy identification and current state
   - Provides methods for policy registration and request checking

4. **`RateLimitStatus`** - Data transfer object representing current rate limit state:
   - Tracks allowed status, requests made, maximum requests, remaining requests
   - Provides reset time and seconds until reset calculations

## Request Flow Example

Here's how a request flows through the rate limiting middleware:

1. **Policy Lookup**: When `AllowRequest` is called, the middleware first looks up the specified policy by name
2. **Window Initialization**: If no window exists for the identifier/policy combination, a new request window is created
3. **Timestamp Cleanup**: Expired timestamps (older than the window duration) are removed from the sliding window
4. **Limit Check**: The middleware checks if the number of requests in the window is below the policy maximum
5. **Recording**: If allowed, the current timestamp is recorded and `true` is returned
6. **Denial**: If the limit is exceeded, `false` is returned and a warning is logged
7. **Per-User Check**: For the overload accepting userId/tenantId, both tenant-level and (if enabled) user-level limits are checked

## Default Policies

The middleware registers these policies by default (see [SlidingWindowRateLimiter documentation](SlidingWindowRateLimiter.md#default-policies) for details):
- `default`: 100 requests per 60 seconds
- `transcode`: 5 requests per 3,600 seconds
- `watermark`: 20 requests per 3,600 seconds
- `merge`: 10 requests per 3,600 seconds

## Usage

See the [SlidingWindowRateLimiter usage examples](SlidingWindowRateLimiter.md#usage) for code samples demonstrating:
- Creating a limiter instance
- Registering custom policies
- Checking and recording requests
- Handling rate limit exceeded scenarios
- Getting current status and reset information

## Thread Safety

All operations on the `SlidingWindowRateLimiter` are thread-safe through the use of locking mechanisms. The internal state (policy registry and request windows) is protected by a shared lock object to ensure consistent state across concurrent requests.

## Extensibility

New rate limiting policies can be registered at any time using the `RegisterPolicy` method. Policies are identified by name and can be retrieved or replaced by that name. Unknown policies are logged and requests are allowed by default to prevent accidental blocking of legitimate traffic.