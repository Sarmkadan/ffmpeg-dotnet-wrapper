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
        public static IEnumerable<string> GetAllErrorMessages(this ValidationException ex)
        {
            if (ex.ValidationErrors == null)
                return Enumerable.Empty<string>();

            return ex.ValidationErrors.Values.SelectMany(v => v);
        }

        /// <summary>
        /// Determines whether the exception contains any errors for the specified field.
        /// </summary>
        public static bool HasErrorForField(this ValidationException ex, string field)
        {
            if (ex.ValidationErrors == null || string.IsNullOrEmpty(field))
                return false;

            return ex.ValidationErrors.TryGetValue(field, out var errors) && errors.Length > 0;
        }

        /// <summary>
        /// Returns a new <see cref="ValidationException"/> that includes an additional error for the given field.
        /// The original exception is left unchanged.
        /// </summary>
        public static ValidationException WithAddedError(this ValidationException ex, string field, string errorMessage)
        {
            if (string.IsNullOrEmpty(field))
                throw new ArgumentException("Field name cannot be null or empty.", nameof(field));

            // Copy existing errors (if any) into a new dictionary.
            var newDict = new Dictionary<string, string[]>(ex.ValidationErrors ?? new Dictionary<string, string[]>(), StringComparer.OrdinalIgnoreCase);

            if (newDict.TryGetValue(field, out var existing))
            {
                var combined = new List<string>(existing) { errorMessage };
                newDict[field] = combined.ToArray();
            }
            else
            {
                newDict[field] = new[] { errorMessage };
            }

            return new ValidationException(ex.Message, newDict, ex.InnerException);
        }

        /// <summary>
        /// Produces a detailed string representation of the exception, including all validation errors.
        /// </summary>
        public static string ToDetailedString(this ValidationException ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine(ex.Message);

            if (ex.ValidationErrors != null && ex.ValidationErrors.Count > 0)
            {
                foreach (var kvp in ex.ValidationErrors)
                {
                    sb.AppendLine($"{kvp.Key}: {string.Join(", ", kvp.Value)}");
                }
            }

            return sb.ToString().TrimEnd();
        }
    }
}
