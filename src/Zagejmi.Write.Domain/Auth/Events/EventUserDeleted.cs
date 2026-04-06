using System;

using Zagejmi.Write.Domain.Abstractions;

namespace Zagejmi.Write.Domain.Auth.Events;

public sealed record EventUserDeleted : IDomainEvent
{
    public EventUserDeleted(Guid aggregateId, Guid userId)
    {
        this.AggregateId = aggregateId;
        this.UserId = userId;
        this.Timestamp = DateTime.UtcNow;
    }

    public Guid UserId { get; }

    public DateTime Timestamp { get; }

    public Guid AggregateId { get; }
}