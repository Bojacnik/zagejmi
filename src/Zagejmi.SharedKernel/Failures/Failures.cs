namespace SharedKernel.Failures;

public abstract class Failure
{
    public string Message { get; init; }

    protected Failure(string message)
    {
        Message = message;
    }
}

public class FailureOperationCancelled(string message) : Failure(message);

public abstract class VerificationFailure : Failure
{
    protected VerificationFailure(string message) : base(message)
    {
    }
}

public class VerificationFailureInvalidLogin : VerificationFailure
{
    protected VerificationFailureInvalidLogin(string message) : base(message)
    {
    }
}

public abstract class FailureFile(string message) : Failure(message);

public  class FailureFileNotFound : FailureFile
{
    public FailureFileNotFound(string message) : base(message)
    {
    }
}

public class FailureFileNotImage : FailureFile
{
    public FailureFileNotImage(string message) : base(message)
    {
    }
}

public class FailureFileInvalidImage : FailureFile
{
    public FailureFileInvalidImage(string message) : base(message)
    {
    }
}

public class FailureFileNotAuthorized : FailureFile
{
    public FailureFileNotAuthorized(string message) : base(message)
    {
    }
}

public class FailureFileOther : FailureFile
{
    public FailureFileOther(string message) : base(message)
    {
    }
}

public abstract class FailureArgument : Failure
{
    public FailureArgument(string message) : base(message)
    {
    }
}

public class FailureArgumentInvalidValue : FailureArgument
{
    public FailureArgumentInvalidValue(string message) : base(message)
    {
    }
}

public class FailureArgumentInvalidType : FailureArgument
{
    public FailureArgumentInvalidType(string message) : base(message)
    {
    }
}

public class FailureArgumentInvalidLength : FailureArgument
{
    public FailureArgumentInvalidLength(string message) : base(message)
    {
    }
}

public abstract class FailureEventStore : Failure
{
    protected FailureEventStore(string message) : base(message)
    {
    }
}

public class FailureEventStoreConnectionLost : FailureEventStore
{
    public FailureEventStoreConnectionLost(string message) : base(message)
    {
    }
}

public class FailureEventStoreUnableToSave : FailureEventStore
{
    public FailureEventStoreUnableToSave(string message) : base(message)
    {
    }
}

