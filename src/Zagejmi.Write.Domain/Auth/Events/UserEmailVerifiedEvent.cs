using System;

using Zagejmi.Write.Domain.Abstractions;

namespace Zagejmi.Write.Domain.Auth.Events;

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