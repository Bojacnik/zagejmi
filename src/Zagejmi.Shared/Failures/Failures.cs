namespace Zagejmi.Shared.Failures;

public abstract record Failure(string message);


public abstract record VerificationFailure(string message) : Failure(message);

public record VerificationFailureInvalidLogin(string message) : VerificationFailure(message);

public abstract record FailureFile(string message) : Failure(message);

public record FailureFileNotFound(string message) : FailureFile(message);

public record FailureFileNotImage(string message) : FailureFile(message);

public record FailureFileInvalidImage(string message) : FailureFile(message);

public record FailureFileNotAuthorized(string message) : FailureFile(message);

public record FailureFileOther(string message) : FailureFile(message);

public abstract record FailureArgument(string message) : Failure(message);

public record FailureArgumentInvalidType(string message) : FailureArgument(message);

public record FailureArgumentInvalidLength(string message) : FailureArgument(message);

public abstract record FailureEventStore(string message) : Failure(message);

public record FailureEventStoreConnectionLost(string message) : FailureEventStore(message);

public record FailureEventStoreUnableToSave(string message) : FailureEventStore(message);