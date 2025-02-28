using Microsoft.EntityFrameworkCore;
using Zagejmi.Infrastructure.Models;

namespace Zagejmi.Infrastructure.Ctx;

public class ZagejmiContext : Microsoft.EntityFrameworkCore.DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure PersonalInfo
        modelBuilder.Entity<PersonalInformationModel>()
            .HasIndex(i => i.Email)
            .IsUnique();
    }
}