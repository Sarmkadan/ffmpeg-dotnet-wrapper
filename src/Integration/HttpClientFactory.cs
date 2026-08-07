// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================
// Factory for creating and configuring HttpClient instances for external integrations.
// Manages named clients, retry policies, and timeout configurations.
// Ensures consistent HTTP behavior across all external API calls.
// =====================================================================

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FFmpegDotnetWrapper.Integration
{
    using Policies;

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
            ArgumentNullException.ThrowIfNull(services);

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
            ArgumentNullException.ThrowIfNull(services);
            ArgumentException.ThrowIfNullOrEmpty(clientName);

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
    /// Implements IRetryPolicy for unified retry handling across the application.
    /// </summary>
    public class ExponentialBackoffRetryPolicy : IRetryPolicy
    {
        private readonly int _maxAttempts;
        private readonly int _initialDelayMilliseconds;
        private readonly double _backoffFactor;
        private readonly double _jitterFactor;

        /// <summary>
        /// Creates a new instance of ExponentialBackoffRetryPolicy.
        /// </summary>
        /// <param name="maxAttempts">Maximum number of retry attempts (1 = no retry).</param>
        /// <param name="initialDelayMilliseconds">Initial delay in milliseconds before first retry.</param>
        /// <param name="backoffFactor">Multiplier for delay between retries (e.g., 2.0 for exponential).</param>
        /// <param name="jitterFactor">Random factor to add jitter to delays (0.0-1.0).</param>
        public ExponentialBackoffRetryPolicy(
            int maxAttempts = 3,
            int initialDelayMilliseconds = 100,
            double backoffFactor = 2.0,
            double jitterFactor = 0.5)
        {
            if (maxAttempts < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxAttempts),
                    "Max attempts must be at least 1");
            }

            if (initialDelayMilliseconds <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(initialDelayMilliseconds),
                    "Initial delay must be positive");
            }

            if (backoffFactor <= 1.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(backoffFactor),
                    "Backoff factor must be greater than 1.0");
            }

            if (jitterFactor < 0.0 || jitterFactor > 1.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(jitterFactor),
                    "Jitter factor must be between 0.0 and 1.0");
            }

            _maxAttempts = maxAttempts;
            _initialDelayMilliseconds = initialDelayMilliseconds;
            _backoffFactor = backoffFactor;
            _jitterFactor = jitterFactor;
        }

        /// <summary>
        /// Gets the maximum number of retry attempts.
        /// </summary>
        public int MaxAttempts => _maxAttempts;

        /// <summary>
        /// Gets the initial delay in milliseconds.
        /// </summary>
        public int InitialDelayMilliseconds => _initialDelayMilliseconds;

        /// <summary>
        /// Executes the specified operation with retry logic.
        /// </summary>
        /// <typeparam name="T">The type of result returned by the operation.</typeparam>
        /// <param name="operation">The operation to execute.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of the operation.</returns>
        public async Task<T> ExecuteAsync<T>(
            Func<CancellationToken, Task<T>> operation,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(operation);

            var attempts = 0;
            Exception? lastException = null;

            while (attempts < _maxAttempts)
            {
                attempts++;

                try
                {
                    return await operation(cancellationToken);
                }
                catch (Exception ex) when (ShouldRetry(ex))
                {
                    lastException = ex;

                    // Don't retry on first attempt
                    if (attempts >= _maxAttempts)
                    {
                        break;
                    }

                    var delay = CalculateDelay(attempts);

                    try
                    {
                        await Task.Delay(delay, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        // Re-throw cancellation if it occurred during delay
                        throw;
                    }
                }
            }

            // All attempts failed - throw the last exception
            throw new RetryFailedException(
                $"Operation failed after {_maxAttempts} attempt(s). Last error: {lastException?.Message}",
                lastException);
        }

        /// <summary>
        /// Executes the specified operation with retry logic.
        /// </summary>
        /// <param name="operation">The operation to execute.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the operation.</returns>
        public async Task ExecuteAsync(
            Func<CancellationToken, Task> operation,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(operation);

            var attempts = 0;
            Exception? lastException = null;

            while (attempts < _maxAttempts)
            {
                attempts++;

                try
                {
                    await operation(cancellationToken);
                    return; // Success - exit the retry loop
                }
                catch (Exception ex) when (ShouldRetry(ex))
                {
                    lastException = ex;

                    // Don't retry on first attempt
                    if (attempts >= _maxAttempts)
                    {
                        break;
                    }

                    var delay = CalculateDelay(attempts);

                    try
                    {
                        await Task.Delay(delay, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        // Re-throw cancellation if it occurred during delay
                        throw;
                    }
                }
            }

            // All attempts failed - throw the last exception
            throw new RetryFailedException(
                $"Operation failed after {_maxAttempts} attempt(s). Last error: {lastException?.Message}",
                lastException);
        }

        /// <summary>
        /// Determines if an exception should be retried.
        /// </summary>
        /// <param name="exception">The exception to check.</param>
        /// <returns><c>true</c> if the exception should be retried; otherwise <c>false</c>.</returns>
        public bool ShouldRetry(Exception exception)
        {
            ArgumentNullException.ThrowIfNull(exception);

            // For backward compatibility with HttpClientUtilities.ShouldRetryOnException
            return exception switch
            {
                HttpRequestException => true,
                TimeoutException => true,
                OperationCanceledException => false,
                _ => false
            };
        }

        /// <summary>
        /// Calculates the delay before the next retry attempt.
        /// Uses exponential backoff with jitter to prevent thundering herd problems.
        /// </summary>
        /// <param name="attemptNumber">The current attempt number (1-based).</param>
        /// <returns>The delay before the next retry.</returns>
        protected virtual TimeSpan CalculateDelay(int attemptNumber)
        {
            // Base delay: initial * factor^(attempt-1)
            var baseDelayMs = _initialDelayMilliseconds * Math.Pow(_backoffFactor, attemptNumber - 1);

            // Add jitter: random factor between 0 and jitterFactor * baseDelay
            var jitterRange = _jitterFactor * baseDelayMs;
            var jitterMs = Random.Shared.NextDouble() * jitterRange;

            var totalDelayMs = baseDelayMs + jitterMs;

            return TimeSpan.FromMilliseconds(totalDelayMs);
        }

        /// <summary>
        /// Gets the delay before the next retry attempt (for backward compatibility).
        /// Uses exponential backoff: 100ms, 200ms, 400ms, etc.
        /// </summary>
        [Obsolete("Use CalculateDelay() or rely on automatic retry delays instead.")]
        public TimeSpan GetRetryDelay(int attemptNumber)
        {
            var delayMs = _initialDelayMilliseconds * (int)Math.Pow(2, attemptNumber - 1);
            // Add jitter to prevent thundering herd
            var jitterMs = new Random().Next(0, delayMs / 2);
            return TimeSpan.FromMilliseconds(delayMs + jitterMs);
        }

        /// <summary>
        /// Determines if another retry should be attempted (for backward compatibility).
        /// </summary>
        [Obsolete("Use ShouldRetry(Exception) instead.")]
        public bool ShouldRetry(int attemptNumber)
        {
            return attemptNumber < _maxAttempts;
        }
    }
}