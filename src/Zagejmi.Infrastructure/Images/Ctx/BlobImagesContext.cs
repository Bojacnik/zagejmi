using Microsoft.EntityFrameworkCore;

using Zagejmi.Infrastructure.Images.Records;

namespace Zagejmi.Infrastructure.Images.Ctx;

/// <summary>
///     The DbContext for the application.
/// </summary>
public class BlobImagesContext : DbContext
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BlobImagesContext" /> class with the specified options.
    /// </summary>
    /// <param name="options">
    ///     The options to be used by the DbContext, typically including the connection string and other
    ///     configuration settings for the database.
    /// </param>
    public BlobImagesContext(DbContextOptions<BlobImagesContext> options)
        : base(options)
    {
    }

    /// <summary>
    ///     Gets or sets image blobs stored as bytea.
    /// </summary>
    public DbSet<ImageBlobRecord> ImageBlobs { get; set; }

    /// <summary>
    ///     Configures the model for the DbContext, including the necessary configurations for the Outbox pattern.
    ///     This method is called by the Entity Framework when the model is being created. It is essential to include the
    ///     configurations
    ///     for the Outbox entities here to ensure that the necessary tables are created in the database for managing the
    ///     outbox messages and states. Without this configuration, the Outbox pattern will not function correctly, leading to
    ///     issues with message delivery and reliability in the system.
    /// </summary>
    /// <param name="modelBuilder">The ModelBuilder used to configure the model for the DbContext.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("image_blobs");

        modelBuilder.Entity<ImageBlobRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(x => x.BlobKey)
                .HasMaxLength(512)
                .IsRequired();

            entity.HasIndex(x => x.BlobKey)
                .IsUnique();

            entity.Property(x => x.ContentType)
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(x => x.Sha256Hash)
                .HasMaxLength(64)
                .IsRequired();
        });

        base.OnModelCreating(modelBuilder);
    }
}