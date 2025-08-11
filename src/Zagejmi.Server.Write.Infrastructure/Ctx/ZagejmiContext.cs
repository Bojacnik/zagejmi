using Microsoft.EntityFrameworkCore;
using Zagejmi.Server.Write.Domain;
using Zagejmi.Server.Write.Domain.Community.People;

namespace Zagejmi.Server.Write.Infrastructure.Ctx;

public class ZagejmiContext(DbContextOptions<ZagejmiContext> options) : DbContext(options) // Or IdentityDbContext, etc.
{
    public DbSet<IDomainEvent<Person, Guid>> OutboxEvents { get; set; }
}