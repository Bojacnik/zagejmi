using System.Collections.Generic;

namespace Zagejmi.Write.Domain.Abstractions;

public abstract record ValueObject
{
    /// <summary>
    /// List of atomic values used for equality and hashing.
    /// Each derived class must implement this to return the values that define its identity.
    /// </summary>
    /// <returns>>An enumerable of atomic values that represent the state of the value object.</returns>
    protected abstract IEnumerable<object?> GetAtomicValues();
}