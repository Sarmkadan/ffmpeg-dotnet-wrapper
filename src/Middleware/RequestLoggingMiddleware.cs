// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using FFmpegDotnetWrapper.Api.DTOs;
using FFmpegDotnetWrapper.Utilities;
using Microsoft.Extensions.Logging;

namespace FFmpegDotnetWrapper.Middleware
{
    /// <summary>
    /// Configuration options for request logging behavior.
    /// Controls what information is logged and at what detail level.
    /// </summary>
    public class RequestLoggingOptions
    {
        /// <summary>Log request arguments (may contain file paths).</summary>
        public bool LogArguments { get; set; } = true;

        /// <summary>Log response data (may be large).</summary>
        public bool LogResponseData { get; set; } = false;

        /// <summary>Log stack traces for errors.</summary>
        public bool LogStackTrace { get; set; } = true;

        /// <summary>Max length for truncating large values in logs.</summary>
        public int MaxLogValueLength { get; set; } = 500;

        /// <summary>Include performance metrics in logs.</summary>
        public bool LogPerformanceMetrics { get; set; } = true;
    }

    /// <summary>
    /// Request/response logging middleware that logs all API operations.
    /// Provides structured logging with performance metrics and audit trails.
    /// Helps with debugging, monitoring, and compliance requirements.
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly ILogger<RequestLoggingMiddleware> _logger;
        private readonly RequestLoggingOptions _options;

        public RequestLoggingMiddleware(ILogger<RequestLoggingMiddleware> logger, RequestLoggingOptions? options = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? new RequestLoggingOptions();
        }

