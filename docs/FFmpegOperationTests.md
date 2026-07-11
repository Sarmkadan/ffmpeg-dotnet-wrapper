# FFmpegOperationTests

Unit test class for verifying the behavior of `FFmpegOperation` and its command-line generation, metric tracking, and async execution. The tests validate input handling, state transitions, metric calculations, and service integration for FFmpeg operations.

## API

### `BuildCommandLine_SingleInputWithArguments_ProducesValidCommand`
Verifies that a single input file with additional arguments generates a syntactically correct FFmpeg command line. The test asserts that the command includes the input path and all provided arguments in the expected format.

### `BuildCommandLine_MultipleInputFiles_IncludesAllInputFlags`
Ensures that when multiple input files are added, each is included in the command line with the correct `-i` flag. The test checks that all input paths are present and properly formatted in the generated command.

### `AddInputFile_NullOrWhitespacePath_IsIgnored`
Confirms that attempting to add a null, empty, or whitespace-only input file path has no effect on the operation. The test asserts that the internal state remains unchanged after such additions.

### `AddArgument_WhitespaceArgument_IsIgnored`
Validates that whitespace-only arguments are ignored and do not appear in the generated command line. The test checks that the operation's argument list remains unaffected by such inputs.

### `Clone_ProducesIndependentCopy_ChangesDontAffectOriginal`
Tests that cloning an `FFmpegOperation` instance creates a deep copy where modifications to the clone do not alter the original. The test asserts that changes to the clone leave the original in its initial state.

### `MarkAsSuccess_SetsIsSuccessTrueAndOutputPath`
Verifies that marking an operation as successful updates its `IsSuccess` flag and sets the output file path. The test checks that both properties are correctly updated and accessible.

### `MarkAsFailed_SetsIsSuccessFalseAndErrorMessage`
Ensures that marking an operation as failed sets `IsSuccess` to `false` and stores the error message. The test asserts that the failure state and message are properly recorded.

### `GetSizeReductionPercentage_WhenNotSuccessful_ReturnsNull`
Confirms that calling `GetSizeReductionPercentage` on a non-successful operation returns `null`. The test validates that the method handles failure states appropriately.

### `GetSizeReductionPercentage_WhenSuccessfulWithSmallerOutput_ReturnsPositivePercentage`
Tests that `GetSizeReductionPercentage` returns a positive percentage when the output file is smaller than the input. The test asserts that the calculation is correct and the value is as expected.

### `SetAndGetMetric_RoundTrip_ReturnsSameValue`
Verifies that setting a metric value and retrieving it returns the same value. The test ensures that metric storage and retrieval are consistent.

### `GetMetric_MissingKey_ReturnsDefault`
Confirms that attempting to retrieve a metric with a non-existent key returns the default value for the metric type. The test checks that missing keys are handled gracefully.

### `GenerateSummary_FailedResult_IncludesErrorInOutput`
Ensures that generating a summary for a failed operation includes the error message in the output. The test asserts that the summary reflects the failure state accurately.

### `TranscodeAsync_WhenCalled_InvokesServiceWithCorrectArguments`
Validates that calling `TranscodeAsync` invokes the underlying FFmpeg service with the expected arguments. The test checks that the service receives the correct input paths, output path, and arguments.

### `IsFFmpegAvailableAsync_WhenMockedTrue_ReturnsTrue`
Tests that `IsFFmpegAvailableAsync` returns `true` when the FFmpeg service is mocked to be available. The test ensures that the availability check behaves as expected under controlled conditions.

## Usage
