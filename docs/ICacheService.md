# ICacheService

The `ICacheService` interface defines the contract for a generic, in-memory caching mechanism within the `ffmpeg-dotnet-wrapper` library. It provides functionality to store, retrieve, and manage cached objects with built-in expiration tracking, access statistics, and capacity monitoring. This service is designed to optimize performance by reducing redundant operations, such as repeated FFmpeg process invocations or heavy resource allocations, while offering visibility into cache health through utilization metrics and access counts.

## API

### `Value`
```csharp
public object? Value { get; set; }
```
Gets or sets the actual object stored in the cache entry. The value is nullable, allowing the cache to hold references to any .NET object or explicitly represent a missing state. Setting this property updates the cached content without necessarily resetting expiration logic unless implemented by the concrete class.

### `ExpirationTime`
```csharp
public DateTime ExpirationTime { get; set; }
```
Gets or sets the absolute `DateTime` at which the cache entry is considered invalid. Entries accessed after this timestamp should be treated as stale. The setter allows manual adjustment of the lifespan for specific entries.

### `LastAccessTime`
```csharp
public DateTime LastAccessTime { get; set; }
```
Gets or sets the timestamp indicating when the cache entry was last retrieved or modified. This property is typically updated automatically by the `Get` method to facilitate Least Recently Used (LRU) eviction policies or usage analytics.

### `AccessCount`
```csharp
public int AccessCount { get; set; }
```
Gets or sets the total number of times this specific cache entry has been accessed. This counter aids in identifying hot data paths and determining entry priority during cleanup operations.

### `CacheService`
```csharp
public CacheService { get; }
```
Provides a reference to the concrete `CacheService` implementation managing this instance. This property allows navigation from an individual entry or scoped view back to the parent manager for broader operations like global clearing or statistical aggregation.

### `Get<T>`
```csharp
public T? Get<T>(string key);
```
Retrieves a value from the cache cast to the specified type `T`.
*   **Parameters**: `key` (string) – The unique identifier for the cached item.
*   **Returns**: The cached value of type `T` if found and valid; otherwise, `null`.
*   **Throws**: May throw an `InvalidCastException` if the stored object cannot be cast to type `T`.

### `Set<T>`
```csharp
public void Set<T>(string key, T value, TimeSpan? expiration = null);
```
Stores a value in the cache under the specified key.
*   **Parameters**: 
    *   `key` (string) – The unique identifier for the item.
    *   `value` (T) – The object to cache.
    *   `expiration` (TimeSpan?, optional) – The duration until the item expires. If null, a default expiration policy applies.
*   **Returns**: `void`.
*   **Throws**: Generally does not throw unless memory constraints are critical or the key is invalid (e.g., null).

### `Remove`
```csharp
public bool Remove(string key);
```
Explicitly removes a specific entry from the cache.
*   **Parameters**: `key` (string) – The unique identifier of the item to remove.
*   **Returns**: `true` if the item was found and removed; `false` if the key did not exist.
*   **Throws**: No standard exceptions expected for normal operation.

### `Clear`
```csharp
public void Clear();
```
Removes all entries from the cache, resetting the storage to an empty state.
*   **Parameters**: None.
*   **Returns**: `void`.
*   **Throws**: No standard exceptions expected.

### `RemoveExpiredEntries`
```csharp
public void RemoveExpiredEntries();
```
Scans the cache and removes all entries where the current time exceeds their `ExpirationTime`. This method is useful for manual maintenance if automatic background cleanup is not enabled in the specific implementation.
*   **Parameters**: None.
*   **Returns**: `void`.
*   **Throws**: No standard exceptions expected.

### `GetStats`
```csharp
public (int Count, int MaxSize, double Utilization) GetStats();
```
Returns current statistical data regarding the cache's state.
*   **Parameters**: None.
*   **Returns**: A tuple containing:
    *   `Count`: The current number of items in the cache.
    *   `MaxSize`: The configured maximum capacity (item count or memory limit representation).
    *   `Utilization`: A double between 0.0 and 1.0 representing the ratio of current usage to maximum capacity.
*   **Throws**: No standard exceptions expected.

## Usage

### Example 1: Caching FFmpeg Probe Results
This example demonstrates storing a complex probe result object with a specific expiration time to avoid re-analyzing the same media file repeatedly within a short window.

```csharp
using System;
using FfmpegDotNetWrapper.Services;

public class MediaAnalyzer
{
    private readonly ICacheService _cache;

    public MediaAnalyzer(ICacheService cache)
    {
        _cache = cache;
    }

    public MediaInfo AnalyzeFile(string filePath)
    {
        string cacheKey = $"probe:{filePath}";
        
        // Attempt to retrieve existing probe data
        var cachedResult = _cache.Get<MediaInfo>(cacheKey);
        if (cachedResult != null)
        {
            return cachedResult;
        }

        // Perform expensive FFmpeg probe operation
        var freshResult = FFmpeg.Probe(filePath);

        // Cache the result for 5 minutes
        _cache.Set(cacheKey, freshResult, TimeSpan.FromMinutes(5));

        return freshResult;
    }
}
```

### Example 2: Cache Maintenance and Monitoring
This example illustrates how to monitor cache utilization and manually trigger cleanup of expired entries based on statistical feedback.

```csharp
using System;
using FfmpegDotNetWrapper.Services;

public class CacheMonitor
{
    private readonly ICacheService _cache;

    public CacheMonitor(ICacheService cache)
    {
        _cache = cache;
    }

    public void PerformMaintenance()
    {
        var stats = _cache.GetStats();
        
        Console.WriteLine($"Cache Status: {stats.Count}/{stats.MaxSize} items");
        Console.WriteLine($"Utilization: {stats.Utilization:P2}");

        // If utilization is high or simply as part of a scheduled task, remove expired items
        if (stats.Utilization > 0.8 || stats.Count == stats.MaxSize)
        {
            Console.WriteLine("High utilization detected. Cleaning expired entries...");
            _cache.RemoveExpiredEntries();
            
            // Re-check stats after cleanup
            var newStats = _cache.GetStats();
            Console.WriteLine($"New Utilization: {newStats.Utilization:P2}");
        }
    }
}
```

## Notes

*   **Thread Safety**: While the interface definition does not explicitly enforce thread safety, implementations of `ICacheService` in this wrapper are generally designed to be thread-safe for concurrent `Get`, `Set`, and `Remove` operations. However, compound actions (e.g., checking `GetStats` then immediately calling `Clear`) are not atomic and may exhibit race conditions in multi-threaded environments without external locking.
*   **Expiration Logic**: The `ExpirationTime` property relies on absolute time. The `RemoveExpiredEntries` method performs a linear scan; for very large caches, frequent manual invocation may impact performance. It is recommended to rely on internal eviction policies if available, using this method primarily for on-demand cleanup.
*   **Type Safety**: The `Get<T>` method performs a runtime cast. If an item was stored as type `A` but requested as type `B`, an `InvalidCastException` will occur. Ensure consistent typing for keys across the application lifecycle.
*   **Null Values**: Storing a `null` value via `Set<T>` is permissible. Distinguishing between a "cache miss" (key does not exist) and a "cached null" (key exists but value is null) requires checking the existence of the key or relying on the return behavior of the specific implementation, as `Get<T>` returns `null` for both scenarios in many generic designs.
*   **Statistics Accuracy**: The `GetStats` method provides a snapshot in time. In highly concurrent systems, the `Count` and `Utilization` values may change immediately after the method returns.
