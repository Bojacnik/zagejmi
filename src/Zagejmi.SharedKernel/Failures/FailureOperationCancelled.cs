namespace Zagejmi.SharedKernel.Failures;

public sealed record FailureOperationCancelled(string Message) : Failure(Message);
