# CliCommand

The `CliCommand` type provides a structured mechanism for defining, configuring, and parsing command-line interfaces within the `ffmpeg-dotnet-wrapper` project. It encapsulates the hierarchy of commands, arguments, and options, allowing developers to define complex CLI structures declaratively and facilitate the generation of help documentation and command validation.

## API

### Properties

*   `Name` (string): The identifier for the command, argument, or option.
*   `Arguments` (List<string> or List<CliArgument>): The collection of arguments defined for this command.
*   `Options` (Dictionary<string, string?> or List<CliOption>): The collection of options defined for this command.
*   `SubCommand` (string?): The identifier of a nested command, if applicable.
*   `Description` (string): A text description used in generated help output.
*   `IsRequired` (bool): Indicates if the argument or option must be provided.
*   `DefaultValue` (string?): The fallback value if an option or argument is not explicitly provided.
*   `LongForm` (string): The full-name variant of a command option (e.g., `--output`).
*   `ShortForm` (string?): The abbreviated-name variant of a command option (e.g., `-o`).

### Methods

*   `RegisterCommand()`: Adds the current command definition to the active parser registry.
*   `ParseCommand()` (CliCommand?): Analyzes input strings against defined configurations and returns a populated `CliCommand` instance or `null` if parsing fails.
*   `GenerateHelpText()` (string): Produces a formatted string containing usage instructions for the command.
*   `GenerateCommandHelpText()` (string): Produces detailed documentation for a specific command and its associated options and arguments.
*   `ValidateCommand()` (List<string>): Evaluates the current configuration against required constraints and returns a list of validation error messages, if any.

## Usage

### Defining a simple command
```csharp
var command = new CliCommand {
    Name = "convert",
    Description = "Converts input media file to target format."
};
command.Arguments.Add(new CliArgument { Name = "input", Description = "Path to source file", IsRequired = true });
command.RegisterCommand();
```

### Parsing input and generating help
```csharp
var parser = new CliParser(); // Assuming a parser entry point
var parsed = command.ParseCommand(args);

if (parsed == null) {
    Console.WriteLine(command.GenerateHelpText());
} else {
    // Execute command logic
}
```

## Notes

*   **Thread-Safety**: Instances of `CliCommand` are generally not thread-safe for concurrent modifications. Configuration should be performed during application initialization.
*   **Validation**: `ValidateCommand` should be called before attempting to execute command logic to ensure all required arguments and options are present and satisfy defined constraints.
*   **Help Generation**: Help text generation relies on the `Description` fields; ensuring these are populated is essential for user-friendly CLI interaction.
