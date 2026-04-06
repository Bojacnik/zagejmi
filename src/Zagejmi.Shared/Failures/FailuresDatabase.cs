namespace Zagejmi.Shared.Failures;

public record FailureDatabase(string message) : Failure(message);


public record FailureDatabaseConnection(string message) : FailureDatabase(message);

public record FailureDatabaseQuery(string message) : FailureDatabase(message);

public record FailureDatabaseWrite(string message) : FailureDatabase(message);