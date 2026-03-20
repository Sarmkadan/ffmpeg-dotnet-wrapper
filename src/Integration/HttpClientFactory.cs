// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FFmpegDotnetWrapper.Integration
{
    /// <summary>
    /// Factory for creating and configuring HttpClient instances for external integrations.
    /// Manages named clients, retry policies, and timeout configurations.
    /// Ensures consistent HTTP behavior across all external API calls.
    /// </summary>
    public static class HttpClientFactoryExtensions
    {
        /// <summary>
        /// Adds HTTP clients for FFmpeg wrapper integrations.
        /// Configures clients for webhooks and external API calls.
        /// </summary>
        public static IServiceCollection AddFFmpegHttpClients(
            this IServiceCollection services,
            Action<HttpClientConfig>? config = null)
        {
            var httpConfig = new HttpClientConfig();
            config?.Invoke(httpConfig);

            // Webhook client for delivering events
            services.AddHttpClient("webhook")
                .ConfigureHttpClient(client =>
                {
                    client.Timeout = TimeSpan.FromSeconds(httpConfig.WebhookTimeoutSeconds);
                    client.DefaultRequestHeaders.Add("User-Agent", "FFmpegDotnetWrapper/1.0");
                    client.DefaultRequestHeaders.Add("Content-Type", "application/json");
                });

            // External API client for probe operations
            services.AddHttpClient("probe")
                .ConfigureHttpClient(client =>
                {
                    client.Timeout = TimeSpan.FromSeconds(httpConfig.ProbeTimeoutSeconds);
                    client.DefaultRequestHeaders.Add("User-Agent", "FFmpegDotnetWrapper/1.0");
                });

            // Upload/download client for large files
            services.AddHttpClient("media")
                .ConfigureHttpClient(client =>
                {
                    client.Timeout = TimeSpan.FromMinutes(httpConfig.MediaTransferTimeoutMinutes);
                    client.DefaultRequestHeaders.Add("User-Agent", "FFmpegDotnetWrapper/1.0");
                });

            return services;
        }

        /// <summary>
        /// Adds a custom HTTP client for external API integration.
        /// </summary>
        public static IHttpClientBuilder AddCustomHttpClient(
            this IServiceCollection services,
            string clientName,
            string? baseAddress = null,
            TimeSpan? timeout = null,
            Dictionary<string, string>? defaultHeaders = null)
        {
            var builder = services.AddHttpClient(clientName);

            builder.ConfigureHttpClient(client =>
            {
                if (!string.IsNullOrEmpty(baseAddress))
                {
                    client.BaseAddress = new Uri(baseAddress);
                }

                client.Timeout = timeout ?? TimeSpan.FromSeconds(30);

                if (defaultHeaders != null)
                {
                    foreach (var header in defaultHeaders)
                    {
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }
            });

            return builder;
        }
    }

    /// <summary>
    /// Configuration for HTTP client timeouts and settings.
    /// </summary>
    public class HttpClientConfig
    {
        /// <summary>Timeout for webhook delivery in seconds.</summary>
        public int WebhookTimeoutSeconds { get; set; } = 30;

        /// <summary>Timeout for FFprobe operations in seconds.</summary>
        public int ProbeTimeoutSeconds { get; set; } = 60;

        /// <summary>Timeout for media file transfers in minutes.</summary>
        public int MediaTransferTimeoutMinutes { get; set; } = 30;

        /// <summary>Enable automatic retry on transient failures.</summary>
        public bool EnableRetries { get; set; } = true;

        /// <summary>Maximum number of retry attempts.</summary>
        public int MaxRetryAttempts { get; set; } = 3;

        /// <summary>Initial backoff delay in milliseconds.</summary>
        public int InitialBackoffMs { get; set; } = 100;
    }

    /// <summary>
    /// HTTP utilities for common operations like status code handling and error detection.
    /// </summary>
    public static class HttpClientUtilities
    {
        /// <summary>
        /// Determines if an HTTP status code indicates a transient error that should be retried.
        /// Includes 429 (too many requests), 503 (service unavailable), 504 (gateway timeout).
        /// </summary>
        public static bool IsTransientError(int statusCode)
        {
            return statusCode switch
            {
                408 => true, // Request Timeout
                429 => true, // Too Many Requests
                500 => true, // Internal Server Error
                503 => true, // Service Unavailable
                504 => true, // Gateway Timeout
                _ => false
            };
        }

        /// <summary>
        /// Determines if an HTTP status code indicates a permanent error that shouldn't be retried.
        /// Includes 400, 401, 403, 404, 405.
        /// </summary>
        public static bool IsPermanentError(int statusCode)
        {
            return statusCode switch
            {
                400 => true, // Bad Request
                401 => true, // Unauthorized
                403 => true, // Forbidden
                404 => true, // Not Found
                405 => true, // Method Not Allowed
                411 => true, // Length Required
                415 => true, // Unsupported Media Type
                _ => false
            };
        }

        /// <summary>
        /// Determines if a request should be retried based on exception type.
        /// Retries for network failures, timeouts, but not for serialization errors.
        /// </summary>
        public static bool ShouldRetryOnException(Exception ex)
        {
            return ex switch
            {
                HttpRequestException => true,
                TimeoutException => true,
                OperationCanceledException => true,
                _ => false
            };
        }

        /// <summary>
        /// Extracts a meaningful error message from an HTTP response.
        /// Attempts to parse JSON error details, falls back to status code description.
        /// </summary>
        public static string GetErrorMessage(HttpResponseMessage response)
        {
            if (response == null)
                return "Unknown HTTP error";

            return response.StatusCode switch
            {
                System.Net.HttpStatusCode.BadRequest => "Bad request - check your parameters",
                System.Net.HttpStatusCode.Unauthorized => "Unauthorized - check your credentials",
                System.Net.HttpStatusCode.Forbidden => "Forbidden - access denied",
                System.Net.HttpStatusCode.NotFound => "Resource not found",
                System.Net.HttpStatusCode.Conflict => "Conflict - resource already exists",
                System.Net.HttpStatusCode.InternalServerError => "Server error - please try again later",
                System.Net.HttpStatusCode.ServiceUnavailable => "Service unavailable - please try again later",
                System.Net.HttpStatusCode.GatewayTimeout => "Request timed out - please try again",
                _ => $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}"
            };
        }
    }

    /// <summary>
    /// Retry policy for HTTP requests with exponential backoff.
    /// </summary>
    public class ExponentialBackoffRetryPolicy
    {
        private readonly int _maxRetries;
        private readonly int _initialDelayMs;

        public ExponentialBackoffRetryPolicy(int maxRetries = 3, int initialDelayMs = 100)
        {
            _maxRetries = maxRetries;
            _initialDelayMs = initialDelayMs;
        }

        /// <summary>
        /// Calculates the delay before the next retry attempt.
        /// Uses exponential backoff: 100ms, 200ms, 400ms, etc.
        /// </summary>
        public TimeSpan GetRetryDelay(int attemptNumber)
        {
            var delayMs = _initialDelayMs * (int)Math.Pow(2, attemptNumber - 1);
            // Add jitter to prevent thundering herd
            var jitterMs = new Random().Next(0, delayMs / 2);
            return TimeSpan.FromMilliseconds(delayMs + jitterMs);
        }

        /// <summary>
        /// Determines if another retry should be attempted.
        /// </summary>
        public bool ShouldRetry(int attemptNumber)
        {
            return attemptNumber < _maxRetries;
        }
    }
}
