namespace Zagejmi.SharedKernel.Failures;

public sealed record FailureMapper(string Message) : Failure(Message);
