# RepositoryExceptionExtensions

Provides a set of extension methods for `RepositoryException` that simplify common exception inspection patterns and enable fluent augmentation of exception context. These helpers allow callers to check for well-known failure conditions—such as missing repositories, duplicate repositories, or access-denied scenarios—without manually parsing exception messages or error codes. Additionally, the `WithContext` method supports attaching structured contextual information to an exception before rethrowing or logging.

## API

### `public static bool IsRepositoryNotFound(this RepositoryException exception)`

Determines whether the given exception represents a repository-not-found condition.

**Parameters**
- `exception` — The `RepositoryException` instance to inspect. Must not be `null`.

**Return Value**
`true` if the exception indicates that the target repository does not exist; otherwise `false`.

**Throws**
- `ArgumentNullException` — when `exception` is `null`.

---

### `public static bool IsRepositoryAlreadyExists(this RepositoryException exception)`

Determines whether the given exception represents a repository-already-exists condition.

**Parameters**
- `exception` — The `RepositoryException` instance to inspect. Must not be `null`.

**Return Value**
`true` if the exception indicates that a repository with the same identity already exists; otherwise `false`.

**Throws**
- `ArgumentNullException` — when `exception` is `null`.

---

### `public static bool IsAccessDenied(this RepositoryException exception)`

Determines whether the given exception represents an access-denied condition.

**Parameters**
- `exception` — The `RepositoryException` instance to inspect. Must not be `null`.

**Return Value**
`true` if the exception indicates that the caller lacks sufficient permissions; otherwise `false`.

**Throws**
- `ArgumentNullException` — when `exception` is `null`.

---

### `public static RepositoryException WithContext(this RepositoryException exception, string key, object value)`

Creates and returns a new `RepositoryException` that carries the supplied contextual key-value pair, preserving the original exception’s message, inner exception, and any previously attached context. The original exception is not modified.

**Parameters**
- `exception` — The `RepositoryException` to augment. Must not be `null`.
- `key` — A non-null, non-empty string identifying the context entry.
- `value` — The value to associate with the key. Can be `null`.

**Return Value**
A new `RepositoryException` instance with the additional context merged in.

**Throws**
- `ArgumentNullException` — when `exception` or `key` is `null`.
- `ArgumentException` — when `key` is an empty string.

## Usage

### Example 1: Branching on exception type

```csharp
try
{
    repoManager.OpenRepository("non-existent-repo");
}
catch (RepositoryException ex)
{
    if (ex.IsRepositoryNotFound())
    {
        Console.WriteLine("Repository not found. Creating a new one.");
        repoManager.CreateRepository("non-existent-repo");
    }
    else if (ex.IsAccessDenied())
    {
        Console.WriteLine("Access denied. Aborting operation.");
        throw;
    }
    else
    {
        Console.WriteLine($"Unexpected repository error: {ex.Message}");
        throw;
    }
}
```

### Example 2: Fluent context enrichment before logging

```csharp
try
{
    repoManager.DeleteRepository("critical-repo");
}
catch (RepositoryException ex) when (!ex.IsRepositoryNotFound())
{
    var enriched = ex
        .WithContext("operation", "delete")
        .WithContext("repositoryName", "critical-repo")
        .WithContext("userId", Environment.UserName);

    logger.LogError(enriched, "Repository deletion failed");
    throw enriched;
}
```

## Notes

- All predicate methods (`IsRepositoryNotFound`, `IsRepositoryAlreadyExists`, `IsAccessDenied`) throw `ArgumentNullException` when passed a `null` exception reference. Callers should guard against `null` before invoking them if the exception source is untrusted.
- `WithContext` always returns a new instance; it does not mutate the original exception. This makes it safe to use in `catch` blocks where the original exception may still be referenced elsewhere.
- The contextual data attached via `WithContext` is intended for diagnostics and logging. Its structure and serialization behaviour are defined by `RepositoryException` itself and are outside the scope of these extension methods.
- These methods are thread-safe in the sense that they operate on immutable inputs and produce immutable outputs. No shared state is accessed or modified.
