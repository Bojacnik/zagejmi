using System;

using Zagejmi.Domain.Auth;

namespace Zagejmi.Infrastructure.Profiles.Records;

/// <summary>
///     Read-only view of the user who owns a profile (table <c>users.Users</c>, owned by the users context and excluded
///     from the profiles migrations). Lets the Hráči and Služby queries split by gender in SQL.
/// </summary>
public sealed record ProfileOwnerRecord
{
    /// <summary>
    ///     Gets the user ID.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    ///     Gets the gender the user chose at registration.
    /// </summary>
    public Gender Gender { get; init; }
}
