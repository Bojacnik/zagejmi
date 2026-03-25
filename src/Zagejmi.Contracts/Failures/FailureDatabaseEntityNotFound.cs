namespace Zagejmi.Contracts.Failures;

public sealed record FailureDatabaseEntityNotFound(string Message) : Failure(Message);
