// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Text.Json;
using FFmpegDotnetWrapper.Api.DTOs;
using FFmpegDotnetWrapper.Exceptions;
using Microsoft.Extensions.Logging;

namespace FFmpegDotnetWrapper.Middleware
{
    /// <summary>
    /// Global error handling middleware that catches unhandled exceptions and converts them to standardized API responses.
    /// Ensures consistent error formatting across all endpoints and provides detailed logging for troubleshooting.
    /// This middleware prevents sensitive internal errors from leaking to clients in production.
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly bool _includeStackTrace;

        public ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger, bool includeStackTrace = false)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _includeStackTrace = includeStackTrace;
        }

        /// <summary>
        /// Wraps operation execution in try-catch to handle all exception types uniformly.
        /// Converts specific exception types to appropriate HTTP status codes and error messages.
        /// Logs all errors for monitoring and debugging purposes.
        /// </summary>
        public ApiResponse<T> HandleOperation<T>(Func<T> operation, string operationName)
        {
            try
            {
                return ApiResponse<T>.Success(operation());
            }
            catch (FFmpegException ex)
            {
                _logger.LogWarning(ex, "FFmpeg operation failed: {OperationName}", operationName);
                return ApiResponse<T>.Failure(
                    ex.Message,
                    new List<ApiError> { new() { Code = "FFMPEG_ERROR", Message = ex.Message } },
                    400);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation: {OperationName}", operationName);
                return ApiResponse<T>.Failure(ex.Message, 400);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument in operation: {OperationName}", operationName);
                return ApiResponse<T>.Failure($"Invalid input: {ex.Message}", 400);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogWarning(ex, "File not found in operation: {OperationName}", operationName);
                return ApiResponse<T>.Failure("The requested file was not found", 404);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access in operation: {OperationName}", operationName);
                return ApiResponse<T>.Failure("Access to the resource is denied", 403);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in operation: {OperationName}", operationName);
                var errorMessage = _includeStackTrace
                    ? $"An unexpected error occurred: {ex.Message}\n{ex.StackTrace}"
                    : "An unexpected error occurred. Please contact support.";

                return ApiResponse<T>.Failure(errorMessage, 500);
            }
        }

        /// <summary>
        /// Async version of HandleOperation for async operations.
        /// Properly awaits Task-based operations and converts exceptions.
        /// </summary>
        public async System.Threading.Tasks.Task<ApiResponse<T>> HandleOperationAsync<T>(Func<System.Threading.Tasks.Task<T>> operation, string operationName)
        {
            try
            {
                var result = await operation();
                return ApiResponse<T>.Success(result);
            }
            catch (FFmpegException ex)
            {
                _logger.LogWarning(ex, "Async FFmpeg operation failed: {OperationName}", operationName);
                return ApiResponse<T>.Failure(ex.Message, 400);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid async operation: {OperationName}", operationName);
                return ApiResponse<T>.Failure(ex.Message, 400);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument in async operation: {OperationName}", operationName);
                return ApiResponse<T>.Failure($"Invalid input: {ex.Message}", 400);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogWarning(ex, "File not found in async operation: {OperationName}", operationName);
                return ApiResponse<T>.Failure("The requested file was not found", 404);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Async operation cancelled: {OperationName}", operationName);
                return ApiResponse<T>.Failure("The operation was cancelled", 408);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in async operation: {OperationName}", operationName);
                var errorMessage = _includeStackTrace
                    ? $"An unexpected error occurred: {ex.Message}\n{ex.StackTrace}"
                    : "An unexpected error occurred. Please contact support.";

                return ApiResponse<T>.Failure(errorMessage, 500);
            }
        }

        /// <summary>
        /// Converts exception to a formatted JSON response for HTTP responses.
        /// Used in actual middleware implementations that need to write to response streams.
        /// </summary>
        public string SerializeErrorResponse(Exception ex, string operationName, string? requestId = null)
        {
            var response = new
            {
                success = false,
                statusCode = GetStatusCode(ex),
                message = GetErrorMessage(ex),
                requestId = requestId ?? Guid.NewGuid().ToString(),
                timestamp = DateTime.UtcNow,
                stackTrace = _includeStackTrace ? ex.StackTrace : null
            };

            return JsonSerializer.Serialize(response);
        }

        /// <summary>
        /// Maps exception types to appropriate HTTP status codes.
        /// Determines the correct status code to return based on exception characteristics.
        /// </summary>
        private int GetStatusCode(Exception ex) => ex switch
        {
            FileNotFoundException => 404,
            UnauthorizedAccessException => 403,
            ArgumentException or InvalidOperationException => 400,
            OperationCanceledException => 408,
            FFmpegException => 422,
            _ => 500
        };

        /// <summary>
        /// Extracts user-friendly error message from exception.
        /// Masks sensitive internal details in production while providing actionable feedback.
        /// </summary>
        private string GetErrorMessage(Exception ex) => ex switch
        {
            FileNotFoundException => "The requested file was not found",
            UnauthorizedAccessException => "You do not have permission to access this resource",
            ArgumentException => $"Invalid input: {ex.Message}",
            InvalidOperationException => ex.Message,
            FFmpegException => "Video processing failed. Please check your input file and try again",
            OperationCanceledException => "The operation was cancelled",
            _ => "An unexpected error occurred. Please try again later"
        };
    }
}
