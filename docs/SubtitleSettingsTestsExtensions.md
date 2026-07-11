# SubtitleSettingsTestsExtensions

The `SubtitleSettingsTestsExtensions` class provides a set of static extension methods and helper utilities designed to streamline unit testing for the `SubtitleSettings` type within the `ffmpeg-dotnet-wrapper` library. It facilitates the creation of default test instances, validates file path acceptance logic for supported subtitle formats (SRT and ASS), verifies object validity states, ensures exception handling for invalid inputs, and confirms that configuration objects produce deep copies rather than shallow references.

## API

### `WithDefaultSettings`
Creates a new instance of `SubtitleSettings` populated with standard default values suitable for immediate use in test scenarios.
*   **Parameters**: None.
*   **Return Value**: A new `SubtitleSettings` instance.
*   **Exceptions**: None.

### `ShouldThrowWhenPathInvalid`
Verifies that the target `SubtitleSettings` instance or associated factory method throws an exception when provided with an invalid file path.
*   **Parameters**: The method operates on the context of the test subject; specific path validation logic is internal to the assertion.
*   **Return Value**: `void`.
*   **Exceptions**: Throws an assertion failure if the expected exception is not raised during path validation.

### `ShouldAcceptSrtFile`
Asserts that the `SubtitleSettings` configuration correctly accepts and processes a file path ending with the `.srt` extension.
*   **Parameters**: Implicitly requires a valid SRT file path context within the test execution.
*   **Return Value**: `void`.
*   **Exceptions**: Throws an assertion failure if the SRT file format is rejected or causes an unexpected error.

### `ShouldAcceptAssFile`
Asserts that the `SubtitleSettings` configuration correctly accepts and processes a file path ending with the `.ass` extension.
*   **Parameters**: Implicitly requires a valid ASS file path context within the test execution.
*   **Return Value**: `void`.
*   **Exceptions**: Throws an assertion failure if the ASS file format is rejected or causes an unexpected error.

### `ShouldBeValid`
Validates the current state of the `SubtitleSettings` instance, ensuring all required properties are set and the object is in a consistent state for FFmpeg processing.
*   **Parameters**: None (operates on the extended instance).
*   **Return Value**: `void`.
*   **Exceptions**: Throws an assertion failure if the instance state is deemed invalid.

### `ShouldProduceIndependentCopy`
Verifies that a copy operation performed on the `SubtitleSettings` instance results in a new object that does not share mutable reference types with the original.
*   **Parameters**: None (operates on the extended instance).
*   **Return Value**: `void`.
*   **Exceptions**: Throws an assertion failure if modifying the copy affects the original instance, indicating a shallow copy error.

## Usage

### Creating and Validating Default Settings
This example demonstrates how to generate a default settings object and immediately verify its validity state within a test method.

```csharp
using FFmpeg.Wrapper.Settings;
using FFmpeg.Wrapper.Tests.Extensions;

[Test]
public void DefaultSettings_ShouldBeValid()
{
    // Arrange & Act
    var settings = SubtitleSettingsTestsExtensions.WithDefaultSettings();

    // Assert
    settings.ShouldBeValid();
}
```

### Verifying Format Support and Isolation
This example illustrates testing specific file format acceptance (SRT) and ensuring that configuration cloning produces an independent copy.

```csharp
using FFmpeg.Wrapper.Settings;
using FFmpeg.Wrapper.Tests.Extensions;

[Test]
public void SubtitleSettings_ShouldHandleSrtAndCloneCorrectly()
{
    // Arrange
    var original = SubtitleSettingsTestsExtensions.WithDefaultSettings();
    
    // Act & Assert - Format Acceptance
    original.ShouldAcceptSrtFile();
    
    // Act & Assert - Independence Check
    // This ensures that internal lists or nested objects are deeply copied
    original.ShouldProduceIndependentCopy();
}
```

## Notes

*   **Thread Safety**: As this class consists entirely of static methods that typically operate on provided instances or create new ones without maintaining internal mutable static state, it is generally thread-safe. However, the `SubtitleSettings` instances returned or validated by these methods are not inherently thread-safe; concurrent modification of a single `SubtitleSettings` instance across multiple threads remains the responsibility of the caller.
*   **Edge Cases**: The `ShouldThrowWhenPathInvalid` method relies on the underlying validation logic to define "invalid." Testers should ensure paths containing null characters, excessively long strings, or unauthorized characters are covered in separate specific tests if the default validation behavior needs verification against OS-specific constraints.
*   **Dependency**: These methods are intended for use within a testing framework (such as NUnit or xUnit) that supports fluent assertion styles. Calling them outside of a test context may result in silent failures or unhandled assertion exceptions depending on the configured test runner.
