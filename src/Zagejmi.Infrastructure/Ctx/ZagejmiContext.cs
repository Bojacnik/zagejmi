using Microsoft.EntityFrameworkCore;
using Zagejmi.Domain;
using Zagejmi.Infrastructure.Models;

namespace Zagejmi.Infrastructure.Ctx;

public class ZagejmiContext(DbContextOptions<ZagejmiContext> options) : DbContext(options) // Or IdentityDbContext, etc.
{
    public DbSet<ModelPerson> People { get; set; }
    public DbSet<IDomainEvent> OutboxEvents { get; set; }
}