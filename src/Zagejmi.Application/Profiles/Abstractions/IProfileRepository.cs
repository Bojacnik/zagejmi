using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Zagejmi.Domain.Profiles.Customer;
using Zagejmi.Domain.Profiles.Players;

namespace Zagejmi.Application.Profiles.Abstractions;

/// <summary>
///     Persists customer and player profiles. Every user owns exactly one customer profile and at most one player profile.
/// </summary>
public interface IProfileRepository
{
    /// <summary>
    ///     Gets the customer profile owned by the user.
    /// </summary>
    /// <param name="userId">The owning user identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The customer profile, or <see langword="null" /> when the user has none.</returns>
    Task<CustomerProfile?> GetCustomerByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the player profile owned by the user.
    /// </summary>
    /// <param name="userId">The owning user identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The player profile, or <see langword="null" /> when the user has none.</returns>
    Task<PlayerProfile?> GetPlayerByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets a player profile by its own ID.
    /// </summary>
    /// <param name="profileId">The player profile ID.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The player profile, or <see langword="null" /> when it does not exist.</returns>
    Task<PlayerProfile?> GetPlayerByIdAsync(Guid profileId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks whether a profile display name is already taken by any customer or player profile.
    /// </summary>
    /// <param name="displayName">The display name to check.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see langword="true" /> when the name is taken.</returns>
    Task<bool> IsDisplayNameTakenAsync(string displayName, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Adds a customer profile.
    /// </summary>
    /// <param name="profile">The profile to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AddCustomerAsync(CustomerProfile profile, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Adds a player profile.
    /// </summary>
    /// <param name="profile">The profile to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AddPlayerAsync(PlayerProfile profile, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists player profiles, newest first, optionally filtered by display name (case-insensitive substring).
    /// </summary>
    /// <param name="search">Text to look for in the display name, or <see langword="null" /> for all players.</param>
    /// <param name="skip">How many players to skip.</param>
    /// <param name="take">How many players to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The matching players.</returns>
    Task<IReadOnlyList<PlayerProfile>> SearchPlayersAsync(
        string? search,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Saves changes to a customer profile (display name, picture).
    /// </summary>
    /// <param name="profile">The profile.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that completes when the profile is saved.</returns>
    Task UpdateCustomerAsync(CustomerProfile profile, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Saves changes to a player profile (display name, description, pictures).
    /// </summary>
    /// <param name="profile">The profile.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that completes when the profile is saved.</returns>
    Task UpdatePlayerAsync(PlayerProfile profile, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists the services of a player, oldest first.
    /// </summary>
    /// <param name="playerProfileId">The player profile ID.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The services.</returns>
    Task<IReadOnlyList<PlayerService>> GetPlayerServicesAsync(
        Guid playerProfileId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Counts the services of a player.
    /// </summary>
    /// <param name="playerProfileId">The player profile ID.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of services.</returns>
    Task<int> CountPlayerServicesAsync(Guid playerProfileId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Stores a new player service.
    /// </summary>
    /// <param name="service">The service.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that completes when the service is stored.</returns>
    Task AddPlayerServiceAsync(PlayerService service, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets a player service by ID.
    /// </summary>
    /// <param name="serviceId">The service ID.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The service, or <see langword="null" /> when it does not exist.</returns>
    Task<PlayerService?> GetPlayerServiceAsync(Guid serviceId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Saves changes to a player service.
    /// </summary>
    /// <param name="service">The service.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that completes when the service is saved.</returns>
    Task UpdatePlayerServiceAsync(PlayerService service, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a player service.
    /// </summary>
    /// <param name="serviceId">The service ID.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that completes when the service is deleted.</returns>
    Task DeletePlayerServiceAsync(Guid serviceId, CancellationToken cancellationToken = default);
}
