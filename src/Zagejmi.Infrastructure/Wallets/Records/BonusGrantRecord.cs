using System;

namespace Zagejmi.Infrastructure.Wallets.Records;

/// <summary>
///     Database row of a bonus grant (table <c>wallets.BonusGrants</c>).
/// </summary>
public record BonusGrantRecord
{
    /// <summary>
    ///     Gets the grant ID.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    ///     Gets the receiving customer profile.
    /// </summary>
    public Guid CustomerProfileId { get; init; }

    /// <summary>
    ///     Gets the granted amount.
    /// </summary>
    public long Amount { get; init; }

    /// <summary>
    ///     Gets or sets the unspent, unexpired amount (0 to <see cref="Amount" />).
    /// </summary>
    public long Remaining { get; set; }

    /// <summary>
    ///     Gets why the bonus was granted.
    /// </summary>
    public required string Reason { get; init; }

    /// <summary>
    ///     Gets when it was granted.
    /// </summary>
    public DateTimeOffset GrantedAt { get; init; }

    /// <summary>
    ///     Gets or sets when the rest expires.
    /// </summary>
    public DateTimeOffset ExpiresAt { get; set; }
}
