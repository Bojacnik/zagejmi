using System;

using Zagejmi.Domain.Abstractions;

namespace Zagejmi.Domain.Auth.Events;

public sealed record UserEmailVerifiedEvent : IDomainEvent
{
    public UserEmailVerifiedEvent(Guid aggregateId)
    {
        this.AggregateId = aggregateId;
        this.OccurredOn = DateTimeOffset.UtcNow;
    }

    public Guid AggregateId { get; }

    public DateTimeOffset OccurredOn { get; }

    public int? AggregateVersion => null;
}