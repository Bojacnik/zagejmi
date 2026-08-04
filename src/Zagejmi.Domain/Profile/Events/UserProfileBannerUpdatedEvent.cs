using System;

using Zagejmi.Domain.Abstractions;

namespace Zagejmi.Domain.Profile.Events;

public sealed record UserProfileBannerUpdatedEvent : IDomainEvent
{
    public UserProfileBannerUpdatedEvent(
        Guid aggregateId,
        Guid userId,
        string bannerImageBlobKey,
        string? bannerImageContentType = null)
    {
        this.AggregateId = aggregateId;
        this.UserId = userId;
        this.BannerImageBlobKey = bannerImageBlobKey;
        this.BannerImageContentType = bannerImageContentType;
        this.OccurredOn = DateTimeOffset.UtcNow;
    }

    public Guid AggregateId { get; }

    public Guid UserId { get; }

    public string BannerImageBlobKey { get; }

    public string? BannerImageContentType { get; }

    public DateTimeOffset OccurredOn { get; }

    public int? AggregateVersion => null;
}
