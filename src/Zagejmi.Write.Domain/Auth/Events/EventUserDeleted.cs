namespace Zagejmi.Write.Domain.Auth.Events;

public sealed record EventUserDeleted : IDomainEvent<User, Guid>
{
    public EventUserDeleted(Guid aggregateId, Guid userId)
    {
        this.AggregateId = aggregateId;
        this.UserId = userId;
        this.Timestamp = DateTime.UtcNow;
        this.EventType = EventTypeDomain.UserDeleted;
    }

    public Guid AggregateId { get; }

    public Guid UserId { get; }

    public DateTime Timestamp { get; }

    public EventTypeDomain EventType { get; }
}