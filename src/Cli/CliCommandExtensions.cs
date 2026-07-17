using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FFmpegDotnetWrapper.Cli
{
    /// <summary>
    /// Provides extension methods for <see cref="CliCommand"/>.
    /// </summary>
    /// <remarks>
/// All methods in this class are designed to be null-safe and throw appropriate exceptions
/// for invalid arguments, following .NET design guidelines.
/// </remarks>
public static class CliCommandExtensions
    {
        /// <summary>
        /// Checks if the command has the specified option.
        /// </summary>
        /// <param name="command">The CLI command to check.</param>
        /// <param name="optionName">The name of the option to check for.</param>
        /// <returns><see langword="true"/> if the option exists; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="optionName"/> is null or empty.</exception>
        public static bool HasOption(this CliCommand command, string optionName)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentException.ThrowIfNullOrEmpty(optionName);

            return command.Options.ContainsKey(optionName);
        }

        /// <summary>
        /// Gets the value of the specified option.
        /// </summary>
        /// <param name="command">The CLI command to check.</param>
        /// <param name="optionName">The name of the option to check for.</param>
        /// <returns>The value of the option if it exists; otherwise, <see langword="null"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="optionName"/> is null or empty.</exception>
        public static string? GetOptionValue(this CliCommand command, string optionName)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentException.ThrowIfNullOrEmpty(optionName);

            return command.Options.TryGetValue(optionName, out var value) ? value : null;
        }

        /// <summary>
        /// Tries to get the value of the specified option.
        /// </summary>
        /// <param name="command">The CLI command to check.</param>
        /// <param name="optionName">The name of the option to check for.</param>
        /// <param name="value">When this method returns, contains the option value if it exists; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the option exists; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="optionName"/> is null or empty.</exception>
        public static bool TryGetOptionValue(this CliCommand command, string optionName, [NotNullWhen(true)] out string? value)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentException.ThrowIfNullOrEmpty(optionName);

            return command.Options.TryGetValue(optionName, out value);
        }
    }
}
