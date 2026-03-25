using LanguageExt;

using Serilog;

using Zagejmi.Write.Infrastructure.Ctx;
using Zagejmi.Write.Infrastructure.Models;

namespace Zagejmi.Write.Infrastructure.Repository.GoinTransaction;

public class RepositoryGoinTransactionRead : IRepositoryGoinTransactionRead
{
    private readonly ZagejmiContext _dbContext;
    private readonly IMapper _mapper;

    public RepositoryGoinTransactionRead(ZagejmiContext dbContext, IMapper mapper)
    {
        this._dbContext = dbContext;
        this._mapper = mapper;
    }

    public async Task<Either<Failure, Domain.Goin.GoinTransaction?>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        ModelGoinTransaction? transactionModel;
        try
        {
            transactionModel = await this._dbContext.Set<ModelGoinTransaction>().FindAsync([id], cancellationToken);
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

        Domain.Goin.GoinTransaction? mappedTransaction =
            this._mapper.Map<ModelGoinTransaction, Domain.Goin.GoinTransaction>(transactionModel);
        if (mappedTransaction is null)
        {
            return new FailureMapper("Mapping from ModelGoinTransaction to GoinTransaction resulted in null.");
        }

        return mappedTransaction;
    }

    public async Task<Either<Failure, List<Domain.Goin.GoinTransaction>>> GetBySenderIdAsync(
        Guid senderId,
        CancellationToken cancellationToken)
    {
        try
        {
            List<ModelGoinTransaction> transactionModels = await this._dbContext.Set<ModelGoinTransaction>()
                .Where(t => t.SenderId == senderId)
                .OrderBy(t => t.Id)
                .ToListAsync(cancellationToken);

            List transactions = new List<Domain.Goin.GoinTransaction>();
            foreach (ModelGoinTransaction model in transactionModels)
            {
                Domain.Goin.GoinTransaction? transaction =
                    this._mapper.Map<ModelGoinTransaction, Domain.Goin.GoinTransaction>(model);
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

    public async Task<Either<Failure, List<Domain.Goin.GoinTransaction>>> GetByReceiver(
        Guid receiverId,
        CancellationToken cancellationToken)
    {
        try
        {
            List<ModelGoinTransaction> transactionModels = await this._dbContext.Set<ModelGoinTransaction>()
                .Where(t => t.ReceiverId == receiverId)
                .OrderBy(t => t.Id)
                .ToListAsync(cancellationToken);

            List transactions = new List<Domain.Goin.GoinTransaction>();
            foreach (ModelGoinTransaction model in transactionModels)
            {
                Domain.Goin.GoinTransaction? transaction =
                    this._mapper.Map<ModelGoinTransaction, Domain.Goin.GoinTransaction>(model);
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