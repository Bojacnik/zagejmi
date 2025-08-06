namespace Zagejmi.SharedKernel.Failures;

public abstract record Failure(string Message);

public record FailureOperationCancelled(string Message) : Failure(Message);

public abstract record VerificationFailure(string Message) : Failure(Message);

public record VerificationFailureInvalidLogin(string Message) : VerificationFailure(Message);

public abstract record FailureFile(string Message) : Failure(Message);

public record FailureFileNotFound(string Message) : FailureFile(Message);

public record FailureFileNotImage(string Message) : FailureFile(Message);

public record FailureFileInvalidImage(string Message) : FailureFile(Message);

public record FailureFileNotAuthorized(string Message) : FailureFile(Message);

public record FailureFileOther(string Message) : FailureFile(Message);

public abstract record FailureArgument(string Message) : Failure(Message);

public record FailureArgumentInvalidValue(string Message) : FailureArgument(Message);

public record FailureArgumentInvalidType(string Message) : FailureArgument(Message);

public record FailureArgumentInvalidLength(string Message) : FailureArgument(Message);

public abstract record FailureEventStore(string Message) : Failure(Message);

public record FailureEventStoreConnectionLost(string Message) : FailureEventStore(Message);

public record FailureEventStoreUnableToSave(string Message) : FailureEventStore(Message);