using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Zagejmi.Application.Auth.Tokens;
using Zagejmi.Infrastructure.Auth.AccessTokens.Ctx;
using Zagejmi.Infrastructure.Auth.AccessTokens.Records;

namespace Zagejmi.Infrastructure.Auth.AccessTokens.Repositories;

/// <summary>
///     EF Core implementation of <see cref="IRefreshTokenRepository" />.
/// </summary>
public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IDbContextFactory<AccessTokensContext> contextFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RefreshTokenRepository" /> class.
    /// </summary>
    /// <param name="contextFactory">Creates a short-lived database context per operation, so concurrent calls on one Blazor circuit never share a DbContext.</param>
    public RefreshTokenRepository(IDbContextFactory<AccessTokensContext> contextFactory)
    {
        this.contextFactory = contextFactory;
    }

    /// <inheritdoc />
    public async Task AddAsync(
        Guid userId,
        string tokenHash,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken = default)
    {
        await using AccessTokensContext context = await this.contextFactory.CreateDbContextAsync(cancellationToken);

        context.RefreshTokens.Add(new RefreshTokenRecord
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            TokenFamilyId = Guid.CreateVersion7(),
            TokenHash = tokenHash,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = expiresAt,
        });

        await context.SaveChangesAsync(cancellationToken);
    }
}
