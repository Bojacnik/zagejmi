using System;
using System.Threading;
using System.Threading.Tasks;

using Zagejmi.Application.Auth.Users.Abstractions;
using Zagejmi.Application.Profiles.Abstractions;
using Zagejmi.Domain.Auth;
using Zagejmi.Domain.Profiles.Customers;
using Zagejmi.Domain.Profiles.Players;
using Zagejmi.Shared;

namespace Zagejmi.Application.Profiles.Reviews;

/// <summary>
///     Who may change a review: the signed-in user's customer profile, not in ghost mode, not about their own player.
/// </summary>
internal static class ReviewAccess
{
    /// <summary>
    ///     The user tried to review their own player profile.
    /// </summary>
    public static readonly ErrorDetail OwnProfile = new(
        "Review.OwnProfile",
        "Svůj vlastní profil hodnotit nemůžete.",
        ErrorType.Forbidden);

    /// <summary>
    ///     Loads the author's customer profile and the reviewed player, and checks the user may review them.
    /// </summary>
    /// <param name="users">The user repository.</param>
    /// <param name="profiles">The profile repository.</param>
    /// <param name="userId">The signed-in user.</param>
    /// <param name="playerProfileId">The reviewed player.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The author and the player, or why the user may not review them.</returns>
    public static async Task<Result<(CustomerProfile Author, PlayerProfile Player)>> ResolveAsync(
        IUserRepository users,
        IProfileRepository profiles,
        Guid userId,
        Guid playerProfileId,
        CancellationToken cancellationToken)
    {
        User? user = await users.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result<(CustomerProfile, PlayerProfile)>.Failure(ErrorDetail.NotFound(nameof(User), userId));
        }

        if (user.IsGhostMode)
        {
            return Result<(CustomerProfile, PlayerProfile)>.Failure(ProfileErrors.GhostMode);
        }

        PlayerProfile? player = await profiles.GetPlayerByIdAsync(playerProfileId, cancellationToken);
        if (player is null)
        {
            return Result<(CustomerProfile, PlayerProfile)>.Failure(
                ErrorDetail.NotFound(nameof(PlayerProfile), playerProfileId));
        }

        if (player.UserId == user.Id)
        {
            return Result<(CustomerProfile, PlayerProfile)>.Failure(OwnProfile);
        }

        CustomerProfile? author = await profiles.GetCustomerByUserIdAsync(user.Id, cancellationToken);
        if (author is null)
        {
            return Result<(CustomerProfile, PlayerProfile)>.Failure(ErrorDetail.Validation(
                "Customer.Missing",
                "Recenze se píší za zákaznický profil, který zatím nemáte."));
        }

        return (author, player);
    }
}
