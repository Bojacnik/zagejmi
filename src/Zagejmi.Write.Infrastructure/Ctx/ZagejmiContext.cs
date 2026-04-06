using Microsoft.EntityFrameworkCore;

namespace Zagejmi.Write.Infrastructure.Ctx;

/// <summary>
///     The DbContext for the application.
/// </summary>
public class ZagejmiContext : DbContext
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ZagejmiContext" /> class with the specified options.
    /// </summary>
    /// <param name="options">
    ///     The options to be used by the DbContext, typically including the connection string and other
    ///     configuration settings for the database.
    /// </param>
    public ZagejmiContext(DbContextOptions<ZagejmiContext> options)
        : base(options)
    {
    }

    /// <summary>
    ///     Gets or sets the DbSet representing the stored events in the event store. This is where domain events are persisted
    ///     when using
    ///     event sourcing. Each event is stored as a record in the database, allowing for the reconstruction of aggregate
    ///     state by replaying these events. The configuration for this DbSet is critical for ensuring that the
    ///     Outbox pattern works correctly, as it relies on the presence of the necessary tables to manage the outbox messages
    ///     and states for reliable message delivery.
    /// </summary>
    public DbSet<StoredEvent> StoredEvents { get; set; }

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
        base.OnModelCreating(modelBuilder);
    }
}