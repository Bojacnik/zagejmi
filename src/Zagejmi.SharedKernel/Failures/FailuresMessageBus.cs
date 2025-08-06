namespace Zagejmi.SharedKernel.Failures;

public abstract record FailureMessageBus(string Message) : Failure(Message);

public record FailureMessageBusUnavailable(string Message) : FailureMessageBus(Message);

public record FailureMessageBusOperationCancelled(string Message) : FailureMessageBus(Message);