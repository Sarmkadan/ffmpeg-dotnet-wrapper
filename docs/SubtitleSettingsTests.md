# SubtitleSettingsTests

Unit tests for the `SubtitleSettings` class, verifying validation logic, file handling, and configuration behavior for subtitle processing in the FFmpeg .NET wrapper.

## API

### `public SubtitleSettingsTests`
Constructor for the test fixture. Initializes a new instance of the `SubtitleSettingsTests` class to run tests against subtitle configuration scenarios.

### `public void Dispose`
Releases all resources used by the current test. Called automatically by the test framework after each test method completes.

### `public void Constructor_DefaultValues_AreCorrect`
Verifies that a newly created `SubtitleSettings` instance has default values for all properties, ensuring predictable initial state.

### `public void SubtitlePath_WithExistingSrtFile_AcceptsPath`
Ensures that a valid `.srt` subtitle file path is accepted and stored correctly in the `SubtitlePath` property.

### `public void SubtitlePath_WithExistingAssFile_AcceptsPath`
Ensures that a valid `.ass` subtitle file path is accepted and stored correctly in the `SubtitlePath` property.

### `public void SubtitlePath_WithNonexistentFile_ThrowsException`
Confirms that providing a non-existent file path to `SubtitlePath` results in an exception, enforcing file existence validation.

### `public void SubtitlePath_WithUnsupportedExtension_ThrowsException`
Validates that attempting to set an unsupported subtitle file extension (e.g., `.txt`) throws an exception, restricting to `.srt` or `.ass`.

### `public void SubtitlePath_WithEmptyString_ThrowsException`
Checks that assigning an empty string to `SubtitlePath` throws an exception, enforcing non-empty path requirements.

### `public void CharEncoding_WithEmptyValue_ThrowsException`
Ensures that setting an empty string for `CharEncoding` throws an exception, requiring a valid character encoding specification.

### `public void FontSize_OutsideValidRange_ThrowsOnValidate`
Confirms that attempting to set a `FontSize` outside the valid range (e.g., less than 1 or greater than 100) throws an exception during validation.

### `public void Validate_WithValidSettings_DoesNotThrow`
Verifies that calling `Validate()` on a properly configured `SubtitleSettings` instance does not throw, indicating all constraints are satisfied.

### `public void Clone_ProducesIndependentCopy`
Asserts that cloning a `SubtitleSettings` instance produces a deep copy, ensuring modifications to the clone do not affect the original.
