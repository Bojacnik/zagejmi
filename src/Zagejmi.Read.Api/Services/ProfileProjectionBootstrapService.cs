using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Npgsql;

using Zagejmi.Read.Api.Ctx;
using Zagejmi.Read.Api.Models;

namespace Zagejmi.Read.Api.Services;

/// <summary>
///     Replays historical profile events on startup to rebuild read projections.
/// </summary>
public sealed class ProfileProjectionBootstrapService : BackgroundService
{
    private const string UndefinedTableSqlState = "42P01";

    private static readonly Action<ILogger, int, Exception?> BootstrapCompletedLogger =
        LoggerMessage.Define<int>(
            LogLevel.Information,
            default,
            "Profile projection bootstrap completed. Replayed {EventCount} events.");

    private static readonly Action<ILogger, string, Exception?> BootstrapSkippedDueToMissingRelationLogger =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            default,
            "Profile projection bootstrap skipped because relation '{RelationName}' does not exist yet.");

    private readonly ILogger<ProfileProjectionBootstrapService> logger;
    private readonly IServiceScopeFactory scopeFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ProfileProjectionBootstrapService" /> class.
    /// </summary>
    /// <param name="scopeFactory">Scope factory for resolving scoped services.</param>
    /// <param name="logger">Service logger.</param>
    public ProfileProjectionBootstrapService(
        IServiceScopeFactory scopeFactory,
        ILogger<ProfileProjectionBootstrapService> logger)
    {
        this.scopeFactory = scopeFactory;
        this.logger = logger;
    }

    /// <summary>
    ///     Runs bootstrap replay until completion.
    /// </summary>
    /// <param name="stoppingToken">Cancellation token.</param>
    /// <returns>A task that completes when replay is finished.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using IServiceScope scope = this.scopeFactory.CreateScope();
            ZagejmiReadContext context = scope.ServiceProvider.GetRequiredService<ZagejmiReadContext>();
            ProfileProjectionApplier applier = scope.ServiceProvider.GetRequiredService<ProfileProjectionApplier>();

            DateTime? checkpoint = await applier.GetCheckpointAsync(stoppingToken);

            IQueryable<StoredEventReadModel> query = context.StoredEvents
                .AsNoTracking()
                .OrderBy(x => x.Timestamp)
                .ThenBy(x => x.Id)
                .Where(x => x.EventType.EndsWith("UserProfileCreatedEvent") ||
                            x.EventType.EndsWith("UserProfileImageUpdatedEvent"));

            if (checkpoint.HasValue)
            {
                query = query.Where(x => x.Timestamp >= checkpoint.Value);
            }

            int replayed = 0;
            List<StoredEventReadModel> eventsToReplay = await query.ToListAsync(stoppingToken);

            foreach (StoredEventReadModel storedEvent in eventsToReplay)
            {
                StoredEventMessage message = new()
                {
                    EventId = storedEvent.Id,
                    AggregateId = storedEvent.AggregateId,
                    TimestampUtc = DateTime.SpecifyKind(storedEvent.Timestamp, DateTimeKind.Utc),
                    EventType = storedEvent.EventType,
                    Data = storedEvent.Data,
                };

                await applier.ApplyAsync(message, stoppingToken);
                await applier.SetCheckpointAsync(message.TimestampUtc, stoppingToken);
                replayed++;
            }

            BootstrapCompletedLogger(this.logger, replayed, null);
        }
        catch (PostgresException ex) when (ex.SqlState == UndefinedTableSqlState)
        {
            BootstrapSkippedDueToMissingRelationLogger(this.logger, ex.TableName ?? "<unknown>", ex);
        }
    }
}