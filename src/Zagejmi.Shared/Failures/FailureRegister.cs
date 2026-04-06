namespace Zagejmi.Shared.Failures;

public record FailureRegister(string message) : Failure(message);