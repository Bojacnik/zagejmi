using System.Text.Json;

using LanguageExt;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using Zagejmi.Write.Application.Commands.Person;
using Zagejmi.Write.Application.EventStore;
using Zagejmi.Write.Infrastructure.Ctx;
using Zagejmi.Write.Infrastructure.Projections;

namespace Zagejmi.Write.Infrastructure.Repository.User;

public class RepositoryUserWrite : IRepositoryUserWrite
{
    private readonly ZagejmiContext _dbContext;
    private readonly IEventStore<Domain.Auth.User, Guid> _eventStore;

    public RepositoryUserWrite(ZagejmiContext dbContext, IEventStore<Domain.Auth.User, Guid> eventStore)
    {
        this._dbContext = dbContext;
        this._eventStore = eventStore;
    }

    public async Task<Option<Domain.Auth.User>> GetByUsernameAsync(string username)
    {
        UserProjection? userProjection = await this._dbContext.UserProjections
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserName == username);

        if (userProjection == null)
        {
            return Option<Domain.Auth.User>.None;
        }

        return await this._eventStore.LoadAggregateAsync(userProjection.Id);
    }

    public async Task<bool> ExistsByUsernameOrEmailAsync(string username, string email)
    {
        return await this._dbContext.UserProjections
            .AsNoTracking()
            .AnyAsync(p => p.UserName == username || p.Email == email);
    }

    public async Task<bool> ExistsByIdAsync(Guid id)
    {
        return await this._dbContext.UserProjections
            .AsNoTracking()
            .AnyAsync(p => p.Id == id);
    }

    public async Task<Either<Failure, Guid>> CreateUserAndPersonProjectionAsync(Domain.Auth.User user)
    {
        await using IDbContextTransaction transaction = await this._dbContext.Database.BeginTransactionAsync();

        try
        {
            foreach (IDomainEvent<Domain.Auth.User, Guid> @event in user.DomainEvents)
            {
                StoredEvent storedEvent = new()
                {
                    Id = Guid.NewGuid(),
                    AggregateId = user.Id,
                    EventType = @event.GetType().AssemblyQualifiedName!,
                    Data = JsonSerializer.Serialize(@event, @event.GetType()),
                    Timestamp = DateTime.UtcNow
                };
                this._dbContext.StoredEvents.Add(storedEvent);
            }

            UserProjection userProjection = new()
            {
                Id = user.Id,
                UserName = user.Username,
                Email = user.Email
            };
            this._dbContext.UserProjections.Add(userProjection);

            // Create PersonProjection
            PersonProjection personProjection = new()
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
            this._dbContext.PersonProjections.Add(personProjection);

            await this._dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return user.Id;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new FailureArgumentInvalidValue(
                $"An unexpected error occurred while creating the user and person projection: {ex.Message}");
        }
    }
}