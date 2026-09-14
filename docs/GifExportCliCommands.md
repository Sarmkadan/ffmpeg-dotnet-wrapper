# GifExportCliCommands Documentation

This document describes the `GifExportCliCommands` class and related types in the `FFmpegDotnetWrapper.Cli` namespace.

## Overview

The `GifExportCliCommands` provides CLI command definitions for GIF export operations and helper methods for parsing time strings, dither modes, and quality presets.

## Classes

### GifExportCliCommands

A static class that provides extension methods to register GIF export commands and parse related values.

#### Purpose
- Register the 'gif' command via `RegisterGifExportCommands`.
- Parse time strings in HH:MM:SS or MM:SS format.
- Parse dither mode strings to the `DitherMode` enum.
- Parse quality preset strings to the `GifQualityPreset` enum.

#### Public API

| Member | Type | Description |
|--------|------|-------------|
| `RegisterGifExportCommands(this CliCommandParser parser)` | `void` | Registers the 'gif' command for GIF export operations. Throws `ArgumentNullException` if `parser` is null. |
| `ParseTimeString(string timeString)` | `TimeSpan` | Parses a time string in HH:MM:SS or MM:SS format into a TimeSpan. Throws `ArgumentException` if the format is invalid. |
| `ParseDitherMode(string ditherString)` | `DitherMode` | Converts a dither mode string to the DitherMode enum. Throws `ArgumentException` if the string is invalid. |
| `ParseQualityPreset(string qualityString)` | `GifQualityPreset` | Converts a quality preset string to the GifQualityPreset enum. Throws `ArgumentException` if the string is invalid. |

### The 'gif' Command

The 'gif' command is used to export a video segment as an optimized GIF using two-pass palette generation.

#### Arguments

| Name | Description | Required |
|------|-------------|----------|
| source-path | Path to the source video file. | Yes |
| output-path | Path where the GIF file will be saved. | Yes |

#### Options

| Long Form | Short Form | Description | Requires Value | Default Value |
|-----------|------------|-------------|----------------|---------------|
| start | s | Start time of the segment in HH:MM:SS format. | Yes | 00:00:00 |
| duration | d | Duration of the segment in HH:MM:SS format. | Yes | 00:00:10 |
| fps | f | Frames per second for the output GIF. | Yes | 10 |
| width | w | Target width of the GIF (height is scaled to preserve aspect ratio). | Yes | 640 |
| max-width | mw | Maximum width of the output GIF. Takes precedence over --width. | Yes | (none) |
| dither | dt | Dithering mode for palette conversion (none, bayer, heckbert, floyd_steinberg, sierra2, sierra2_4a, sierra3, burkes, atkinson). | Yes | sierra2_4a |
| loop | l | Number of times the GIF should loop. Use -1 for infinite loop. | Yes | -1 |
| quality | q | Quality preset (low, medium, high). | Yes | medium |

#### Mapping to GifExportSettings

The parsed command options are mapped to the `GifExportSettings` object as follows:
- `source-path` → `SourcePath`
- `output-path` → `OutputPath`
- `start` → `StartTime` (as `TimeSpan`)
- `duration` → `Duration` (as `TimeSpan`)
- `fps` → `FramesPerSecond`
- `width` → `Width`
- `max-width` → `MaxWidth`
- `dither` → `DitherMode`
- `loop` → `LoopCount`
- `quality` → `QualityPreset`

#### Sample CLI Invocation

```bash
ffmpeg-dotnet-wrapper gif input.mp4 output.gif --start 00:00:05 --duration 00:00:03 --fps 15 --width 320 --dither floyd_steinberg --loop 0 --quality high
```