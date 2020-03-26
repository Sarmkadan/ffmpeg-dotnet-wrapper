# FileUtilitiesTests

Test suite for the `FileUtilities` class, validating file path validation, input/output file verification, and file extension extraction logic. These tests ensure that path handling adheres to security requirements (absolute paths only, no traversal) and correctly identifies valid files and directories for FFmpeg operations.

## API

### `public FileUtilitiesTests()`
Initializes a new instance of the test class. No parameters. Does not throw.

### `public void IsValidFilePath_WithAbsolutePath_ReturnsTrue`
Verifies that an absolute file path passes validation. No parameters. Returns `void`. Does not throw.

### `public void IsValidFilePath_WithRelativePath_ReturnsFalse`
Verifies that a relative file path fails validation. No parameters. Returns `void`. Does not throw.

### `public void IsValidFilePath_WithDirectoryTraversal_ReturnsFalse`
Verifies that paths containing directory traversal sequences (e.g., `..`) fail validation. No parameters. Returns `void`. Does not throw.

### `public void IsValidFilePath_WithNull_ReturnsFalse`
Verifies that a `null` path input fails validation. No parameters. Returns `void`. Does not throw.

### `public void IsValidFilePath_WithEmptyString_ReturnsFalse`
Verifies that an empty string path fails validation. No parameters. Returns `void`. Does not throw.

### `public void IsValidFilePath_WithWhitespace_ReturnsFalse`
Verifies that a whitespace-only string path fails validation. No parameters. Returns `void`. Does not throw.

### `public void IsValidFilePath_WithTildaExpansion_ReturnsFalse`
Verifies that paths containing tilde (`~`) for home directory expansion fail validation. No parameters. Returns `void`. Does not throw.

### `public void IsValidFilePath_WithEnvironmentVariable_ReturnsFalse`
Verifies that paths containing environment variable references (e.g., `%VAR%` or `$VAR`) fail validation. No parameters. Returns `void`. Does not throw.

### `public void IsValidInputFile_WithValidFile_ReturnsTrue`
Verifies that an existing, absolute file path passes input file validation. No parameters. Returns `void`. Does not throw.

### `public void IsValidInputFile_WithNonexistentFile_ReturnsFalse`
Verifies that a non-existent file path fails input file validation. No parameters. Returns `void`. Does not throw.

### `public void IsValidInputFile_WithRelativePath_ReturnsFalse`
Verifies that a relative path fails input file validation even if the file exists. No parameters. Returns `void`. Does not throw.

### `public void IsValidInputFile_WithNull_ReturnsFalse`
Verifies that a `null` input fails input file validation. No parameters. Returns `void`. Does not throw.

### `public void IsValidOutputPath_WithValidDirectory_ReturnsTrue`
Verifies that a path within an existing directory passes output path validation. No parameters. Returns `void`. Does not throw.

### `public void IsValidOutputPath_WithNonexistentDirectoryAndCreateFlag_ReturnsTrue`
Verifies that a path within a non-existent directory passes validation when directory creation is permitted. No parameters. Returns `void`. Does not throw.

### `public void IsValidOutputPath_WithNonexistentDirectoryNoCreateFlag_ReturnsFalse`
Verifies that a path within a non-existent directory fails validation when directory creation is not permitted. No parameters. Returns `void`. Does not throw.

### `public void IsValidOutputPath_WithRelativePath_ReturnsFalse`
Verifies that a relative output path fails validation. No parameters. Returns `void`. Does not throw.

### `public void IsValidOutputPath_WithNull_ReturnsFalse`
Verifies that a `null` output path fails validation. No parameters. Returns `void`. Does not throw.

### `public void GetFileExtension_WithValidFile_ReturnsExtensionWithoutDot`
Verifies that the extension is returned without the leading dot for a standard file name. No parameters. Returns `void`. Does not throw.

### `public void GetFileExtension_WithDifferentExtension_ReturnsCorrectExtension`
Verifies that various extensions (e.g., `.mp4`, `.mkv`, `.wav`) are correctly extracted without the dot. No parameters. Returns `void`. Does not throw.

## Usage

```csharp
[Fact]
public void IsValidInputFile_WithValidFile_ReturnsTrue()
{
    // Arrange
    var tempFile = Path.GetTempFileName();
    try
    {
        // Act
        var result = FileUtilities.IsValidInputFile(tempFile);

        // Assert
        Assert.True(result);
    }
    finally
    {
        File.Delete(tempFile);
    }
}
```

```csharp
[Fact]
public void IsValidOutputPath_WithNonexistentDirectoryAndCreateFlag_ReturnsTrue()
{
    // Arrange
    var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    var outputPath = Path.Combine(tempDir, "output.mp4");

    // Act
    var result = FileUtilities.IsValidOutputPath(outputPath, createDirectory: true);

    // Assert
    Assert.True(result);
    Assert.True(Directory.Exists(tempDir));

    // Cleanup
    Directory.Delete(tempDir, recursive: true);
}
```

## Notes

- All path validation methods enforce absolute paths only; relative paths are rejected to prevent working directory ambiguity.
- Directory traversal sequences (`..`), tilde expansion, and environment variables are explicitly rejected to mitigate path injection risks.
- `IsValidOutputPath` with `createDirectory: true` will create intermediate directories; the caller is responsible for cleanup in test scenarios.
- `GetFileExtension` returns the extension without the leading dot (e.g., `mp4` not `.mp4`); it does not validate whether the extension is a known media format.
- Test methods are stateless and thread-safe; they rely on temporary file system resources that are isolated per test execution.
