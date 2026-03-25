namespace Zagejmi.Contracts.Failures;

public record FailureRegister(string Message) : Failure(Message);