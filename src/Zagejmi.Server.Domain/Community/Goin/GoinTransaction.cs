using System.Reflection.Metadata.Ecma335;
using Zagejmi.Server.Domain.Entity;
using Zagejmi.Server.Domain.Events;
using Zagejmi.Server.Domain.Events.Auth;
using Zagejmi.Server.Write.Domain.Community.Goin;

namespace Zagejmi.Server.Domain.Community.Goin;

public sealed class GoinTransaction : Aggregate<GoinTransaction, Guid>
{
    public GoinWallet Sender { get; private set; }
    public GoinWallet Receiver { get; private set; }
    public Write.Domain.Community.Goin.Goin Goin { get; private set; }

    public GoinTransaction(
        Guid id,
        GoinWallet sender,
        GoinWallet receiver,
        Write.Domain.Community.Goin.Goin goin
    ) : base(id)
    {
        Sender = sender;
        Receiver = receiver;
        Goin = goin;
        EventGoinTransactionCreated evt = new EventGoinTransactionCreated(id, sender, receiver, goin);
    }

    protected override void Apply(IDomainEvent<GoinTransaction, Guid> evt)
    {
    }
}
