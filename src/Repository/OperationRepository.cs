// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Models;

namespace FFmpegDotnetWrapper.Repository;

/// <summary>
/// In-memory implementation of the operation repository for tracking FFmpeg operations.
/// </summary>
public class OperationRepository : IOperationRepository
{
    private readonly Dictionary<string, FFmpegOperation> _operations = new();
    private readonly object _lockObject = new();
    private readonly int _maxOperationsInMemory = 1000;

    public Task<FFmpegOperation?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            _operations.TryGetValue(id, out var operation);
            return Task.FromResult(operation);
        }
    }

    public Task<IEnumerable<FFmpegOperation>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            return Task.FromResult(_operations.Values.AsEnumerable());
        }
    }

    public Task<FFmpegOperation> AddAsync(FFmpegOperation operation, CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            // Implement memory management
            if (_operations.Count >= _maxOperationsInMemory)
                EvictOldestOperation();

            _operations[operation.Id] = operation;
            return Task.FromResult(operation);
        }
    }

    public Task<FFmpegOperation> UpdateAsync(FFmpegOperation operation, CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            if (!_operations.ContainsKey(operation.Id))
                throw new InvalidOperationException($"Operation with ID {operation.Id} not found");

            _operations[operation.Id] = operation;
            return Task.FromResult(operation);
        }
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            return Task.FromResult(_operations.Remove(id));
        }
    }

    public Task<IEnumerable<FFmpegOperation>> GetByTypeAsync(FFmpegOperationType type, CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            var results = _operations.Values
                .Where(op => op.Type == type)
                .AsEnumerable();

            return Task.FromResult(results);
        }
    }

    public Task<IEnumerable<FFmpegOperation>> GetRecentAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            var results = _operations.Values
                .OrderByDescending(op => op.CreatedAt)
                .Take(count)
                .AsEnumerable();

            return Task.FromResult(results);
        }
    }

    public Task<IEnumerable<FFmpegOperation>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            var results = _operations.Values
                .Where(op => op.CreatedAt >= from && op.CreatedAt <= to)
                .AsEnumerable();

            return Task.FromResult(results);
        }
    }

    public Task<int> ClearOldAsync(int olderThanDays, CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-olderThanDays);
            var idsToDelete = _operations.Values
                .Where(op => op.CreatedAt < cutoffDate)
                .Select(op => op.Id)
                .ToList();

            int deletedCount = 0;
            foreach (var id in idsToDelete)
            {
                if (_operations.Remove(id))
                    deletedCount++;
            }

            return Task.FromResult(deletedCount);
        }
    }

    public Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            return Task.FromResult(_operations.Count);
        }
    }

    /// <summary>
    /// Evicts the oldest operation when memory limit is reached.
    /// </summary>
    private void EvictOldestOperation()
    {
        if (_operations.Count == 0)
            return;

        var oldestOp = _operations.Values.OrderBy(op => op.CreatedAt).FirstOrDefault();
        if (oldestOp != null)
            _operations.Remove(oldestOp.Id);
    }
}
