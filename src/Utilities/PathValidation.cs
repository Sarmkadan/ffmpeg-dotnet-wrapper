// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System;
using System.IO;
using FFmpegDotnetWrapper.Exceptions;

namespace FFmpegDotnetWrapper.Utilities;

/// <summary>
/// Provides utilities for validating file system paths to prevent path traversal attacks.
/// Ensures that resolved absolute paths stay within an allowed base directory.
/// </summary>
public static class PathValidation
{
    /// <summary>
    /// Validates that a file path is safe and stays within the specified base directory.
    /// </summary>
    /// <param name="path">The file path to validate.</param>
    /// <param name="baseDirectory">The base directory that the path must stay within.</param>
    /// <param name="paramName">The name of the parameter being validated (for error messages).</param>
    /// <returns>The resolved absolute path if validation passes.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> or <paramref name="baseDirectory"/> is null.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the path is null, empty, or whitespace, or when it attempts to traverse outside the base directory.
    /// </exception>
    public static string ValidatePathWithinBaseDirectory(string path, string baseDirectory, string paramName)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(baseDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(paramName);

        // Normalize and validate base directory
        baseDirectory = NormalizeDirectoryPath(baseDirectory);

        // Resolve the input path to an absolute path
        string resolvedPath;
        try
        {
            // Use Path.GetFullPath to resolve relative paths and normalize separators
            resolvedPath = Path.GetFullPath(path.Trim());
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            throw new ArgumentException(
                $"The path '{path}' is invalid: {ex.Message}",
                paramName,
                ex);
        }

        // Normalize the resolved path
        resolvedPath = NormalizePath(resolvedPath);

        // Check if the resolved path starts with the base directory
        if (!resolvedPath.StartsWith(baseDirectory, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"The path '{path}' resolves to '{resolvedPath}' which is outside the allowed base directory '{baseDirectory}'. " +
                $"Path traversal attacks are not permitted.",
                paramName);
        }

        return resolvedPath;
    }

