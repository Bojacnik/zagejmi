using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Zagejmi.Application.Abstractions;
using Zagejmi.Application.Profiles.Abstractions;
using Zagejmi.Application.Profiles.Reviews;
using Zagejmi.Application.Profiles.Reviews.Abstractions;
using Zagejmi.Domain.Profiles.Players;
using Zagejmi.Shared;

namespace Zagejmi.Application.Profiles.SearchPlayers;

/// <summary>
///     Lists player profiles for the public directory, most complete first, then newest, optionally filtered by display name.
/// </summary>
public sealed class SearchPlayersHandler : IHandlerRequest<SearchPlayersQuery, Result<PagedResult<PlayerListItem>>>
{
    private readonly IProfileRepository profileRepository;
    private readonly IPlayerReviewRepository reviewRepository;
    private readonly ReviewOptions reviewOptions;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SearchPlayersHandler" /> class.
    /// </summary>
    /// <param name="profileRepository">The profile repository.</param>
    /// <param name="reviewRepository">The recommendation repository (average ratings on the cards).</param>
    /// <param name="reviewOptions">When an average is shown.</param>
    public SearchPlayersHandler(
        IProfileRepository profileRepository,
        IPlayerReviewRepository reviewRepository,
        IOptions<ReviewOptions> reviewOptions)
    {
        this.profileRepository = profileRepository;
        this.reviewRepository = reviewRepository;
        this.reviewOptions = reviewOptions.Value;
    }

    /// <summary>
    ///     Returns one page of players.
    /// </summary>
    /// <param name="request">The query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The players on the requested page.</returns>
    public async Task<Result<PagedResult<PlayerListItem>>> Handle(
        SearchPlayersQuery request,
        CancellationToken cancellationToken = default)
    {
        if (request.Gender is { } gender && !Enum.IsDefined(gender))
        {
            return Result<PagedResult<PlayerListItem>>.Failure(
                ErrorDetail.Validation("Players.Gender", "Neznámá skupina pohlaví."));
        }

        int page = Math.Max(1, request.Page);
        int pageSize = Math.Clamp(request.PageSize, 1, SearchPlayersQuery.MaxPageSize);
        string? search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim();

        PagedResult<PlayerProfile> players = await this.profileRepository.SearchPlayersAsync(
            search,
            request.Gender,
            (page - 1) * pageSize,
            pageSize,
            cancellationToken);

        IReadOnlyDictionary<Guid, RatingSummary> ratings = players.Items.Count == 0
            ? new Dictionary<Guid, RatingSummary>()
            : await this.reviewRepository.GetSummariesAsync(players.Items.Select(p => p.Id).ToList(), cancellationToken);

        return new PagedResult<PlayerListItem>(
            players.Items
                .Select(p => new PlayerListItem(
                    p.Id,
                    p.DisplayName,
                    p.Description,
                    p.CreatedAt,
                    p.CardProfilePicture,
                    this.reviewOptions.Visible(ratings.GetValueOrDefault(p.Id))))
                .ToList(),
            players.TotalCount);
    }
}
