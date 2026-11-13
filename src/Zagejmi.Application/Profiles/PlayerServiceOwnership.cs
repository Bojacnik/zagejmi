using System;
using System.Threading;
using System.Threading.Tasks;

using Zagejmi.Application.Auth.Users.Abstractions;
using Zagejmi.Application.Profiles.Abstractions;
using Zagejmi.Domain.Auth;
using Zagejmi.Domain.Profiles.Players;
using Zagejmi.Shared;

namespace Zagejmi.Application.Profiles;

/// <summary>
///     Loads a player service for a change, making sure it belongs to the user and the user is not a ghost.
/// </summary>
internal static class PlayerServiceOwnership
{
    /// <summary>
    ///     Returns the service when the user may change it. Someone else's service is reported as not found, so the
    ///     response does not reveal which service IDs exist.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="profileRepository">The profile repository.</param>
    /// <param name="userId">The acting user.</param>
    /// <param name="serviceId">The service.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The service, or a failure.</returns>
    public static async Task<Result<PlayerService>> GetOwnedAsync(
        IUserRepository userRepository,
        IProfileRepository profileRepository,
        Guid userId,
        Guid serviceId,
        CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result<PlayerService>.Failure(ErrorDetail.NotFound(nameof(User), userId));
        }

        if (user.IsGhostMode)
        {
            return Result<PlayerService>.Failure(ProfileErrors.GhostMode);
        }

        PlayerProfile? player = await profileRepository.GetPlayerByUserIdAsync(user.Id, cancellationToken);
        PlayerService? service = await profileRepository.GetPlayerServiceAsync(serviceId, cancellationToken);

        if (player is null || service is null || service.PlayerProfileId != player.Id)
        {
            return Result<PlayerService>.Failure(new ErrorDetail(
                "Service.NotFound",
                "Tato služba neexistuje nebo už byla smazána.",
                ErrorType.NotFound));
        }

        return service;
    }
}
