using System;

namespace Zagejmi.Domain.Auth.Verification;

/// <summary>
///     Represents a verification record for a user's identification documents, encapsulating the unique identifiers of the
///     front and back images of the identification documents. The VerificationId value object is essential for managing
///     user verification processes, ensuring that the user's identification documents are securely stored and handled
///     according to best practices. The VerificationId should be treated as sensitive information and should be protected
///     to prevent unauthorized access, ensuring the security and integrity of user verification within the system.
/// </summary>
public sealed record VerificationId : Abstractions.Verification
{
    /// <summary>
    ///     Gets the unique identifier for the back image of the user's identification document. This identifier is used to
    ///     uniquely identify the back image of the identification document and is typically generated when the verification
    ///     record is created.
    /// </summary>
    public Guid BackImageId { get; init; }

    /// <summary>
    ///     Gets the unique identifier for the front image of the user's identification document. This identifier is used to
    ///     uniquely identify the front image of the identification document and is typically generated when the verification
    ///     record is created.
    /// </summary>
    public Guid FrontImageId { get; init; }
}
