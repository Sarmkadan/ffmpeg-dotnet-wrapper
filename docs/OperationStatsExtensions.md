# OperationStatsExtensions

Extension methods for `OperationStats` that provide statistical analysis and reporting capabilities for FFmpeg operation timings and outcomes.

## API

### `Get95thPercentileTimeMs`

Calculates the 95th percentile of operation execution times from a collection of `OperationStats`.

- **Parameters**:
  - `stats` (`IEnumerable<OperationStats>`): A sequence of `OperationStats` instances to analyze.
- **Return value**: `double` representing the 95th percentile time in milliseconds.
- **Exceptions**: Throws `ArgumentNullException` if `stats` is `null`.

### `GetFailureRate`

Computes the failure rate as a percentage of failed operations relative to the total number of operations.

- **Parameters**:
  - `stats` (`IEnumerable<OperationStats>`): A sequence of `OperationStats` instances to analyze.
- **Return value**: `double` representing the failure rate as a percentage (0.0 to 100.0).
- **Exceptions**: Throws `ArgumentNullException` if `stats` is `null`.

### `ToFormattedTable`

Generates a human-readable table representation of the provided `OperationStats` collection.

- **Parameters**:
  - `stats` (`IEnumerable<OperationStats>`): A sequence of `OperationStats` instances to format.
  - `includeHeader` (`bool`, optional): If `true`, includes a header row in the output. Defaults to `true`.
- **Return value**: `string` containing the formatted table.
- **Exceptions**: Throws `ArgumentNullException` if `stats` is `null`.

### `CompareWith`

Compares two sequences of `OperationStats` and returns a formatted comparison table highlighting differences in key metrics.

- **Parameters**:
  - `baseline` (`IEnumerable<OperationStats>`): The baseline sequence of `OperationStats` to compare against.
  - `comparison` (`IEnumerable<OperationStats>`): The sequence of `OperationStats` to compare with the baseline.
  - `includeHeader` (`bool`, optional): If `true`, includes a header row in the output. Defaults to `true`.
- **Return value**: `string` containing the comparison table.
- **Exceptions**:
  - Throws `ArgumentNullException` if either `baseline` or `comparison` is `null`.
  - Throws `ArgumentException` if the sequences contain different operation types or are otherwise incompatible for comparison.

## Usage
