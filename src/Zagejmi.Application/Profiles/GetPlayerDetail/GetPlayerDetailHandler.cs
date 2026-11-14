using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Zagejmi.Application.Abstractions;
using Zagejmi.Application.Auth.Users.Abstractions;
using Zagejmi.Application.Profiles.Abstractions;
using Zagejmi.Application.Profiles.Reviews;
using Zagejmi.Application.Profiles.Reviews.Abstractions;
using Zagejmi.Domain.Auth;
using Zagejmi.Domain.Profiles.Players;
using Zagejmi.Shared;

namespace Zagejmi.Application.Profiles.GetPlayerDetail;

/// <summary>
///     Returns the public detail of a player profile: names, pictures and services.
/// </summary>
public sealed class GetPlayerDetailHandler : IHandlerRequest<Guid, Result<PlayerDetail>>
{
    private readonly IProfileRepository profileRepository;
    private readonly IUserRepository userRepository;
    private readonly IServiceReviewRepository reviewRepository;
    private readonly ReviewOptions reviewOptions;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GetPlayerDetailHandler" /> class.
    /// </summary>
    /// <param name="profileRepository">The profile repository.</param>
    /// <param name="userRepository">The user repository (for the grammatical gender).</param>
    /// <param name="reviewRepository">The service review repository (averages per service).</param>
    /// <param name="reviewOptions">When an average is shown.</param>
    public GetPlayerDetailHandler(
        IProfileRepository profileRepository,
        IUserRepository userRepository,
        IServiceReviewRepository reviewRepository,
        IOptions<ReviewOptions> reviewOptions)
    {
        this.profileRepository = profileRepository;
        this.userRepository = userRepository;
        this.reviewRepository = reviewRepository;
        this.reviewOptions = reviewOptions.Value;
    }

    /// <summary>
    ///     Loads the player detail.
    /// </summary>
    /// <param name="request">The player profile ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The player detail, or NotFound.</returns>
    public async Task<Result<PlayerDetail>> Handle(Guid request, CancellationToken cancellationToken = default)
    {
        PlayerProfile? player = await this.profileRepository.GetPlayerByIdAsync(request, cancellationToken);
        if (player is null)
        {
            return Result<PlayerDetail>.Failure(ErrorDetail.NotFound("Player", request));
        }

        User? user = await this.userRepository.GetByIdAsync(player.UserId, cancellationToken);
        IReadOnlyList<PlayerService> services =
            await this.profileRepository.GetPlayerServicesAsync(player.Id, cancellationToken);
        IReadOnlyDictionary<Guid, RatingSummary> ratings = services.Count == 0
            ? new Dictionary<Guid, RatingSummary>()
            : await this.reviewRepository.GetSummariesAsync([.. services.Select(s => s.Id)], cancellationToken);

        return new PlayerDetail(
            player.Id,
            player.DisplayName,
            player.Description,
            player.CreatedAt,
            user?.GrammaticalGender ?? GrammaticalGender.Masculine,
            player.CardProfilePicture,
            player.BannerPicture,
            services
                .Select(s => new PlayerServiceItem(
                    s.Id,
                    s.Name,
                    s.Description,
                    s.Price,
                    s.PriceUnit,
                    s.Category,
                    s.GameKey,
                    s.RankKey,
                    s.PlayStyles,
                    s.ImageId,
                    s.IsAvailable,
                    this.reviewOptions.Visible(ratings.GetValueOrDefault(s.Id))))
                .ToList());
    }
}
