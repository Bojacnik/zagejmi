using System;

using Zagejmi.Shared;

namespace Zagejmi.Domain.Profiles.Abstractions;

/// <summary>
///     Represents a user profile within the Zagejmi application, encapsulating personal information, statistics, wallets,
///     and
///     associations with other profiles. This aggregate root is responsible for managing the state and behavior of user
///     profiles,
///     ensuring that all operations and changes to the profile are consistent with the business rules and domain logic of
///     the application. The Profile aggregate handles events related to profile creation, updates, and associations,
///     maintaining the integrity of the profile's state and ensuring that all changes are properly recorded and applied
///     through domain events. It serves as the central point of interaction for any operations related to user profiles,
///     including the management of personal information, statistics, wallets, and associations with other profiles, while
///     adhering to the principles of domain-driven design and ensuring that all business rules and invariants are
///     maintained throughout the lifecycle of the profile.
/// </summary>
public abstract class Profile : ISoftDeletable
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Profile" /> class.
    /// </summary>
    /// <param name="id">The profile identifier.</param>
    /// <param name="userId">The identifier of the user that owns the profile.</param>
    /// <param name="displayName">Display name of the profile.</param>
    protected Profile(Guid id, Guid userId, string displayName)
    {
        this.Id = id;
        this.UserId = userId;
        this.DisplayName = displayName;
        this.CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    ///     Gets the profile identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    ///     Gets the identifier of the user that owns the profile.
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    ///     Gets the time the profile was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    ///     Gets or sets the display name of the profile, which is a user-friendly representation of the profile's identity.
    ///     This property is used for display purposes in the application, allowing users to easily identify and interact with
    ///     their profiles. The display name can be customized by the user and may include personal or professional
    ///     identifiers, such as a full name, nickname, or business name. It is important to ensure that the display name
    ///     adheres to any validation rules or guidelines set by the application, such as length restrictions or prohibited
    ///     characters, to maintain a consistent and user-friendly experience across the platform. The DisplayName property is
    ///     immutable, meaning that once it is set during profile creation, it cannot be changed, ensuring that the profile's
    ///     identity remains consistent throughout its lifecycle.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the profile has been softly deleted. A soft-deleted profile is one that has
    ///     been marked as deleted but is not permanently removed from the database, allowing for potential recovery or
    ///     auditing purposes. This property is used to manage the visibility and accessibility of the profile within the
    ///     application, ensuring that soft-deleted profiles are not displayed or interacted with by users. The IsSoftDeleted
    ///     property is typically set to true when a user or administrator initiates a soft delete operation, and it can be
    ///     used in conjunction with other properties or methods to enforce business rules related to profile deletion and
    ///     recovery. It is important to handle soft-deleted profiles appropriately within the application, ensuring that they
    ///     are excluded from active queries and operations while still being retained for auditing or recovery purposes.
    /// </summary>
    public bool IsSoftDeleted { get; set; }
}