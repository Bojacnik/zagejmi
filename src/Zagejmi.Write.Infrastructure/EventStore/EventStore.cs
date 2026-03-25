using LanguageExt;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Zagejmi.Write.Application.EventStore;
using Zagejmi.Write.Domain.Events;
using Zagejmi.SharedKernel.Failures;
using Zagejmi.Write.Domain.Abstractions;
using Zagejmi.Write.Infrastructure.Ctx;
using StoredEventEntity = Zagejmi.Write.Infrastructure.StoredEvent;

namespace Zagejmi.Write.Infrastructure.EventStore;

public class EventStore<TAggregateRoot, TAggregateRootId> : IEventStore<TAggregateRoot, TAggregateRootId>
    where TAggregateRoot : Aggregate<TAggregateRoot, TAggregateRootId>
    where TAggregateRootId : notnull
{
    private readonly ZagejmiContext _context;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

    public EventStore(ZagejmiContext context)
    {
        _context = context;
    }

    public async Task<Option<TAggregateRoot>> LoadAggregateAsync(TAggregateRootId aggregateId, CancellationToken cancellationToken = default)
    {
        var storedEvents = await _context.StoredEvents
            .Where(e => e.AggregateId == Guid.Parse(aggregateId.ToString()!))
            .OrderBy(e => e.Timestamp)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (storedEvents.Count == 0)
        {
            return Option<TAggregateRoot>.None;
        }

        var aggregate = (TAggregateRoot)Activator.CreateInstance(typeof(TAggregateRoot), BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { aggregateId }, null)!;

        var history = new List<IDomainEvent<TAggregateRoot, TAggregateRootId>>();
        foreach (var storedEvent in storedEvents)
        {
            var eventType = Type.GetType(storedEvent.EventType);
            if (eventType == null)
            {
                // Log or handle the case where the event type cannot be found
                continue;
            }

            var domainEvent = (IDomainEvent<TAggregateRoot, TAggregateRootId>)JsonSerializer.Deserialize(storedEvent.Data, eventType, _jsonSerializerOptions)!;
            history.Add(domainEvent);
        }

        aggregate.LoadFromHistory(history);

        return aggregate;
    }
    
    public Task<Either<FailureEventStore, Unit>> SaveAsync(TAggregateRoot aggregate, CancellationToken cancellationToken = default)
    {
        try
        {
            var domainEvents = aggregate.DomainEvents.ToList();
            if (!domainEvents.Any())
            {
                return Task.FromResult<Either<FailureEventStore, Unit>>(Unit.Default);
            }

            foreach (var @event in domainEvents)
            {
                var storedEvent = new Infrastructure.StoredEvent
                {
                    Id = Guid.NewGuid(),
                    AggregateId = Guid.Parse(@event.AggregateId.ToString()!),
                    EventType = @event.GetType().AssemblyQualifiedName!,
                    Data = JsonSerializer.Serialize(@event, @event.GetType()),
                    Timestamp = DateTime.UtcNow
                };
                _context.StoredEvents.Add(storedEvent);
            }

            aggregate.ClearEvents();

            return Task.FromResult<Either<FailureEventStore, Unit>>(Unit.Default);
        }
        catch (Exception e)
        {
            return Task.FromResult<Either<FailureEventStore, Unit>>(new FailureEventStoreUnableToSave(e.Message));
        }
    }
}
