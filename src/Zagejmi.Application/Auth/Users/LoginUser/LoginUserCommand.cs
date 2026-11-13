namespace Zagejmi.Application.Auth.Users.LoginUser;

/// <summary>
///     Represents a command to sign a user in with their user name or email and password.
/// </summary>
public class LoginUserCommand
{
    /// <summary>
    ///     Gets the user name or email the user signs in with.
    /// </summary>
    public required string UserIdentifier { get; init; }

    /// <summary>
    ///     Gets the plain text password.
    /// </summary>
    public required string Password { get; init; }
}
