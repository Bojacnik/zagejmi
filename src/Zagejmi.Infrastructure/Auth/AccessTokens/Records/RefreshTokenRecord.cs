using System;

namespace Zagejmi.Infrastructure.Auth.AccessTokens.Records;

/// <summary>
///     Persisted refresh token state used for rotation and revocation.
/// </summary>
public sealed record RefreshTokenRecord
{
    /// <summary>
    ///     Gets or sets the unique identifier for the refresh token record, typically generated as a GUID to ensure
    ///     uniqueness across different instances and systems. This property serves as the primary key for the refresh token
    ///     entity and is essential for tracking and managing refresh token records, allowing for efficient retrieval, updates,
    ///     and associations with other entities in the domain model.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the user that owns the refresh token. Users live in a separate DbContext, so the
    ///     relationship is kept by ID only.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    ///     Gets or sets the unique identifier for the token family associated with the refresh token record. The TokenFamilyId
    ///     property is used to group related refresh tokens together, allowing for efficient management of token rotation and
    ///     revocation. By associating refresh tokens with a specific token family, the system can track and manage the
    ///     lifecycle of related tokens, ensuring that when one token is revoked or rotated, the associated tokens within the
    ///     same family can be appropriately handled. This property is essential for maintaining the integrity and security of
    ///     the authentication system, enabling effective management of user sessions and access control.
    /// </summary>
    public Guid TokenFamilyId { get; set; }

    /// <summary>
    ///     Gets or sets the securely hashed value of the refresh token. The TokenHash property stores the result of hashing
    ///     the original refresh token value combined with a unique salt and a secret pepper. It is crucial to ensure that the
    ///     refresh token is securely hashed using a strong hashing algorithm (e.g., bcrypt, Argon2) to protect against
    ///     brute-force attacks and unauthorized access. The TokenHash should never store the original plaintext refresh token,
    ///     and it should be treated as sensitive information that requires proper security measures to prevent exposure.
    /// </summary>
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the timestamp indicating when the refresh token record was created. The CreatedAt property is used to
    ///     track the creation time of the refresh token, allowing for proper management of token expiration and rotation.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    ///     Gets or sets the timestamp indicating when the refresh token is set to expire. The ExpiresAt property is used to
    ///     track the expiration time of the refresh token, allowing for proper management of token validity and ensuring that
    ///     refresh tokens are not used beyond their intended lifespan. It is important to enforce expiration policies to
    ///     enhance security and prevent unauthorized access to protected resources. The ExpiresAt property should be treated
    ///     as a critical security measure, and the system should implement appropriate checks to ensure that expired tokens
    ///     are not accepted for authentication or authorization purposes.
    /// </summary>
    public DateTimeOffset ExpiresAt { get; set; }

    /// <summary>
    ///     Gets or sets the timestamp indicating when the refresh token was revoked. The RevokedAt property is used to track
    ///     the revocation time of the refresh token, allowing for proper management of token invalidation and ensuring that
    ///     revoked tokens are not used for authentication or authorization purposes. It is important to enforce revocation
    ///     policies to enhance security and prevent unauthorized access to protected resources. The RevokedAt property should
    ///     be treated as a critical security measure, and the system should implement appropriate checks to ensure that
    ///     revoked tokens are not accepted for authentication or authorization purposes.
    /// </summary>
    public DateTimeOffset? RevokedAt { get; set; }

    /// <summary>
    ///     Gets or sets the unique identifier of the refresh token that replaced this token, if applicable. The
    ///     ReplacedByTokenId property is used to track the relationship between refresh tokens in cases where token rotation
    ///     occurs. When a refresh token is rotated, a new token is issued to replace the old one, and this property stores the
    ///     identifier of the new token that supersedes the current token. This allows for proper management of token
    ///     lifecycles and ensures that the system can accurately track which tokens are valid and which have been replaced.
    ///     The ReplacedByTokenId property is essential for maintaining the integrity and security of the authentication
    ///     system, enabling effective management of user sessions and access control.
    /// </summary>
    public Guid? ReplacedByTokenId { get; set; }

    /// <summary>
    ///     Gets or sets the reason for revocation of the refresh token, if applicable. The RevocationReason property is used
    ///     to provide context and information about why a refresh token was revoked, allowing for better understanding and
    ///     auditability of the revocation process.
    /// </summary>
    public string? RevocationReason { get; set; }
}