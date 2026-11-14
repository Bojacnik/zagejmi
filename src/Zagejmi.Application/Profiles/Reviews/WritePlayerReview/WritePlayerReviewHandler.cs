using System;
using System.Threading;
using System.Threading.Tasks;

using Zagejmi.Application.Abstractions;
using Zagejmi.Application.Auth.Users.Abstractions;
using Zagejmi.Application.Orders.Abstractions;
using Zagejmi.Application.Profiles.Abstractions;
using Zagejmi.Application.Profiles.Reviews.Abstractions;
using Zagejmi.Domain.Auth;
using Zagejmi.Domain.Profiles.Customers;
using Zagejmi.Domain.Profiles.Players;
using Zagejmi.Shared;

namespace Zagejmi.Application.Profiles.Reviews.WritePlayerReview;

/// <summary>
///     Writes or replaces the customer's recommendation (doporučení) of a player. Allowed after at least one completed
///     order with the player where the service was provided; not for the user's own player profile, not for ghosts.
/// </summary>
public sealed class WritePlayerReviewHandler : IHandlerRequest<WritePlayerReviewCommand, Result>
{
    private readonly IUserRepository userRepository;
    private readonly IProfileRepository profileRepository;
    private readonly IPlayerReviewRepository reviewRepository;
    private readonly IOrderRepository orderRepository;
    private readonly TimeProvider clock;

    /// <summary>
    ///     Initializes a new instance of the <see cref="WritePlayerReviewHandler" /> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="profileRepository">The profile repository.</param>
    /// <param name="reviewRepository">The review repository.</param>
    /// <param name="orderRepository">The order repository (a recommendation needs a completed order).</param>
    /// <param name="clock">The clock.</param>
    public WritePlayerReviewHandler(
        IUserRepository userRepository,
        IProfileRepository profileRepository,
        IPlayerReviewRepository reviewRepository,
        IOrderRepository orderRepository,
        TimeProvider clock)
    {
        this.userRepository = userRepository;
        this.profileRepository = profileRepository;
        this.reviewRepository = reviewRepository;
        this.orderRepository = orderRepository;
        this.clock = clock;
    }

    /// <inheritdoc />
    public async Task<Result> Handle(WritePlayerReviewCommand request, CancellationToken cancellationToken = default)
    {
        Result<string?> comment = ReviewRules.Validate(request.Rating, request.Comment);
        if (comment.IsFailure)
        {
            return Result.Failure(comment.Error!);
        }

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

        if (!await this.orderRepository.HasReviewableOrderAsync(
                parties.Value.Author.Id,
                parties.Value.Player.Id,
                cancellationToken))
        {
            return Result.Failure(ReviewAccess.NoCompletedOrder);
        }

        DateTimeOffset now = this.clock.GetUtcNow();
        await this.reviewRepository.UpsertAsync(
            new PlayerReview
            {
                Id = Guid.CreateVersion7(),
                PlayerProfileId = parties.Value.Player.Id,
                AuthorProfileId = parties.Value.Author.Id,
                Rating = request.Rating,
                Comment = comment.Value,
                CreatedAt = now,
                UpdatedAt = now,
            },
            cancellationToken);

        return Result.Success();
    }
}
