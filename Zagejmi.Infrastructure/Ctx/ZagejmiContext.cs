using Microsoft.EntityFrameworkCore;
using Zagejmi.Infrastructure.Models;

namespace Zagejmi.Infrastructure.Ctx;

public class ZagejmiContext : DbContext
{
    // TODO: Fix me
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure PersonalInfo
        modelBuilder.Entity<PersonalInformationModel>()
            .HasIndex(i => i.Email)
            .IsUnique();
    }
}