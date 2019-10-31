// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using FFmpegDotnetWrapper.Api.DTOs;
using FFmpegDotnetWrapper.Models;

namespace FFmpegDotnetWrapper.Serialization
{
    /// <summary>
    /// Output formatter for serializing API responses to JSON with pretty-printing and custom converters.
    /// Provides consistent JSON serialization across all API endpoints.
    /// Handles special types like TimeSpan, DateTime, and custom domain models.
    /// </summary>
    public class JsonOutputFormatter
    {
        private readonly JsonSerializerOptions _options;

        public JsonOutputFormatter(bool indent = true)
        {
            _options = new JsonSerializerOptions
            {
                WriteIndented = indent,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters =
                {
                    new TimeSpanConverter(),
                    new DateTimeConverter(),
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
            };
        }

        /// <summary>
        /// Serializes an API response object to JSON string.
        /// Automatically handles nested objects and collections.
        /// </summary>
        public string Format<T>(ApiResponse<T> response)
        {
            try
            {
                return JsonSerializer.Serialize(response, _options);
            }
            catch (Exception ex)
            {
                // Fallback to error response if serialization fails
                return JsonSerializer.Serialize(
                    new { error = "Failed to serialize response", details = ex.Message },
                    _options);
            }
        }

        /// <summary>
        /// Serializes a non-generic API response to JSON.
        /// </summary>
        public string Format(ApiResponse response)
        {
            try
            {
                return JsonSerializer.Serialize(response, _options);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(
                    new { error = "Failed to serialize response", details = ex.Message },
                    _options);
            }
        }

        /// <summary>
        /// Serializes any object to JSON with standard formatting.
        /// </summary>
        public string Format<T>(T obj)
        {
            try
            {
                return JsonSerializer.Serialize(obj, _options);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(
                    new { error = "Failed to serialize object", details = ex.Message },
                    _options);
            }
        }

        /// <summary>
        /// Deserializes JSON string to an API response object.
        /// Handles type conversion and validation.
        /// </summary>
        public ApiResponse<T>? Deserialize<T>(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<ApiResponse<T>>(json, _options);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Failed to deserialize JSON response", ex);
            }
        }

        /// <summary>
        /// Deserializes JSON to any specified type.
        /// </summary>
        public T? Deserialize<T>(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(json, _options);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Failed to deserialize JSON", ex);
            }
        }

        /// <summary>
        /// Custom converter for TimeSpan serialization.
        /// Formats as ISO 8601 duration format (PT1H30M45S).
        /// </summary>
        private class TimeSpanConverter : JsonConverter<TimeSpan>
        {
            public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var str = reader.GetString();
                return TimeSpan.TryParse(str, out var ts) ? ts : TimeSpan.Zero;
            }

            public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
            {
                var formatted = XmlConvert.ToString(value);
                writer.WriteStringValue(formatted);
            }
        }

        /// <summary>
        /// Custom converter for DateTime serialization.
        /// Formats as ISO 8601 with UTC timezone.
        /// </summary>
        private class DateTimeConverter : JsonConverter<DateTime>
        {
            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var str = reader.GetString();
                return DateTime.TryParse(str, out var dt) ? dt.ToUniversalTime() : DateTime.MinValue;
            }

            public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToUniversalTime().ToString("O"));
            }
        }
    }

    /// <summary>
    /// CSV output formatter for batch operation results.
    /// Exports operation results in comma-separated format for spreadsheet applications.
    /// </summary>
    public class CsvOutputFormatter
    {
        /// <summary>
        /// Formats a list of conversion results as CSV.
        /// Includes headers and proper escaping of special characters.
        /// </summary>
        public string FormatResults(List<ConversionResult> results)
        {
            var lines = new List<string>();

            // Header
            lines.Add("Input,Output,Status,Duration (seconds),ExecutionTime (ms),ErrorMessage");

            // Data rows
            foreach (var result in results)
            {
                var statusText = result.Success ? "Success" : "Failed";
                var errorMsg = EscapeCsvValue(result.ErrorMessage ?? string.Empty);

                var line = $"{EscapeCsvValue(result.InputFile)}," +
                           $"{EscapeCsvValue(result.OutputFile)}," +
                           $"{statusText}," +
                           $"{result.Duration}," +
                           $"{result.ExecutionTime.TotalMilliseconds}," +
                           $"{errorMsg}";

                lines.Add(line);
            }

            return string.Join(Environment.NewLine, lines);
        }

        /// <summary>
        /// Escapes CSV values that contain special characters (quotes, commas, newlines).
        /// Wraps values in quotes and escapes internal quotes.
        /// </summary>
        private string EscapeCsvValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }

            return value;
        }
    }

    /// <summary>
    /// Plain text formatter for simple logging and debugging output.
    /// Produces human-readable summaries suitable for console output.
    /// </summary>
    public class PlainTextFormatter
    {
        /// <summary>
        /// Formats an API response as plain text with indentation.
        /// </summary>
        public string Format<T>(ApiResponse<T> response)
        {
            var lines = new List<string>();

            lines.Add($"Status: {(response.Success ? "SUCCESS" : "FAILED")}");
            lines.Add($"Code: {response.StatusCode}");
            lines.Add($"Message: {response.Message}");

            if (response.Errors.Count > 0)
            {
                lines.Add("Errors:");
                foreach (var error in response.Errors)
                {
                    lines.Add($"  - [{error.Code}] {error.Message}");
                    if (!string.IsNullOrEmpty(error.Field))
                        lines.Add($"    Field: {error.Field}");
                }
            }

            if (response.Data != null)
            {
                lines.Add($"Data: {response.Data}");
            }

            return string.Join(Environment.NewLine, lines);
        }

        /// <summary>
        /// Formats a conversion result as plain text with summary statistics.
        /// </summary>
        public string FormatResult(ConversionResult result)
        {
            var lines = new List<string>();

            lines.Add($"Input File: {result.InputFile}");
            lines.Add($"Output File: {result.OutputFile}");
            lines.Add($"Status: {(result.Success ? "SUCCESS" : "FAILED")}");

            if (result.Success)
            {
                lines.Add($"Duration: {result.Duration:0.0} seconds");
                lines.Add($"Execution Time: {result.ExecutionTime.TotalMilliseconds:0} ms");
            }
            else if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                lines.Add($"Error: {result.ErrorMessage}");
            }

            return string.Join(Environment.NewLine, lines);
        }
    }
}
