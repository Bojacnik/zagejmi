namespace Zagejmi.SharedKernel.Failures;

public record FailureLogin(string Message) : Failure(Message);