using System;

using Zagejmi.Domain.Profiles;

namespace Zagejmi.Domain.Auth;

/// <summary>
///     Represents a user in the system, encapsulating authentication and authorization information such as username,
///     password, email, and associated person IDs. The User aggregate is responsible for managing the state and behavior
///     related to user accounts, including creation, updates, and deletion. It ensures that all operations on the user
///     account are consistent and adhere to the business rules defined for user management.
/// </summary>
public sealed class User
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="User" /> class with the specified unique identifier. This constructor
    ///     is private to enforce the use of factory methods for creating user instances, ensuring that all necessary
    ///     properties are properly initialized and that any business rules related to user creation are enforced. The unique
    ///     identifier (id) is essential for tracking and managing user accounts within the system, allowing for efficient
    ///     retrieval, updates, and associations with other entities in the domain model.
    /// </summary>
    /// <param name="id">
    ///     The unique identifier for the user, typically generated as a GUID to ensure uniqueness across
    ///     different instances and systems. This parameter is essential for tracking and managing user accounts effectively,
    ///     allowing for efficient retrieval, updates, and associations with other entities in the domain model.
    /// </param>
    public User(Guid id)
    {
        this.Id = id;
    }

    /// <summary>
    ///     Gets the ID of the user.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    ///     Gets the authentication credentials associated with the user. The AuthCredentials property encapsulates the
    ///     necessary information for user authentication, including the username, securely hashed password, and email
    ///     address. This property is essential for managing user authentication and ensuring that the user's credentials are
    ///     securely stored and handled according to best practices. The AuthCredentials property should be treated as
    ///     sensitive information and should be protected to prevent unauthorized access, ensuring the security and integrity
    ///     of user accounts within the system.
    /// </summary>
    public required AuthCredentials AuthCredentials { get; init; }

    /// <summary>
    ///     Gets or sets the gender the user identifies as.
    /// </summary>
    public Gender Gender { get; set; }

    /// <summary>
    ///     Gets or sets the grammatical gender used when addressing the user in Czech (Hráč/Hráčka,
    ///     Zákazník/Zákaznice). See <see cref="GrammaticalGenderRules.Resolve" />.
    /// </summary>
    public GrammaticalGender GrammaticalGender { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the user is in ghost mode. A ghost is invisible to others (no online
    ///     status, no activity traces) and can only browse the site as a customer; every action that changes state is
    ///     denied until ghost mode is turned off.
    /// </summary>
    public bool IsGhostMode { get; set; }

    /// <summary>
    ///     Gets or sets the profile that is active right after the user signs in. Every user has a customer profile,
    ///     so <see cref="ProfileType.Customer" /> is the default.
    /// </summary>
    public ProfileType DefaultProfile { get; set; } = ProfileType.Customer;
}