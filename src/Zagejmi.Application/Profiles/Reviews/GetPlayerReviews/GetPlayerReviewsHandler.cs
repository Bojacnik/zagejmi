using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Zagejmi.Application.Abstractions;
using Zagejmi.Application.Auth.Users.Abstractions;
using Zagejmi.Application.Profiles.Abstractions;
using Zagejmi.Application.Profiles.Reviews.Abstractions;
using Zagejmi.Domain.Auth;
using Zagejmi.Domain.Profiles.Customers;
using Zagejmi.Domain.Profiles.Players;
using Zagejmi.Shared;

namespace Zagejmi.Application.Profiles.Reviews.GetPlayerReviews;

/// <summary>
///     Gets a page of a player's reviews, the average rating, and the viewer's own review.
/// </summary>
public sealed class GetPlayerReviewsHandler : IHandlerRequest<GetPlayerReviewsQuery, Result<PlayerReviewsView>>
{
    private readonly IUserRepository userRepository;
    private readonly IProfileRepository profileRepository;
    private readonly IPlayerReviewRepository reviewRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GetPlayerReviewsHandler" /> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="profileRepository">The profile repository.</param>
    /// <param name="reviewRepository">The review repository.</param>
    public GetPlayerReviewsHandler(
        IUserRepository userRepository,
        IProfileRepository profileRepository,
        IPlayerReviewRepository reviewRepository)
    {
        this.userRepository = userRepository;
        this.profileRepository = profileRepository;
        this.reviewRepository = reviewRepository;
    }

    /// <inheritdoc />
    public async Task<Result<PlayerReviewsView>> Handle(
        GetPlayerReviewsQuery request,
        CancellationToken cancellationToken = default)
    {
        PlayerProfile? player = await this.profileRepository.GetPlayerByIdAsync(request.PlayerProfileId, cancellationToken);
        if (player is null)
        {
            return Result<PlayerReviewsView>.Failure(ErrorDetail.NotFound(nameof(PlayerProfile), request.PlayerProfileId));
        }

        int page = Math.Max(1, request.Page);
        int pageSize = Math.Clamp(request.PageSize, 1, GetPlayerReviewsQuery.MaxPageSize);
        PagedResult<PlayerReviewItem> reviews = await this.reviewRepository.GetForPlayerAsync(
            player.Id,
            (page - 1) * pageSize,
            pageSize,
            cancellationToken);
        IReadOnlyDictionary<Guid, RatingSummary> summaries =
            await this.reviewRepository.GetSummariesAsync([player.Id], cancellationToken);

        (ReviewEligibility eligibility, PlayerReviewItem? mine) = await this.GetViewerStateAsync(
            request.ViewerUserId,
            player,
            cancellationToken);

        return new PlayerReviewsView(
            summaries.GetValueOrDefault(player.Id, RatingSummary.None),
            reviews.Items,
            reviews.TotalCount,
            mine,
            eligibility);
    }

    private static PlayerReviewItem ToItem(PlayerReview review, CustomerProfile author) => new(
        author.Id,
        author.DisplayName,
        author.ProfilePicture,
        review.Rating,
        review.Comment,
        review.CreatedAt,
        review.UpdatedAt);

    private async Task<(ReviewEligibility Eligibility, PlayerReviewItem? Mine)> GetViewerStateAsync(
        Guid? viewerUserId,
        PlayerProfile player,
        CancellationToken cancellationToken)
    {
        if (viewerUserId is not { } userId || await this.userRepository.GetByIdAsync(userId, cancellationToken) is not { } user)
        {
            return (ReviewEligibility.SignedOut, null);
        }

        if (player.UserId == user.Id)
        {
            return (ReviewEligibility.OwnProfile, null);
        }

        // A ghost may still have written a review before turning ghost mode on; show it, but read-only.
        PlayerReviewItem? mine = null;
        if (await this.profileRepository.GetCustomerByUserIdAsync(user.Id, cancellationToken) is { } customer
            && await this.reviewRepository.GetAsync(player.Id, customer.Id, cancellationToken) is { } review)
        {
            mine = ToItem(review, customer);
        }

        return (user.IsGhostMode ? ReviewEligibility.GhostMode : ReviewEligibility.CanReview, mine);
    }
}
