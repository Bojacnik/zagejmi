namespace Zagejmi.Write.Domain.Goin;

public sealed class GoinTransaction : Aggregate<GoinTransaction, Guid>
{
    public GoinTransaction(
        Guid id,
        GoinWallet sender,
        GoinWallet receiver,
        Goin goin) : base(id)
    {
        this.Sender = sender;
        this.Receiver = receiver;
        this.Goin = goin;
        EventGoinTransactionCreated evt = new EventGoinTransactionCreated(id, sender, receiver, goin);
    }

    public GoinWallet Sender { get; private set; }

    public GoinWallet Receiver { get; private set; }

    public Goin Goin { get; private set; }

    protected override void Apply(IDomainEvent<GoinTransaction, Guid> @event)
    {
    }
}