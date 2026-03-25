namespace Zagejmi.Contracts.Failures;

public record FailureWallet(string Message) : Failure(Message);