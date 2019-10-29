// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FFmpegDotnetWrapper.Models;

namespace FFmpegDotnetWrapper.Repository;

/// <summary>
/// Interface for FFmpeg operation tracking and history.
/// </summary>
public interface IOperationRepository
{
    /// <summary>
    /// Gets an operation by ID.
    /// </summary>
    Task<FFmpegOperation?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all operations.
    /// </summary>
    Task<IEnumerable<FFmpegOperation>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new operation record.
    /// </summary>
    Task<FFmpegOperation> AddAsync(FFmpegOperation operation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an operation record.
    /// </summary>
    Task<FFmpegOperation> UpdateAsync(FFmpegOperation operation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an operation by ID.
    /// </summary>
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets operations by type.
    /// </summary>
    Task<IEnumerable<FFmpegOperation>> GetByTypeAsync(FFmpegOperationType type, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recently executed operations.
    /// </summary>
    Task<IEnumerable<FFmpegOperation>> GetRecentAsync(int count = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets operations created within a date range.
    /// </summary>
    Task<IEnumerable<FFmpegOperation>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears old operations (older than specified days).
    /// </summary>
    Task<int> ClearOldAsync(int olderThanDays, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of operations.
    /// </summary>
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
}
