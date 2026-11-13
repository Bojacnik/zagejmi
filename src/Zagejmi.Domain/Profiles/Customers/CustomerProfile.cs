using System;
using System.Collections.Generic;

using Zagejmi.Domain.Profiles.Abstractions;

namespace Zagejmi.Domain.Profiles.Customers;

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
public class CustomerProfile : Profile
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CustomerProfile" /> class.
    /// </summary>
    /// <param name="id">The profile identifier.</param>
    /// <param name="userId">The identifier of the owning user.</param>
    /// <param name="displayName">The display name for the customer profile.</param>
    public CustomerProfile(Guid id, Guid userId, string displayName)
        : base(id, userId, displayName)
    {
    }

    /// <summary>
    ///     Gets or sets the profile picture associated with the customer, referenced by the id of its stored image. This property holds the visual representation of the customer's profile picture, which can be used for
    ///     display purposes in the application. The image can be in various formats (e.g., JPEG, PNG) and is typically stored
    ///     as a binary object in the database. The <see cref="ProfilePicture" /> property allows the application to retrieve
    ///     and display the customer's profile picture when needed, such as in user interfaces or profile views.
    /// </summary>
    public Guid? ProfilePicture { get; set; }

    /// <summary>
    ///     Gets or sets the collection of subscriptions associated with the customer profile. Each subscription represents a
    ///     specific service or product that the customer has subscribed to within the Zagejmi application. The
    ///     <see cref="Subscriptions" /> property allows the application to manage and track the customer's subscriptions,
    ///     including their start dates and any relevant details. This collection is initialized as an empty array to ensure
    ///     that it is always in a valid state, even if the customer has no subscriptions at the time of profile creation. The
    ///     subscriptions can be added, removed, or updated as needed, and the aggregate root ensures that all operations on
    ///     the subscriptions are consistent with the business rules and domain logic of the application.
    /// </summary>
    public List<Guid> Subscriptions { get; set; } = [];
}
