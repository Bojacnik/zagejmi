namespace Zagejmi.Shared.Failures;

public sealed record FailureArgumentInvalidValue(string Message) : FailureArgument(Message);