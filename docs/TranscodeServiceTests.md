# TranscodeServiceTests

Test suite for the `TranscodeService` class, verifying transcoding operations, parameter validation, and error propagation when interacting with the underlying FFmpeg service.

## API

### TranscodeServiceTests

```csharp
public TranscodeServiceTests()
```

Initializes a new instance of the test class. Sets up mock dependencies required for testing `TranscodeService` behavior in isolation.

---

### TranscodeToWebAsync_ShouldCallFFmpegService_WithCorrectSettings

```csharp
public async Task TranscodeToWebAsync_ShouldCallFFmpegService_WithCorrectSettings()
```

Verifies that `TranscodeToWebAsync` invokes the FFmpeg service with the expected encoding parameters for web-optimized output.

**Returns:** `Task` — completes when the assertion passes.

**Throws:** `Xunit.Sdk.XunitException` — if the FFmpeg service is not called, or is called with incorrect settings.

---

### TranscodeToWebAsync_ShouldPropagateException_WhenFFmpegServiceThrows

```csharp
public async Task TranscodeToWebAsync_ShouldPropagateException_WhenFFmpegServiceThrows()
```

Ensures exceptions thrown by the FFmpeg service during web transcoding are propagated to the caller without wrapping or suppression.

**Returns:** `Task` — completes when the exception propagation is confirmed.

**Throws:** `Xunit.Sdk.XunitException` — if the original exception is not thrown or is altered.

---

### TranscodeWithBitrateAsync_ShouldThrowException_WhenBitrateIsOutOfRange

```csharp
public async Task TranscodeWithBitrateAsync_ShouldThrowException_WhenBitrateIsOutOfRange()
```

Confirms that `TranscodeWithBitrateAsync` throws an `ArgumentOutOfRangeException` when the provided bitrate falls outside the supported range.

**Returns:** `Task` — completes when the expected exception is thrown.

**Throws:** `Xunit.Sdk.XunitException` — if no exception is thrown or an incorrect exception type is thrown.

---

### ResizeVideoAsync_ShouldThrowException_WhenDimensionsAreZero

```csharp
public async Task ResizeVideoAsync_ShouldThrowException_WhenDimensionsAreZero()
```

Validates that `ResizeVideoAsync` throws an `ArgumentException` when width or height parameters are zero.

**Returns:** `Task` — completes when the expected exception is thrown.

**Throws:** `Xunit.Sdk.XunitException` — if no exception is thrown or an incorrect exception type is thrown.

---

### ExtractAudioAsync_ShouldThrowException_WhenInputIsNotVideo

```csharp
public async Task ExtractAudioAsync_ShouldThrowException_WhenInputIsNotVideo()
```

Checks that `ExtractAudioAsync` throws an `InvalidOperationException` when the input file does not contain a video stream.

**Returns:** `Task` — completes when the expected exception is thrown.

**Throws:** `Xunit.Sdk.XunitException` — if no exception is thrown or an incorrect exception type is thrown.

## Usage

### Running the test suite

```csharp
using Xunit;
using FFmpegDotNetWrapper.Tests;

public class TranscodeServiceTestRunner
{
    [Fact]
    public async Task RunAllTranscodeServiceTests()
    {
        var tests = new TranscodeServiceTests();
        
        await tests.TranscodeToWebAsync_ShouldCallFFmpegService_WithCorrectSettings();
        await tests.TranscodeToWebAsync_ShouldPropagateException_WhenFFmpegServiceThrows();
        await tests.TranscodeWithBitrateAsync_ShouldThrowException_WhenBitrateIsOutOfRange();
        await tests.ResizeVideoAsync_ShouldThrowException_WhenDimensionsAreZero();
        await tests.ExtractAudioAsync_ShouldThrowException_WhenInputIsNotVideo();
    }
}
```

### Testing a specific validation scenario

```csharp
using Xunit;
using FFmpegDotNetWrapper.Tests;

public class BitrateValidationTests
{
    [Fact]
    public async Task BitrateOutOfRange_ThrowsArgumentOutOfRangeException()
    {
        var tests = new TranscodeServiceTests();
        
        await Assert.ThrowsAsync<Xunit.Sdk.XunitException>(() => 
            tests.TranscodeWithBitrateAsync_ShouldThrowException_WhenBitrateIsOutOfRange());
    }
}
```

## Notes

- All test methods are asynchronous and return `Task`; they must be awaited to ensure proper execution and assertion evaluation.
- Tests rely on mocked FFmpeg service implementations; no actual FFmpeg binary is invoked during test runs.
- Each test is independent and can be executed in any order; no shared state exists between test methods.
- The class is not thread-safe; instantiate a new `TranscodeServiceTests` per test run or test class to avoid cross-test contamination.
- Exception assertions verify both type and propagation behavior; changes to exception wrapping in the service will cause these tests to fail.
