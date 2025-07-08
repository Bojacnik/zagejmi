using Serilog;
using AnyMapper;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Failures;
using Zagejmi.Domain.Repository;
using Zagejmi.Infrastructure.Ctx;
using Zagejmi.Infrastructure.Models;

namespace Zagejmi.Infrastructure.Repository.GoinTransaction;

public class RepositoryGoinTransactionRead : IRepositoryGoinTransactionRead
{
    public RepositoryGoinTransactionRead(ZagejmiContext dbContext) => _dbContext = dbContext;

    public async Task<Either<Failure, Domain.Community.Goin.GoinTransaction?>> GetByIdAsync(
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

        if (transactionModel == null)
            return default;


        return Mapper.Map<Domain.Community.Goin.GoinTransaction>(transactionModel);
    }

    public async Task<Either<Failure, List<Domain.Community.Goin.GoinTransaction>>> GetBySenderIdAsync(
        Guid sender,
        CancellationToken cancellationToken)
    {
        var senderModel = Mapper.Map<ModelGoinWallet>(sender);

        List<Domain.Community.Goin.GoinTransaction> transactions;
        try
        {
            transactions = await _dbContext.Set<ModelGoinTransaction>()
                .Where(goinTransactionModel => goinTransactionModel.SenderId == senderModel.Id)
                .OrderBy(model => model.Id)
                .Select<ModelGoinTransaction, Domain.Community.Goin.GoinTransaction>(model =>
                    Mapper.Map<Domain.Community.Goin.GoinTransaction>(model))
                .ToListAsync(cancellationToken);
        }
        catch (OperationCanceledException e)
        {
            Log.Information(e, "Operation cancelled");
            return new FailureOperationCancelled(e.Message);
        }

        return transactions;
    }

    public async Task<Either<Failure, List<Domain.Community.Goin.GoinTransaction>>> GetByReceiver(
        Guid receiver,
        CancellationToken cancellationToken)
    {
        List<Domain.Community.Goin.GoinTransaction> transactions;
        try
        {
            transactions = await _dbContext.Set<ModelGoinTransaction>()
                .Where(goinTransactionModel => goinTransactionModel.DomainId == receiver)
                .OrderBy(model => model.Id)
                .Select<ModelGoinTransaction, Domain.Community.Goin.GoinTransaction>(model =>
                    Mapper.Map<Domain.Community.Goin.GoinTransaction>(model))
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