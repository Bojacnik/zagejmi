namespace Zagejmi.Shared.Failures;

public sealed record FailureOperationCancelled(string Message) : Failure(Message);