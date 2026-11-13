using System;

namespace Zagejmi.Domain.Wallets;

/// <summary>
///     A transfer of goins from one wallet owner to another.
/// </summary>
public sealed class GoinTransaction
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="GoinTransaction" /> class.
    /// </summary>
    /// <param name="id">The transaction identifier.</param>
    /// <param name="sender">The identifier of the sending user.</param>
    /// <param name="receiver">The identifier of the receiving user.</param>
    /// <param name="goin">The amount transferred.</param>
    public GoinTransaction(Guid id, Guid sender, Guid receiver, Goin goin)
    {
        this.Id = id;
        this.Sender = sender;
        this.Receiver = receiver;
        this.Goin = goin;
    }

    /// <summary>
    ///     Gets the transaction identifier.
    /// </summary>
    public Guid Id { get; private init; }

    /// <summary>
    ///     Gets the identifier of the sending user.
    /// </summary>
    public Guid Sender { get; private init; }

    /// <summary>
    ///     Gets the identifier of the receiving user.
    /// </summary>
    public Guid Receiver { get; private init; }

    /// <summary>
    ///     Gets the amount transferred.
    /// </summary>
    public Goin Goin { get; private init; }
}
