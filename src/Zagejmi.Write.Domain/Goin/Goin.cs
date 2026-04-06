using System;
using System.Collections.Generic;

using Zagejmi.Write.Domain.Abstractions;

namespace Zagejmi.Write.Domain.Goin;

/// <summary>
///     Represents a certain amount of Goin, the in-app currency used for transactions and rewards within the Zagejmi
///     ecosystem.
/// </summary>
public sealed record Goin : ValueObject
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Goin" /> class with the specified amount.
    /// </summary>
    /// <param name="amount">The amount of Goin. Must be a non-negative decimal value.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the provided amount is negative.</exception>
    public Goin(decimal amount)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount of Goin cannot be negative.");
        }

        this.Amount = amount;
    }

    /// <summary>
    ///     Gets the amount of Goin. This value is immutable and must be a non-negative decimal.
    /// </summary>
    public decimal Amount { get; init; }

    /// <summary>
    ///     Returns an enumerable of the atomic values that define the equality of this value object. In this case, it
    ///     includes only the <see cref="Amount" /> property, as it is the sole defining characteristic of the Goin value
    ///     object. This method is used by the base ValueObject class to implement equality and hashing based on the values of
    ///     the properties that define the value object. By yielding the Amount property, we ensure that two Goin instances
    ///     with the same amount are considered equal, while instances with different amounts are not. This method is essential
    ///     for maintaining the integrity of value objects and ensuring that they behave correctly in collections and
    ///     comparisons based on their defining properties.
    /// </summary>
    /// <returns>An enumerable of the atomic values that define the equality of this value object.</returns>
    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return this.Amount;
    }
}