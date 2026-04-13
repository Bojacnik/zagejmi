namespace Zagejmi.Shared.Failures;

public record FailureLogin(string Message) : Failure(Message);