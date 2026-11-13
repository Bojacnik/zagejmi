using System;
using System.Threading;
using System.Threading.Tasks;

using Zagejmi.Application.Auth.Users.Abstractions;
using Zagejmi.Application.Profiles;
using Zagejmi.Application.Profiles.Abstractions;
using Zagejmi.Domain.Auth;
using Zagejmi.Domain.Profiles;
using Zagejmi.Domain.Profiles.Customers;
using Zagejmi.Domain.Profiles.Players;
using Zagejmi.Shared;

namespace Zagejmi.Application.Wallets;

/// <summary>
///     Finds the profile that owns a user's Zákazník or Hráč wallet.
/// </summary>
internal static class WalletOwners
{
    /// <summary>
    ///     Resolves the profile ID of the user's customer or player profile.
    /// </summary>
    /// <param name="users">The user repository.</param>
    /// <param name="profiles">The profile repository.</param>
    /// <param name="userId">The user.</param>
    /// <param name="profile">Which profile.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The profile ID, or a failure when the user or profile does not exist.</returns>
    public static async Task<Result<Guid>> ResolveAsync(
        IUserRepository users,
        IProfileRepository profiles,
        Guid userId,
        ProfileType profile,
        CancellationToken cancellationToken)
    {
        User? user = await users.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result<Guid>.Failure(ErrorDetail.NotFound(nameof(User), userId));
        }

        if (profile == ProfileType.Player)
        {
            PlayerProfile? player = await profiles.GetPlayerByUserIdAsync(userId, cancellationToken);
            return player is null
                ? Result<Guid>.Failure(ProfileErrors.PlayerMissing(user.GrammaticalGender))
                : player.Id;
        }

        CustomerProfile? customer = await profiles.GetCustomerByUserIdAsync(userId, cancellationToken);
        return customer is null
            ? Result<Guid>.Failure(ErrorDetail.NotFound(nameof(CustomerProfile), userId))
            : customer.Id;
    }
}
