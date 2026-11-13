using System;

namespace Zagejmi.Domain.Wallets;

/// <summary>
///     A non-negative, whole amount of Goins, the in-app currency. Goins are never split, so the amount is an integer.
/// </summary>
public readonly record struct Goin : IComparable<Goin>
{
    /// <summary>
    ///     No Goins.
    /// </summary>
    public static readonly Goin Zero = new(0);

    /// <summary>
    ///     Initializes a new instance of the <see cref="Goin" /> class.
    /// </summary>
    /// <param name="amount">The amount of Goins. Must not be negative.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the amount is negative.</exception>
    public Goin(long amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        this.Amount = amount;
    }

    /// <summary>
    ///     Gets the amount of Goins.
    /// </summary>
    public long Amount { get; }

    /// <summary>
    ///     Gets a value indicating whether this is zero Goins.
    /// </summary>
    public bool IsZero => this.Amount == 0;

    /// <summary>
    ///     Adds two amounts.
    /// </summary>
    /// <param name="left">The first amount.</param>
    /// <param name="right">The second amount.</param>
    /// <returns>The sum.</returns>
    public static Goin operator +(Goin left, Goin right)
    {
        return new Goin(checked(left.Amount + right.Amount));
    }

    /// <summary>
    ///     Subtracts two amounts.
    /// </summary>
    /// <param name="left">The amount to subtract from.</param>
    /// <param name="right">The amount to subtract.</param>
    /// <returns>The difference.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the result would be negative.</exception>
    public static Goin operator -(Goin left, Goin right)
    {
        return new Goin(left.Amount - right.Amount);
    }

    /// <summary>
    ///     Compares two amounts.
    /// </summary>
    /// <param name="left">The first amount.</param>
    /// <param name="right">The second amount.</param>
    /// <returns><see langword="true" /> when <paramref name="left" /> is smaller.</returns>
    public static bool operator <(Goin left, Goin right)
    {
        return left.Amount < right.Amount;
    }

    /// <summary>
    ///     Compares two amounts.
    /// </summary>
    /// <param name="left">The first amount.</param>
    /// <param name="right">The second amount.</param>
    /// <returns><see langword="true" /> when <paramref name="left" /> is larger.</returns>
    public static bool operator >(Goin left, Goin right)
    {
        return left.Amount > right.Amount;
    }

    /// <summary>
    ///     Compares two amounts.
    /// </summary>
    /// <param name="left">The first amount.</param>
    /// <param name="right">The second amount.</param>
    /// <returns><see langword="true" /> when <paramref name="left" /> is smaller or equal.</returns>
    public static bool operator <=(Goin left, Goin right)
    {
        return left.Amount <= right.Amount;
    }

    /// <summary>
    ///     Compares two amounts.
    /// </summary>
    /// <param name="left">The first amount.</param>
    /// <param name="right">The second amount.</param>
    /// <returns><see langword="true" /> when <paramref name="left" /> is larger or equal.</returns>
    public static bool operator >=(Goin left, Goin right)
    {
        return left.Amount >= right.Amount;
    }

    /// <summary>
    ///     Returns the smaller of two amounts.
    /// </summary>
    /// <param name="left">The first amount.</param>
    /// <param name="right">The second amount.</param>
    /// <returns>The smaller amount.</returns>
    public static Goin Min(Goin left, Goin right)
    {
        return left <= right ? left : right;
    }

    /// <inheritdoc />
    public int CompareTo(Goin other)
    {
        return this.Amount.CompareTo(other.Amount);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return this.Amount.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }
}
