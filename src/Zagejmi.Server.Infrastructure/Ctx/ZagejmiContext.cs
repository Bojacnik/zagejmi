using Microsoft.EntityFrameworkCore;
using Zagejmi.Server.Domain.Community.People;
using Zagejmi.Server.Domain.Events;

namespace Zagejmi.Server.Infrastructure.Ctx;

public class ZagejmiContext(DbContextOptions<ZagejmiContext> options) : DbContext(options) // Or IdentityDbContext, etc.
{
    public DbSet<IDomainEvent<Person, Guid>> OutboxEvents { get; set; }
}