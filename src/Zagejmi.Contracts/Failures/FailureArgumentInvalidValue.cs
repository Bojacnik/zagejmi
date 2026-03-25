namespace Zagejmi.Contracts.Failures;

public sealed record FailureArgumentInvalidValue(string Message) : FailureArgument(Message);
