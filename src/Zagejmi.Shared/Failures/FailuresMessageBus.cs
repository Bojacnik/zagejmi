namespace Zagejmi.Shared.Failures;

public abstract record FailureMessageBus(string message) : Failure(message);

public record FailureMessageBusUnavailable(string message) : FailureMessageBus(message);

public record FailureMessageBusOperationCancelled(string message) : FailureMessageBus(message);