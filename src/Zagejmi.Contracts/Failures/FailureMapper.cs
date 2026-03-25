namespace Zagejmi.Contracts.Failures;

public sealed record FailureMapper(string Message) : Failure(Message);
