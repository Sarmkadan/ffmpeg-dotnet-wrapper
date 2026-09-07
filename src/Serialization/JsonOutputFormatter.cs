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

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonOutputFormatter"/> class.
        /// </summary>
        /// <param name="indent">Whether the serialized JSON should be indented.</param>
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
        /// <typeparam name="T">The type of data contained in the response.</typeparam>
        /// <param name="response">The API response to serialize.</param>
        /// <returns>A JSON representation of the API response.</returns>
        public string Format<T>(ApiResponse<T> response)
        {
            ArgumentNullException.ThrowIfNull(response);
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
        /// <param name="response">The API response to serialize.</param>
        /// <returns>A JSON representation of the API response.</returns>
        public string Format(ApiResponse response)
        {
            ArgumentNullException.ThrowIfNull(response);
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
        /// <typeparam name="T">The type of object to serialize.</typeparam>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>A JSON representation of the object.</returns>
        public string Format<T>(T obj)
        {
            ArgumentNullException.ThrowIfNull(obj);
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
        /// Deserializes JSON string to an API response envelope object.
        /// Handles type conversion and validation.
        /// </summary>
        /// <typeparam name="T">The type of data contained in the response.</typeparam>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized API response, or <see langword="null"/> if the JSON represents a null value.</returns>
        public ApiResponse<T>? DeserializeApiResponse<T>(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);
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
        /// <typeparam name="T">The type to deserialize the JSON into.</typeparam>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized value, or <see langword="null"/> if the JSON represents a null value.</returns>
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
        /// <param name="results">The conversion results to format.</param>
        /// <returns>A CSV string containing a header and one row for each conversion result.</returns>
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
        /// <typeparam name="T">The type of data contained in the response.</typeparam>
        /// <param name="response">The API response to format.</param>
        /// <returns>A human-readable plain-text representation of the API response.</returns>
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
        /// <param name="result">The conversion result to format.</param>
        /// <returns>A human-readable plain-text representation of the conversion result.</returns>
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
