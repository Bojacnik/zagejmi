using System;

public class Failure
{
    public string Message { get; }
    public Failure(string message) => Message = message ?? throw new ArgumentNullException(nameof(message));
    public override string ToString() => Message;
}
