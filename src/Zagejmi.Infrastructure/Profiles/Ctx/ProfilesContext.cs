using Microsoft.EntityFrameworkCore;

using Zagejmi.Domain.Profiles.Players;
using Zagejmi.Infrastructure.Profiles.Records;

namespace Zagejmi.Infrastructure.Profiles.Ctx;

/// <summary>
///     The DbContext for the application.
/// </summary>
public class ProfilesContext : DbContext
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ProfilesContext" /> class with the specified options.
    /// </summary>
    /// <param name="options">
    ///     The options to be used by the DbContext, typically including the connection string and other
    ///     configuration settings for the database.
    /// </param>
    public ProfilesContext(DbContextOptions<ProfilesContext> options)
        : base(options)
    {
    }

    /// <summary>
    ///     Gets or sets player profile records.
    /// </summary>
    public DbSet<PlayerProfileRecord> Players { get; set; }

    /// <summary>
    ///     Gets or sets customer profile records.
    /// </summary>
    public DbSet<CustomerProfileRecord> Customers { get; set; }

    /// <summary>
    ///     Gets or sets subscription records.
    /// </summary>
    public DbSet<SubscriptionRecord> Subscriptions { get; set; }

    /// <summary>
    ///     Gets or sets the services offered by players.
    /// </summary>
    public DbSet<PlayerServiceRecord> PlayerServices { get; set; }

    /// <summary>
    ///     Gets or sets the users who own the profiles (read-only; the table belongs to the users context).
    /// </summary>
    public DbSet<ProfileOwnerRecord> ProfileOwners { get; set; }

    /// <summary>
    ///     Configures the model for the DbContext.
    /// </summary>
    /// <param name="modelBuilder">The ModelBuilder used to configure the model for the DbContext.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("profiles");

        modelBuilder.Entity<CustomerProfileRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasIndex(e => e.DisplayName).IsUnique();

            // A user can own at most one customer profile.
            entity.HasIndex(e => e.UserId).IsUnique();
        });

        modelBuilder.Entity<PlayerProfileRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasIndex(e => e.DisplayName).IsUnique();

            // A user can own at most one player profile.
            entity.HasIndex(e => e.UserId).IsUnique();
        });

        modelBuilder.Entity<PlayerServiceRecord>(entity =>
        {
            entity.ToTable("PlayerServices");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.Name).HasMaxLength(PlayerService.NameMaxLength).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(PlayerService.DescriptionMaxLength).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(PlayerService.CategoryMaxLength);
            entity.Property(e => e.PriceUnit).HasMaxLength(PlayerService.PriceUnitMaxLength);
            entity.Property(e => e.PromoTag).HasMaxLength(50);
            entity.Property(e => e.GameKey).HasMaxLength(PlayerService.GameKeyMaxLength);
            entity.Property(e => e.RankKey).HasMaxLength(PlayerService.GameKeyMaxLength);

            // Services disappear together with the player profile.
            entity.HasOne<PlayerProfileRecord>()
                .WithMany()
                .HasForeignKey(e => e.PlayerProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.PlayerProfileId, e.CreatedAt });

            // The Služby page filters by game.
            entity.HasIndex(e => e.GameKey);
        });

        modelBuilder.Entity<ProfileOwnerRecord>(entity =>
        {
            entity.ToTable("Users", "users", table => table.ExcludeFromMigrations());
            entity.HasKey(e => e.Id);

            // Same text conversion as in ZagejmiUsersContext.
            entity.Property(e => e.Gender).HasConversion<string>().HasMaxLength(16);
        });

        modelBuilder.Entity<SubscriptionRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        base.OnModelCreating(modelBuilder);
    }
}
