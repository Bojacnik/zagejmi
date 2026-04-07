using Zagejmi.Contracts.Abstractions;

namespace Zagejmi.Contracts.Commands;

/// <summary>
///     Represents a command to authenticate a user and retrieve a login token.
///     This command encapsulates the credentials needed for user authentication.
/// </summary>
public class UserLoginCommand : IMessage
{
    /// <summary>
    ///     Gets or sets the username for authentication.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the plaintext password for authentication.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}