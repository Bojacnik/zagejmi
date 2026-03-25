using MassTransit;
using Microsoft.EntityFrameworkCore;
using Zagejmi.Write.Infrastructure.Models;
using Zagejmi.Write.Infrastructure.Projections;

namespace Zagejmi.Write.Infrastructure.Ctx;

/// <summary>
/// The DbContext for the application.
/// </summary>
public class ZagejmiContext(DbContextOptions<ZagejmiContext> options) : DbContext(options)
{
    // This DbSet will create the 'StoredEvents' table in your database.
    public DbSet<StoredEvent> StoredEvents { get; set; }
    public DbSet<UserProjection> UserProjections { get; set; }
    public DbSet<PersonProjection> PersonProjections { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserProjection>()
            .HasIndex(u => u.UserName)
            .IsUnique();

        modelBuilder
            .Entity<UserProjection>()
            .HasIndex(u => u.Email).IsUnique();

        modelBuilder.Entity<PersonProjection>()
            .HasIndex(p => p.UserId)
            .IsUnique();

        // This is the critical configuration that was missing.
        // It tells Entity Framework to include the tables necessary for the MassTransit Outbox.
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}