# PathValidation

`PathValidation` provides static helpers for resolving file-system paths and ensuring that they remain within an allowed base directory. It is intended to reject path traversal outside that boundary, optionally verify that a file exists, and create an output directory when necessary.

Paths are normalized to absolute paths using `Path.GetFullPath`. Relative paths are therefore resolved against the process's current working directory, not relative to `baseDirectory`. Directory results include a trailing platform-specific directory separator. Containment comparisons are case-insensitive.

## Public API

| Member | Purpose | Return value |
| --- | --- | --- |
| `ValidatePathWithinBaseDirectory(string path, string baseDirectory, string paramName)` | Resolves and normalizes a file-system path and verifies that it is within `baseDirectory`. | The resolved absolute path. |
| `ValidateDirectoryWithinBaseDirectory(string directoryPath, string baseDirectory, string paramName)` | Resolves and normalizes a directory path, adds a trailing directory separator, and verifies that it is within `baseDirectory`. | The resolved absolute directory path with a trailing separator. |
| `ValidateExistingFileWithinBaseDirectory(string filePath, string baseDirectory, string paramName)` | Validates containment and then verifies that the resolved file exists. | The resolved absolute file path. |
| `ValidateOutputPathWithinBaseDirectory(string outputPath, string baseDirectory, string paramName)` | Validates an output file path using the same rules as `ValidatePathWithinBaseDirectory`; it does not create the file. | The resolved absolute output file path. |
| `ValidateOutputDirectoryWithinBaseDirectory(string outputDirectory, string baseDirectory, string paramName)` | Validates an output directory and creates it, including missing parent directories, if it does not exist. | The resolved absolute output directory path with a trailing separator. |

## Example

```csharp
using System.IO;
using FFmpegDotnetWrapper.Utilities;

string mediaDirectory = Path.GetFullPath("media");
string inputPath = Path.Combine(mediaDirectory, "input.mp4");
string outputDirectory = Path.Combine(mediaDirectory, "converted");

string validatedInput = PathValidation.ValidateExistingFileWithinBaseDirectory(
    inputPath,
    mediaDirectory,
    nameof(inputPath));

string validatedOutputDirectory = PathValidation.ValidateOutputDirectoryWithinBaseDirectory(
    outputDirectory,
    mediaDirectory,
    nameof(outputDirectory));

string validatedOutput = PathValidation.ValidateOutputPathWithinBaseDirectory(
    Path.Combine(validatedOutputDirectory, "output.mp4"),
    mediaDirectory,
    "outputPath");
```

## Exceptions

| Exception | When it is thrown |
| --- | --- |
| `ArgumentNullException` | A path argument or `baseDirectory` is `null`, or `paramName` is `null`. |
| `ArgumentException` | `paramName` is empty or whitespace; an input path is invalid; a resolved path is outside `baseDirectory`; or an output directory cannot be created because of an I/O, permissions, or path-length error. Invalid-path exceptions handled by the class are wrapped as `ArgumentException`. |
| `NotSupportedException` | Path normalization of `baseDirectory` encounters an unsupported path format. Invalid formats encountered while resolving the path being validated are wrapped as `ArgumentException`. |
| `PathTooLongException` | Normalization of `baseDirectory` exceeds the platform path-length limit. Path-length errors encountered while resolving the path being validated are wrapped as `ArgumentException`. |
| `FileNotFoundException` | `ValidateExistingFileWithinBaseDirectory` resolves a safe path, but no file exists at that location. |

`ValidateOutputDirectoryWithinBaseDirectory` wraps `IOException`, `UnauthorizedAccessException`, and `PathTooLongException` raised while creating a missing directory in an `ArgumentException` whose parameter name is the supplied `paramName`.
