using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Zagejmi.Application.Abstractions;
using Zagejmi.Application.Profiles.Abstractions;
using Zagejmi.Domain.Profiles.Players;
using Zagejmi.Shared;

namespace Zagejmi.Application.Profiles.SearchPlayers;

/// <summary>
///     Lists player profiles for the public directory, newest first, optionally filtered by display name.
/// </summary>
public sealed class SearchPlayersHandler : IHandlerRequest<SearchPlayersQuery, Result<IReadOnlyList<PlayerListItem>>>
{
    private readonly IProfileRepository profileRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SearchPlayersHandler" /> class.
    /// </summary>
    /// <param name="profileRepository">The profile repository.</param>
    public SearchPlayersHandler(IProfileRepository profileRepository)
    {
        this.profileRepository = profileRepository;
    }

    /// <summary>
    ///     Returns one page of players.
    /// </summary>
    /// <param name="request">The query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The players on the requested page.</returns>
    public async Task<Result<IReadOnlyList<PlayerListItem>>> Handle(
        SearchPlayersQuery request,
        CancellationToken cancellationToken = default)
    {
        int page = Math.Max(1, request.Page);
        int pageSize = Math.Clamp(request.PageSize, 1, SearchPlayersQuery.MaxPageSize);
        string? search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim();

        IReadOnlyList<PlayerProfile> players = await this.profileRepository.SearchPlayersAsync(
            search,
            (page - 1) * pageSize,
            pageSize,
            cancellationToken);

        return players
            .Select(p => new PlayerListItem(p.Id, p.DisplayName, p.Description, p.CreatedAt, p.CardProfilePicture))
            .ToList();
    }
}
