using Microsoft.EntityFrameworkCore;

using Zagejmi.Infrastructure.Auth.Users.Records;

namespace Zagejmi.Infrastructure.Auth.Users.Ctx;

/// <summary>
///     Represents the context for Zagejmi users, providing access to user-related data and operations.
/// </summary>
public class ZagejmiUsersContext : DbContext
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ZagejmiUsersContext" /> class with the specified options.
    /// </summary>
    /// <param name="options">
    ///     The options to be used by the DbContext, typically including the connection string and other
    ///     configuration settings for the database.
    /// </param>
    public ZagejmiUsersContext(DbContextOptions<ZagejmiUsersContext> options)
        : base(options)
    {
    }

    /// <summary>
    ///     Gets the DbSet of user records, representing the users in the application.
    /// </summary>
    public DbSet<UserRecord> Users => this.Set<UserRecord>();

    /// <summary>
    ///     Configures the model for the DbContext, including the necessary configurations for the user entities.
    /// </summary>
    /// <param name="modelBuilder">The ModelBuilder used to configure the model for the DbContext.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("users");

        modelBuilder.Entity<UserRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .IsRequired();

            entity.Property(x => x.Email)
                .HasMaxLength(320)
                .IsRequired();

            entity.HasIndex(x => x.Email)
                .IsUnique();

            entity.Property(x => x.UserName)
                .HasMaxLength(64)
                .IsRequired();

            entity.HasIndex(x => x.UserName)
                .IsUnique();

            entity.Property(x => x.PasswordHash)
                .HasMaxLength(512)
                .IsRequired();

            // Stored as text so the values stay readable in the database and survive enum reordering.
            entity.Property(x => x.Gender)
                .HasConversion<string>()
                .HasMaxLength(16);

            entity.Property(x => x.GrammaticalGender)
                .HasConversion<string>()
                .HasMaxLength(16);

            entity.Property(x => x.DefaultProfile)
                .HasConversion<string>()
                .HasMaxLength(16);
        });

        base.OnModelCreating(modelBuilder);
    }
}
