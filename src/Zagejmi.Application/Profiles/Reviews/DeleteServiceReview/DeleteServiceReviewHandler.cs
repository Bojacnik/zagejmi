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

namespace Zagejmi.Application.Profiles.Reviews.DeleteServiceReview;

/// <summary>
///     A customer removes the review of one of their orders.
/// </summary>
/// <param name="UserId">The signed-in user.</param>
/// <param name="OrderId">The order.</param>
public sealed record DeleteServiceReviewCommand(Guid UserId, Guid OrderId);

/// <summary>
///     Removes the customer's review of an order. Only the author can remove it; the player cannot.
/// </summary>
public sealed class DeleteServiceReviewHandler : IHandlerRequest<DeleteServiceReviewCommand, Result>
{
    private readonly IUserRepository userRepository;
    private readonly IProfileRepository profileRepository;
    private readonly IOrderRepository orderRepository;
    private readonly IServiceReviewRepository reviewRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeleteServiceReviewHandler" /> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="profileRepository">The profile repository.</param>
    /// <param name="orderRepository">The order repository.</param>
    /// <param name="reviewRepository">The service review repository.</param>
    public DeleteServiceReviewHandler(
        IUserRepository userRepository,
        IProfileRepository profileRepository,
        IOrderRepository orderRepository,
        IServiceReviewRepository reviewRepository)
    {
        this.userRepository = userRepository;
        this.profileRepository = profileRepository;
        this.orderRepository = orderRepository;
        this.reviewRepository = reviewRepository;
    }

    /// <inheritdoc />
    public async Task<Result> Handle(DeleteServiceReviewCommand request, CancellationToken cancellationToken = default)
    {
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

        return await this.reviewRepository.DeleteByOrderAsync(request.OrderId, cancellationToken)
            ? Result.Success()
            : Result.Failure(ErrorDetail.NotFound(nameof(ServiceReview), request.OrderId));
    }
}
