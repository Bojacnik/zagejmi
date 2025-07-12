namespace SharedKernel.Failures;

public abstract class FailureMessageBus : Failure
{
    protected FailureMessageBus(string message) : base(message)
    {
    }
}

public class FailureMessageBusUnavailable : FailureMessageBus
{
    public FailureMessageBusUnavailable(string message) : base(message)
    {
    }
}


public class FailureMessageBusOperationCancelled : FailureMessageBus
{
    public FailureMessageBusOperationCancelled(string message) : base(message)
    {
    }
}