        /// <summary>
        /// Logs an incoming API request with its parameters.
        /// Captures request ID, timestamp, and operation details.
        /// </summary>
        public void LogRequest<T>(T request, string operationName) where T : class
        {
            try
            {
                var requestId = (request as ApiRequest)?.RequestId ?? Guid.NewGuid().ToString();
                var timestamp = FormattingUtilities.FormatTimestamp(DateTime.UtcNow);

                var logMessage = new StringBuilder();
                logMessage.AppendLine($"[{timestamp}] REQUEST: {operationName}");
                logMessage.AppendLine($"  RequestId: {requestId}");

                if (_options.LogArguments && request != null)
                {
                    var properties = request.GetType().GetProperties();
                    foreach (var prop in properties)
                    {
                        if (prop.Name == "RequestId" || prop.Name == "CreatedAt")
                            continue;

                        var value = prop.GetValue(request);
                        var valueStr = FormatPropertyValue(value);
                        logMessage.AppendLine($"  {prop.Name}: {valueStr}");
                    }
                }

                _logger.LogInformation(logMessage.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging request for operation: {OperationName}", operationName);
            }
        }

        /// <summary>
        /// Logs a completed API operation with response details and performance metrics.
        /// Records execution time, response status, and any error information.
        /// </summary>
        public void LogResponse<T>(ApiResponse<T> response, string operationName, TimeSpan executionTime)
        {
            try
            {
                var timestamp = FormattingUtilities.FormatTimestamp(DateTime.UtcNow);
                var statusText = response.Success ? "SUCCESS" : "FAILED";

                var logMessage = new StringBuilder();
                logMessage.AppendLine($"[{timestamp}] RESPONSE: {operationName} [{statusText}]");
                logMessage.AppendLine($"  RequestId: {response.RequestId}");
                logMessage.AppendLine($"  StatusCode: {response.StatusCode}");
                logMessage.AppendLine($"  Message: {response.Message}");

                if (_options.LogPerformanceMetrics)
                {
                    logMessage.AppendLine($"  ExecutionTime: {FormattingUtilities.FormatDuration(executionTime)}");
                }

                if (!response.Success && response.Errors.Count > 0)
                {
                    logMessage.AppendLine("  Errors:");
                    foreach (var error in response.Errors)
                    {
                        logMessage.AppendLine($"    - {error.Code}: {error.Message}");
                    }
                }

                if (_options.LogResponseData && response.Data != null)
                {
                    var dataStr = FormatPropertyValue(response.Data);
                    logMessage.AppendLine($"  Data: {dataStr}");
                }

                var logLevel = response.Success ? LogLevel.Information : LogLevel.Warning;
                _logger.Log(logLevel, logMessage.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging response for operation: {OperationName}", operationName);
            }
        }

        /// <summary>
        /// Logs an error that occurred during operation processing.
        /// Records exception details, operation context, and recovery suggestions.
        /// </summary>
        public void LogError(Exception ex, string operationName, string? requestId = null, Dictionary<string, object>? context = null)
        {
            try
            {
                var timestamp = FormattingUtilities.FormatTimestamp(DateTime.UtcNow);

                var logMessage = new StringBuilder();
                logMessage.AppendLine($"[{timestamp}] ERROR: {operationName}");

                if (!string.IsNullOrEmpty(requestId))
                {
                    logMessage.AppendLine($"  RequestId: {requestId}");
                }

                logMessage.AppendLine($"  Exception: {ex.GetType().Name}");
                logMessage.AppendLine($"  Message: {ex.Message}");

                if (context != null && context.Count > 0)
                {
                    logMessage.AppendLine("  Context:");
                    foreach (var kvp in context)
                    {
                        var valueStr = FormatPropertyValue(kvp.Value);
                        logMessage.AppendLine($"    {kvp.Key}: {valueStr}");
                    }
                }

                if (_options.LogStackTrace && ex.StackTrace != null)
                {
                    logMessage.AppendLine($"  StackTrace: {ex.StackTrace}");
                }

                _logger.LogError(logMessage.ToString());
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "Error logging exception for operation: {OperationName}", operationName);
            }
        }

        /// <summary>
        /// Logs performance metrics for an operation including timing and resource usage.
        /// Useful for identifying slow operations and performance bottlenecks.
        /// </summary>
        public void LogPerformanceMetrics(string operationName, TimeSpan duration, long inputSize, long outputSize, Dictionary<string, object>? metrics = null)
        {
            try
            {
                if (!_options.LogPerformanceMetrics)
                    return;

                var timestamp = FormattingUtilities.FormatTimestamp(DateTime.UtcNow);

                var logMessage = new StringBuilder();
                logMessage.AppendLine($"[{timestamp}] PERFORMANCE: {operationName}");
                logMessage.AppendLine($"  Duration: {FormattingUtilities.FormatDuration(duration)}");
                logMessage.AppendLine($"  InputSize: {FormattingUtilities.FormatBytes(inputSize)}");
                logMessage.AppendLine($"  OutputSize: {FormattingUtilities.FormatBytes(outputSize)}");

                if (inputSize > 0)
                {
                    var compressionRatio = (double)outputSize / inputSize * 100;
                    logMessage.AppendLine($"  CompressionRatio: {compressionRatio:0.0}%");
                }

                if (duration.TotalSeconds > 0)
                {
                    var throughput = outputSize / duration.TotalSeconds / 1024 / 1024; // MB/s
                    logMessage.AppendLine($"  Throughput: {throughput:0.0} MB/s");
                }

                if (metrics != null && metrics.Count > 0)
                {
                    logMessage.AppendLine("  Metrics:");
                    foreach (var kvp in metrics)
                    {
                        var valueStr = FormatPropertyValue(kvp.Value);
                        logMessage.AppendLine($"    {kvp.Key}: {valueStr}");
                    }
                }

                _logger.LogInformation(logMessage.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging performance metrics for operation: {OperationName}", operationName);
            }
        }

        /// <summary>
        /// Formats a property value for logging, handling nulls and truncating large values.
        /// Prevents sensitive information from being logged in full.
        /// </summary>
        private string FormatPropertyValue(object? value)
        {
            if (value == null)
                return "<null>";

            if (value is string str)
            {
                return FormattingUtilities.TruncateString(str, _options.MaxLogValueLength);
            }

            if (value is IEnumerable<object> list)
            {
                var count = 0;
                foreach (var _ in list)
                {
                    count++;
                    if (count > 10)
                        return $"[List with {count}+ items]";
                }
                return $"[{count} items]";
            }

            var str_value = value.ToString() ?? "<empty>";
            return FormattingUtilities.TruncateString(str_value, _options.MaxLogValueLength);
        }
    }
}
