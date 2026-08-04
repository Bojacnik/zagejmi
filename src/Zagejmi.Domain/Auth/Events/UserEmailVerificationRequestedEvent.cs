using System;

using Zagejmi.Domain.Abstractions;

namespace Zagejmi.Domain.Auth.Events;

public sealed record UserEmailVerificationRequestedEvent : IDomainEvent
{
    public UserEmailVerificationRequestedEvent(Guid aggregateId, string tokenHash, DateTimeOffset expiresAtUtc)
    {
        this.AggregateId = aggregateId;
        this.TokenHash = tokenHash;
        this.ExpiresAtUtc = expiresAtUtc;
        this.OccurredOn = DateTimeOffset.UtcNow;
    }

    public string TokenHash { get; }

    public DateTimeOffset ExpiresAtUtc { get; }

    public Guid AggregateId { get; }

    public DateTimeOffset OccurredOn { get; }

    public int? AggregateVersion => null;
}