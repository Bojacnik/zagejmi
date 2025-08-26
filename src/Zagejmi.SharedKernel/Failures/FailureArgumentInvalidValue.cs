namespace Zagejmi.SharedKernel.Failures;

public sealed record FailureArgumentInvalidValue(string Message) : FailureArgument(Message);
