using System;
using System.Collections.Generic;

using Zagejmi.Write.Domain.Abstractions;

namespace Zagejmi.Write.Domain.Auth;

/// <summary>
///     Represents the authentication credentials for a user, encapsulating the necessary information for user
///     authentication,
///     including the username, securely hashed password, and email address. The AuthCredentials value object is essential
///     for managing user authentication and ensuring that the user's credentials are securely stored and handled according
///     to best practices. The AuthCredentials should be treated as sensitive information and should be protected to
///     prevent unauthorized access, ensuring the security and integrity of user accounts within the system.
/// </summary>
public record AuthCredentials : ValueObject
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AuthCredentials" /> class with the specified unique identifier,
    ///     username, password hash, and email address. This constructor allows for the creation of a complete authentication
    ///     record, ensuring that all necessary information is provided to manage user authentication effectively. The Id
    ///     parameter serves as the primary key for the authentication record, while the UserName, PasswordHash, and Email
    ///     parameters provide essential details for user identification and communication. It is important to ensure that the
    ///     provided values adhere to validation rules (e.g., unique username and email) to maintain data integrity and
    ///     security within the authentication system.
    /// </summary>
    /// <param name="id">
    ///     The unique identifier for the authentication record, typically generated as a GUID to ensure
    ///     uniqueness across different instances and systems. This parameter serves as the primary key for the Auth entity and
    ///     is essential for tracking and managing authentication records effectively.
    /// </param>
    /// <param name="userName">
    ///     The username associated with the authentication record, which serves as a unique identifier for
    ///     the user within the authentication system. This parameter is used for login and identification purposes, and it is
    ///     important to ensure that it is unique across all authentication records to prevent conflicts and ensure proper user
    ///     management. The UserName should be treated as a case-insensitive value to provide a better user experience and
    ///     avoid issues with case sensitivity during login attempts.
    /// </param>
    /// <param name="passwordHash">
    ///     The securely hashed password for the authentication record, which is the result of hashing
    ///     the user's original password combined with a unique salt and a secret pepper. This parameter is crucial for
    ///     protecting against brute-force attacks and unauthorized access, and it should be securely hashed using a strong
    ///     hashing algorithm (e.g., bcrypt, Argon2). The PasswordHash should never store the original plaintext password, and
    ///     it should be treated as sensitive information that requires proper security measures to prevent exposure.
    /// </param>
    /// <param name="email">
    ///     The email address associated with the authentication record, which is used for communication and
    ///     identification purposes. This parameter allows the system to send notifications, password reset instructions, and
    ///     other relevant information to the user. It is important to validate the email address format and ensure that it is
    ///     unique across all authentication records to prevent conflicts and ensure proper user management. The Email property
    ///     should be treated as a case-insensitive value to provide a better user experience and avoid issues with case
    ///     sensitivity during email-based operations.
    /// </param>
    public AuthCredentials(Guid id, string userName, string passwordHash, string email)
    {
        this.Id = id;
        this.UserName = userName;
        this.PasswordHash = passwordHash;
        this.Email = email;
    }

    /// <summary>
    ///     Gets or sets the unique identifier for the authentication record. This property serves as the primary key for the
    ///     Auth entity and is used to uniquely identify each authentication record in the system. It is typically generated as
    ///     a GUID (Globally Unique Identifier) to ensure uniqueness across different instances and systems. The Id property is
    ///     essential for tracking and managing authentication records, allowing for efficient retrieval, updates, and
    ///     associations with other entities in the domain model.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Gets or sets the username associated with the authentication record. The UserName property is a unique identifier
    ///     for the user within the authentication system and is used for login and identification purposes. It is important to
    ///     ensure that the UserName is unique across all authentication records to prevent conflicts and ensure proper user
    ///     management. The UserName should be treated as a case-insensitive value to provide a better user experience and
    ///     avoid issues with case sensitivity during login attempts.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    ///     Gets or sets the securely hashed password for the authentication record. The PasswordHash property stores the
    ///     result of hashing the user's original password combined with a unique salt and a secret pepper. It is crucial to
    ///     ensure that the password is securely hashed using a strong hashing algorithm (e.g., bcrypt, Argon2) to protect
    ///     against brute-force attacks and unauthorized access. The PasswordHash should never store the original plaintext
    ///     password, and it should be treated as sensitive information that requires proper security measures to prevent
    ///     exposure.
    /// </summary>
    public string PasswordHash { get; set; }

    /// <summary>
    ///     Gets or sets the email address associated with the authentication record. The Email property is used for
    ///     communication and identification purposes, allowing the system to send notifications, password reset instructions,
    ///     and other relevant information to the user. It is important to validate the email address format and ensure that it
    ///     is unique across all authentication records to prevent conflicts and ensure proper user management. The Email
    ///     property should be treated as a case-insensitive value to provide a better user experience and avoid issues with
    ///     case sensitivity during email-based operations.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    ///     Returns the atomic values that define the equality of this value object. This method is used by the base
    ///     ValueObject class to implement equality checks and hashing. Each property that contributes to the identity of the
    ///     value object should be returned in this method. The order of the values returned should be consistent, as it
    ///     affects the equality comparison and hash code generation.
    /// </summary>
    /// <returns>An enumerable of atomic values that define the equality of this value object.</returns>
    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return this.Id;
        yield return this.UserName;
        yield return this.PasswordHash;
        yield return this.Email;
    }
}