using System;
using System.Threading;
using System.Threading.Tasks;

namespace Zagejmi.Application.Auth.Tokens;

/// <summary>
///     Stores issued refresh tokens (as hashes) for later rotation and revocation.
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>
    ///     Stores a newly issued refresh token, starting a new token family.
    /// </summary>
    /// <param name="userId">The owner of the token.</param>
    /// <param name="tokenHash">The hash produced by <see cref="ITokenService.HashRefreshToken" />.</param>
    /// <param name="expiresAt">When the token stops being valid.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that completes when the token is stored.</returns>
    Task AddAsync(Guid userId, string tokenHash, DateTimeOffset expiresAt, CancellationToken cancellationToken = default);
}
