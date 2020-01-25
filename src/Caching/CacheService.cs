// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace FFmpegDotnetWrapper.Caching
{
    /// <summary>
    /// In-memory cache service for storing frequently accessed data like media metadata and operation results.
    /// Supports time-based expiration, size limits, and LRU (Least Recently Used) eviction policy.
    /// Used to reduce unnecessary file system access and FFmpeg probing operations.
    /// </summary>
    public interface ICacheService
    {
        T? Get<T>(string key);
        void Set<T>(string key, T value, TimeSpan? expiration = null);
        bool Remove(string key);
        void Clear();
        int Count { get; }
    }

    public class CacheService : ICacheService
    {
        private class CacheEntry
        {
            public object? Value { get; set; }
            public DateTime ExpirationTime { get; set; }
            public DateTime LastAccessTime { get; set; }
            public int AccessCount { get; set; }
        }

        private readonly Dictionary<string, CacheEntry> _cache = new();
        private readonly ILogger<CacheService> _logger;
        private readonly int _maxCacheSize;
        private readonly TimeSpan _defaultExpiration;
        private readonly object _lockObject = new();

        public int Count => _cache.Count;

        /// <summary>
        /// Initializes a new cache service with configurable size limits and default expiration time.
        /// Default max size is 1000 entries, default expiration is 1 hour.
        /// </summary>
        public CacheService(ILogger<CacheService> logger, int maxCacheSize = 1000, TimeSpan? defaultExpiration = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _maxCacheSize = maxCacheSize;
            _defaultExpiration = defaultExpiration ?? TimeSpan.FromHours(1);
        }

        /// <summary>
        /// Retrieves a cached value if it exists and hasn't expired.
        /// Updates last access time for LRU tracking.
        /// Returns default value (null) if key doesn't exist or entry expired.
        /// </summary>
        public T? Get<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
                return default;

            lock (_lockObject)
            {
                if (!_cache.TryGetValue(key, out var entry))
                {
                    _logger.LogDebug("Cache miss for key: {Key}", key);
                    return default;
                }

                // Check if entry has expired
                if (entry.ExpirationTime < DateTime.UtcNow)
                {
                    _logger.LogDebug("Cache entry expired: {Key}", key);
                    _cache.Remove(key);
                    return default;
                }

                // Update access time and count for LRU
                entry.LastAccessTime = DateTime.UtcNow;
                entry.AccessCount++;

                _logger.LogDebug("Cache hit for key: {Key} (Access #{Count})", key, entry.AccessCount);
                return (T?)entry.Value;
            }
        }

        /// <summary>
        /// Stores a value in the cache with optional custom expiration time.
        /// If cache is full, removes least recently used entries first.
        /// Overwrites existing entries with the same key.
        /// </summary>
        public void Set<T>(string key, T value, TimeSpan? expiration = null)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Cache key cannot be empty", nameof(key));

            lock (_lockObject)
            {
                // Check if we need to evict entries
                if (_cache.Count >= _maxCacheSize && !_cache.ContainsKey(key))
                {
                    EvictLRU();
                }

                var expirationTime = DateTime.UtcNow.Add(expiration ?? _defaultExpiration);
                var entry = new CacheEntry
                {
                    Value = value,
                    ExpirationTime = expirationTime,
                    LastAccessTime = DateTime.UtcNow,
                    AccessCount = 0
                };

                _cache[key] = entry;
                _logger.LogDebug(
                    "Cache set for key: {Key} (Expires in {Minutes} minutes)",
                    key,
                    (int)(expiration ?? _defaultExpiration).TotalMinutes);
            }
        }

        /// <summary>
        /// Removes a specific cache entry by key.
        /// Returns true if entry was found and removed, false if not found.
        /// </summary>
        public bool Remove(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            lock (_lockObject)
            {
                var removed = _cache.Remove(key);
                if (removed)
                {
                    _logger.LogDebug("Cache entry removed: {Key}", key);
                }
                return removed;
            }
        }

        /// <summary>
        /// Clears all entries from the cache.
        /// Useful during cleanup or cache invalidation scenarios.
        /// </summary>
        public void Clear()
        {
            lock (_lockObject)
            {
                var count = _cache.Count;
                _cache.Clear();
                _logger.LogInformation("Cache cleared ({Count} entries removed)", count);
            }
        }

        /// <summary>
        /// Removes expired entries from the cache.
        /// Can be called periodically by a background task to clean up stale entries.
        /// </summary>
        public void RemoveExpiredEntries()
        {
            lock (_lockObject)
            {
                var expiredKeys = _cache
                    .Where(kvp => kvp.Value.ExpirationTime < DateTime.UtcNow)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var key in expiredKeys)
                {
                    _cache.Remove(key);
                }

                if (expiredKeys.Count > 0)
                {
                    _logger.LogDebug("Removed {Count} expired cache entries", expiredKeys.Count);
                }
            }
        }

        /// <summary>
        /// Gets cache statistics for monitoring and debugging.
        /// Returns count of entries and utilization percentage.
        /// </summary>
        public (int Count, int MaxSize, double Utilization) GetStats()
        {
            lock (_lockObject)
            {
                var utilization = ((double)_cache.Count / _maxCacheSize) * 100;
                return (_cache.Count, _maxCacheSize, utilization);
            }
        }

        /// <summary>
        /// Evicts the least recently used entry from the cache.
        /// Called when cache size reaches maximum capacity.
        /// </summary>
        private void EvictLRU()
        {
            if (_cache.Count == 0)
                return;

            // Find the entry with the oldest last access time
            var lruEntry = _cache
                .OrderBy(kvp => kvp.Value.LastAccessTime)
                .FirstOrDefault();

            if (lruEntry.Key != null)
            {
                _cache.Remove(lruEntry.Key);
                _logger.LogDebug("Evicted LRU cache entry: {Key}", lruEntry.Key);
            }
        }
    }
}
