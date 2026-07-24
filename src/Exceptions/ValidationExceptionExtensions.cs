using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFmpegDotnetWrapper.Exceptions
{
    /// <summary>
    /// Extension methods that make working with <see cref="ValidationException"/> more convenient.
    /// </summary>
    public static class ValidationExceptionExtensions
    {
        /// <summary>
        /// Flattens all validation error messages into a single enumerable.
        /// </summary>
        /// <param name="ex">The validation exception instance.</param>
        /// <returns>An enumerable containing all validation error messages across all fields.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ex"/> is <see langword="null"/>.</exception>
        public static IEnumerable<string> GetAllErrorMessages(this ValidationException ex)
        {
            ArgumentNullException.ThrowIfNull(ex);

            return ex.ValidationErrors?.Values.SelectMany(v => v) ?? Enumerable.Empty<string>();
        }

        /// <summary>
        /// Determines whether the exception contains any errors for the specified field.
        /// </summary>
        /// <param name="ex">The validation exception instance.</param>
        /// <param name="field">The field name to check for errors.</param>
        /// <returns><see langword="true"/> if the exception has errors for the specified field; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ex"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="field"/> is <see langword="null"/> or empty.</exception>
        public static bool HasErrorForField(this ValidationException ex, string field)
        {
            ArgumentNullException.ThrowIfNull(ex);
            ArgumentException.ThrowIfNullOrEmpty(field, nameof(field));

            return ex.ValidationErrors?.TryGetValue(field, out var errors) == true && errors.Length > 0;
        }

        /// <summary>
        /// Returns a new <see cref="ValidationException"/> that includes an additional error for the given field.
        /// The original exception is left unchanged.
        /// </summary>
        /// <param name="ex">The original validation exception instance.</param>
        /// <param name="field">The field name to add the error to.</param>
        /// <param name="errorMessage">The error message to add.</param>
        /// <returns>A new <see cref="ValidationException"/> with the additional error included.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="ex"/> is <see langword="null"/>.
        /// <paramref name="field"/> is <see langword="null"/>.
        /// <paramref name="errorMessage"/> is <see langword="null"/>.
        /// </exception>
        public static ValidationException WithAddedError(this ValidationException ex, string field, string errorMessage)
        {
            ArgumentNullException.ThrowIfNull(ex);
            ArgumentException.ThrowIfNullOrEmpty(field, nameof(field));
            ArgumentNullException.ThrowIfNull(errorMessage);

            // Copy existing errors (if any) into a new dictionary
            var existingErrors = ex.ValidationErrors ?? new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            var newDict = new Dictionary<string, string[]>(existingErrors, StringComparer.OrdinalIgnoreCase);

            if (newDict.TryGetValue(field, out var existing))
            {
                var combined = new List<string>(existing) { errorMessage };
                newDict[field] = combined.ToArray();
            }
            else
            {
                newDict[field] = new[] { errorMessage };
            }

            return ex.InnerException != null
                ? new ValidationException(ex.Message, newDict, ex.InnerException)
                : new ValidationException(ex.Message, newDict);
        }

        /// <summary>
        /// Produces a detailed string representation of the exception, including all validation errors.
        /// </summary>
        /// <param name="ex">The validation exception instance.</param>
        /// <returns>A formatted string containing the exception message and all validation errors.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ex"/> is <see langword="null"/>.</exception>
        public static string ToDetailedString(this ValidationException ex)
        {
            ArgumentNullException.ThrowIfNull(ex);

            var sb = new StringBuilder();
            sb.AppendLine(ex.Message);

            if (ex.ValidationErrors is { Count: > 0 })
            {
                foreach (var kvp in ex.ValidationErrors)
                {
                    sb.AppendLine($"{kvp.Key}: {string.Join(", ", kvp.Value)}");
                }
            }

            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// Adds additional context to the exception's Context dictionary.
        /// </summary>
        /// <param name="ex">The validation exception to update.</param>
        /// <param name="key">The context key to add.</param>
        /// <param name="value">The context value to add.</param>
        /// <returns>The same exception instance for fluent chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="ex"/> is null.</exception>
        public static ValidationException WithContext(this ValidationException ex, string key, string value)
        {
            ArgumentNullException.ThrowIfNull(ex);
            ArgumentException.ThrowIfNullOrEmpty(key, nameof(key));
            ArgumentException.ThrowIfNullOrEmpty(value, nameof(value));

            ex.Context[key] = value;
            return ex;
        }

        /// <summary>
        /// Gets the validation errors count from the exception's Context dictionary.
        /// </summary>
        /// <param name="ex">The validation exception to check.</param>
        /// <returns>The validation errors count if available; otherwise, 0.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="ex"/> is null.</exception>
        public static int GetValidationErrorsCount(this ValidationException ex)
        {
            ArgumentNullException.ThrowIfNull(ex);
            return ex.ValidationErrors?.Count ?? 0;
        }
    }
}
