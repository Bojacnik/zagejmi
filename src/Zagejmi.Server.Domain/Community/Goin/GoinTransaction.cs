using Zagejmi.Server.Domain.Entity;
using Zagejmi.Server.Domain.Events;

namespace Zagejmi.Server.Write.Domain.Community.Goin;

public sealed class GoinTransaction : AggregateRoot<GoinTransaction, Guid>
{
    public GoinWallet Sender { get; private set; }
    public GoinWallet Receiver { get; private set; }
    public Goin Goin { get; private set; }

    public GoinTransaction(
        Guid id,
        GoinWallet sender,
        GoinWallet receiver,
        Goin goin
    ) : base(id)
    {
        Sender = sender;
        Receiver = receiver;
        Goin = goin;
    }

    // Private constructor for persistence frameworks
    private GoinTransaction(Guid id) : base(id) { }

    protected override void Apply(IDomainEvent<GoinTransaction, Guid> evt)
    {
        evt.Apply(this);
    }
}
