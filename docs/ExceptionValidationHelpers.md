# ExceptionValidationHelpers

This document describes the validation helpers for exception and policy classes in the FFmpegDotnetWrapper library. These helpers provide consistent validation patterns for ensuring objects are in a valid state before use.

## ServiceExceptionValidation

`ServiceExceptionValidation` provides extension methods for validating a [`ServiceException`](src/Exceptions/ServiceException.cs) instance. Validation reports all detected problems rather than stopping at the first one.

### API

#### `Validate`

```csharp
IReadOnlyList<string> problems = exception.Validate();
```

Returns a read-only list of human-readable validation problems. An empty list means the exception is valid. Passing `null` throws `ArgumentNullException`.

#### `IsValid`

```csharp
bool valid = exception.IsValid();
```

Returns `true` when `Validate` returns no problems. Passing `null` throws `ArgumentNullException` through `Validate`.

#### `EnsureValid`

```csharp
exception.EnsureValid();
```

Returns normally for a valid exception. For an invalid exception, it throws `ArgumentException` with all problems joined into the exception message. Passing `null` throws `ArgumentNullException`.

### Validation rules

| Property | Requirement |
| --- | --- |
| `ServiceName` | If set, must not be whitespace-only. |
| `Message` | Must not be null, empty, or whitespace. |
| `ExitCode` | When set, must be a non-negative integer. |
| `ErrorOutput` | When `ExitCode` is set, must not be null or whitespace. |

## ProcessExecutionExceptionValidation

`ProcessExecutionExceptionValidation` provides extension methods for validating a [`ProcessExecutionException`](src/Exceptions/ProcessExecutionException.cs) instance. Validation reports all detected problems rather than stopping at the first one.

### API

#### `Validate`

```csharp
IReadOnlyList<string> problems = exception.Validate();
```

Returns a read-only list of human-readable validation problems. An empty list means the exception is valid. Passing `null` throws `ArgumentNullException`.

#### `IsValid`

```csharp
bool valid = exception.IsValid();
```

Returns `true` when `Validate` returns no problems. Passing `null` throws `ArgumentNullException` through `Validate`.

#### `EnsureValid`

```csharp
exception.EnsureValid();
```

Returns normally for a valid exception. For an invalid exception, it throws `ArgumentException` with all problems joined into the exception message. Passing `null` throws `ArgumentNullException`.

### Validation rules

| Property | Requirement |
| --- | --- |
| `Message` | Must not be null, empty, or whitespace. |
| `ExitCode` | When set, must be a non-negative integer. |
| `ErrorOutput` | When `ExitCode` is set, must not be null or whitespace. |

## RateLimitPolicyValidation

`RateLimitPolicyValidation` provides extension methods for validating a [`RateLimitPolicy`](src/Middleware/RateLimitPolicy.cs) instance. Validation reports all detected problems rather than stopping at the first one.

### API

#### `Validate`

```csharp
IReadOnlyList<string> problems = policy.Validate();
```

Returns a read-only list of human-readable validation problems. An empty list means the policy is valid. Passing `null` throws `ArgumentNullException`.

#### `IsValid`

```csharp
bool valid = policy.IsValid();
```

Returns `true` when `Validate` returns no problems. Passing `null` throws `ArgumentNullException` through `Validate`.

#### `EnsureValid`

```csharp
policy.EnsureValid();
```

Returns normally for a valid policy. For an invalid policy, it throws `ArgumentException` with all problems joined into the exception message. Passing `null` throws `ArgumentNullException`.

### Validation rules

| Property | Requirement |
| --- | --- |
| `MaxRequests` | Must be greater than 0. |
| `WindowSeconds` | Must be greater than 0. |
| `PolicyName` | Must not be null or whitespace and must not exceed 100 characters. |
| `PerUserLimit` | No validation needed (it's a boolean). |

## JsonOutputFormatterValidation

`JsonOutputFormatterValidation` provides extension methods for validating a [`JsonOutputFormatter`](src/Serialization/JsonOutputFormatter.cs) instance. Since the formatter has no configurable properties exposed publicly, validation always passes.

### API

#### `Validate`

```csharp
IReadOnlyList<string> problems = formatter.Validate();
```

Returns an empty read-only list (the formatter is always considered valid). Passing `null` throws `ArgumentNullException`.

#### `IsValid`

```csharp
bool valid = formatter.IsValid();
```

Returns `true` (the formatter is always considered valid when not null). Passing `null` throws `ArgumentNullException` through `Validate`.

#### `EnsureValid`

```csharp
formatter.EnsureValid();
```

Returns normally (no validation is performed as the formatter is always considered valid). Passing `null` throws `ArgumentNullException`.

### Validation rules

The `JsonOutputFormatter` has no configurable properties to validate. The constructor parameter 'indent' is private and has no public accessors. All validation is handled by the constructor itself, so the validation helpers always return an empty list of problems.