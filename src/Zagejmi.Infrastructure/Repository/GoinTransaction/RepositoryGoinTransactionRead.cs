using Serilog;
using AnyMapper;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Failures;
using Zagejmi.Domain.Repository;
using Zagejmi.Domain.Community.User;
using Zagejmi.Infrastructure.Ctx;
using Zagejmi.Infrastructure.Models;

namespace Zagejmi.Infrastructure.Repository.GoinTransaction;

public class RepositoryGoinTransactionRead : IRepositoryGoinTransactionRead
{
    public RepositoryGoinTransactionRead(ZagejmiContext dbContext) => _dbContext = dbContext;

    public async Task<Either<Failure, Domain.Community.Goin.GoinTransaction?>> GetByIdAsync(
        ulong id,
        CancellationToken cancellationToken)
    {
        GoinTransactionModel? transactionModel;
        try
        {
            transactionModel = await _dbContext.Set<GoinTransactionModel>()
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

    public async Task<Either<Failure, List<Domain.Community.Goin.GoinTransaction>>> GetBySenderAsync(
        Domain.Community.User.Person sender,
        CancellationToken cancellationToken)
    {
        var senderModel = Mapper.Map<PersonModel>(sender);

        List<Domain.Community.Goin.GoinTransaction> transactions;
        try
        {
            transactions = await _dbContext.Set<GoinTransactionModel>()
                .Where(goinTransactionModel => goinTransactionModel.SenderId == senderModel.Id)
                .OrderBy(model => model.Id)
                .Select<GoinTransactionModel, Domain.Community.Goin.GoinTransaction>(model => Mapper.Map<Domain.Community.Goin.GoinTransaction>(model))
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
        Domain.Community.User.Person receiver,
        CancellationToken cancellationToken)
    {
        var receiverModel = Mapper.Map<PersonModel>(receiver);

        List<Domain.Community.Goin.GoinTransaction> transactions;
        try
        {
            transactions = await _dbContext.Set<GoinTransactionModel>()
                .Where(goinTransactionModel => goinTransactionModel.ReceiverId == receiverModel.Id)
                .OrderBy(model => model.Id)
                .Select<GoinTransactionModel, Domain.Community.Goin.GoinTransaction>(model => Mapper.Map<Domain.Community.Goin.GoinTransaction>(model))
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