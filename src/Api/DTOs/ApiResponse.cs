// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace FFmpegDotnetWrapper.Api.DTOs
{
    /// <summary>
    /// Generic API response envelope for standardized response formatting.
    /// Wraps actual response data with metadata, status codes, and error information.
    /// This ensures consistent API contracts across all endpoints.
    /// </summary>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Indicates whether the operation succeeded (true) or failed (false).
        /// Used by clients to determine if Data contains valid results or error details.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// HTTP status code for the response (200, 400, 500, etc).
        /// Helps REST clients determine retry strategy and error handling.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Human-readable message describing the operation result or error.
        /// Should be localized in production systems handling multiple languages.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// The actual response data when operation succeeds.
        /// Will be null if Success is false (check ErrorDetails instead).
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// List of validation or business logic errors encountered during processing.
        /// Each error includes field name and error description for API validation failures.
        /// </summary>
        public List<ApiError> Errors { get; set; } = [];

        /// <summary>
        /// Timestamp when the response was generated on the server.
        /// Enables client-side caching strategies and debugging of time-related issues.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Unique request ID for tracking this specific operation through logs.
        /// Matches the RequestId from the incoming request for correlation.
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// Optional stack trace in development environments for debugging.
        /// Must be omitted in production due to security concerns (info disclosure).
        /// </summary>
        public string? StackTrace { get; set; }

        /// <summary>
        /// Creates a successful API response with the provided data.
        /// </summary>
        public static ApiResponse<T> Ok(T data, string message = "Operation completed successfully")
        {
            ArgumentException.ThrowIfNullOrEmpty(message);
            return new ApiResponse<T>
            {
                Success = true,
                StatusCode = 200,
                Message = message,
                Data = data
            };
        }

        /// <summary>
        /// Creates a successful API response with custom status code.
        /// Used for operations that return 201 (Created) or other non-200 success codes.
        /// </summary>
        public static ApiResponse<T> Ok(T data, int statusCode, string message = "Operation completed successfully")
        {
            ArgumentException.ThrowIfNullOrEmpty(message);
            return new ApiResponse<T>
            {
                Success = true,
                StatusCode = statusCode,
                Message = message,
                Data = data
            };
        }

        /// <summary>
        /// Creates a failure response indicating the operation could not be completed.
        /// Used for business logic failures, validation errors, and resource not found scenarios.
        /// </summary>
        public static ApiResponse<T> Failure(string message, int statusCode = 400)
        {
            ArgumentException.ThrowIfNullOrEmpty(message);
            return new ApiResponse<T>
            {
                Success = false,
                StatusCode = statusCode,
                Message = message
            };
        }

        /// <summary>
        /// Creates a failure response with detailed error information.
        /// Useful for API validation where multiple fields may have errors.
        /// </summary>
        public static ApiResponse<T> Failure(string message, List<ApiError> errors, int statusCode = 400)
        {
            ArgumentException.ThrowIfNullOrEmpty(message);
            return new ApiResponse<T>
            {
                Success = false,
                StatusCode = statusCode,
                Message = message,
                Errors = errors
            };
        }

        /// <summary>
        /// Creates a failure response with a specific error code for programmatic handling.
        /// Allows clients to implement error-code-specific logic (e.g., retry on timeout).
        /// </summary>
        public static ApiResponse<T> Failure(string message, string errorCode, int statusCode = 400)
        {
            ArgumentException.ThrowIfNullOrEmpty(message);
            return new ApiResponse<T>
            {
                Success = false,
                StatusCode = statusCode,
                Message = message,
                Errors = [new ApiError { Code = errorCode, Message = message }]
            };
        }
    }

    /// <summary>
    /// Represents a single error within an API response.
    /// Used to communicate validation failures, business rule violations, or operational errors.
    /// </summary>
    public class ApiError
    {
        /// <summary>
        /// Error code for programmatic error handling (e.g., "FILE_NOT_FOUND", "INVALID_FORMAT").
        /// Allows clients to respond intelligently to specific error conditions.
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// Name of the field that caused the error (if applicable).
        /// Null for non-field-specific errors like database connection failures.
        /// </summary>
        public string? Field { get; set; }

        /// <summary>
        /// Human-readable description of what went wrong and how to fix it.
        /// Should be clear enough for non-technical users to understand.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Suggested corrective action for the user.
        /// Examples: "Please check the file format" or "Try again in a few moments".
        /// </summary>
        public string? Suggestion { get; set; }
    }

    /// <summary>
    /// Non-generic API response for operations that don't return data.
    /// Used for delete operations, status updates, and action confirmations.
    /// </summary>
    public class ApiResponse
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<ApiError> Errors { get; set; } = [];
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ApiResponse Ok(string message = "Operation completed successfully")
        {
            ArgumentException.ThrowIfNullOrEmpty(message);
            return new ApiResponse
            {
                Success = true,
                StatusCode = 200,
                Message = message
            };
        }

        public static ApiResponse Failure(string message, int statusCode = 400)
        {
            return new ApiResponse
            {
                Success = false,
                StatusCode = statusCode,
                Message = message
            };
        }
    }
}
