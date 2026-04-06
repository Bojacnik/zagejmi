namespace Zagejmi.Shared.Failures;

public record FailureWallet(string message) : Failure(message);