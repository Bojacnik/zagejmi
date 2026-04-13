using System;

namespace Zagejmi.Write.Infrastructure.Ctx;

/// <summary>
///     Persisted blacklist entry for revoked access token JWT IDs.
/// </summary>
public sealed class RevokedAccessTokenRecord
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string JwtId { get; set; } = string.Empty;

    public DateTimeOffset RevokedAtUtc { get; set; }

    public DateTimeOffset ExpiresAtUtc { get; set; }

    public string? Reason { get; set; }
}