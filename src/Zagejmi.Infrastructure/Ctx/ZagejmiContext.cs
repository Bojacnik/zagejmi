using Microsoft.EntityFrameworkCore;

namespace Zagejmi.Infrastructure.Ctx;

/// <summary>
///     The DbContext for the application.
/// </summary>
public class ZagejmiContext : DbContext
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ZagejmiContext" /> class with the specified options.
    /// </summary>
    /// <param name="options">
    ///     The options to be used by the DbContext, typically including the connection string and other
    ///     configuration settings for the database.
    /// </param>
    public ZagejmiContext(DbContextOptions<ZagejmiContext> options)
        : base(options)
    {
    }

    /// <summary>
    ///     Gets or sets the DbSet representing the stored events in the event store. This is where domain events are persisted
    ///     when using
    ///     event sourcing. Each event is stored as a record in the database, allowing for the reconstruction of aggregate
    ///     state by replaying these events. The configuration for this DbSet is critical for ensuring that the
    ///     Outbox pattern works correctly, as it relies on the presence of the necessary tables to manage the outbox messages
    ///     and states for reliable message delivery.
    /// </summary>
    public DbSet<StoredEvent> StoredEvents { get; set; }

    /// <summary>
    ///     Gets or sets persisted refresh tokens used for rotation/revocation checks.
    /// </summary>
    public DbSet<RefreshTokenRecord> RefreshTokens { get; set; }

    /// <summary>
    ///     Gets or sets revoked JWT IDs for access-token blacklist checks.
    /// </summary>
    public DbSet<RevokedAccessTokenRecord> RevokedAccessTokens { get; set; }

    /// <summary>
    ///     Gets or sets image blobs stored as bytea.
    /// </summary>
    public DbSet<StoredImage> ImageBlobs { get; set; }

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
        modelBuilder.HasDefaultSchema("write");

        modelBuilder.Entity<StoredEvent>(entity =>
        {
            entity.Property(x => x.Id)
                .IsRequired()
                .HasColumnOrder(0);

            entity.Property(x => x.AggregateId)
                .IsRequired()
                .HasColumnOrder(1);

            entity.Property(x => x.Timestamp)
                .IsRequired()
                .HasColumnType("timestamp without time zone")
                .HasColumnOrder(2);

            entity.Property(x => x.EventType)
                .IsRequired()
                .HasMaxLength(1000)
                .HasColumnOrder(3);

            entity.Property(x => x.Data)
                .IsRequired()
                .HasMaxLength(-1)
                .HasColumnOrder(4);
        });

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

            entity.HasIndex(x => x.ExpiresAtUtc);
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

            entity.HasIndex(x => x.ExpiresAtUtc);
        });

        modelBuilder.Entity<StoredImage>(entity =>
        {
            entity.HasKey(x => x.BlobKey);

            entity.Property(x => x.BlobKey)
                .HasMaxLength(256);

            entity.Property(x => x.Data)
                .IsRequired()
                .HasColumnType("bytea");

            entity.Property(x => x.ContentType)
                .IsRequired()
                .HasMaxLength(128);

            entity.Property(x => x.CreatedAtUtc)
                .HasColumnType("timestamp without time zone");
        });

        base.OnModelCreating(modelBuilder);
    }
}