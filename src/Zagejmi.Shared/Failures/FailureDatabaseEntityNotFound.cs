namespace Zagejmi.Shared.Failures;

public sealed record FailureDatabaseEntityNotFound(string Message) : Failure(Message);