using System;

using Zagejmi.Write.Domain.Abstractions;

namespace Zagejmi.Write.Domain.Profile.Events;

public sealed record UserProfileCreatedEvent : IDomainEvent
{
    public UserProfileCreatedEvent(
        Guid aggregateId,
        Guid userId,
        PersonType personType,
        string displayName,
        string? profileImageBlobKey)
    {
        this.AggregateId = aggregateId;
        this.UserId = userId;
        this.PersonType = personType;
        this.DisplayName = displayName;
        this.ProfileImageBlobKey = profileImageBlobKey;
        this.OccurredOn = DateTimeOffset.UtcNow;
    }

    public Guid UserId { get; }

    public PersonType PersonType { get; }

    public string DisplayName { get; }

    public string? ProfileImageBlobKey { get; }

    public Guid AggregateId { get; }

    public DateTimeOffset OccurredOn { get; }

    public int? AggregateVersion => 0;
}