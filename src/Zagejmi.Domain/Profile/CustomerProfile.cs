using System;
using System.Collections.Generic;

using SixLabors.ImageSharp;

using Zagejmi.Domain.Abstractions;
using Zagejmi.Domain.Goin;

namespace Zagejmi.Domain.Profile;

/// <summary>
///     Represents the profile of a customer, containing information such as the profile picture and subscriptions
///     associated
///     with the customer. This class extends the abstract Profile class and provides specific properties and behaviors
///     relevant to customer profiles within the Zagejmi application. It encapsulates the state and behavior of customer
///     profiles, ensuring that all operations and changes to the profile are consistent with the business rules and domain
///     logic of the application. The CustomerProfile aggregate handles events related to profile creation, updates, and
///     subscriptions, maintaining the integrity of the profile's state and ensuring that all changes are properly recorded
///     and applied through domain events. It serves as the central point of interaction for any operations related to
///     customer profiles, including the management of personal information, profile pictures, and subscriptions, while
///     adhering to the principles of domain-driven design and ensuring that all business rules and invariants are
///     maintained throughout the lifecycle of the profile.
/// </summary>
public class CustomerProfile : Abstractions.Profile
{
    internal CustomerProfile(Guid id)
        : base(id)
    {
    }

    /// <summary>
    ///     Gets or sets the profile picture associated with the customer, represented as an <see cref="Image" />
    ///     object. This property holds the visual representation of the customer's profile picture, which can be used for
    ///     display purposes in the application. The image can be in various formats (e.g., JPEG, PNG) and is typically stored
    ///     as a binary object in the database. The <see cref="ProfilePicture" /> property allows the application to retrieve
    ///     and display the customer's profile picture when needed, such as in user interfaces or profile views.
    /// </summary>
    public Image? ProfilePicture { get; set; }

    /// <summary>
    ///     Gets or sets the collection of subscriptions associated with the customer profile. Each subscription represents a
    ///     specific service or product that the customer has subscribed to within the Zagejmi application. The
    ///     <see cref="Subscriptions" /> property allows the application to manage and track the customer's subscriptions,
    ///     including their start dates and any relevant details. This collection is initialized as an empty array to ensure
    ///     that it is always in a valid state, even if the customer has no subscriptions at the time of profile creation. The
    ///     subscriptions can be added, removed, or updated as needed, and the aggregate root ensures that all operations on
    ///     the subscriptions are consistent with the business rules and domain logic of the application.
    /// </summary>
    public Subscription[] Subscriptions { get; set; } = Array.Empty<Subscription>();

    /// <summary>
    ///     Applies the specified domain event to the customer profile aggregate. This method is responsible for handling
    ///     the state changes and updates to the aggregate based on the event data. It ensures that all business rules and
    ///     invariants are maintained during the application of the event. The method is called internally by the aggregate
    ///     root when a domain event is raised, allowing the aggregate to react to changes in its state and maintain
    ///     consistency with the domain model. The implementation of this method should include the necessary logic to update
    ///     the properties of the customer profile based on the specific type of domain event being applied.
    /// </summary>
    /// <param name="domainEvent">
    ///     The domain event to be applied to the customer profile aggregate. This parameter contains the
    ///     data and context necessary for updating the state of the aggregate based on the event's information.
    /// </param>
    protected override void Apply(IDomainEvent domainEvent)
    {
        throw new NotImplementedException();
    }

    public override Abstractions.Profile Create(Guid userId, string displayName, GoinWallet walletId)
    {
        throw new NotImplementedException();
    }

    public override Abstractions.Profile? FromHistory(Guid id, IReadOnlyList<IDomainEvent> history)
    {
        throw new NotImplementedException();
    }
}