using System;

using Zagejmi.Domain.Abstractions;

namespace Zagejmi.Domain.Profile.Events;

public sealed record UserProfileCreatedEvent : IDomainEvent
{
    public UserProfileCreatedEvent(
        Guid aggregateId,
        Guid userId,
        string displayName,
        string personType,
        string? profileImageBlobKey,
        string? profileImageContentType = null)
    {
        this.AggregateId = aggregateId;
        this.UserId = userId;
        this.DisplayName = displayName;
        this.PersonType = personType;
        this.ProfileImageBlobKey = profileImageBlobKey;
        this.ProfileImageContentType = profileImageContentType;
        this.OccurredOn = DateTimeOffset.UtcNow;
    }

    public Guid UserId { get; }

    public string DisplayName { get; }

    public string PersonType { get; }

    public string? ProfileImageBlobKey { get; }

    public string? ProfileImageContentType { get; }

    public Guid AggregateId { get; }

    public DateTimeOffset OccurredOn { get; }

    public int? AggregateVersion => 0;
}