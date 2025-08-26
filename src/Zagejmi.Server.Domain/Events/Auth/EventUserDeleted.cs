using Zagejmi.Server.Domain.Auth;

namespace Zagejmi.Server.Domain.Events.Auth;

public sealed record EventUserDeleted : IDomainEvent<User, Guid>
{
    public Guid AggregateId { get; }
    public Guid UserId { get; }
    public DateTime Timestamp { get; }
    public EventTypeDomain EventType { get; }

    public EventUserDeleted(Guid aggregateId, Guid userId)
    {
        AggregateId = aggregateId;
        UserId = userId;
        Timestamp = DateTime.UtcNow;
        EventType = EventTypeDomain.UserDeleted;
    }

}