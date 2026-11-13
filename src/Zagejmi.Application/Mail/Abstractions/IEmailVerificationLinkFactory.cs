using System;

namespace Zagejmi.Application.Mail.Abstractions;

/// <summary>
///     Defines a contract for creating email verification links for users. Implementations of this interface are
///     responsible for generating secure and unique verification links that can be sent to users via email. The generated
///     links typically include a unique token and the user's identifier, allowing the system to verify the user's email
///     address when they click on the link. This interface is essential for managing user email verification processes,
///     ensuring that users can confirm their email addresses securely and efficiently.
/// </summary>
public interface IEmailVerificationLinkFactory
{
    /// <summary>
    ///     Creates a unique email verification link for the specified user, incorporating the provided token. The generated
    ///     link should be secure and include the user's identifier and the token, allowing the system to verify the user's
    ///     email address when they click on the link. Implementations of this method should ensure that the generated link is
    ///     valid for a limited time and cannot be easily guessed or forged, providing a secure mechanism for email
    ///     verification. The method should return the complete URL that can be sent to the user via email, enabling them to
    ///     confirm their email address and complete the verification process.
    /// </summary>
    /// <param name="userId">
    ///     The unique identifier of the user for whom the email verification link is being created. This parameter is
    ///     essential for associating the generated link with the correct user in the system, allowing for accurate
    ///     verification of the user's email address. The userId should be a valid GUID that corresponds to an existing user in
    ///     the system.
    /// </param>
    /// <param name="token">
    ///     The unique token for the email verification link. This parameter is essential for ensuring the security and
    ///     uniqueness of the generated link, preventing unauthorized access or manipulation.
    /// </param>
    /// <returns>
    ///     The complete URL for the email verification link that can be sent to the user via email.
    /// </returns>
    string Create(Guid userId, string token);
}
