using AnyMapper;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Zagejmi.Server.Domain.Repository;
using Zagejmi.Server.Infrastructure.Ctx;
using Zagejmi.Server.Infrastructure.Models;
using Zagejmi.SharedKernel.Failures;

namespace Zagejmi.Server.Infrastructure.Repository.GoinTransaction;

public class RepositoryGoinTransactionRead : IRepositoryGoinTransactionRead
{
    public RepositoryGoinTransactionRead(ZagejmiContext dbContext) => _dbContext = dbContext;

    public async Task<Either<Failure, Write.Domain.Community.Goin.GoinTransaction?>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        ModelGoinTransaction? transactionModel;
        try
        {
            transactionModel = await _dbContext.Set<ModelGoinTransaction>()
                .FindAsync([id, cancellationToken], cancellationToken);
        }
        catch (OperationCanceledException e)
        {
            Log.Information(e, "Operation cancelled");
            return new FailureOperationCancelled(e.Message);
        }

        return transactionModel == null ? default(Either<Failure, Write.Domain.Community.Goin.GoinTransaction?>) : Mapper.Map<Write.Domain.Community.Goin.GoinTransaction>(transactionModel);
    }

    public async Task<Either<Failure, List<Write.Domain.Community.Goin.GoinTransaction>>> GetBySenderIdAsync(
        Guid sender,
        CancellationToken cancellationToken)
    {
        var senderModel = Mapper.Map<ModelGoinWallet>(sender);

        List<Write.Domain.Community.Goin.GoinTransaction> transactions;
        try
        {
            transactions = await _dbContext.Set<ModelGoinTransaction>()
                .Where(goinTransactionModel => goinTransactionModel.SenderId == senderModel.Id)
                .OrderBy(model => model.Id)
                .Select<ModelGoinTransaction, Write.Domain.Community.Goin.GoinTransaction>(model =>
                    Mapper.Map<Write.Domain.Community.Goin.GoinTransaction>(model))
                .ToListAsync(cancellationToken);
        }
        catch (OperationCanceledException e)
        {
            Log.Information(e, "Operation cancelled");
            return new FailureOperationCancelled(e.Message);
        }

        return transactions;
    }

    public async Task<Either<Failure, List<Write.Domain.Community.Goin.GoinTransaction>>> GetByReceiver(
        Guid receiver,
        CancellationToken cancellationToken)
    {
        List<Write.Domain.Community.Goin.GoinTransaction> transactions;
        try
        {
            transactions = await _dbContext.Set<ModelGoinTransaction>()
                .Where(goinTransactionModel => goinTransactionModel.DomainId == receiver)
                .OrderBy(model => model.Id)
                .Select<ModelGoinTransaction, Write.Domain.Community.Goin.GoinTransaction>(model =>
                    Mapper.Map<Write.Domain.Community.Goin.GoinTransaction>(model))
                .ToListAsync(cancellationToken);
        }
        catch (OperationCanceledException e)
        {
            Log.Information(e, "Operation cancelled");
            return new FailureOperationCancelled(e.Message);
        }

        return transactions;
    }

    private readonly ZagejmiContext _dbContext;
}