using System;
using System.Collections.Generic;

using Zagejmi.Write.Domain.Abstractions;
using Zagejmi.Write.Domain.Auth;
using Zagejmi.Write.Domain.Profile.Associate;

namespace Zagejmi.Write.Domain.Profile;

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
public sealed class Profile : Aggregate
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Profile" /> class with the specified unique identifier.
    ///     This constructor is private to enforce the use of the static factory method for creating instances of the
    ///     Profile aggregate. The factory method ensures that all necessary validations and event raisings are performed
    ///     during the creation process, maintaining the integrity of the aggregate's state and adhering to the principles
    ///     of domain-driven design.
    /// </summary>
    /// <param name="id">Unique identifier for the profile aggregate.</param>
    private Profile(Guid id)
        : base(id)
    {
    }

    public Guid UserId { get; private set; }

    public PersonType PersonType { get; private set; }

    public PersonalInformation PersonalInformation { get; private set; }

    public PersonalStatistics PersonalStatistics { get; private set; }

    public List<Guid> Wallets { get; private set; } = [];

    public AssociateProfile? AssociateProfile { get; private set; }

    public bool Deleted { get; private set; }

    protected override void Apply(IDomainEvent domainEvent)
    {
    }
}