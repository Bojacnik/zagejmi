using Microsoft.EntityFrameworkCore;

using Zagejmi.Infrastructure.Auth.AccessTokens.Records;

namespace Zagejmi.Infrastructure.Auth.AccessTokens.Ctx;

/// <summary>
///     The DbContext for the application.
/// </summary>
public class AccessTokensContext : DbContext
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AccessTokensContext" /> class with the specified options.
    /// </summary>
    /// <param name="options">
    ///     The options to be used by the DbContext, typically including the connection string and other
    ///     configuration settings for the database.
    /// </param>
    public AccessTokensContext(DbContextOptions<AccessTokensContext> options)
        : base(options)
    {
    }

    /// <summary>
    ///     Gets or sets persisted refresh tokens used for rotation/revocation checks.
    /// </summary>
    public DbSet<RefreshTokenRecord> RefreshTokens { get; set; }

    /// <summary>
    ///     Gets or sets revoked JWT IDs for access-token blacklist checks.
    /// </summary>
    public DbSet<RevokedAccessTokenRecord> RevokedAccessTokens { get; set; }


    /// <summary>
    ///     Configures the model for the DbContext, including the necessary configurations for the Outbox pattern.
    ///     This method is called by the Entity Framework when the model is being created. It is essential to include the
    ///     configurations
    ///     for the Outbox entities here to ensure that the necessary tables are created in the database for managing the
    ///     outbox messages and states. Without this configuration, the Outbox pattern will not function correctly, leading to
    ///     issues with message delivery and reliability in the system.
    /// </summary>
    /// <param name="modelBuilder">The ModelBuilder used to configure the model for the DbContext.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("access_tokens");

        modelBuilder.Entity<RefreshTokenRecord>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.TokenHash)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(x => x.RevocationReason)
                .HasMaxLength(256);

            entity.HasIndex(x => x.TokenHash)
                .IsUnique();

            entity.HasIndex(x => new { x.UserId, x.TokenFamilyId });

            entity.HasIndex(x => x.ExpiresAt);
        });

        modelBuilder.Entity<RevokedAccessTokenRecord>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.JwtId)
                .IsRequired()
                .HasMaxLength(128);

            entity.Property(x => x.Reason)
                .HasMaxLength(256);

            entity.HasIndex(x => x.JwtId)
                .IsUnique();

            entity.HasIndex(x => x.UserId);

            entity.HasIndex(x => x.ExpiresAt);
        });

        base.OnModelCreating(modelBuilder);
    }
}