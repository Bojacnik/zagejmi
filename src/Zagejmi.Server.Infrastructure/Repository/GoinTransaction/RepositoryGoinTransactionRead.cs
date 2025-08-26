using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Zagejmi.Server.Domain.Repository;
using Zagejmi.Server.Infrastructure.Ctx;
using Zagejmi.Server.Infrastructure.Models;
using Zagejmi.SharedKernel.Failures;
using Zagejmi.SharedKernel.Util;

namespace Zagejmi.Server.Infrastructure.Repository.GoinTransaction;

public class RepositoryGoinTransactionRead : IRepositoryGoinTransactionRead
{
    private readonly ZagejmiContext _dbContext;
    private readonly IMapper _mapper;

    public RepositoryGoinTransactionRead(ZagejmiContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<Either<Failure, Domain.Community.Goin.GoinTransaction?>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        ModelGoinTransaction? transactionModel;
        try
        {
            transactionModel = await _dbContext.Set<ModelGoinTransaction>().FindAsync([id], cancellationToken);
        }
        catch (OperationCanceledException e)
        {
            Log.Information(e, "Operation cancelled");
            return new FailureOperationCancelled(e.Message);
        }

        if (transactionModel == null)
        {
            return new FailureDatabaseEntityNotFound("Transaction not found");
        }

        Domain.Community.Goin.GoinTransaction? mappedTransaction =
            _mapper.Map<ModelGoinTransaction, Domain.Community.Goin.GoinTransaction>(transactionModel);
        if (mappedTransaction is null)
        {
            return new FailureMapper("Mapping from ModelGoinTransaction to GoinTransaction resulted in null.");
        }

        return mappedTransaction;
    }

    public async Task<Either<Failure, List<Domain.Community.Goin.GoinTransaction>>> GetBySenderIdAsync(
        Guid senderId,
        CancellationToken cancellationToken)
    {
        try
        {
            List<ModelGoinTransaction> transactionModels = await _dbContext.Set<ModelGoinTransaction>()
                .Where(t => t.SenderId == senderId)
                .OrderBy(t => t.Id)
                .ToListAsync(cancellationToken);

            var transactions = new List<Domain.Community.Goin.GoinTransaction>();
            foreach (ModelGoinTransaction model in transactionModels)
            {
                Domain.Community.Goin.GoinTransaction? transaction = _mapper.Map<ModelGoinTransaction, Domain.Community.Goin.GoinTransaction>(model);
                if (transaction is null)
                {
                    return new FailureMapper($"Mapping for transaction with id {model.Id} resulted in null.");
                }

                transactions.Add(transaction);
            }

            return transactions;
        }
        catch (OperationCanceledException e)
        {
            Log.Information(e, "Operation cancelled");
            return new FailureOperationCancelled(e.Message);
        }
    }

    public async Task<Either<Failure, List<Domain.Community.Goin.GoinTransaction>>> GetByReceiver(
        Guid receiverId,
        CancellationToken cancellationToken)
    {
        try
        {
            List<ModelGoinTransaction> transactionModels = await _dbContext.Set<ModelGoinTransaction>()
                .Where(t => t.ReceiverId == receiverId)
                .OrderBy(t => t.Id)
                .ToListAsync(cancellationToken);

            var transactions = new List<Domain.Community.Goin.GoinTransaction>();
            foreach (ModelGoinTransaction model in transactionModels)
            {
                Domain.Community.Goin.GoinTransaction? transaction = _mapper.Map<ModelGoinTransaction, Domain.Community.Goin.GoinTransaction>(model);
                if (transaction is null)
                {
                    return new FailureMapper($"Mapping for transaction with id {model.Id} resulted in null.");
                }

                transactions.Add(transaction);
            }

            return transactions;
        }
        catch (OperationCanceledException e)
        {
            Log.Information(e, "Operation cancelled");
            return new FailureOperationCancelled(e.Message);
        }
    }
}