#!/bin/bash

# Add documentation to all test methods in MediaFileTests.cs

# Constructor_WithFilePath
sed -i '/public void Constructor_WithFilePath_SetsPropertiesFromFile()/i\
        /// <summary>\
        /// Tests that the constructor with a file path sets the FilePath, Name, and FileSize properties\
        /// from the actual file information.\
        /// </summary>' MediaFileTests.cs

# FilePath_WithValidFile
sed -i '/public void FilePath_WithValidFile_AcceptsPath()/i\
        /// <summary>\
        /// Tests that setting FilePath to a valid file path accepts the path and validates file existence.\
        /// </summary>' MediaFileTests.cs

# FilePath_WithNonexistentFile
sed -i '/public void FilePath_WithNonexistentFile_ThrowsException()/i\
        /// <summary>\
        /// Tests that setting FilePath to a nonexistent file path throws <see cref="InvalidMediaFileException"/>.\
        /// </summary>' MediaFileTests.cs

# FilePath_WithEmptyString
sed -i '/public void FilePath_WithEmptyString_ThrowsException()/i\
        /// <summary>\
        /// Tests that setting FilePath to an empty string throws <see cref="InvalidMediaFileException"/> with appropriate message.\
        /// </summary>' MediaFileTests.cs

# Extension_ReturnsFileExtension
sed -i '/public void Extension_ReturnsFileExtension()/i\
        /// <summary>\
        /// Tests that the Extension property returns the correct file extension from the FilePath.\
        /// </summary>' MediaFileTests.cs

# Name_ReturnsFileNameWithoutExtension
sed -i '/public void Name_ReturnsFileNameWithoutExtension()/i\
        /// <summary>\
        /// Tests that the Name property returns the filename without the extension.\
        /// </summary>' MediaFileTests.cs

# FileSize_ReturnsActualFileSize
sed -i '/public void FileSize_ReturnsActualFileSize()/i\
        /// <summary>\
        /// Tests that the FileSize property returns the actual file size in bytes.\
        /// </summary>' MediaFileTests.cs

# ValidateAsVideo_WithValidDimensions
sed -i '/public void ValidateAsVideo_WithValidDimensions_DoesNotThrow()/i\
        /// <summary>\
        /// Tests that ValidateAsVideo does not throw when all required video dimensions (Width, Height, Duration) are set.\
        /// </summary>' MediaFileTests.cs

# ValidateAsVideo_WithoutWidth
sed -i '/public void ValidateAsVideo_WithoutWidth_ThrowsException()/i\
        /// <summary>\
        /// Tests that ValidateAsVideo throws <see cref="InvalidMediaFileException"/> when Width is not set.\
        /// </summary>' MediaFileTests.cs

# ValidateAsVideo_WithoutHeight
sed -i '/public void ValidateAsVideo_WithoutHeight_ThrowsException()/i\
        /// <summary>\
        /// Tests that ValidateAsVideo throws <see cref="InvalidMediaFileException"/> when Height is not set.\
        /// </summary>' MediaFileTests.cs

# ValidateAsVideo_WithoutDuration
sed -i '/public void ValidateAsVideo_WithoutDuration_ThrowsException()/i\
        /// <summary>\
        /// Tests that ValidateAsVideo throws <see cref="InvalidMediaFileException"/> when Duration is not set.\
        /// </summary>' MediaFileTests.cs

# ValidateAsVideo_WithZeroDuration
sed -i '/public void ValidateAsVideo_WithZeroDuration_ThrowsException()/i\
        /// <summary>\
        /// Tests that ValidateAsVideo throws <see cref="InvalidMediaFileException"/> when Duration is set to TimeSpan.Zero.\
        /// </summary>' MediaFileTests.cs

# Metadata_CanStoreArbitraryKeyValuePairs
sed -i '/public void Metadata_CanStoreArbitraryKeyValuePairs()/i\
        /// <summary>\
        /// Tests that the Metadata dictionary can store arbitrary key-value pairs for additional media file properties.\
        /// </summary>' MediaFileTests.cs

# Description_CanBeSet
sed -i '/public void Description_CanBeSet()/i\
        /// <summary>\
        /// Tests that the Description property can be set and retrieved correctly.\
        /// </summary>' MediaFileTests.cs

# ModifiedAt_CanBeSet
sed -i '/public void ModifiedAt_CanBeSet()/i\
        /// <summary>\
        /// Tests that the ModifiedAt property can be set and retrieved correctly.\
        /// </summary>' MediaFileTests.cs

# MediaProperties_CanBeSetIndependently
sed -i '/public void MediaProperties_CanBeSetIndependently()/i\
        /// <summary>\
        /// Tests that various media properties (VideoCodec, AudioCodec, FrameRate, Bitrate, etc.) can be set independently.\
        /// </summary>' MediaFileTests.cs

# Id_IsUniqueForEachInstance
sed -i '/public void Id_IsUniqueForEachInstance()/i\
        /// <summary>\
        /// Tests that each <see cref="MediaFile"/> instance gets a unique ID.\
        /// </summary>' MediaFileTests.cs

# FilePath_NormalizesToAbsolutePath
sed -i '/public void FilePath_NormalizesToAbsolutePath()/i\
        /// <summary>\
        /// Tests that the FilePath property normalizes to an absolute path.\
        /// </summary>' MediaFileTests.cs

echo "Documentation added to all test methods"
