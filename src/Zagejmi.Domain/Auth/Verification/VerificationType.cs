namespace Zagejmi.Domain.Auth.Verification;

/// <summary>
///     Represents the different types of verification methods available for user authentication and identity verification.
/// </summary>
public enum VerificationType
{
    /// <summary>
    ///     Represents verification using a unique identifier, such as a user ID or account ID, to confirm the user's identity.
    /// </summary>
    Id,

    /// <summary>
    ///     Represents verification using a government-issued identification document, such as a passport or driver's license,
    ///     to confirm the user's identity.
    /// </summary>
    Face,

    /// <summary>
    ///     Represents verification using personal information, such as a phone number or email address, to confirm the user's
    ///     identity.
    /// </summary>
    Personal,
}