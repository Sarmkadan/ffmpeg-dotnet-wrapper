# Testing Guide for FFmpeg .NET Wrapper

This guide explains how to unit test code that depends on the FFmpeg .NET Wrapper library.

## Testing Dependencies

The library provides several interfaces and test doubles to facilitate unit testing:

### FakeFFmpegProcessRunner

Located at: `src/Services/FakeFFmpegProcessRunner.cs`

This test double implements `IFFmpegProcessRunner` and allows you to:
- Pre-define responses for FFmpeg command executions
- Verify which commands were called and with what arguments
- Simulate different exit codes and output scenarios

Example usage:
```csharp
var fakeRunner = new FakeFFmpegProcessRunner();
// Configure expected command and output
fakeRunner.AddCommandResult(
    new ProcessResult(0, "output", string.Empty));
// Inject into your service under test
var service = new FFmpegService(fakeRunner);
```

### Mocking Interfaces

The library depends on several interfaces that should be mocked in unit tests:

1. `IFFmpegService` - Core FFmpeg operations
2. `IMediaRepository` - Media file metadata storage
3. `IOperationRepository` - Operation tracking and persistence

Use your preferred mocking framework (e.g., Moq, NSubstitute) to create mocks:
```csharp
var ffmpegServiceMock = new Mock<IFFmpegService>();
var mediaRepositoryMock = new Mock<IMediaRepository>();
var operationRepositoryMock = new Mock<IOperationRepository>();
```

### CacheService

The `CacheService` can be used with an in-memory implementation for testing:
```csharp
// Use MemoryCache for testing
var memoryCache = new MemoryCache(new MemoryCacheOptions());
var cacheService = new CacheService(memoryCache);
```

## Running Existing Tests

The solution includes a test project located at:
`tests/ffmpeg-dotnet-wrapper.Tests/ffmpeg-dotnet-wrapper.Tests.csproj`

To run the tests:
```bash
dotnet test tests/ffmpeg-dotnet-wrapper.Tests/ffmpeg-dotnet-wrapper.Tests.csproj
```

## Best Practices

1. **Isolate External Dependencies**: Always mock or fake external services (FFmpeg process, repositories, etc.)
2. **Verify Interactions**: Check that your code calls the expected methods with correct parameters
3. **Test Edge Cases**: Simulate various FFmpeg exit codes, output formats, and error conditions
4. **Use In-Memory Implementations**: For repositories and caches, prefer in-memory versions over mocks when complex state is needed
5. **Keep Tests Fast**: Avoid actual FFmpeg process execution in unit tests; use fakes/mocks instead

## Example Test Structure

```csharp
public class FFmpegServiceTests
{
    private Mock<IFFmpegService> _ffmpegServiceMock;
    private Mock<IMediaRepository> _mediaRepositoryMock;
    private Mock<IOperationRepository> _operationRepositoryMock;
    private FakeFFmpegProcessRunner _fakeRunner;
    private FFmpegService _service;

    [SetUp]
    public void Setup()
    {
        _ffmpegServiceMock = new Mock<IFFmpegService>();
        _mediaRepositoryMock = new Mock<IMediaRepository>();
        _operationRepositoryMock = new Mock<IOperationRepository>();
        _fakeRunner = new FakeFFmpegProcessRunner();
        
        _service = new FFmpegService(
            _ffmpegServiceMock.Object,
            _mediaRepositoryMock.Object,
            _operationRepositoryMock.Object,
            _fakeRunner);
    }

    [Test]
    public void ConvertVideo_ShouldCallFFmpegWithCorrectArguments()
    {
        // Arrange
        var inputFile = "input.mp4";
        var outputFile = "output.avi";
        _fakeRunner.AddCommandResult(new ProcessResult(0, string.Empty, string.Empty));
        
        // Act
        _service.ConvertVideo(inputFile, outputFile, VideoFormat.Avi);
        
        // Assert
        _fakeRunner.ReceivedCommandShouldContain($"-i {inputFile}");
        _fakeRunner.ReceivedCommandShouldContain($"-f avi {outputFile}");
    }
}
```

Remember to add the test project to your solution if it's not already included, and ensure all test dependencies are restored before running.