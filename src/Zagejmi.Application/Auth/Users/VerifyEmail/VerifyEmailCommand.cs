using System;

namespace Zagejmi.Application.Auth.Users.VerifyEmail;

/// <summary>
///     Confirms a user's email address with the token from the verification link.
/// </summary>
/// <param name="UserId">The user from the link.</param>
/// <param name="Token">The token from the link.</param>
public sealed record VerifyEmailCommand(Guid UserId, string Token);
