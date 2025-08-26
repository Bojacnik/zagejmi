using LanguageExt;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using Zagejmi.Server.Domain.Community.People;
using Zagejmi.Server.Domain.Events;
using Zagejmi.Server.Domain.Repository;
using Zagejmi.Server.Infrastructure.Ctx;
using Zagejmi.SharedKernel.Failures;
using Zagejmi.Server.Infrastructure.Models;
using Zagejmi.SharedKernel.Util;
using StoredEventEntity = Zagejmi.Server.Infrastructure.StoredEvent;

namespace Zagejmi.Server.Infrastructure.Repository.People;

public class RepositoryPersonWrite : IRepositoryPersonWrite
{
    private readonly ZagejmiContext _dbContext;
    private readonly IMapper _mapper;

    public RepositoryPersonWrite(ZagejmiContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<bool> ExistsByUserIdAsync(Guid userId)
    {
        return await _dbContext.Set<ModelPerson>()
            .AsNoTracking()
            .AnyAsync(p => p.UserId == userId);
    }

    public async Task<Either<Failure, Guid>> CreatePersonWithProjectionAsync(Person person)
    {
        await using IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            foreach (IDomainEvent<Person, Guid> @event in person.DomainEvents)
            {
                var storedEvent = new StoredEventEntity
                {
                    Id = Guid.NewGuid(),
                    AggregateId = person.Id,
                    EventType = @event.GetType().AssemblyQualifiedName!,
                    Data = JsonSerializer.Serialize(@event, @event.GetType()),
                    Timestamp = DateTime.UtcNow
                };
                _dbContext.StoredEvents.Add(storedEvent);
            }

            var personProjection = _mapper.Map<Person, ModelPerson>(person);
            if (personProjection is null)
            {
                return new FailureMapper("Mapping from Person to ModelPerson resulted in null.");
            }
            
            _dbContext.Set<ModelPerson>().Add(personProjection);

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return person.Id;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new FailureArgumentInvalidValue(
                $"An unexpected error occurred while creating the person: {ex.Message}");
        }
    }
}
