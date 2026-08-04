using System;

public class Failure
{
    public string Message { get; }
    public Failure(string message) => Message = message ?? throw new ArgumentNullException(nameof(message));
    public override string ToString() => Message;
}

public sealed class FailureArgumentInvalidValue : Failure { public FailureArgumentInvalidValue(string message) : base(message) {} }
public sealed class FailureDatabaseEntityNotFound : Failure { public FailureDatabaseEntityNotFound(string message) : base(message) {} }
public sealed class FailureRegister : Failure { public FailureRegister(string message) : base(message) {} }
public sealed class FailureEventStoreConnectionLost : Failure { public FailureEventStoreConnectionLost(string message) : base(message) {} }
public sealed class FailureEventStoreUnableToSave : Failure { public FailureEventStoreUnableToSave(string message) : base(message) {} }
