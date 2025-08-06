using Microsoft.EntityFrameworkCore;
using Zagejmi.Domain;
using Zagejmi.Domain.Community.People;

namespace Zagejmi.Infrastructure.Ctx;

public class ZagejmiContext(DbContextOptions<ZagejmiContext> options) : DbContext(options) // Or IdentityDbContext, etc.
{
    public DbSet<IDomainEvent<Person, Guid>> OutboxEvents { get; set; }
}