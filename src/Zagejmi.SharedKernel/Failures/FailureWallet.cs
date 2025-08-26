namespace Zagejmi.SharedKernel.Failures;

public record FailureWallet(string Message) : Failure(Message);