using System;

using Zagejmi.Domain.Auth;
using Zagejmi.Domain.Profiles;

namespace Zagejmi.Infrastructure.Auth.Users.Records;

/// <summary>
///     Represents a record for a user in the authentication system, containing information such as username, email,
///     password hash, salt, and associated customer profiles.
/// </summary>
public record UserRecord
{
    /// <summary>
    ///     Gets the unique identifier for the user record. This identifier is used to uniquely identify a user in the
    ///     authentication system and is typically generated when the user is created.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    ///     Gets or sets the username for the user. This username is used for authentication and identification purposes within
    ///     the authentication system.
    /// </summary>
    public required string UserName { get; set; }

    /// <summary>
    ///     Gets or sets the email address for the user. This email address is used for communication, account recovery, and
    ///     verification purposes within the authentication system.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the user's email address has been verified. This property is used to track
    ///     the verification status of the user's email address and is typically updated when the user successfully verifies
    ///     their email address through a verification process.
    /// </summary>
    public DateTimeOffset? EmailVerifiedAt { get; set; }

    /// <summary>
    ///     Gets or sets the hash of the pending email verification token. This property is used to store the hash of the token
    ///     that is sent to the user's email address for verification purposes. When the user clicks on the verification link
    ///     in the email, the system compares the hash of the provided token with this stored hash to verify the user's email
    ///     address.
    /// </summary>
    public required string? PendingEmailVerificationTokenHash { get; set; }

    /// <summary>
    ///     Gets or sets the timestamp indicating when the pending email verification token was generated. This property is
    ///     used to track the age of the pending email verification token and can be used to enforce expiration policies for
    ///     email verification tokens. If the token is not used within a certain time frame, it may be considered expired and
    ///     invalid for verification purposes.
    /// </summary>
    public DateTimeOffset? PendingEmailVerificationTokenExpiresAt { get; set; }

    /// <summary>
    ///     Gets or sets the hash of the user's password. This property is used to securely store the user's password in a
    ///     hashed format. Has encoded salt and pepper. The password hash is used for authentication purposes, allowing the
    ///     system to verify the user's password during login without storing the actual plaintext password.
    /// </summary>
    public required string PasswordHash { get; set; }

    /// <summary>
    ///     Gets or sets the gender the user identifies as.
    /// </summary>
    public Gender Gender { get; set; }

    /// <summary>
    ///     Gets or sets the grammatical gender used when addressing the user in Czech.
    /// </summary>
    public GrammaticalGender GrammaticalGender { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the user is in ghost mode.
    /// </summary>
    public bool IsGhostMode { get; set; }

    /// <summary>
    ///     Gets or sets the profile that is active right after sign-in.
    /// </summary>
    public ProfileType DefaultProfile { get; set; } = ProfileType.Customer;
}
