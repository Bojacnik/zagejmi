using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Zagejmi.Application.Abstractions;
using Zagejmi.Application.Admin;
using Zagejmi.Application.Auth.Users.Abstractions;
using Zagejmi.Application.Orders.Abstractions;
using Zagejmi.Application.Orders.GetOrders;
using Zagejmi.Application.Profiles.Abstractions;
using Zagejmi.Domain.Auth;
using Zagejmi.Domain.Orders;
using Zagejmi.Shared;

namespace Zagejmi.Application.Orders.GetDisputedOrders;

/// <summary>
///     Lists the reported orders for an admin, oldest report first.
/// </summary>
public sealed class GetDisputedOrdersHandler : IHandlerRequest<Guid, Result<IReadOnlyList<OrderView>>>
{
    private readonly IUserRepository userRepository;
    private readonly IProfileRepository profileRepository;
    private readonly IOrderRepository orderRepository;
    private readonly AdminOptions admins;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GetDisputedOrdersHandler" /> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="profileRepository">The profile repository.</param>
    /// <param name="orderRepository">The order repository.</param>
    /// <param name="admins">Who the admins are.</param>
    public GetDisputedOrdersHandler(
        IUserRepository userRepository,
        IProfileRepository profileRepository,
        IOrderRepository orderRepository,
        IOptions<AdminOptions> admins)
    {
        this.userRepository = userRepository;
        this.profileRepository = profileRepository;
        this.orderRepository = orderRepository;
        this.admins = admins.Value;
    }

    /// <summary>
    ///     Gets the disputed orders.
    /// </summary>
    /// <param name="request">The signed-in user's ID.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The orders, or a failure when the user is not an admin.</returns>
    public async Task<Result<IReadOnlyList<OrderView>>> Handle(Guid request, CancellationToken cancellationToken = default)
    {
        User? user = await this.userRepository.GetByIdAsync(request, cancellationToken);
        if (user is null || !this.admins.IsAdmin(user))
        {
            return Result<IReadOnlyList<OrderView>>.Failure(
                new ErrorDetail("Admin.Required", "Tohle může jen administrátor.", ErrorType.Forbidden));
        }

        IReadOnlyList<Order> orders = await this.orderRepository.GetDisputedAsync(cancellationToken);
        return Result<IReadOnlyList<OrderView>>.Success(
            await GetOrdersHandler.ToViewsAsync(this.profileRepository, orders, _ => [], cancellationToken));
    }
}
