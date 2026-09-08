# Throw

`Throw` is a static exception helper in the `FFmpegDotnetWrapper.Exceptions` namespace. It provides argument guards, creates `FFmpegException` subclasses, and enriches exception instances with diagnostic context. Context methods return the same exception instance so they can be used while constructing or throwing an exception.

## Public API

| Member | Purpose |
| --- | --- |
| `void IfNull<T>(T? value, string paramName) where T : class` | Throws `ArgumentNullException` when `value` is `null`. |
| `void IfNullOrEmpty(string? value, string paramName)` | Throws `ArgumentException` when `value` is `null` or empty. |
| `void IfNullOrWhitespace(string? value, string paramName)` | Throws `ArgumentException` when `value` is `null`, empty, or consists only of whitespace. |
| `T WithCliContext<T>(T exception, string? cliCommand, int? exitCode = null, string? errorOutput = null) where T : FFmpegException` | Adds non-null CLI values to `exception.Data` under `cliCommand`, `exitCode`, and `errorOutput`. Also sets `ExitCode` and `ErrorOutput` on the exception when supplied. |
| `T WithConfigurationContext<T>(T exception, string? configurationKey) where T : FFmpegException` | Adds a non-null value to `exception.Data["configurationKey"]`; also sets `ConfigurationKey` when the exception is an `InvalidOperationConfigurationException`. |
| `T WithFileContext<T>(T exception, string? filePath) where T : FFmpegException` | Adds a non-null value to `exception.Data["filePath"]`; also sets `FilePath` when the exception is a `FileOperationException`. |
| `T WithRepositoryContext<T>(T exception, string? repositoryName) where T : FFmpegException` | Adds a non-null value to `exception.Data["repositoryName"]`; also sets `RepositoryName` when the exception is a `RepositoryException`. |
| `T WithServiceContext<T>(T exception, string? serviceName) where T : FFmpegException` | Adds a non-null value to `exception.Data["serviceName"]`; also sets `ServiceName` when the exception is a `ServiceException`. |
| `T WithMediaFileContext<T>(T exception, string? filePath) where T : FFmpegException` | Adds a non-null value to `exception.Data["filePath"]`; also sets `FilePath` when the exception is an `InvalidMediaFileException`. |
| `T WithValidationContext<T>(T exception, Dictionary<string, string[]>? validationErrors) where T : FFmpegException` | Adds a non-null, non-empty dictionary to `exception.Data["validationErrors"]`; also sets `ValidationErrors` when the exception is a `ValidationException`. |
| `T New<T>(string message, Exception? innerException = null) where T : FFmpegException` | Creates `T` through its `(string)` or `(string, Exception)` constructor. An empty message causes `ArgumentException`; construction can fail if the matching constructor is unavailable. |

Every context method throws `ArgumentNullException` when `exception` is `null`. Optional context values that are `null` are ignored.

## Usage

```csharp
using FFmpegDotnetWrapper.Exceptions;

static void ValidateInput(string? inputPath)
{
    Throw.IfNullOrWhitespace(inputPath, nameof(inputPath));
}

var exception = Throw.New<FileOperationException>("Could not open the input file.");
throw Throw.WithFileContext(exception, "/media/input.mp4");
```
