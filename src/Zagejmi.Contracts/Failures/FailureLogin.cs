namespace Zagejmi.Contracts.Failures;

public record FailureLogin(string Message) : Failure(Message);