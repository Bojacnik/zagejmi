namespace Zagejmi.Shared.Failures;

public sealed record FailureDatabaseEntityNotFound(string message) : Failure(message);
