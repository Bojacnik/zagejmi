using System;
using System.Threading;
using System.Threading.Tasks;

using Zagejmi.Application.Auth.Users.Abstractions;
using Zagejmi.Application.Orders.Abstractions;
using Zagejmi.Application.Profiles.Abstractions;
using Zagejmi.Domain.Auth;
using Zagejmi.Domain.Orders;
using Zagejmi.Domain.Profiles.Customers;
using Zagejmi.Shared;

namespace Zagejmi.Application.Profiles.Reviews;

/// <summary>
///     Who may review an order: the customer who placed it, not in ghost mode, once it is completed and the service was
///     actually provided.
/// </summary>
internal static class ServiceReviewAccess
{
    /// <summary>
    ///     The order cannot be reviewed (yet).
    /// </summary>
    public static readonly ErrorDetail NotReviewable = ErrorDetail.Validation(
        "Review.OrderNotCompleted",
        "Recenzi můžete napsat, až bude objednávka dokončená a služba proběhne.");

    /// <summary>
    ///     Loads the author's customer profile and the order, and checks the user may review it.
    /// </summary>
    /// <param name="users">The user repository.</param>
    /// <param name="profiles">The profile repository.</param>
    /// <param name="orders">The order repository.</param>
    /// <param name="userId">The signed-in user.</param>
    /// <param name="orderId">The order.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The author and the order, or why the user may not review it.</returns>
    public static async Task<Result<(CustomerProfile Author, Order Order)>> ResolveAsync(
        IUserRepository users,
        IProfileRepository profiles,
        IOrderRepository orders,
        Guid userId,
        Guid orderId,
        CancellationToken cancellationToken)
    {
        User? user = await users.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result<(CustomerProfile, Order)>.Failure(ErrorDetail.NotFound(nameof(User), userId));
        }

        if (user.IsGhostMode)
        {
            return Result<(CustomerProfile, Order)>.Failure(ProfileErrors.GhostMode);
        }

        CustomerProfile? author = await profiles.GetCustomerByUserIdAsync(user.Id, cancellationToken);
        Order? order = await orders.GetAsync(orderId, cancellationToken);
        if (author is null || order is null || order.CustomerProfileId != author.Id)
        {
            return Result<(CustomerProfile, Order)>.Failure(ErrorDetail.NotFound(nameof(Order), orderId));
        }

        return order.UnlocksReview
            ? (author, order)
            : Result<(CustomerProfile, Order)>.Failure(NotReviewable);
    }
}
