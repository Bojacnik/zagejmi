using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Zagejmi.Application.Abstractions;
using Zagejmi.Application.Orders.Abstractions;
using Zagejmi.Application.Profiles;
using Zagejmi.Application.Profiles.Abstractions;
using Zagejmi.Domain.Orders;
using Zagejmi.Shared;

namespace Zagejmi.Application.Orders.GetOrders;

/// <summary>
///     Lists the signed-in user's orders on one side.
/// </summary>
/// <param name="UserId">The signed-in user.</param>
/// <param name="Role">
///     <see cref="OrderRole.Customer" />: what the user ordered; <see cref="OrderRole.Player" />: orders of their services.
/// </param>
/// <param name="Page">1-based page.</param>
/// <param name="PageSize">Orders per page.</param>
public sealed record GetOrdersQuery(Guid UserId, OrderRole Role, int Page = 1, int PageSize = 20)
{
    /// <summary>
    ///     The largest page.
    /// </summary>
    public const int MaxPageSize = 50;
}

/// <summary>
///     Gets a page of the user's orders with both parties' names and the actions available to the user.
/// </summary>
public sealed class GetOrdersHandler : IHandlerRequest<GetOrdersQuery, Result<PagedResult<OrderView>>>
{
    private readonly IProfileRepository profileRepository;
    private readonly IOrderRepository orderRepository;
    private readonly TimeProvider clock;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GetOrdersHandler" /> class.
    /// </summary>
    /// <param name="profileRepository">The profile repository.</param>
    /// <param name="orderRepository">The order repository.</param>
    /// <param name="clock">The clock.</param>
    public GetOrdersHandler(IProfileRepository profileRepository, IOrderRepository orderRepository, TimeProvider clock)
    {
        this.profileRepository = profileRepository;
        this.orderRepository = orderRepository;
        this.clock = clock;
    }

    /// <inheritdoc />
    public async Task<Result<PagedResult<OrderView>>> Handle(GetOrdersQuery request, CancellationToken cancellationToken = default)
    {
        int page = Math.Max(1, request.Page);
        int pageSize = Math.Clamp(request.PageSize, 1, GetOrdersQuery.MaxPageSize);
        int skip = (page - 1) * pageSize;

        PagedResult<Order> orders;
        if (request.Role == OrderRole.Customer)
        {
            Guid? customerId = (await this.profileRepository.GetCustomerByUserIdAsync(request.UserId, cancellationToken))?.Id;
            orders = customerId is { } id
                ? await this.orderRepository.ListForCustomerAsync(id, skip, pageSize, cancellationToken)
                : new PagedResult<Order>([], 0);
        }
        else
        {
            Guid? playerId = (await this.profileRepository.GetPlayerByUserIdAsync(request.UserId, cancellationToken))?.Id;
            orders = playerId is { } id
                ? await this.orderRepository.ListForPlayerAsync(id, skip, pageSize, cancellationToken)
                : new PagedResult<Order>([], 0);
        }

        IReadOnlyList<OrderView> views = await ToViewsAsync(
            this.profileRepository,
            orders.Items,
            o => OrderActions.Available(o, request.Role, this.clock.GetUtcNow()),
            cancellationToken);
        return new PagedResult<OrderView>(views, orders.TotalCount);
    }

    /// <summary>
    ///     Adds the parties' names and pictures to orders.
    /// </summary>
    /// <param name="profiles">The profile repository.</param>
    /// <param name="orders">The orders.</param>
    /// <param name="actions">The actions to offer per order.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The views, in the same order.</returns>
    internal static async Task<IReadOnlyList<OrderView>> ToViewsAsync(
        IProfileRepository profiles,
        IReadOnlyList<Order> orders,
        Func<Order, IReadOnlyList<OrderAction>> actions,
        CancellationToken cancellationToken)
    {
        if (orders.Count == 0)
        {
            return [];
        }

        IReadOnlyDictionary<Guid, ProfileCard> cards = await profiles.GetProfileCardsAsync(
            orders.SelectMany(o => new[] { o.CustomerProfileId, o.PlayerProfileId }).Distinct().ToList(),
            cancellationToken);
        ProfileCard Card(Guid id) => cards.GetValueOrDefault(id) ?? ProfileCard.Deleted(id);

        return [.. orders.Select(o => new OrderView(o, Card(o.CustomerProfileId), Card(o.PlayerProfileId), actions(o)))];
    }
}
