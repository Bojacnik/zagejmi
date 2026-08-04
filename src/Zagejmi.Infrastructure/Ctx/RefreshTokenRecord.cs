using System;

namespace Zagejmi.Infrastructure.Ctx;

/// <summary>
///     Persisted refresh token state used for rotation and revocation.
/// </summary>
public sealed record RefreshTokenRecord
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid TokenFamilyId { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset ExpiresAtUtc { get; set; }

    public DateTimeOffset? RevokedAtUtc { get; set; }

    public Guid? ReplacedByTokenId { get; set; }

    public string? RevocationReason { get; set; }
}