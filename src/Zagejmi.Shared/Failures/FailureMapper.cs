namespace Zagejmi.Shared.Failures;

public sealed record FailureMapper(string message) : Failure(message);
