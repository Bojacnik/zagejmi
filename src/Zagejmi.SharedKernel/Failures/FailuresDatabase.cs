namespace Zagejmi.SharedKernel.Failures;

public record FailureDatabase(string Message) : Failure(Message);

public record FailureDatabaseEntityNotFound(string Message) : FailureDatabase(Message);

public record FailureDatabaseConnection(string Message) : FailureDatabase(Message);

public record FailureDatabaseQuery(string Message) : FailureDatabase(Message);

public record FailureDatabaseWrite(string Message) : FailureDatabase(Message);