namespace Zagejmi.Shared.Failures;

public sealed record FailureMapper(string Message) : Failure(Message);