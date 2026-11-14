using System;
using System.Threading;
using System.Threading.Tasks;

using Zagejmi.Application.Abstractions;
using Zagejmi.Application.Auth.Users.Abstractions;
using Zagejmi.Application.Orders.Abstractions;
using Zagejmi.Application.Profiles.Abstractions;
using Zagejmi.Application.Profiles.Reviews.Abstractions;
using Zagejmi.Domain.Orders;
using Zagejmi.Domain.Profiles.Customers;
using Zagejmi.Domain.Profiles.Players;
using Zagejmi.Shared;

namespace Zagejmi.Application.Profiles.Reviews.WriteServiceReview;

/// <summary>
///     A customer reviews one of their completed orders.
/// </summary>
/// <param name="UserId">The signed-in user.</param>
/// <param name="OrderId">The completed order.</param>
/// <param name="Rating">1–5 stars.</param>
/// <param name="Comment">The optional text.</param>
public sealed record WriteServiceReviewCommand(Guid UserId, Guid OrderId, int Rating, string? Comment);

/// <summary>
///     Writes or replaces the review of a completed order. Each completed order where the service was provided can be
///     reviewed once (and edited later).
/// </summary>
public sealed class WriteServiceReviewHandler : IHandlerRequest<WriteServiceReviewCommand, Result>
{
    private readonly IUserRepository userRepository;
    private readonly IProfileRepository profileRepository;
    private readonly IOrderRepository orderRepository;
    private readonly IServiceReviewRepository reviewRepository;
    private readonly TimeProvider clock;

    /// <summary>
    ///     Initializes a new instance of the <see cref="WriteServiceReviewHandler" /> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="profileRepository">The profile repository.</param>
    /// <param name="orderRepository">The order repository.</param>
    /// <param name="reviewRepository">The service review repository.</param>
    /// <param name="clock">The clock.</param>
    public WriteServiceReviewHandler(
        IUserRepository userRepository,
        IProfileRepository profileRepository,
        IOrderRepository orderRepository,
        IServiceReviewRepository reviewRepository,
        TimeProvider clock)
    {
        this.userRepository = userRepository;
        this.profileRepository = profileRepository;
        this.orderRepository = orderRepository;
        this.reviewRepository = reviewRepository;
        this.clock = clock;
    }

    /// <inheritdoc />
    public async Task<Result> Handle(WriteServiceReviewCommand request, CancellationToken cancellationToken = default)
    {
        Result<string?> comment = ReviewRules.Validate(request.Rating, request.Comment);
        if (comment.IsFailure)
        {
            return Result.Failure(comment.Error!);
        }

        Result<(CustomerProfile Author, Order Order)> access = await ServiceReviewAccess.ResolveAsync(
            this.userRepository,
            this.profileRepository,
            this.orderRepository,
            request.UserId,
            request.OrderId,
            cancellationToken);
        if (access.IsFailure)
        {
            return Result.Failure(access.Error!);
        }

        (CustomerProfile author, Order order) = access.Value;
        DateTimeOffset now = this.clock.GetUtcNow();
        await this.reviewRepository.UpsertAsync(
            new ServiceReview
            {
                Id = Guid.CreateVersion7(),
                OrderId = order.Id,
                ServiceId = order.ServiceId,
                ServiceName = order.ServiceName,
                PlayerProfileId = order.PlayerProfileId,
                AuthorProfileId = author.Id,
                Rating = request.Rating,
                Comment = comment.Value,
                CreatedAt = now,
                UpdatedAt = now,
            },
            cancellationToken);
        return Result.Success();
    }
}
