namespace Zagejmi.Shared.Failures;

public sealed record FailureArgumentInvalidValue(string message) : FailureArgument(message);
