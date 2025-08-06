namespace Zagejmi.SharedKernel.Failures;

public record FailureRegister(string Message) : Failure(Message);