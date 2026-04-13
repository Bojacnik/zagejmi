namespace Zagejmi.Shared.Failures;

public record FailureRegister(string Message) : Failure(Message);