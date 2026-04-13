namespace Zagejmi.Shared.Failures;

public record FailureWallet(string Message) : Failure(Message);