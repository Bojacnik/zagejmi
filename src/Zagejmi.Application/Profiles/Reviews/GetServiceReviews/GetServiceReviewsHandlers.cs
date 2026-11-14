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

namespace Zagejmi.Application.Profiles.Reviews.GetServiceReviews;

/// <summary>
///     Reads the reviews of a player's services.
/// </summary>
/// <param name="PlayerProfileId">The player.</param>
/// <param name="Page">1-based page.</param>
/// <param name="PageSize">Reviews per page.</param>
public sealed record GetServiceReviewsQuery(Guid PlayerProfileId, int Page = 1, int PageSize = 10)
{
    /// <summary>
    ///     The largest page.
    /// </summary>
    public const int MaxPageSize = 50;
}

/// <summary>
///     Reads the signed-in customer's review of one of their orders (to edit it).
/// </summary>
/// <param name="UserId">The signed-in user.</param>
/// <param name="OrderId">The order.</param>
public sealed record GetOrderReviewQuery(Guid UserId, Guid OrderId);

/// <summary>
///     Gets a page of the reviews of a player's services, newest first. Shown on the player's profile to everyone.
/// </summary>
public sealed class GetServiceReviewsHandler : IHandlerRequest<GetServiceReviewsQuery, PagedResult<ServiceReviewItem>>
{
    private readonly IServiceReviewRepository reviewRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GetServiceReviewsHandler" /> class.
    /// </summary>
    /// <param name="reviewRepository">The service review repository.</param>
    public GetServiceReviewsHandler(IServiceReviewRepository reviewRepository)
    {
        this.reviewRepository = reviewRepository;
    }

    /// <inheritdoc />
    public Task<PagedResult<ServiceReviewItem>> Handle(GetServiceReviewsQuery request, CancellationToken cancellationToken = default)
    {
        int page = Math.Max(1, request.Page);
        int pageSize = Math.Clamp(request.PageSize, 1, GetServiceReviewsQuery.MaxPageSize);
        return this.reviewRepository.GetForPlayerAsync(request.PlayerProfileId, (page - 1) * pageSize, pageSize, cancellationToken);
    }
}

/// <summary>
///     Gets the customer's existing review of an order, after checking they may review it.
/// </summary>
public sealed class GetOrderReviewHandler : IHandlerRequest<GetOrderReviewQuery, Result<ServiceReview?>>
{
    private readonly IUserRepository userRepository;
    private readonly IProfileRepository profileRepository;
    private readonly IOrderRepository orderRepository;
    private readonly IServiceReviewRepository reviewRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GetOrderReviewHandler" /> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="profileRepository">The profile repository.</param>
    /// <param name="orderRepository">The order repository.</param>
    /// <param name="reviewRepository">The service review repository.</param>
    public GetOrderReviewHandler(
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
    public async Task<Result<ServiceReview?>> Handle(GetOrderReviewQuery request, CancellationToken cancellationToken = default)
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
            return Result<ServiceReview?>.Failure(access.Error!);
        }

        return Result<ServiceReview?>.Success(await this.reviewRepository.GetByOrderAsync(request.OrderId, cancellationToken));
    }
}
