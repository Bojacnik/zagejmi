/*
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Zagejmi.Domain.Community.User;
using Zagejmi.Domain.Repository;
using Zagejmi.Infrastructure.Ctx;
using Zagejmi.Infrastructure.Models;

namespace Zagejmi.Infrastructure.Repository;

public class PersonRepository(ZagejmiContext dbContext, IMapper mapper) : IPersonRepository
{
    public Task<List<Person>> GetAllAsync(CancellationToken cancellationToken)
    {
        return Task.Run(async () =>
        {
            List<PersonModel> transactionModels = await
                dbContext
                    .Set<PersonModel>()
                    .ToListAsync(cancellationToken);

            return mapper
                .Map<List<PersonModel>, List<Person>>(
                    transactionModels
                );
        }, cancellationToken);
    }

    public Task<Person> GetByIdAsync(ulong id, CancellationToken cancellationToken)
    {
        ValueTask<PersonModel?> result = dbContext.Set<PersonModel>().FindAsync([id], cancellationToken);
        return Task.Run(
            async () => mapper.Map<Person>(await result),
            cancellationToken
        );
    }

    public Task<Person> AddAsync(Person entity, CancellationToken cancellationToken)
    {
        var model = mapper.Map<PersonModel>(entity);

        dbContext.Database.BeginTransaction();

        ValueTask<EntityEntry<PersonModel>> result =
            dbContext
                .Set<PersonModel>()
                .AddAsync(
                    model,
                    cancellationToken
                );

        dbContext.Database.CommitTransaction();

        // TODO: will this work ??
        return Task.Run(async () =>
                mapper.Map<Person>(
                    (await result).Entity),
            cancellationToken
        );
    }

    public Task<Person> UpdateAsync(Person oldEntity, Person newEntity, CancellationToken cancellationToken)
    {
        dbContext.Database.BeginTransaction();

        ValueTask<PersonModel?> oldModel =
            dbContext
                .Set<PersonModel>()
                .FindAsync(
                    [oldEntity.Id],
                    cancellationToken
                );

        dbContext
            .Entry(oldModel)
            .CurrentValues
            .SetValues(
                mapper.Map<PersonModel>(newEntity)
            );

        ValueTask<PersonModel?> updatedModel =
            dbContext
                .Set<PersonModel>()
                .FindAsync(
                    [oldEntity.Id],
                    cancellationToken
                );

        dbContext.Database.CommitTransaction();

        return Task.Run(
            async () => mapper.Map<Person>(await updatedModel),
            cancellationToken
        );
    }

    public Task<bool> DeleteAsync(Person entity, CancellationToken cancellationToken)
    {
        var model = mapper.Map<PersonModel>(entity);

        dbContext.Database.BeginTransaction();

        dbContext
            .Set<PersonModel>()
            .Remove(model);

        dbContext.Database.CommitTransaction();

        return Task.FromResult(true);
    }

    public Task<bool> FlushAsync(CancellationToken cancellationToken)
    {
        return Task.Run(
            async () =>
            {
                int result = await dbContext.SaveChangesAsync(cancellationToken);
                return result > 0;
            },
            cancellationToken
        );
    }

    public Task<Person> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        Task<PersonModel?> model = dbContext.Set<PersonModel>()
            .SingleOrDefaultAsync(
                personModel => personModel.PersonalInfo != null &&
                               personModel.PersonalInfo.Email.Equals(email),
                cancellationToken);

        return Task.Run(
            async () => mapper.Map<Person>(await model),
            cancellationToken
        );
    }

    public Task<Person> GetByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        Task<PersonModel?> model =
            dbContext
                .Set<PersonModel>()
                .SingleOrDefaultAsync(
                    personModel =>
                        personModel.PersonalInfo != null &&
                        personModel.PersonalInfo.UserName == username,
                    cancellationToken
                );

        return Task.Run(
            async () => mapper.Map<Person>(await model),
            cancellationToken
        );
    }
}
*/