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
using Zagejmi.Domain.Games;
using Zagejmi.Shared;

namespace Zagejmi.Application.Profiles.SearchServices;

/// <summary>
///     Lists available services of all players for the Služby page (MARKET-001, MARKET-003), most complete first, then newest.
/// </summary>
public sealed class SearchServicesHandler : IHandlerRequest<SearchServicesQuery, Result<PagedResult<ServiceListItem>>>
{
    private readonly IProfileRepository profileRepository;
    private readonly IServiceReviewRepository reviewRepository;
    private readonly ReviewOptions reviewOptions;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SearchServicesHandler" /> class.
    /// </summary>
    /// <param name="profileRepository">The profile repository.</param>
    /// <param name="reviewRepository">The service review repository (averages on the cards).</param>
    /// <param name="reviewOptions">When an average is shown.</param>
    public SearchServicesHandler(
        IProfileRepository profileRepository,
        IServiceReviewRepository reviewRepository,
        IOptions<ReviewOptions> reviewOptions)
    {
        this.profileRepository = profileRepository;
        this.reviewRepository = reviewRepository;
        this.reviewOptions = reviewOptions.Value;
    }

    /// <summary>
    ///     Validates the filters and returns one page of services.
    /// </summary>
    /// <param name="request">The query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The services, or a validation failure for an unknown game or rank.</returns>
    public async Task<Result<PagedResult<ServiceListItem>>> Handle(
        SearchServicesQuery request,
        CancellationToken cancellationToken = default)
    {
        Result<ServiceSearchFilter> filter = ToFilter(request);
        if (filter.IsFailure)
        {
            return Result<PagedResult<ServiceListItem>>.Failure(filter.Error!);
        }

        int page = Math.Max(1, request.Page);
        int pageSize = Math.Clamp(request.PageSize, 1, SearchServicesQuery.MaxPageSize);

        PagedResult<ServiceListItem> services = await this.profileRepository.SearchServicesAsync(
            filter.Value,
            (page - 1) * pageSize,
            pageSize,
            cancellationToken);
        IReadOnlyDictionary<Guid, RatingSummary> ratings = services.Items.Count == 0
            ? new Dictionary<Guid, RatingSummary>()
            : await this.reviewRepository.GetSummariesAsync([.. services.Items.Select(s => s.ServiceId)], cancellationToken);

        return Result<PagedResult<ServiceListItem>>.Success(new PagedResult<ServiceListItem>(
            [.. services.Items.Select(s => s with { Rating = this.reviewOptions.Visible(ratings.GetValueOrDefault(s.ServiceId)) })],
            services.TotalCount));
    }

    /// <summary>
    ///     Turns the query into a repository filter; a minimum rank becomes the list of that rank and all higher ones.
    /// </summary>
    internal static Result<ServiceSearchFilter> ToFilter(SearchServicesQuery request)
    {
        string? search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim();
        string? gameKey = string.IsNullOrWhiteSpace(request.GameKey) ? null : request.GameKey.Trim();
        string? minRankKey = string.IsNullOrWhiteSpace(request.MinRankKey) ? null : request.MinRankKey.Trim();

        if (request.Gender is { } gender && !Enum.IsDefined(gender))
        {
            return Fail("Services.Gender", "Neznámá skupina pohlaví.");
        }

        if ((request.PlayStyles & ~PlayStyles.Both) != 0)
        {
            return Fail("Services.PlayStyles", "Neznámý styl hraní.");
        }

        if (gameKey is null)
        {
            return new ServiceSearchFilter(search, null, null, request.PlayStyles, request.Gender);
        }

        Game? game = GameCatalog.Find(gameKey);
        if (game is null)
        {
            return Fail("Services.Game", "Neznámá hra.");
        }

        IReadOnlyCollection<string>? rankKeys = null;
        if (minRankKey is not null)
        {
            if (game.FindRank(minRankKey) is not { } minRank)
            {
                return Fail("Services.Rank", $"Neznámý rank pro {game.Name}.");
            }

            rankKeys = game.Ranks.Where(r => r.Order >= minRank.Order).Select(r => r.Key).ToArray();
        }

        return new ServiceSearchFilter(search, game.Key, rankKeys, request.PlayStyles, request.Gender);
    }

    private static Result<ServiceSearchFilter> Fail(string code, string message)
    {
        return Result<ServiceSearchFilter>.Failure(ErrorDetail.Validation(code, message));
    }
}
