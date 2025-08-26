namespace Zagejmi.SharedKernel.Failures;

public sealed record FailureDatabaseEntityNotFound(string Message) : Failure(Message);
