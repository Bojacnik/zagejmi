using Zagejmi.Server.Write.Domain.Community.Goin;

namespace Zagejmi.Server.Domain.Events.Goin;

public sealed class GoinTransactionCreated : IDomainEvent<GoinTransaction, Guid>
{
    public Guid TransactionId { get; }
    public GoinWallet Sender { get; }
    public GoinWallet Receiver { get; }
    public Zagejmi.Server.Write.Domain.Community.Goin.Goin Goin { get; }
    public DateTime Timestamp { get; }
    public EventTypeDomain EventType { get; }

    public GoinTransactionCreated(
        Guid transactionId,
        GoinWallet sender,
        GoinWallet receiver,
        Zagejmi.Server.Write.Domain.Community.Goin.Goin goin)
    {
        TransactionId = transactionId;
        Sender = sender;
        Receiver = receiver;
        Goin = goin;
        Timestamp = DateTime.UtcNow;
        EventType = EventTypeDomain.TransactionCreated;
    }

    public GoinTransaction Apply(GoinTransaction aggregate)
    {
        // Creation event doesn't modify the aggregate state here
        return aggregate;
    }
}