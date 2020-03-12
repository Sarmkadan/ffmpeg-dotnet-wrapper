# RateLimitPolicy

A rate-limiting policy implementation for managing request throughput in FFmpeg operations, supporting both fixed window and sliding window strategies with per-user and global limits.

## API

### Properties

#### `MaxRequests`
- **Purpose**: Gets or sets the maximum number of requests allowed within the defined window.
- **Type**: `int`
- **Remarks**: Must be a positive integer. Changing this value does not reset the current state.

#### `WindowSeconds`
- **Purpose**: Gets or sets the duration of the rate-limiting window in seconds.
- **Type**: `int`
- **Remarks**: Must be a positive integer. Adjusting this value does not reset the current state.

#### `PerUserLimit`
- **Purpose**: Determines whether the rate limit is enforced per user or globally.
- **Type**: `bool`
- **Default**: `false`

#### `PolicyName`
- **Purpose**: Gets the name of the rate-limiting policy.
- **Type**: `string`
- **Remarks**: Used for identification and logging purposes.

#### `IsAllowed`
- **Purpose**: Gets a value indicating whether the next request is currently allowed under the policy.
- **Type**: `bool`
- **Remarks**: Read-only. Reflects the current state of the rate limiter.

#### `RequestsMade`
- **Purpose**: Gets the number of requests made in the current window.
- **Type**: `int`
- **Remarks**: Read-only. Resets when the window elapses or `Reset` is called.

#### `ResetTime`
- **Purpose**: Gets the DateTime at which the current window will reset.
- **Type**: `DateTime`
- **Remarks**: Read-only. Useful for estimating when the next request will be allowed.

#### `Timestamps`
- **Purpose**: Gets the queue of timestamps for requests made in the current window.
- **Type**: `Queue<DateTime>`
- **Remarks**: Read-only. Used internally for sliding window calculations.

#### `WindowStart`
- **Purpose**: Gets the DateTime when the current window started.
- **Type**: `DateTime`
- **Remarks**: Read-only. Useful for tracking window boundaries.

#### `SlidingWindowRateLimiter`
- **Purpose**: Gets the underlying sliding window rate limiter instance.
- **Type**: `SlidingWindowRateLimiter`
- **Remarks**: Read-only. Exposes advanced configuration and diagnostics.

### Methods

#### `RegisterPolicy()`
- **Purpose**: Registers the policy with the global rate limiter registry.
- **Parameters**: None
- **Return Value**: `void`
- **Remarks**: Must be called before the policy can be used. Throws if registration fails.

#### `AllowRequest()`
- **Purpose**: Determines whether a new request is allowed under the policy.
- **Parameters**: None
- **Return Value**: `bool`
- **Remarks**: Thread-safe. Returns `true` if the request is allowed, `false` otherwise.

#### `AllowRequest(bool perUser)`
- **Purpose**: Determines whether a new request is allowed, optionally enforcing per-user limits.
- **Parameters**:
  - `perUser` (`bool`): If `true`, enforces the limit per user; otherwise, uses global limit.
- **Return Value**: `bool`
- **Remarks**: Thread-safe. Throws if `perUser` is `true` but `PerUserLimit` is `false`.

#### `GetStatus()`
- **Purpose**: Retrieves the current status of the rate limiter.
- **Parameters**: None
- **Return Value**: `RateLimitStatus`
- **Remarks**: Returns a snapshot including `RequestsMade`, `ResetTime`, and `IsAllowed`.

#### `Reset()`
- **Purpose**: Resets the rate limiter state to the beginning of the current window.
- **Parameters**: None
- **Return Value**: `void`
- **Remarks**: Thread-safe. Does not alter `MaxRequests` or `WindowSeconds`.

#### `ResetAll()`
- **Purpose**: Resets all rate limiter state across the application.
- **Parameters**: None
- **Return Value**: `void`
- **Remarks**: Thread-safe. Useful for testing or emergency recovery. Affects all registered policies.

## Usage

### Example 1: Basic Global Rate Limiting
