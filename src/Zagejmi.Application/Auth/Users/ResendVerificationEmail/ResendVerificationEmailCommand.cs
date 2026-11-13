namespace Zagejmi.Application.Auth.Users.ResendVerificationEmail;

/// <summary>
///     Sends a new verification link.
/// </summary>
/// <param name="UserIdentifier">The user name or email the user typed on the login page.</param>
public sealed record ResendVerificationEmailCommand(string UserIdentifier);
