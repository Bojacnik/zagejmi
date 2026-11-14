using System;
using System.Threading;
using System.Threading.Tasks;

using Zagejmi.Application.Abstractions;
using Zagejmi.Application.Auth.Users.Abstractions;
using Zagejmi.Application.Profiles.Abstractions;
using Zagejmi.Application.Profiles.Reviews.Abstractions;
using Zagejmi.Domain.Profiles.Customers;
using Zagejmi.Domain.Profiles.Players;
using Zagejmi.Shared;

namespace Zagejmi.Application.Profiles.Reviews.DeletePlayerReview;

/// <summary>
///     Removes the customer's own review of a player. Only the author can remove a review; the player cannot.
/// </summary>
public sealed class DeletePlayerReviewHandler : IHandlerRequest<DeletePlayerReviewCommand, Result>
{
    private readonly IUserRepository userRepository;
    private readonly IProfileRepository profileRepository;
    private readonly IPlayerReviewRepository reviewRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeletePlayerReviewHandler" /> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="profileRepository">The profile repository.</param>
    /// <param name="reviewRepository">The review repository.</param>
    public DeletePlayerReviewHandler(
        IUserRepository userRepository,
        IProfileRepository profileRepository,
        IPlayerReviewRepository reviewRepository)
    {
        this.userRepository = userRepository;
        this.profileRepository = profileRepository;
        this.reviewRepository = reviewRepository;
    }

    /// <inheritdoc />
    public async Task<Result> Handle(DeletePlayerReviewCommand request, CancellationToken cancellationToken = default)
    {
        Result<(CustomerProfile Author, PlayerProfile Player)> parties = await ReviewAccess.ResolveAsync(
            this.userRepository,
            this.profileRepository,
            request.UserId,
            request.PlayerProfileId,
            cancellationToken);
        if (parties.IsFailure)
        {
            return Result.Failure(parties.Error!);
        }

        bool removed = await this.reviewRepository.DeleteAsync(
            parties.Value.Player.Id,
            parties.Value.Author.Id,
            cancellationToken);

        return removed
            ? Result.Success()
            : Result.Failure(ErrorDetail.NotFound(nameof(PlayerReview), request.PlayerProfileId));
    }
}
