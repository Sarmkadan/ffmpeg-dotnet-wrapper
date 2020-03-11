# OperationRepository

A repository implementation that provides asynchronous CRUD operations and query capabilities for `FFmpegOperation` entities. It encapsulates data access logic for managing FFmpeg operations, including retrieval by identifier, type, date ranges, and recency, as well as bulk operations like clearing old entries and counting records.

## API

### `Task<FFmpegOperation?> GetByIdAsync(Guid id)`

Retrieves an `FFmpegOperation` by its unique identifier. Returns `null` if no operation with the specified `id` exists.

- **Parameters**
  - `id` (Guid): The unique identifier of the operation to retrieve.
- **Return Value**
  - `Task<FFmpegOperation?>`: A task that resolves to the found operation or `null`.
- **Exceptions**
  - Throws `ArgumentException` if `id` is `Guid.Empty`.

---

### `Task<IEnumerable<FFmpegOperation>> GetAllAsync()`

Retrieves all `FFmpegOperation` entities stored in the repository.

- **Return Value**
  - `Task<IEnumerable<FFmpegOperation>>`: A task that resolves to an enumerable of all operations.
- **Exceptions**
  - None.

---

### `Task<FFmpegOperation> AddAsync(FFmpegOperation operation)`

Adds a new `FFmpegOperation` to the repository.

- **Parameters**
  - `operation` (FFmpegOperation): The operation to add. Must not be `null`.
- **Return Value**
  - `Task<FFmpegOperation>`: A task that resolves to the added operation, typically with an updated identifier or timestamp.
- **Exceptions**
  - Throws `ArgumentNullException` if `operation` is `null`.
  - May throw if the operation conflicts with existing data (e.g., duplicate identifiers).

---
### `Task<FFmpegOperation> UpdateAsync(FFmpegOperation operation)`

Updates an existing `FFmpegOperation` in the repository.

- **Parameters**
  - `operation` (FFmpegOperation): The operation to update. Must not be `null` and must have a valid identifier.
- **Return Value**
  - `Task<FFmpegOperation>`: A task that resolves to the updated operation.
- **Exceptions**
  - Throws `ArgumentNullException` if `operation` is `null`.
  - Throws `InvalidOperationException` if the operation does not exist or if the identifier is invalid.

---
### `Task<bool> DeleteAsync(Guid id)`

Deletes an `FFmpegOperation` by its identifier.

- **Parameters**
  - `id` (Guid): The unique identifier of the operation to delete.
- **Return Value**
  - `Task<bool>`: A task that resolves to `true` if the operation was found and deleted, `false` otherwise.
- **Exceptions**
  - Throws `ArgumentException` if `id` is `Guid.Empty`.

---
### `Task<IEnumerable<FFmpegOperation>> GetByTypeAsync(FFmpegOperationType type)`

Retrieves all `FFmpegOperation` entities filtered by their type.

- **Parameters**
  - `type` (FFmpegOperationType): The type of operations to retrieve.
- **Return Value**
  - `Task<IEnumerable<FFmpegOperation>>`: A task that resolves to an enumerable of operations matching the type.
- **Exceptions**
  - None.

---
### `Task<IEnumerable<FFmpegOperation>> GetRecentAsync(int count)`

Retrieves the most recently added `FFmpegOperation` entities, limited by `count`.

- **Parameters**
  - `count` (int): The maximum number of recent operations to return. Must be non-negative.
- **Return Value**
  - `Task<IEnumerable<FFmpegOperation>>`: A task that resolves to an enumerable of the most recent operations, ordered by insertion time.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `count` is negative.

---
### `Task<IEnumerable<FFmpegOperation>> GetByDateRangeAsync(DateTime start, DateTime end)`

Retrieves `FFmpegOperation` entities whose timestamps fall within the specified date range.

- **Parameters**
  - `start` (DateTime): The inclusive start of the date range.
  - `end` (DateTime): The inclusive end of the date range. Must be greater than or equal to `start`.
- **Return Value**
  - `Task<IEnumerable<FFmpegOperation>>`: A task that resolves to an enumerable of operations within the range.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `end` is earlier than `start`.

---
### `Task<int> ClearOldAsync(TimeSpan ageThreshold)`

Removes all `FFmpegOperation` entities older than the specified age threshold.

- **Parameters**
  - `ageThreshold` (TimeSpan): The minimum age of operations to retain. Operations older than this will be deleted.
- **Return Value**
  - `Task<int>`: A task that resolves to the number of operations deleted.
- **Exceptions**
  - None.

---
### `Task<int> GetCountAsync()`

Retrieves the total number of `FFmpegOperation` entities stored in the repository.

- **Return Value**
  - `Task<int>`: A task that resolves to the count of operations.
- **Exceptions**
  - None.

## Usage

### Example 1: Adding and Retrieving an Operation
