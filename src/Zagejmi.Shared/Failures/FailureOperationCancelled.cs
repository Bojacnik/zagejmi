namespace Zagejmi.Shared.Failures;

public sealed record FailureOperationCancelled(string message) : Failure(message);
