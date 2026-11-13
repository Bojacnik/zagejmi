using System;

namespace Zagejmi.Application.Wallets.GrantReward;

/// <summary>
///     Grants bonus Goins to a Zákazník wallet. Internal only: there is no endpoint for it, the platform grants rewards
///     itself (e.g. the sign-up bonus).
/// </summary>
public sealed record GrantRewardCommand
{
    /// <summary>
    ///     Gets the receiving customer profile.
    /// </summary>
    public required Guid CustomerProfileId { get; init; }

    /// <summary>
    ///     Gets the number of bonus Goins.
    /// </summary>
    public required long Amount { get; init; }

    /// <summary>
    ///     Gets why the bonus is granted, shown in the wallet history (e.g. „Bonus za registraci“).
    /// </summary>
    public required string Reason { get; init; }

    /// <summary>
    ///     Gets a key that identifies this reward, so it is never granted twice (e.g. <c>signup-bonus:{userId}</c>).
    /// </summary>
    public required string IdempotencyKey { get; init; }
}
