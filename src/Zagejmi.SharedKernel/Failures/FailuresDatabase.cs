namespace SharedKernel.Failures;

public class FailureDatabase(string message) : Failure(message);

public class FailureDatabaseEntityNotFound(string message) : FailureDatabase(message);

public class FailureDatabaseConnection(string message) : FailureDatabase(message);

public class FailureDatabaseQuery(string message) : FailureDatabase(message);

public class FailureDatabaseWrite(string message) : FailureDatabase(message);