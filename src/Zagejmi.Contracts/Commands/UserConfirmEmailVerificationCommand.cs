using System;

using Zagejmi.Contracts.Abstractions;

namespace Zagejmi.Contracts.Commands;

/// <summary>
///     Confirms email ownership for a user with a verification token.
/// </summary>
public sealed class UserConfirmEmailVerificationCommand : IMessage
{
    /// <summary>
    ///     Gets or sets the user aggregate ID.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    ///     Gets or sets the raw verification token from email.
    /// </summary>
    public string Token { get; set; } = string.Empty;
}