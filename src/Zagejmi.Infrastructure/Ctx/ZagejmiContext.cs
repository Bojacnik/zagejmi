using Microsoft.EntityFrameworkCore;
using SharedKernel.Outbox;
using Zagejmi.Infrastructure.Models;

namespace Zagejmi.Infrastructure.Ctx;

public class ZagejmiContext : DbContext // Or IdentityDbContext, etc.
{
    // Your existing DbSets for other entities
    public DbSet<ModelPerson> People { get; set; }
    // ... other entities

    // --- Add this line ---
    public DbSet<OutboxEvent> OutboxEvents { get; set; }

    public ZagejmiContext(DbContextOptions<ZagejmiContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Your existing configurations
    }
}