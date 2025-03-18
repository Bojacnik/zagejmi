using System.Net.Sockets;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Zagejmi.Application.Repository;
using Zagejmi.Domain.Community.Goin;
using Zagejmi.Domain.Community.User;
using Zagejmi.Infrastructure.Ctx;
using Zagejmi.Infrastructure.Models;

namespace Zagejmi.Infrastructure.Repository;

public class GoinTransactionRepository(ZagejmiContext dbContext, IMapper mapper) : IGoinTransactionRepository
{
    public Task<List<GoinTransaction>> GetAllAsync(CancellationToken cancellationToken)
    {
        return Task.Run(async () =>
        {
            List<GoinTransactionModel> transactionModels = await
                dbContext
                    .Set<GoinTransactionModel>()
                    .ToListAsync(cancellationToken);

            return mapper
                .Map<List<GoinTransactionModel>, List<GoinTransaction>>(
                    transactionModels
                );
        }, cancellationToken);
    }

    public Task<GoinTransaction> GetByIdAsync(ulong id, CancellationToken cancellationToken)
    {
        ValueTask<GoinTransaction?> result =
            dbContext
                .Set<GoinTransaction>()
                .FindAsync([
                        id,
                        cancellationToken
                    ],
                    cancellationToken
                );

        return Task.Run(
            async () => mapper.Map<GoinTransaction>(await result),
            cancellationToken
        );
    }

    public Task<GoinTransaction> AddAsync(GoinTransaction entity, CancellationToken cancellationToken)
    {
        var model = mapper.Map<GoinTransactionModel>(entity);

        dbContext.Database.BeginTransaction();

        ValueTask<EntityEntry<GoinTransactionModel>> result = dbContext
            .Set<GoinTransactionModel>()
            .AddAsync(model, cancellationToken
            );

        dbContext.Database.CommitTransaction();

        return Task.Run(
            async () =>
                mapper.Map<GoinTransaction>(await result),
            cancellationToken
        );
    }

    public Task<GoinTransaction> UpdateAsync(GoinTransaction oldEntity, GoinTransaction newEntity,
        CancellationToken cancellationToken)
    {
        dbContext.Database.BeginTransaction();

        ValueTask<GoinTransactionModel?> oldModel =
            dbContext
                .Set<GoinTransactionModel>()
                .FindAsync(
                    [oldEntity.Id],
                    cancellationToken
                );

        dbContext
            .Entry(oldModel)
            .CurrentValues
            .SetValues(
                mapper.Map<GoinTransactionModel>(newEntity)
            );

        ValueTask<GoinTransactionModel?> updatedModel =
            dbContext
                .Set<GoinTransactionModel>()
                .FindAsync(
                    [oldEntity.Id],
                    cancellationToken
                );

        dbContext.Database.CommitTransaction();

        return Task.Run(
            async () => mapper.Map<GoinTransaction>(await updatedModel),
            cancellationToken
        );
    }

    public Task<bool> DeleteAsync(GoinTransaction entity, CancellationToken cancellationToken)
    {
        var model = mapper.Map<GoinTransactionModel>(entity);

        dbContext.Database.BeginTransaction();

        dbContext
            .Set<GoinTransactionModel>()
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

    public Task<List<GoinTransaction>> GetBySender(Person sender)
    {
        IQueryable<GoinTransactionModel> transactionModels =
            from transaction in dbContext.Set<GoinTransactionModel>()
            where transaction.SenderId == sender.Id
            select transaction;

        return Task.Run(
            // TODO: Check how to map Queryable to List<GoinTransaction> using automapper
            () => mapper.Map<List<GoinTransaction>>(transactionModels)
        );
    }

    public Task<List<GoinTransaction>> GetByReceiver(Person sender)
    {
        IQueryable<GoinTransactionModel> transactionModels =
            from transaction in dbContext.Set<GoinTransactionModel>()
            where transaction.ReceiverId == sender.Id
            select transaction;

        return Task.Run(
            // TODO: Check how to map Queryable to List<GoinTransaction> using automapper
            () => mapper.Map<List<GoinTransaction>>(transactionModels)
        );
    }
}