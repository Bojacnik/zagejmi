namespace Zagejmi.Contracts.Failures;

public sealed record FailureOperationCancelled(string Message) : Failure(Message);