    /// <summary>
    /// Validates that a directory path is safe and stays within the specified base directory.
    /// </summary>
    /// <param name="directoryPath">The directory path to validate.</param>
    /// <param name="baseDirectory">The base directory that the path must stay within.</param>
    /// <param name="paramName">The name of the parameter being validated (for error messages).</param>
    /// <returns>The resolved absolute directory path if validation passes.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="directoryPath"/> or <paramref name="baseDirectory"/> is null.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the path is null, empty, or whitespace, or when it attempts to traverse outside the base directory.
    /// </exception>
    public static string ValidateDirectoryWithinBaseDirectory(string directoryPath, string baseDirectory, string paramName)
    {
        ArgumentNullException.ThrowIfNull(directoryPath);
        ArgumentNullException.ThrowIfNull(baseDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(paramName);

        // Normalize and validate base directory
        baseDirectory = NormalizeDirectoryPath(baseDirectory);

        // Resolve the input path to an absolute path
        string resolvedPath;
        try
        {
            resolvedPath = Path.GetFullPath(directoryPath.Trim());
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            throw new ArgumentException(
                $"The directory path '{directoryPath}' is invalid: {ex.Message}",
                paramName,
                ex);
        }

        // Ensure the resolved path is a directory (ends with directory separator)
        resolvedPath = EnsureTrailingDirectorySeparator(resolvedPath);
        resolvedPath = NormalizePath(resolvedPath);

        // Check if the resolved path starts with the base directory
        if (!resolvedPath.StartsWith(baseDirectory, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"The directory path '{directoryPath}' resolves to '{resolvedPath}' which is outside the allowed base directory '{baseDirectory}'. " +
                $"Path traversal attacks are not permitted.",
                paramName);
        }

        return resolvedPath;
    }

    /// <summary>
    /// Validates that a file exists and is within the allowed base directory.
    /// </summary>
    /// <param name="filePath">The file path to validate.</param>
    /// <param name="baseDirectory">The base directory that the path must stay within.</param>
    /// <param name="paramName">The name of the parameter being validated (for error messages).</param>
    /// <returns>The resolved absolute file path if validation passes.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="filePath"/> or <paramref name="baseDirectory"/> is null.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the file doesn't exist, is null/empty/whitespace, or attempts to traverse outside the base directory.
    /// </exception>
    public static string ValidateExistingFileWithinBaseDirectory(string filePath, string baseDirectory, string paramName)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        ArgumentNullException.ThrowIfNull(baseDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(paramName);

        // First validate the path is safe
        string resolvedPath = ValidatePathWithinBaseDirectory(filePath, baseDirectory, paramName);

        // Then check if the file exists
        if (!File.Exists(resolvedPath))
        {
            throw new FileNotFoundException(
                $"File not found: {resolvedPath}",
                resolvedPath);
        }

        return resolvedPath;
    }

    /// <summary>
    /// Validates that an output file path is safe and stays within the specified base directory.
    /// Ensures the output path is within the allowed base directory to prevent path traversal attacks.
    /// </summary>
    /// <param name="outputPath">The output file path to validate.</param>
    /// <param name="baseDirectory">The base directory that the output path must stay within.</param>
    /// <param name="paramName">The name of the parameter being validated (for error messages).</param>
    /// <returns>The resolved absolute output file path if validation passes.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="outputPath"/> or <paramref name="baseDirectory"/> is null.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the output path is null, empty, or whitespace, or when it attempts to traverse outside the base directory.
    /// </exception>
    public static string ValidateOutputPathWithinBaseDirectory(string outputPath, string baseDirectory, string paramName)
    {
        ArgumentNullException.ThrowIfNull(outputPath);
        ArgumentNullException.ThrowIfNull(baseDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(paramName);

        // Validate the path is safe and stays within the base directory
        return ValidatePathWithinBaseDirectory(outputPath, baseDirectory, paramName);
    }

    /// <summary>
    /// Validates that an output directory path is safe and stays within the specified base directory.
    /// Ensures the output directory path is within the allowed base directory to prevent path traversal attacks.
    /// </summary>
    /// <param name="outputDirectory">The output directory path to validate.</param>
    /// <param name="baseDirectory">The base directory that the output path must stay within.</param>
    /// <param name="paramName">The name of the parameter being validated (for error messages).</param>
    /// <returns>The resolved absolute output directory path if validation passes.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="outputDirectory"/> or <paramref name="baseDirectory"/> is null.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the output directory path is null, empty, or whitespace, or when it attempts to traverse outside the base directory.
    /// </exception>
    public static string ValidateOutputDirectoryWithinBaseDirectory(string outputDirectory, string baseDirectory, string paramName)
    {
        ArgumentNullException.ThrowIfNull(outputDirectory);
        ArgumentNullException.ThrowIfNull(baseDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(paramName);

        // Validate the directory path is safe and stays within the base directory
        string resolvedPath = ValidateDirectoryWithinBaseDirectory(outputDirectory, baseDirectory, paramName);

        // Ensure the path exists (create it if needed)
        if (!Directory.Exists(resolvedPath))
        {
            try
            {
                Directory.CreateDirectory(resolvedPath);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or PathTooLongException)
            {
                throw new ArgumentException(
                    $"The output directory '{outputDirectory}' resolves to '{resolvedPath}' which cannot be created. " +
                    $"Please ensure you have write permissions and the path is valid.",
                    paramName,
                    ex);
            }
        }

        return resolvedPath;
    }

    /// <summary>
    /// Normalizes a file path by:
    /// 1. Converting to absolute path
    /// 2. Converting separators to platform-specific format
    /// 3. Removing relative segments (../, ./)
    /// </summary>
    /// <param name="path">The path to normalize.</param>
    /// <returns>The normalized path.</returns>
    private static string NormalizePath(string path)
    {
        // Convert to platform-specific separators
        path = path.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

        // Normalize the path (resolves . and .. segments)
        path = Path.GetFullPath(path);

        return path;
    }

    /// <summary>
    /// Normalizes a directory path by ensuring it ends with a directory separator.
    /// </summary>
    /// <param name="path">The path to normalize.</param>
    /// <returns>The normalized directory path.</returns>
    private static string NormalizeDirectoryPath(string path)
    {
        path = NormalizePath(path);
        return EnsureTrailingDirectorySeparator(path);
    }

    /// <summary>
    /// Ensures that a path ends with a directory separator.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns>The path with a trailing directory separator.</returns>
    private static string EnsureTrailingDirectorySeparator(string path)
    {
        if (!path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
        {
            return path + Path.DirectorySeparatorChar;
        }

        return path;
    }
}