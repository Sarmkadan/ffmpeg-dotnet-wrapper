# FileOperationExceptionExtensions

Provides extension methods for `FileOperationException` to facilitate richer error handling and diagnostic reporting when file operations fail.

## API

### `public static string GetFileName(FileOperationException exception)`

Extracts the file name from the file path embedded in the exception message.

- **Parameters**
  `exception` – The `FileOperationException` instance from which to extract the file name.
- **Return value**
  The extracted file name as a string, or `null` if the path cannot be parsed.
- **Exceptions**
  Throws `ArgumentNullException` if `exception` is `null`.

---

### `public static bool HasFilePath(FileOperationException exception)`

Determines whether the exception message contains a recognizable file path.

- **Parameters**
  `exception` – The `FileOperationException` instance to inspect.
- **Return value**
  `true` if a valid file path is found; otherwise, `false`.
- **Exceptions**
  Throws `ArgumentNullException` if `exception` is `null`.

---

### `public static string ToLogString(FileOperationException exception)`

Formats the exception into a standardized log-friendly string including the file path, operation, and additional context.

- **Parameters**
  `exception` – The `FileOperationException` instance to format.
- **Return value**
  A non-null string representation suitable for logging.
- **Exceptions**
  Throws `ArgumentNullException` if `exception` is `null`.

---
### `public static FileOperationException WithAdditionalInfo(FileOperationException exception, string key, string value)`

Creates a new `FileOperationException` with the same data as the original but appends additional key-value context to the exception message.

- **Parameters**
  `exception` – The original exception.
  `key` – The context key to add.
  `value` – The context value to add.
- **Return value**
  A new `FileOperationException` instance with the additional information included.
- **Exceptions**
  Throws `ArgumentNullException` if `exception` is `null` or if `key` is `null`.
  Throws `ArgumentException` if `key` is empty.

## Usage
