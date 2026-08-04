using System;

using Zagejmi.Domain.Abstractions;

namespace Zagejmi.Domain.Profile.Events;

public sealed record UserProfileImageRemovedEvent : IDomainEvent
{
    public UserProfileImageRemovedEvent(Guid aggregateId, Guid userId)
    {
        this.AggregateId = aggregateId;
        this.UserId = userId;
        this.OccurredOn = DateTimeOffset.UtcNow;
    }

    public Guid AggregateId { get; }

    public Guid UserId { get; }

    public DateTimeOffset OccurredOn { get; }

    public int? AggregateVersion => null;
}
