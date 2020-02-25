# CliOutputFormatter

The `CliOutputFormatter` class provides a set of standardized string formatting methods designed to produce consistent and professional console output for command-line interfaces. It encapsulates formatting logic for various common CLI scenarios, including status messages, progress reporting, tabular data, and structured key-value pairs, ensuring uniform visual presentation across the `ffmpeg-dotnet-wrapper` ecosystem.

## API

### Constructors

#### `public CliOutputFormatter()`
Initializes a new instance of the `CliOutputFormatter` class.

### Properties

#### `public string FormatSuccess`
A getter property that returns a standard formatted string for success messages, typically prefixed with an appropriate success indicator.

#### `public string FormatError`
A getter property that returns a standard formatted string for error messages, typically prefixed with an appropriate error indicator.

#### `public string FormatWarning`
A getter property that returns a standard formatted string for warning messages, typically prefixed with an appropriate warning indicator.

#### `public string FormatInfo`
A getter property that returns a standard formatted string for general information messages, typically prefixed with an appropriate information indicator.

### Methods

#### `public string FormatConversionResult(object result)`
Formats a conversion operation result object into a human-readable string.

#### `public string FormatResultsTable(IEnumerable<object> data)`
Formats a collection of data objects into a structured table string suitable for console output.

#### `public string FormatProgressBar(double progress, string label)`
Generates a visual progress bar string based on the provided percentage (0.0 to 1.0) and an optional label.

#### `public string FormatSummary(string title, IEnumerable<string> items)`
Creates a formatted summary block with a title followed by a list of items.

#### `public string FormatHelpBox(string content)`
Formats the provided content into a visual help box or guidance block.

#### `public string FormatKeyValue(string key, object value)`
Formats a key-value pair into a structured string, ideal for displaying configuration settings or metadata.

#### `public string FormatApiResponse<T>(T response)`
Formats an API response object of type `T` into a structured, readable string representation.

## Usage

### Displaying Standard Messages
```csharp
var formatter = new CliOutputFormatter();

Console.WriteLine(formatter.FormatSuccess + "Conversion completed successfully.");
Console.WriteLine(formatter.FormatError + "An error occurred during transcoding.");
```

### Displaying a Progress Bar
```csharp
var formatter = new CliOutputFormatter();

// Simulating progress
double progress = 0.75;
Console.Write("\r" + formatter.FormatProgressBar(progress, "Transcoding"));
```

## Notes

- **Thread-Safety**: This class is designed to be thread-safe for reading properties; however, it does not maintain internal state. Consumers should handle their own console locking if multiple threads attempt to write to the console simultaneously.
- **Formatting Exceptions**: Methods may throw `ArgumentNullException` if provided input parameters are null, or `FormatException` if the underlying objects cannot be successfully serialized or converted to string representations.
- **Console Width**: `FormatResultsTable` assumes a reasonable console width. Extremely long data strings may cause wrapping that disrupts the visual alignment of the table.
