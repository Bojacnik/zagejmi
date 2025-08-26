using Zagejmi.Server.Domain.Community.Goin;
using Zagejmi.Server.Write.Domain.Community.Goin;

namespace Zagejmi.Server.Domain.Events.Auth;

public sealed record EventGoinTransactionCreated(
    Guid TransactionId,
    GoinWallet Sender,
    GoinWallet Receiver,
    Write.Domain.Community.Goin.Goin Goin
) : IDomainEvent<GoinTransaction, Guid>
{
    public Guid AggregateId => TransactionId;
    public DateTime Timestamp { get; } = DateTime.UtcNow;
    public EventTypeDomain EventType { get; } = EventTypeDomain.GoinTransactionCreated;
}