using System;

namespace Zagejmi.Domain.Wallets;

/// <summary>
///     Represents a certain amount of Goin, the in-app currency used for transactions and rewards within the Zagejmi
///     ecosystem.
/// </summary>
public sealed record Goin
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
}