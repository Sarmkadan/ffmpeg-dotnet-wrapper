# Companion Extensions

This document documents extension classes that complement the main types.

## CliCommandExtensions

See [CliCommand](./CliCommand.md) for the base class.

| Method | Signature | Example |
|--------|-----------|---------|
| HasOption | `public static bool HasOption(this CliCommand command, string optionName)` | `var hasOption = command.HasOption("output");` |
| GetOptionValue | `public static string? GetOptionValue(this CliCommand command, string optionName)` | `var value = `var value = command.GetOptionValue("output");` |
| TryGetOptionValue | `public static bool TryGetOptionValue(this CliCommand command, string optionName, [NotNullWhen(true)] out string? value)` | `if (command.TryGetOptionValue("output", out var value)) { ... }` |

## WebhookEndpointExtensions

See [WebhookEndpoint](./WebhookEndpoint.md) for the base class.

| Method | Signature | Example |
|--------|-----------|---------|
| ValidateConfiguration | `public static void ValidateConfiguration(this WebhookEndpoint endpoint)` | `endpoint.ValidateConfiguration();` |
| MergeHeaders | `public static void MergeHeaders(this WebhookEndpoint endpoint, IDictionary<string, string?> newHeaders)` | `endpoint.MergeHeaders(new Dictionary<string, string?> { { "Authorization", "Bearer token" } });` |
| IsExpired | `public static bool IsExpired(this WebhookEndpoint endpoint, TimeSpan expirationPeriod)` | `var isExpired = endpoint.IsExpired(TimeSpan.FromDays(30));` |
| CanHandleEvent | `public static bool CanHandleEvent(this WebhookEndpoint endpoint, string eventType)` | `var canHandle = endpoint.CanHandleEvent("video.completed");` |

## BatchOperationServiceExtensions

See [BatchOperationService](./BatchOperationService.md) for the base class.

| Method | Signature | Example |
|--------|-----------|---------|
| GetSuccessfulConversions | `public static List<ConversionResult> GetSuccessfulConversions(this BatchOperationService service, BatchOperationResult result)` | `var successful = service.GetSuccessfulConversions(result);` |
| GetFailedConversions | `public static List<ConversionResult> GetFailedConversions(this BatchOperationService service, BatchOperationResult result)` | `var failed = service.GetFailedConversions(result);` |
| GetTotalDuration | `public static TimeSpan GetTotalDuration(this BatchOperationService service, BatchOperationResult result)` | `var totalDuration = service.GetTotalDuration(result);` |
| GetAverageDuration | `public static TimeSpan GetAverageDuration(this BatchOperationService service, BatchOperationResult result)` | `var avgDuration = service.GetAverageDuration(result);` |
| CreateSummaryReport | `public static string CreateSummaryReport(this BatchOperationService service, BatchOperationResult result)` | `var report = service.CreateSummaryReport(result);` |
| GetLargestFileSize | `public static long GetLargestFileSize(this BatchOperationService service, BatchOperationResult result)` | `var largest = service.GetLargestFileSize(result);` |
| GetSmallestFileSize | `public static long GetSmallestFileSize(this BatchOperationService service, BatchOperationResult result)` | `var smallest = service.GetSmallestFileSize(result);` |
| GetAverageFileSize | `public static long GetAverageFileSize(this BatchOperationService service, BatchOperationResult result)` | `var avgSize = service.GetAverageFileSize(result);` |
| GetCompletionPercentage | `public static double GetCompletionPercentage(this BatchOperationService service, BatchOperationResult result)` | `var percentage = service.GetCompletionPercentage(result);` |
| AllSuccessful | `public static bool AllSuccessful(this BatchOperationService service, BatchOperationResult result)` | `var allSuccess = service.AllSuccessful(result);` |
| AnyFailed | `public static bool AnyFailed(this BatchOperationService service, BatchOperationResult result)` | `var anyFailed = service.AnyFailed(result);` |