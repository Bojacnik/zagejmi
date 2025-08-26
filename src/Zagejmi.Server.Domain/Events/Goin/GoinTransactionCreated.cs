using Zagejmi.Server.Domain.Community.Goin;

namespace Zagejmi.Server.Domain.Events.Goin;

public sealed class GoinTransactionCreated : IDomainEvent<GoinTransaction, Guid>
{
    public Guid AggregateId { get; }
    public Guid TransactionId { get; }
    public GoinWallet Sender { get; }
    public GoinWallet Receiver { get; }
    public Zagejmi.Server.Write.Domain.Community.Goin.Goin Goin { get; }
    public DateTime Timestamp { get; }
    public EventTypeDomain EventType { get; }

    public GoinTransactionCreated(
        Guid aggregateId,
        Guid transactionId,
        GoinWallet sender,
        GoinWallet receiver,
        Zagejmi.Server.Write.Domain.Community.Goin.Goin goin)
    {
        TransactionId = transactionId;
        Sender = sender;
        Receiver = receiver;
        Goin = goin;
        AggregateId = aggregateId;
        Timestamp = DateTime.UtcNow;
    }
}