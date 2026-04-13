using System;

using Zagejmi.Write.Domain.Abstractions;

namespace Zagejmi.Write.Domain.Profile.Events;

public sealed record UserProfileImageUpdatedEvent : IDomainEvent
{
    public UserProfileImageUpdatedEvent(Guid aggregateId, Guid userId, string profileImageBlobKey)
    {
        this.AggregateId = aggregateId;
        this.UserId = userId;
        this.ProfileImageBlobKey = profileImageBlobKey;
        this.OccurredOn = DateTimeOffset.UtcNow;
    }

    public Guid UserId { get; }

    public string ProfileImageBlobKey { get; }

    public Guid AggregateId { get; }

    public DateTimeOffset OccurredOn { get; }

    public int? AggregateVersion => null;
}