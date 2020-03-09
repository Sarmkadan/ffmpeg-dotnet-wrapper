# FFmpegOperation

Represents a single, self-contained FFmpeg processing task within the `ffmpeg-dotnet-wrapper` library. An `FFmpegOperation` encapsulates all parameters required to execute an FFmpeg command—including input files, output file, arguments, and execution constraints such as timeout and priority—and provides methods for building the command-line string, validating the configuration, and cloning the operation for reuse or modification.

## API

### Properties

#### `public string Id`
A unique identifier for the operation, typically assigned at creation time. This value is used for tracking, logging, and correlation purposes.

#### `public string Name`
A human-readable label for the operation. May be used in UI displays, logs, or job management interfaces to distinguish operations without inspecting their full argument lists.

#### `public FFmpegOperationType Type`
Indicates the category of the operation (e.g., transcode, remux, filter). The exact values are defined by the `FFmpegOperationType` enumeration and influence validation logic and default argument handling.

#### `public List<string> InputFiles`
The ordered list of input file paths that FFmpeg will process. The order corresponds directly to the order of `-i` arguments in the generated command line. This list is mutable and can be modified via `AddInputFile`.

#### `public string OutputFile`
The path to the output file that FFmpeg will produce. This value is required for validation and becomes the final positional argument in the command line.

#### `public List<string> Arguments`
The list of raw FFmpeg arguments (flags, filters, codec options, etc.) to be placed between the input file specifications and the output file in the generated command. Each element is appended as a separate argument token. This list is mutable via `AddArgument` and `AddArguments`.

#### `public TimeSpan? Timeout`
An optional maximum duration for the operation’s execution. When set, the wrapper will terminate the FFmpeg process if it exceeds this duration. A `null` value indicates no timeout is enforced.

#### `public int? Priority`
An optional OS-level process priority hint. The interpretation depends on the execution environment; `null` means the default priority is used.

#### `public bool IsParallel`
Indicates whether this operation is eligible for concurrent execution alongside other operations. When `false`, the scheduler will execute this operation in isolation.

#### `public Dictionary<string, string> CustomProperties`
A general-purpose dictionary for attaching arbitrary metadata to the operation. Keys and values are user-defined strings. This data is not used by the command-line builder or validator but is preserved during cloning.

#### `public DateTime CreatedAt`
The timestamp (UTC) when the operation instance was created. Set automatically at instantiation and not modified thereafter.

#### `public DateTime? ExecutedAt`
The timestamp (UTC) when the operation was actually executed by the wrapper’s scheduler. `null` if the operation has not yet been executed.

### Methods

#### `public void AddInputFile(string filePath)`
Appends a single input file path to the `InputFiles` list.

- **Parameters**: `filePath` — the path to the input file.
- **Throws**: `ArgumentNullException` if `filePath` is `null`; `ArgumentException` if `filePath` is empty or consists only of whitespace.

#### `public void AddArgument(string argument)`
Appends a single argument token to the `Arguments` list.

- **Parameters**: `argument` — the argument string (e.g., `"-c:v"`, `"libx264"`).
- **Throws**: `ArgumentNullException` if `argument` is `null`; `ArgumentException` if `argument` is empty or consists only of whitespace.

#### `public void AddArguments(IEnumerable<string> arguments)`
Appends multiple argument tokens to the `Arguments` list in the order they are provided.

- **Parameters**: `arguments` — a collection of argument strings.
- **Throws**: `ArgumentNullException` if `arguments` is `null`; `ArgumentException` if any element in the collection is `null`, empty, or whitespace.

#### `public string BuildCommandLine()`
Constructs the full FFmpeg command-line string from the operation’s current state. The generated string follows the pattern: `ffmpeg [arguments] -i [input1] -i [input2] ... [arguments] [output]`.

- **Returns**: The complete command-line string suitable for passing to an FFmpeg process.
- **Throws**: `InvalidOperationException` if `OutputFile` is `null` or empty, or if `InputFiles` contains no entries.

