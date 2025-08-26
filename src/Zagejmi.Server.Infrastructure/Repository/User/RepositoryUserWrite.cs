using LanguageExt;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage;
using Zagejmi.Server.Application.EventStore;
using Zagejmi.Server.Domain.Events;
using Zagejmi.Server.Domain.Repository;
using Zagejmi.Server.Infrastructure.Ctx;
using Zagejmi.SharedKernel.Failures;
using StoredEventEntity = Zagejmi.Server.Infrastructure.StoredEvent;
using UserProjectionEntity = Zagejmi.Server.Infrastructure.UserProjection;
using PersonProjectionEntity = Zagejmi.Server.Infrastructure.Projections.PersonProjection;
using Zagejmi.Server.Application.Commands.Person;

namespace Zagejmi.Server.Infrastructure.Repository.User;

public class RepositoryUserWrite : IRepositoryUserWrite
{
    private readonly ZagejmiContext _dbContext;
    private readonly IEventStore<Domain.Auth.User, Guid> _eventStore;

    public RepositoryUserWrite(ZagejmiContext dbContext, IEventStore<Domain.Auth.User, Guid> eventStore)
    {
        _dbContext = dbContext;
        _eventStore = eventStore;
    }

    public async Task<Option<Domain.Auth.User>> GetByUsernameAsync(string username)
    {
        UserProjectionEntity? userProjection = await _dbContext.UserProjections
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserName == username);

        if (userProjection == null)
        {
            return Option<Domain.Auth.User>.None;
        }

        return await _eventStore.LoadAggregateAsync(userProjection.Id);
    }

    public async Task<bool> ExistsByUsernameOrEmailAsync(string username, string email)
    {
        return await _dbContext.UserProjections
            .AsNoTracking()
            .AnyAsync(p => p.UserName == username || p.Email == email);
    }

    public async Task<bool> ExistsByIdAsync(Guid id)
    {
        return await _dbContext.UserProjections
            .AsNoTracking()
            .AnyAsync(p => p.Id == id);
    }

    public async Task<Either<Failure, Guid>> CreateUserAndPersonProjectionAsync(Domain.Auth.User user)
    {
        await using IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            foreach (IDomainEvent<Domain.Auth.User, Guid> @event in user.DomainEvents)
            {
                StoredEventEntity storedEvent = new StoredEventEntity
                {
                    Id = Guid.NewGuid(),
                    AggregateId = user.Id,
                    EventType = @event.GetType().AssemblyQualifiedName!,
                    Data = JsonSerializer.Serialize(@event, @event.GetType()),
                    Timestamp = DateTime.UtcNow
                };
                _dbContext.StoredEvents.Add(storedEvent);
            }

            UserProjectionEntity userProjection = new UserProjectionEntity
            {
                Id = user.Id,
                UserName = user.Username,
                Email = user.Email
            };
            _dbContext.UserProjections.Add(userProjection);

            // Create PersonProjection
            PersonProjectionEntity personProjection = new PersonProjectionEntity
            {
                Id = Guid.NewGuid(), // Generate a new ID for the PersonProjection
                UserId = user.Id,
                UserName = user.Username,
                FirstName = "", // Default empty string
                LastName = "", // Default empty string
                Email = user.Email,
                BirthDate = DateTime.MinValue.AddYears(1900), // Default date
                Gender = Gender.Unknown, // Default empty string
                GoinAmount = 0
            };
            _dbContext.PersonProjections.Add(personProjection);

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return user.Id;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new FailureArgumentInvalidValue($"An unexpected error occurred while creating the user and person projection: {ex.Message}");
        }
    }
}
