# FormattingUtilitiesTests

Unit tests for the `FormattingUtilities` class, verifying correct string formatting behavior for durations, byte sizes, bitrates, strings, percentages, ETA, and resolution values.

## API

### `FormatDuration_LessThanOneMinute_ReturnsZeroHoursAndMinutes`
Verifies that durations under one minute are formatted as `0h 0m`.

### `FormatDuration_BetweenOneAndSixtyMinutes_ReturnsZeroHours`
Verifies that durations between one and sixty minutes are formatted as `0h XXm`.

### `FormatDuration_MoreThanOneHour_IncludesHours`
Verifies that durations exceeding one hour include hours in the output, e.g., `Xh XXm`.

### `FormatBytes_LessThanOneKilobyte_ReturnsByteSuffix`
Verifies that byte values under 1024 are suffixed with `B`.

### `FormatBytes_ExactMegabyte_ReturnsMbSuffix`
Verifies that exact megabyte values (1024 * 1024) are suffixed with `MB`.

### `FormatBytes_LargeGigabyteValue_ReturnsGbSuffix`
Verifies that large values in the gigabyte range are suffixed with `GB`.

### `FormatBitrate_BelowOneThousand_ReturnsKbps`
Verifies that bitrates under 1000 are formatted with `kbps`.

### `FormatBitrate_Thousands_ReturnsMbps`
Verifies that bitrates in the thousands are formatted with `Mbps`.

### `FormatBitrate_Millions_ReturnsGbps`
Verifies that bitrates in the millions are formatted with `Gbps`.

### `TruncateString_BelowMaxLength_ReturnsUnchanged`
Verifies that strings shorter than the max length are returned unchanged.

### `TruncateString_ExceedsMaxLength_AppendsEllipsis`
Verifies that strings exceeding the max length are truncated and suffixed with `…`.

### `TruncateString_NullOrEmpty_ReturnsEmptyString`
Verifies that null or empty strings are returned as empty strings.

### `TitleCase_KebabOrSnakeCase_ReturnsTitleCase`
Verifies that kebab-case or snake_case strings are converted to Title Case.

### `FormatPercentage_VariousValues_ReturnsOneDecimalPlace`
Verifies that percentage values are formatted with one decimal place, e.g., `12.3%`.

### `FormatETA_ZeroProgress_ReturnsCalculatingMessage`
Verifies that a zero progress value returns `Calculating…`.

### `FormatETA_HalfwayThrough_ReturnsRemainingTimeEstimate`
Verifies that a halfway progress value returns a formatted remaining time estimate.

### `SanitizeForDisplay_StringWithControlChars_RemovesThem`
Verifies that control characters are removed from display strings.

### `SanitizeForDisplay_StringWithNewline_PreservesNewline`
Verifies that newline characters are preserved in display strings.

### `FormatResolution_StandardHd_ReturnsWidthXHeight`
Verifies that standard HD resolutions (e.g., 1280x720) are formatted as `WxH`.

## Usage
