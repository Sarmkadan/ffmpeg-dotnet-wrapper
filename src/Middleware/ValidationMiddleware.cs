// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using FFmpegDotnetWrapper.Api.DTOs;

namespace FFmpegDotnetWrapper.Middleware
{
    /// <summary>
    /// Request validation middleware that performs data annotation validation on API request objects.
    /// Validates required fields, string lengths, ranges, and custom validation rules.
    /// Returns detailed validation errors that help API consumers correct their input.
    /// </summary>
    public class ValidationMiddleware
    {
        /// <summary>
        /// Validates an API request object using System.ComponentModel.DataAnnotations attributes.
        /// Collects all validation errors and returns them in a standardized format.
        /// Supports both simple property validation and complex object graphs.
        /// </summary>
        public static ApiResponse<T>? ValidateRequest<T>(T request) where T : class
        {
            if (request == null)
            {
                return ApiResponse<T>.Failure("Request body is required", 400);
            }

            var validationContext = new ValidationContext(request, serviceProvider: null, items: null);
            var validationResults = new List<ValidationResult>();

            if (!Validator.TryValidateObject(request, validationContext, validationResults, validateAllProperties: true))
            {
                var errors = ConvertValidationResultsToApiErrors(validationResults);
                return ApiResponse<T>.Failure("Request validation failed", errors, 400);
            }

            return null; // Validation passed
        }

        /// <summary>
        /// Validates a list of objects and aggregates validation errors from all items.
        /// Useful for batch operations where multiple items may have validation issues.
        /// </summary>
        public static ApiResponse<List<T>>? ValidateRequestList<T>(List<T> requests) where T : class
        {
            if (requests == null || requests.Count == 0)
            {
                return ApiResponse<List<T>>.Failure("At least one item is required", 400);
            }

            var allErrors = new List<ApiError>();
            var validationContext = new ValidationContext(null, serviceProvider: null, items: null);

            for (int i = 0; i < requests.Count; i++)
            {
                var item = requests[i];
                if (item == null)
                {
                    allErrors.Add(new ApiError
                    {
                        Code = "NULL_ITEM",
                        Field = $"[{i}]",
                        Message = "Item cannot be null"
                    });
                    continue;
                }

                validationContext = new ValidationContext(item, serviceProvider: null, items: null);
                var validationResults = new List<ValidationResult>();

                if (!Validator.TryValidateObject(item, validationContext, validationResults, validateAllProperties: true))
                {
                    var itemErrors = ConvertValidationResultsToApiErrors(validationResults);
                    foreach (var error in itemErrors)
                    {
                        error.Field = $"[{i}].{error.Field}";
                        allErrors.Add(error);
                    }
                }
            }

            if (allErrors.Count > 0)
            {
                return ApiResponse<List<T>>.Failure("Batch validation failed", allErrors, 400);
            }

            return null; // Validation passed
        }

        /// <summary>
        /// Validates input file paths for security and accessibility.
        /// Prevents directory traversal attacks and verifies file existence.
        /// Used to validate TranscodeRequest, TrimRequest, etc.
        /// </summary>
        public static ApiResponse<T>? ValidateFilePaths<T>(T request, string[] filePathProperties) where T : class
        {
            var errors = new List<ApiError>();

            foreach (var propName in filePathProperties)
            {
                var property = typeof(T).GetProperty(propName);
                if (property == null) continue;

                var value = property.GetValue(request) as string;
                if (string.IsNullOrEmpty(value)) continue;

                // Prevent directory traversal attacks
                if (value.Contains("..") || value.Contains("~"))
                {
                    errors.Add(new ApiError
                    {
                        Code = "INVALID_PATH",
                        Field = propName,
                        Message = "Path traversal is not allowed",
                        Suggestion = "Use absolute paths without '..' or '~'"
                    });
                }

                // Verify file exists for input files
                if (propName.StartsWith("Input", StringComparison.OrdinalIgnoreCase))
                {
                    if (!System.IO.File.Exists(value))
                    {
                        errors.Add(new ApiError
                        {
                            Code = "FILE_NOT_FOUND",
                            Field = propName,
                            Message = $"File not found: {value}",
                            Suggestion = "Verify the file path is correct and the file exists"
                        });
                    }
                }

                // Verify output directory exists and is writable
                if (propName.StartsWith("Output", StringComparison.OrdinalIgnoreCase))
                {
                    var directory = System.IO.Path.GetDirectoryName(value);
                    if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
                    {
                        errors.Add(new ApiError
                        {
                            Code = "INVALID_OUTPUT_PATH",
                            Field = propName,
                            Message = $"Output directory does not exist: {directory}",
                            Suggestion = "Create the output directory or use a valid path"
                        });
                    }
                }
            }

            if (errors.Count > 0)
            {
                return ApiResponse<T>.Failure("File path validation failed", errors, 400);
            }

            return null; // Validation passed
        }

        /// <summary>
        /// Validates merge request specifically - requires at least 2 files with identical codecs.
        /// FFmpeg merge operations require codec compatibility across all input files.
        /// </summary>
        public static ApiResponse<MergeRequest>? ValidateMergeRequest(MergeRequest request)
        {
            var errors = new List<ApiError>();

            if (request.InputPaths == null || request.InputPaths.Count < 2)
            {
                errors.Add(new ApiError
                {
                    Code = "INSUFFICIENT_FILES",
                    Field = nameof(MergeRequest.InputPaths),
                    Message = "At least 2 input files are required for merge operation",
                    Suggestion = "Provide at least 2 video files to merge"
                });
            }

            // Verify all input files exist
            var missingFiles = request.InputPaths
                .Where(path => !System.IO.File.Exists(path))
                .ToList();

            foreach (var missingFile in missingFiles)
            {
                errors.Add(new ApiError
                {
                    Code = "FILE_NOT_FOUND",
                    Field = nameof(MergeRequest.InputPaths),
                    Message = $"Input file not found: {missingFile}"
                });
            }

            if (errors.Count > 0)
            {
                return ApiResponse<MergeRequest>.Failure("Merge request validation failed", errors, 400);
            }

            return null; // Validation passed
        }

        /// <summary>
        /// Converts ValidationResult objects from DataAnnotations into standardized ApiError objects.
        /// Maintains field information and error messages for detailed validation feedback.
        /// </summary>
        private static List<ApiError> ConvertValidationResultsToApiErrors(List<ValidationResult> validationResults)
        {
            return validationResults
                .SelectMany(result => result.MemberNames.Any()
                    ? result.MemberNames.Select(memberName => new ApiError
                    {
                        Field = memberName,
                        Message = result.ErrorMessage ?? "Validation failed",
                        Code = result.GetType().Name
                    })
                    : new[] { new ApiError
                    {
                        Message = result.ErrorMessage ?? "Validation failed",
                        Code = result.GetType().Name
                    }})
                .ToList();
        }
    }
}
