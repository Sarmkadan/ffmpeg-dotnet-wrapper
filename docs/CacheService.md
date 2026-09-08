# CacheService

`CacheService` is an in-memory, generic cache implemented by `FFmpegDotnetWrapper.Caching.CacheService`. It stores entries in a dictionary and protects its cache operations with a private lock. The class implements the [`ICacheService`](ICacheService.md) contract and also exposes concrete maintenance and statistics methods.

## Construction

```csharp
public CacheService(
    ILogger<CacheService> logger,
    int maxCacheSize = 1000,
    TimeSpan? defaultExpiration = null)
```

The logger is required; passing `null` throws `ArgumentNullException`. The default maximum size is 1,000 entries, and the default lifetime is one hour. The implementation stores the supplied maximum size without additional validation.

## Entry lifecycle and expiration

`Set<T>` records the value, an absolute expiration time based on `DateTime.UtcNow`, the current UTC time as its last-access time, and an access count of zero. Supplying an existing key replaces the complete entry. A custom `expiration` overrides the configured default for that call.

`Get<T>` returns `default` immediately for a null or empty key. For a missing key it logs a cache miss and returns `default`. An entry is expired when its expiration time is strictly earlier than `DateTime.UtcNow`; accessing such an entry removes it and returns `default`. A successful read updates the entry's last-access time, increments its access count, and casts the stored object to `T?`. An incompatible requested type can therefore produce `InvalidCastException`.

Expiration is checked on access and by explicit cleanup. There is no timer or background cleanup in this class, so an expired but unaccessed entry remains stored and contributes to `Count` until it is removed, replaced, cleared, evicted, or processed by `RemoveExpiredEntries`.

## LRU eviction

When `Set<T>` adds a new key and the current entry count is greater than or equal to the configured maximum, the service evicts one entry before inserting the new one. It selects the entry with the earliest `LastAccessTime`, regardless of whether that entry has expired. Reads refresh `LastAccessTime`; writes create a replacement entry with a new timestamp. `AccessCount` is tracked and logged on successful reads but is not used to choose an eviction candidate.

Updating an existing key does not trigger eviction, even when the cache is at its configured limit.

## Maintenance and statistics

### `RemoveExpiredEntries`

```csharp
public void RemoveExpiredEntries()
```

While holding the cache lock, this method collects and removes every entry whose expiration time is earlier than the current UTC time. It logs the number removed when at least one expired entry is found. The method is public on `CacheService`, but it is not declared by `ICacheService`.

### `GetStats`

```csharp
public (int Count, int MaxSize, double Utilization) GetStats()
```

This method returns a locked snapshot containing:

- `Count`: the number of entries currently stored, including expired entries that have not yet been removed.
- `MaxSize`: the maximum size supplied to the constructor.
- `Utilization`: `Count / MaxSize * 100`, expressed as a percentage rather than a fraction.

`GetStats` is public on `CacheService`, but it is not declared by `ICacheService`.

### `Count`

```csharp
public int Count { get; }
```

`Count` returns the dictionary's current entry count. It does not remove expired entries, and unlike the other cache operations its getter does not acquire the service's private lock.

## Other operations

| Member | Implemented behavior |
| --- | --- |
| `Set<T>(string key, T value, TimeSpan? expiration = null)` | Adds or replaces an entry. A null or empty key throws `ArgumentException`. |
| `Get<T>(string key)` | Returns a valid cached value or `default`; removes the requested entry if it has expired. |
| `Remove(string key)` | Removes the matching entry and returns whether it was present. A null or empty key returns `false`. |
| `Clear()` | Removes every stored entry and logs how many were removed. |

## Example

```csharp
using FFmpegDotnetWrapper.Caching;
using Microsoft.Extensions.Logging;

ILogger<CacheService> logger = loggerFactory.CreateLogger<CacheService>();
var cache = new CacheService(logger, maxCacheSize: 100, defaultExpiration: TimeSpan.FromHours(1));

cache.Set("probe:input.mp4", "probe output", TimeSpan.FromMinutes(10));
var cachedResult = cache.Get<string>("probe:input.mp4");

cache.RemoveExpiredEntries();
var (count, maxSize, utilization) = cache.GetStats();

Console.WriteLine($"{count}/{maxSize} entries ({utilization:F1}%)");
```

Use the concrete `CacheService` type when calling `RemoveExpiredEntries` or `GetStats`; references typed as `ICacheService` expose only `Get`, `Set`, `Remove`, `Clear`, and `Count`.
