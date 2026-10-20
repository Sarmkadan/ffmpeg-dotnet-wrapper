# OptionsValidationException

`OptionsValidationException` is thrown when an options object fails validation, such as [`ConversionOptions.Validate()`](ConversionOptions.md). It derives from [`ValidationException`](ValidationException.md).

## Members

- `Violations` (`IReadOnlyList<string>`): one human-readable entry per violated rule, in the order the rules were checked. Always contains at least one entry.
- `Message`: a header with the violation count, followed by each violation on its own line:

```
Invalid options (2 violation(s)):
  - Crf must be between 0 and 51, but was 60
  - Width must be a positive even number, but was 3
```

## Constructor

`OptionsValidationException(IReadOnlyList<string> violations)` throws `ArgumentNullException` for `null` and `ArgumentException` for an empty list.
