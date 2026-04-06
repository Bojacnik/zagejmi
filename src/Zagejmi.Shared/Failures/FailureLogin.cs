namespace Zagejmi.Shared.Failures;

public record FailureLogin(string message) : Failure(message);