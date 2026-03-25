using System;
using System.Text.Json;
using System.Threading.Tasks;

using LanguageExt;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using Zagejmi.Write.Domain.Profile;
using Zagejmi.Write.Infrastructure.Ctx;
using Zagejmi.Write.Infrastructure.Models;

namespace Zagejmi.Write.Infrastructure.Repository.People;

public class RepositoryPersonWrite : IRepositoryPersonWrite
{
    private readonly ZagejmiContext _dbContext;
    private readonly IMapper _mapper;

    public RepositoryPersonWrite(ZagejmiContext dbContext, IMapper mapper)
    {
        this._dbContext = dbContext;
        this._mapper = mapper;
    }

    public async Task<bool> ExistsByUserIdAsync(Guid userId)
    {
        return await this._dbContext.Set<ModelPerson>()
            .AsNoTracking()
            .AnyAsync(p => p.UserId == userId);
    }

    public async Task<Either<Failure, Guid>> CreatePersonWithProjectionAsync(Profile profile)
    {
        await using IDbContextTransaction transaction = await this._dbContext.Database.BeginTransactionAsync();

        try
        {
            foreach (IDomainEvent<Profile, Guid> @event in profile.DomainEvents)
            {
                StoredEvent storedEvent = new()
                {
                    Id = Guid.NewGuid(),
                    AggregateId = profile.Id,
                    EventType = @event.GetType().AssemblyQualifiedName!,
                    Data = JsonSerializer.Serialize(@event, @event.GetType()),
                    Timestamp = DateTime.UtcNow
                };
                this._dbContext.StoredEvents.Add(storedEvent);
            }

            var personProjection = this._mapper.Map<Profile, ModelPerson>(profile);
            if (personProjection is null)
            {
                return new FailureMapper("Mapping from Person to ModelPerson resulted in null.");
            }

            this._dbContext.Set<ModelPerson>().Add(personProjection);

            await this._dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return profile.Id;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new FailureArgumentInvalidValue(
                $"An unexpected error occurred while creating the person: {ex.Message}");
        }
    }
}