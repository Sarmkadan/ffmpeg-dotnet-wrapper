# MediaRepositoryValidation

`MediaRepositoryValidation` is a static helper class that provides a set of validation utilities for the `MediaRepository` domain type. It exposes methods to inspect a repository instance, its constituent media files, and related data such as identifiers, file paths, dates, and numeric values. The class is intentionally stateless and thread‑safe, making it suitable for use in both synchronous and asynchronous contexts.

## API

### `public static IReadOnlyList<string> Validate(this MediaRepository value)`

Validates the supplied `MediaRepository` instance and returns a read‑only list of error messages.  
- **Parameters**:  
  - `value`: The repository to validate.  
- **Return value**: A list of validation error strings; an empty list indicates a valid repository.  
- **Throws**: None. The method never throws; it simply reports errors.

### `public static bool IsValid(this MediaRepository value)`

Convenience wrapper that returns `true` when `Validate` yields no errors.  
- **Parameters**:  
  - `value`: The repository to test.  
- **Return value**: `true` if the repository is valid; otherwise `false`.  
- **Throws**: None.

### `public static void EnsureValid(this MediaRepository value)`

Validates the repository and throws a `ValidationException` if any errors are found.  
- **Parameters**:  
  - `value`: The repository to validate.  
- **Return value**: None.  
- **Throws**: `ValidationException` containing the list of validation errors.

### `public static IReadOnlyList<string> ValidateMediaFiles(this MediaRepository value)`

Validates all media files contained within the repository and returns a list of error messages.  
- **Parameters**:  
  - `value`: The repository whose media files are to be validated.  
- **Return value**: A list of error strings; empty if all files are valid.  
- **Throws**: None.

### `public static bool IsValidId(string id)`

Checks whether a string is a valid identifier for a media item.  
- **Parameters**:  
  - `id`: The identifier to validate.  
- **Return value**: `true` if the identifier matches the expected pattern; otherwise `false`.  
- **Throws**: None.

### `public static bool IsValidFilePath(string path)`

Validates that a file path is syntactically correct and points to an existing file.  
- **Parameters**:  
  - `path`: The file path to validate.  
- **Return value**: `true` if the path is valid; otherwise `false`.  
- **Throws**: None.

### `public static bool IsValidDate(DateTime date)`

Ensures that a `DateTime` value is within an acceptable range (e.g., not in the future).  
- **Parameters**:  
  - `date`: The date to validate.  
- **Return value**: `true` if the date is valid; otherwise `false`.  
- **Throws**: None.

### `public static bool IsValidPositiveNumber(int number)`

Validates that an integer is strictly greater than zero.  
- **Parameters**:  
  - `number`: The integer to validate.  
- **Return value**: `true` if the number is positive; otherwise `false`.  
- **Throws**: None.

### `public static bool IsValidPositiveNumber(double number)`

Validates that a double is strictly greater than zero.  
- **Parameters**:  
  - `number`: The double to validate.  
- **Return value**: `true` if the number is positive; otherwise `false`.  
- **Throws**: None.

### `public static bool IsValidPositiveNumber(long number)`

Validates that a long integer is strictly greater than zero.  
- **Parameters**:  
  - `number`: The long to validate.  
- **Return value**: `true` if the number is positive; otherwise `false`.  
- **Throws**: None.

## Usage

