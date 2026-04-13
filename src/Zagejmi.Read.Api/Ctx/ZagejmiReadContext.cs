using Microsoft.EntityFrameworkCore;

using Zagejmi.Read.Api.Models;

namespace Zagejmi.Read.Api.Ctx;

/// <summary>
///     Entity Framework context for read-side projections.
/// </summary>
public class ZagejmiReadContext : DbContext
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ZagejmiReadContext" /> class.
    /// </summary>
    /// <param name="options">Context options.</param>
    public ZagejmiReadContext(DbContextOptions<ZagejmiReadContext> options)
        : base(options)
    {
    }

    /// <summary>
    ///     Gets or sets users projection set.
    /// </summary>
    public DbSet<UserReadModel> Users { get; set; } = null!;

    /// <summary>
    ///     Gets or sets user profiles projection set.
    /// </summary>
    public DbSet<UserProfileProjection> UserProfiles { get; set; } = null!;

    /// <summary>
    ///     Gets or sets projection checkpoint records.
    /// </summary>
    public DbSet<ProjectionCheckpoint> ProjectionCheckpoints { get; set; } = null!;

    /// <summary>
    ///     Gets or sets stored events used for projection replay.
    /// </summary>
    public DbSet<StoredEventReadModel> StoredEvents { get; set; } = null!;

    /// <summary>
    ///     Configures read-side entity mappings.
    /// </summary>
    /// <param name="modelBuilder">Model builder instance.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("read");

        modelBuilder.Entity<UserReadModel>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.UserName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(320);

            entity.Property(x => x.PasswordHash)
                .IsRequired();

            entity.HasIndex(x => x.UserName)
                .IsUnique();

            entity.HasIndex(x => x.Email)
                .IsUnique();
        });

        modelBuilder.Entity<UserProfileProjection>(entity =>
        {
            entity.ToTable("UserProfiles");
            entity.HasKey(x => x.ProfileId);

            entity.Property(x => x.ProfileType)
                .IsRequired()
                .HasMaxLength(32);

            entity.Property(x => x.DisplayName)
                .IsRequired()
                .HasMaxLength(64);

            entity.Property(x => x.ProfileImageBlobKey)
                .HasMaxLength(256);

            entity.HasIndex(x => x.UserId);
        });

        modelBuilder.Entity<ProjectionCheckpoint>(entity =>
        {
            entity.ToTable("ProjectionCheckpoints");
            entity.HasKey(x => x.ProjectorName);

            entity.Property(x => x.ProjectorName)
                .HasMaxLength(128)
                .IsRequired();
        });

        modelBuilder.Entity<StoredEventReadModel>(entity =>
        {
            entity.ToTable("StoredEvents", "write", table => table.ExcludeFromMigrations());
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EventType).HasMaxLength(1000);
        });

        base.OnModelCreating(modelBuilder);
    }
}