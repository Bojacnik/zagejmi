using System;

namespace Zagejmi.Domain.Goin;

public sealed class GoinTransaction
{
    public GoinTransaction(Guid id, Guid sender, Guid receiver, Goin goin)
    {
        this.Sender = sender;
        this.Receiver = receiver;
        this.Goin = goin;
    }

    public Guid Sender { get; private init; }

    public Guid Receiver { get; private init; }

    public Goin Goin { get; private init; }
}