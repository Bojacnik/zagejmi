namespace Zagejmi.Domain.Auth.Verification;

/// <summary>
///     Represents a verification record for a user's facial recognition data, encapsulating the path to the facial image
///     used for verification. The VerificationFace value object is essential for managing user verification processes,
///     ensuring that the user's facial recognition data is securely stored and handled according to best practices. The
///     Path should be treated as sensitive information and should be protected to prevent unauthorized access, ensuring
///     the security and integrity of user verification within the system.
/// </summary>
public sealed record VerificationFace : Abstractions.Verification
{
    /// <summary>
    ///     Gets the path to the facial image used for verification. This property is used to locate and access the facial
    ///     image for verification purposes and should be securely stored and protected to prevent unauthorized access.
    ///     The Path property is essential for managing user verification processes and ensuring that the user's facial
    ///     recognition data is handled according to best practices.
    /// </summary>
    public required string Path { get; init; }
}