namespace FFmpegDotnetWrapper.Exceptions;

/// <summary>
/// Provides static methods for exception throwing with context enrichment.
/// This class consolidates exception handling logic across all FFmpeg-related exceptions.
/// </summary>
public static class Throw
{
    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> if the specified value is null.
    /// </summary>
    /// <typeparam name="T">The type of the value to check.</typeparam>
    /// <param name="value">The value to check for null.</param>
    /// <param name="paramName">The name of the parameter being checked.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static void IfNull<T>([ValidatedNotNull] T? value, string paramName) where T : class
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the specified string is null or empty.
    /// </summary>
    /// <param name="value">The string value to check.</param>
    /// <param name="paramName">The name of the parameter being checked.</param>
    /// <exception cref="ArgumentException"><paramref name="value"/> is null or empty.</exception>
    public static void IfNullOrEmpty([ValidatedNotNull] string? value, string paramName)
    {
        ArgumentException.ThrowIfNullOrEmpty(value, paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the specified string is null, empty, or whitespace.
    /// </summary>
    /// <param name="value">The string value to check.</param>
    /// <param name="paramName">The name of the parameter being checked.</param>
    /// <exception cref="ArgumentException"><paramref name="value"/> is null, empty, or whitespace.</exception>
    public static void IfNullOrWhitespace([ValidatedNotNull] string? value, string paramName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);
    }

    /// <summary>
    /// Enriches an exception with context data from a CLI command execution.
    /// Adds CliCommand, ExitCode, and ErrorOutput to the exception's Data dictionary.
    /// </summary>
    /// <param name="exception">The exception to enrich with context.</param>
    /// <param name="cliCommand">The CLI command that was executed.</param>
    /// <param name="exitCode">The exit code from the process execution.</param>
    /// <param name="errorOutput">The error output from the process.</param>
    /// <returns>The enriched exception (same instance).</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null.</exception>
    public static T WithCliContext<T>(
        T exception,
        string? cliCommand,
        int? exitCode = null,
        string? errorOutput = null) where T : FFmpegException
    {
        ArgumentNullException.ThrowIfNull(exception);

        if (cliCommand is not null)
        {
            exception.Data[nameof(cliCommand)] = cliCommand;
        }

        if (exitCode.HasValue)
        {
            exception.Data[nameof(exitCode)] = exitCode.Value;
            exception.ExitCode = exitCode.Value;
        }

        if (errorOutput is not null)
        {
            exception.Data[nameof(errorOutput)] = errorOutput;
            exception.ErrorOutput = errorOutput;
        }

        return exception;
    }

    /// <summary>
    /// Enriches an exception with configuration context.
    /// Adds ConfigurationKey to the exception's Data dictionary.
    /// </summary>
    /// <param name="exception">The exception to enrich with context.</param>
    /// <param name="configurationKey">The configuration key that caused the error.</param>
    /// <returns>The enriched exception (same instance).</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null.</exception>
    public static T WithConfigurationContext<T>(
        T exception,
        string? configurationKey) where T : FFmpegException
    {
        ArgumentNullException.ThrowIfNull(exception);

        if (configurationKey is not null)
        {
            exception.Data[nameof(configurationKey)] = configurationKey;
            if (exception is InvalidOperationConfigurationException configEx)
            {
                configEx.ConfigurationKey = configurationKey;
            }
        }

        return exception;
    }

    /// <summary>
    /// Enriches an exception with file operation context.
    /// Adds FilePath to the exception's Data dictionary.
    /// </summary>
    /// <param name="exception">The exception to enrich with context.</param>
    /// <param name="filePath">The file path involved in the operation.</param>
    /// <returns>The enriched exception (same instance).</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null.</exception>
    public static T WithFileContext<T>(
        T exception,
        string? filePath) where T : FFmpegException
    {
        ArgumentNullException.ThrowIfNull(exception);

        if (filePath is not null)
        {
            exception.Data[nameof(filePath)] = filePath;
            if (exception is FileOperationException fileEx)
            {
                fileEx.FilePath = filePath;
            }
        }

        return exception;
    }

    /// <summary>
    /// Enriches an exception with repository context.
    /// Adds RepositoryName to the exception's Data dictionary.
    /// </summary>
    /// <param name="exception">The exception to enrich with context.</param>
    /// <param name="repositoryName">The repository name involved in the operation.</param>
    /// <returns>The enriched exception (same instance).</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null.</exception>
    public static T WithRepositoryContext<T>(
        T exception,
        string? repositoryName) where T : FFmpegException
    {
        ArgumentNullException.ThrowIfNull(exception);

        if (repositoryName is not null)
        {
            exception.Data[nameof(repositoryName)] = repositoryName;
            if (exception is RepositoryException repoEx)
            {
                repoEx.RepositoryName = repositoryName;
            }
        }

        return exception;
    }

    /// <summary>
    /// Enriches an exception with service context.
    /// Adds ServiceName to the exception's Data dictionary.
    /// </summary>
    /// <param name="exception">The exception to enrich with context.</param>
    /// <param name="serviceName">The service name involved in the operation.</param>
    /// <returns>The enriched exception (same instance).</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null.</exception>
    public static T WithServiceContext<T>(
        T exception,
        string? serviceName) where T : FFmpegException
    {
        ArgumentNullException.ThrowIfNull(exception);

        if (serviceName is not null)
        {
            exception.Data[nameof(serviceName)] = serviceName;
            if (exception is ServiceException serviceEx)
            {
                serviceEx.ServiceName = serviceName;
            }
        }

        return exception;
    }

    /// <summary>
    /// Enriches an exception with media file context.
    /// Adds FilePath to the exception's Data dictionary.
    /// </summary>
    /// <param name="exception">The exception to enrich with context.</param>
    /// <param name="filePath">The media file path involved in the operation.</param>
    /// <returns>The enriched exception (same instance).</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null.</exception>
    public static T WithMediaFileContext<T>(
        T exception,
        string? filePath) where T : FFmpegException
    {
        ArgumentNullException.ThrowIfNull(exception);

        if (filePath is not null)
        {
            exception.Data[nameof(filePath)] = filePath;
            if (exception is InvalidMediaFileException mediaEx)
            {
                mediaEx.FilePath = filePath;
            }
        }

        return exception;
    }

    /// <summary>
    /// Enriches an exception with validation context.
    /// Adds ValidationErrors to the exception's Data dictionary.
    /// </summary>
    /// <param name="exception">The exception to enrich with context.</param>
    /// <param name="validationErrors">The validation errors dictionary.</param>
    /// <returns>The enriched exception (same instance).</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null.</exception>
    public static T WithValidationContext<T>(
        T exception,
        Dictionary<string, string[]>? validationErrors) where T : FFmpegException
    {
        ArgumentNullException.ThrowIfNull(exception);

        if (validationErrors is not null && validationErrors.Count > 0)
        {
            exception.Data[nameof(validationErrors)] = validationErrors;
            if (exception is ValidationException valEx)
            {
                valEx.ValidationErrors = validationErrors;
            }
        }

        return exception;
    }

    /// <summary>
    /// Creates a new exception with the specified type and message.
    /// </summary>
    /// <typeparam name="T">The exception type to create.</typeparam>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    /// <returns>A new exception instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="message"/> is null.</exception>
    public static T New<T>(
        string message,
        Exception? innerException = null) where T : FFmpegException
    {
        ArgumentException.ThrowIfNullOrEmpty(message, nameof(message));

        // Use reflection to call the appropriate constructor
        if (innerException is null)
        {
            return (T)Activator.CreateInstance(typeof(T), message)!;
        }
        else
        {
            return (T)Activator.CreateInstance(typeof(T), message, innerException)!;
        }
    }
}

/// <summary>
/// Marker attribute to indicate that a parameter should be validated as not null.
/// Used with the Throw class validation methods.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class ValidatedNotNullAttribute : Attribute { }
