using System;

namespace Zagejmi.Application.Auth.Users.LoginUser;

/// <summary>
///     The tokens issued to a user after a successful sign-in.
/// </summary>
/// <param name="UserId">The ID of the signed-in user.</param>
/// <param name="AccessToken">The JWT access token.</param>
/// <param name="RefreshToken">The opaque refresh token (only its hash is stored).</param>
public sealed record LoginUserResult(Guid UserId, string AccessToken, string RefreshToken);
