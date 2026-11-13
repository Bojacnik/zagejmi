using System;

namespace Zagejmi.Infrastructure.Profiles.Records;

/// <summary>
///     Represents a subscription record between a subscriber and a subscribed player.
/// </summary>
public record SubscriptionRecord
{
    /// <summary>
    ///     Gets the unique identifier for the subscription record.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    ///     Gets the subscriber's customer profile record.
    /// </summary>
    public required CustomerProfileRecord Subscriber { get; init; }

    /// <summary>
    ///     Gets the subscribed player's profile record.
    /// </summary>
    public required PlayerProfileRecord Subscribed { get; init; }

    /// <summary>
    ///     Gets the start date of the subscription.
    /// </summary>
    public DateTimeOffset StartDate { get; init; }
}
