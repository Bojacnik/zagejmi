public sealed class FailureArgumentInvalidValue : Failure { public FailureArgumentInvalidValue(string message) : base(message) {} }
public sealed class FailureDatabaseEntityNotFound : Failure { public FailureDatabaseEntityNotFound(string message) : base(message) {} }
public sealed class FailureRegister : Failure { public FailureRegister(string message) : base(message) {} }
public sealed class FailureEventStoreConnectionLost : Failure { public FailureEventStoreConnectionLost(string message) : base(message) {} }
public sealed class FailureEventStoreUnableToSave : Failure { public FailureEventStoreUnableToSave(string message) : base(message) {} }
