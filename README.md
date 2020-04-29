// ... (rest of README.md content remains unchanged)

## CliCommand

The `CliCommand` class represents a single command with its arguments and options. It provides properties to access the command name, arguments, options, and sub-command. Here's an example of how to create and use a `CliCommand` object:

```csharp
var command = new CliCommand
{
    Name = "transcode",
    Arguments = new List<string> { "input.mp4", "output.mkv" },
    Options = new Dictionary<string, string?> { { "codec", "h265" }, { "bitrate", "5000" } },
    SubCommand = "video"
};

Console.WriteLine($"Command: {command.Name}");
Console.WriteLine($"Arguments: {string.Join(", ", command.Arguments)}");
Console.WriteLine($"Options: {string.Join(", ", command.Options.Select(x => $"{x.Key}={x.Value}"))}");
Console.WriteLine($"SubCommand: {command.SubCommand}");
```

## ... (rest of README.md content remains unchanged)
