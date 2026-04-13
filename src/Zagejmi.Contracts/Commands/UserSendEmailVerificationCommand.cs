using Zagejmi.Contracts.Abstractions;

namespace Zagejmi.Contracts.Commands;

/// <summary>
///     Requests that a verification email is sent to a user.
/// </summary>
public sealed class UserSendEmailVerificationCommand : IMessage
{
    /// <summary>
    ///     Gets or sets username or email used to identify the user.
    /// </summary>
    public string UserIdentifier { get; set; } = string.Empty;
}