using System;

using Zagejmi.Domain.Abstractions;

namespace Zagejmi.Domain.Profile.Events;

public sealed record UserProfileImageUpdatedEvent : IDomainEvent
{
    public UserProfileImageUpdatedEvent(
        Guid aggregateId,
        Guid userId,
        string profileImageBlobKey,
        string? profileImageContentType = null)
    {
        this.AggregateId = aggregateId;
        this.UserId = userId;
        this.ProfileImageBlobKey = profileImageBlobKey;
        this.ProfileImageContentType = profileImageContentType;
        this.OccurredOn = DateTimeOffset.UtcNow;
    }

    public Guid AggregateId { get; }

    public Guid UserId { get; }

    public string ProfileImageBlobKey { get; }

    public string? ProfileImageContentType { get; }

    public DateTimeOffset OccurredOn { get; }

    public int? AggregateVersion => null;
}