#### `public void Validate()`
Performs a comprehensive validation of the operation’s configuration. Checks include: `OutputFile` must be set, `InputFiles` must contain at least one entry, and type-specific constraints defined by `FFmpegOperationType` must be satisfied.

- **Throws**: `ValidationException` (or a derived type) if any validation rule fails, with a message describing the specific failure.

#### `public FFmpegOperation Clone()`
Creates a deep copy of the operation. All properties, including lists and the custom properties dictionary, are duplicated. The clone receives a new `Id` and its `ExecutedAt` is set to `null`. `CreatedAt` is set to the current time.

- **Returns**: A new `FFmpegOperation` instance that is functionally identical to the original but independent of it.

#### `public string GetDescription()`
Returns a human-readable summary of the operation, typically including the `Name`, `Type`, input file count, and output file. Intended for logging and diagnostic output.

- **Returns**: A formatted string describing the operation.

## Usage

### Example 1: Basic Transcoding Operation

```csharp
var operation = new FFmpegOperation
{
    Name = "Convert to H.264",
    Type = FFmpegOperationType.Transcode,
    OutputFile = @"C:\output\video.mp4",
    Timeout = TimeSpan.FromMinutes(10)
};

operation.AddInputFile(@"C:\input\source.mov");
operation.AddArguments(new[] { "-c:v", "libx264", "-preset", "medium", "-crf", "23" });

operation.Validate();

string commandLine = operation.BuildCommandLine();
Console.WriteLine(commandLine);
// Output: ffmpeg -i "C:\input\source.mov" -c:v libx264 -preset medium -crf 23 "C:\output\video.mp4"
```

### Example 2: Cloning and Modifying for Batch Processing

```csharp
var baseOperation = new FFmpegOperation
{
    Name = "Watermark Template",
    Type = FFmpegOperationType.Filter,
    OutputFile = @"C:\output\watermarked.mp4",
    Priority = 2,
    IsParallel = true
};

baseOperation.AddArguments(new[] { "-vf", "drawtext=text='Confidential':x=10:y=10:fontsize=24:fontcolor=white" });

foreach (string sourceFile in Directory.GetFiles(@"C:\input", "*.mp4"))
{
    var job = baseOperation.Clone();
    job.Name = $"Watermark - {Path.GetFileName(sourceFile)}";
    job.AddInputFile(sourceFile);
    job.OutputFile = Path.Combine(@"C:\output", Path.GetFileName(sourceFile));
    job.CustomProperties["source"] = sourceFile;

    job.Validate();
    // Submit job to scheduler...
}
```

## Notes

- **Thread safety**: Instance members are not synchronized. Concurrent reads and writes to the same `FFmpegOperation` from multiple threads will result in undefined behavior. Cloning is the recommended approach when an operation template must be shared across threads.
- **Validation timing**: `Validate()` should be called after all inputs and arguments are configured and before execution is requested. `BuildCommandLine()` does not implicitly call `Validate()`; it only checks the minimal conditions required to produce a syntactically valid command line.
- **Argument ordering**: The `Arguments` list is emitted in the exact order elements were added. FFmpeg is sensitive to argument placement relative to input files; ensure arguments intended for specific inputs are added after the corresponding `AddInputFile` call, as `BuildCommandLine` places all global arguments before the first `-i` and does not interleave per-input options automatically.
- **Cloning semantics**: `Clone()` assigns a new `Id` and resets `ExecutedAt` to `null`. The `CustomProperties` dictionary is deep-copied; modifications to the clone’s dictionary do not affect the original.
- **Timeout and Priority**: These properties are hints consumed by the execution layer. Setting `Timeout` to a very low value may cause premature termination of valid long-running operations. `Priority` values are platform-dependent; consult the execution environment’s documentation for valid ranges.
- **Empty collections**: `InputFiles` and `Arguments` may be empty during intermediate configuration steps, but `Validate()` and `BuildCommandLine()` will fail if `InputFiles` is empty or `OutputFile` is not set at the time of their invocation.
