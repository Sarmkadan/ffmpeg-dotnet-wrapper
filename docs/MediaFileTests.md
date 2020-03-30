# MediaFileTests

Unit tests for the `MediaFile` class, validating file handling, property management, and validation logic for media files.

## API

### `public MediaFileTests`
Constructor for the test fixture. Initializes a new instance of the `MediaFileTests` class to run tests against `MediaFile` functionality.

### `public void Constructor_DefaultValues_CreatesNewInstance`
Verifies that a new `MediaFile` instance is created with default property values when constructed without parameters.

### `public void Constructor_WithFilePath_SetsPropertiesFromFile`
Ensures that constructing a `MediaFile` with a valid file path populates its properties (e.g., `Name`, `Extension`, `FileSize`) from the file system.

### `public void FilePath_WithValidFile_AcceptsPath`
Confirms that setting a valid file path on a `MediaFile` instance updates the `FilePath` property without throwing an exception.

### `public void FilePath_WithNonexistentFile_ThrowsException`
Validates that attempting to set a `FilePath` to a non-existent file throws an appropriate exception.

### `public void FilePath_WithEmptyString_ThrowsException`
Ensures that assigning an empty string to `FilePath` results in an exception being thrown.

### `public void Extension_ReturnsFileExtension`
Checks that the `Extension` property returns the correct file extension derived from the `FilePath`.

### `public void Name_ReturnsFileNameWithoutExtension`
Confirms that the `Name` property returns the file name without its extension.

### `public void FileSize_ReturnsActualFileSize`
Validates that the `FileSize` property returns the actual size of the file in bytes.

### `public void ValidateAsVideo_WithValidDimensions_DoesNotThrow`
Ensures that calling `ValidateAsVideo` on a `MediaFile` with valid video dimensions (width and height) does not throw an exception.

### `public void ValidateAsVideo_WithoutWidth_ThrowsException`
Confirms that `ValidateAsVideo` throws an exception when the video width is missing.

### `public void ValidateAsVideo_WithoutHeight_ThrowsException`
Ensures that `ValidateAsVideo` throws an exception when the video height is missing.

### `public void ValidateAsVideo_WithoutDuration_ThrowsException`
Validates that `ValidateAsVideo` throws an exception when the video duration is missing.

### `public void ValidateAsVideo_WithZeroDuration_ThrowsException`
Ensures that `ValidateAsVideo` throws an exception when the video duration is zero.

### `public void Metadata_CanStoreArbitraryKeyValuePairs`
Confirms that the `Metadata` property allows storing arbitrary key-value pairs without restriction.

### `public void Description_CanBeSet`
Validates that the `Description` property can be set to any string value.

### `public void ModifiedAt_CanBeSet`
Ensures that the `ModifiedAt` property can be updated to reflect the last modification time of the file.

### `public void MediaProperties_CanBeSetIndependently`
Confirms that individual media properties (e.g., `Width`, `Height`, `Duration`) can be set independently of each other.

### `public void Id_IsUniqueForEachInstance`
Verifies that each `MediaFile` instance is assigned a unique `Id` upon creation.

### `public void FilePath_NormalizesToAbsolutePath`
Ensures that the `FilePath` property normalizes relative paths to absolute paths when set.

## Usage
