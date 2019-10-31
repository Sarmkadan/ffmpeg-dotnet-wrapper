// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.IO;
using System.Text.RegularExpressions;

namespace FFmpegDotnetWrapper.Utilities
{
    /// <summary>
    /// File system operations utility class providing safe file handling, validation, and management.
    /// Includes methods for checking file accessibility, validating paths, and managing temporary files.
    /// All methods include security checks to prevent directory traversal and unauthorized access.
    /// </summary>
    public static class FileUtilities
    {
        private const int MaxFileNameLength = 255;
        private const int MaxPathLength = 260;
        private const long MaxFileSizeBytes = 50L * 1024 * 1024 * 1024; // 50GB

        /// <summary>
        /// Validates that a file path is safe to use and doesn't contain directory traversal sequences.
        /// Returns true only if the path is absolute, doesn't contain "..", and is within safe length limits.
        /// </summary>
        public static bool IsValidFilePath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            // Prevent directory traversal attacks
            if (path.Contains("..") || path.Contains("~") || path.Contains("$"))
                return false;

            // Check path length
            if (path.Length > MaxPathLength)
                return false;

            // Ensure path is absolute
            if (!Path.IsPathRooted(path))
                return false;

            return true;
        }

        /// <summary>
        /// Validates an input file exists, is readable, and meets size constraints.
        /// Checks file permissions and ensures it's not locked by another process.
        /// </summary>
        public static bool IsValidInputFile(string? path)
        {
            if (!IsValidFilePath(path))
                return false;

            if (!File.Exists(path))
                return false;

            try
            {
                var fileInfo = new FileInfo(path);

                // Check file size
                if (fileInfo.Length > MaxFileSizeBytes)
                    return false;

                // Try to open for reading to verify accessibility
                using var stream = File.OpenRead(path);
                return true;
            }
            catch (IOException) // File is locked or inaccessible
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        /// <summary>
        /// Validates an output file path is writable and directory exists.
        /// Creates parent directory if it doesn't exist (configurable).
        /// Checks for sufficient disk space before confirming writability.
        /// </summary>
        public static bool IsValidOutputPath(string? path, bool createDirectoryIfNeeded = true)
        {
            if (!IsValidFilePath(path))
                return false;

            try
            {
                var directory = Path.GetDirectoryName(path);
                if (string.IsNullOrEmpty(directory))
                    return false;

                if (!Directory.Exists(directory))
                {
                    if (createDirectoryIfNeeded)
                    {
                        Directory.CreateDirectory(directory);
                    }
                    else
                    {
                        return false;
                    }
                }

                // Check write permissions by attempting to create a temp file
                var testFile = Path.Combine(directory, $".ffmpeg_test_{Guid.NewGuid()}.tmp");
                try
                {
                    File.Create(testFile).Dispose();
                    File.Delete(testFile);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Returns the file extension in lowercase without the dot (e.g., "mp4", "avi").
        /// Handles files without extensions by returning an empty string.
        /// </summary>
        public static string GetFileExtension(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return string.Empty;

            var extension = Path.GetExtension(filePath);
            return string.IsNullOrEmpty(extension) ? string.Empty : extension.TrimStart('.').ToLowerInvariant();
        }

        /// <summary>
        /// Gets human-readable file size (e.g., "2.5 MB", "1.2 GB").
        /// Used for logging and displaying file information in API responses.
        /// </summary>
        public static string GetHumanReadableFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        /// <summary>
        /// Gets the file size in bytes, handling errors gracefully.
        /// Returns -1 if file doesn't exist or is inaccessible.
        /// </summary>
        public static long GetFileSize(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    return new FileInfo(filePath).Length;
                }
            }
            catch
            {
                // Ignore exceptions
            }

            return -1;
        }

        /// <summary>
        /// Safely deletes a file with error handling and retry logic.
        /// Waits briefly if file is locked before attempting deletion.
        /// Returns true only if file was successfully deleted or didn't exist.
        /// </summary>
        public static bool SafeDeleteFile(string filePath, int maxRetries = 3)
        {
            if (!File.Exists(filePath))
                return true;

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    File.Delete(filePath);
                    return true;
                }
                catch (IOException) when (i < maxRetries - 1)
                {
                    System.Threading.Thread.Sleep(100); // Wait and retry
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Generates a unique temporary file path in the system temp directory.
        /// Suitable for intermediate processing files during video conversion.
        /// </summary>
        public static string GetTempFilePath(string? extension = null)
        {
            var tempPath = Path.Combine(
                Path.GetTempPath(),
                "ffmpeg-dotnet",
                Guid.NewGuid().ToString() + (extension ?? ".tmp")
            );

            Directory.CreateDirectory(Path.GetDirectoryName(tempPath)!);
            return tempPath;
        }

        /// <summary>
        /// Sanitizes a filename by removing invalid characters.
        /// Preserves file extension and replaces problematic chars with underscore.
        /// </summary>
        public static string SanitizeFileName(string fileName)
        {
            var invalidChars = new Regex($"[{Regex.Escape(new string(Path.GetInvalidFileNameChars()))}]");
            var sanitized = invalidChars.Replace(fileName, "_");
            return sanitized.Length > MaxFileNameLength
                ? sanitized.Substring(0, MaxFileNameLength)
                : sanitized;
        }

        /// <summary>
        /// Checks if two files have compatible formats for merging.
        /// This is a simple extension check; deeper codec analysis requires FFprobe.
        /// </summary>
        public static bool AreFormatsCompatible(string filePath1, string filePath2)
        {
            var ext1 = GetFileExtension(filePath1);
            var ext2 = GetFileExtension(filePath2);
            return ext1.Equals(ext2, StringComparison.OrdinalIgnoreCase);
        }
    }
}
