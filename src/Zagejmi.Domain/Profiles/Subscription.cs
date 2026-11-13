using System;

namespace Zagejmi.Domain.Profiles;

/// <summary>
///     Represents a subscription relationship between two users, encapsulating the necessary information for managing
///     subscriptions, including the subscriber's unique identifier, the subscribed-to user's unique identifier, and the
///     start date of the subscription. The Subscription value object is essential for tracking and managing user
///     subscriptions, ensuring that the relationship between users is properly maintained and recorded.
/// </summary>
public record Subscription
{
    /// <summary>
    ///     Gets or sets the unique identifier of the subscriber, representing the user who is subscribing to another user's
    ///     subscription. This property is essential for identifying the subscriber within the subscription relationship and
    ///     ensuring that the subscription is properly associated with the correct user. The SubscriberId should be treated as
    ///     a unique value to prevent conflicts and ensure proper management of subscription relationships within the system.
    /// </summary>
    public Guid SubscriberId { get; set; }

    /// <summary>
    ///     Gets or sets the unique identifier of the subscribed-to user, representing the user who is being subscribed to by
    ///     the subscriber. This property is essential for identifying the subscribed-to user within the subscription
    ///     relationship and ensuring that the subscription is properly associated with the correct user. The SubscribedToId
    ///     should be treated as a unique value to prevent conflicts and ensure proper management of subscription relationships
    ///     within the system.
    /// </summary>
    public Guid SubscribedToId { get; set; }

    /// <summary>
    ///     Gets or sets the start date of the subscription, representing the date and time when the subscription relationship
    ///     was initiated between the subscriber and the subscribed-to user. This property is essential for tracking the
    ///     duration of the subscription and managing subscription-related features, such as notifications, content access, and
    ///     other subscription-based functionalities. The StartDate should be treated as a timestamp value to ensure accurate
    ///     tracking of subscription relationships and proper management of subscription-related features within the system.
    /// </summary>
    public DateTimeOffset StartDate { get; set; }
}
