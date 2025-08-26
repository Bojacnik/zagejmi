using LanguageExt;
using Serilog;
using Zagejmi.Server.Domain.Repository;
using Zagejmi.Server.Infrastructure.Ctx;
using Zagejmi.Server.Infrastructure.Models;
using Zagejmi.SharedKernel.Failures;
using Zagejmi.SharedKernel.Util;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Zagejmi.Server.Infrastructure.Repository.GoinTransaction;

public class RepositoryGoinTransactionWrite : IRepositoryGoinTransactionWrite
{
    private readonly ZagejmiContext _context;
    private readonly IMapper _mapper;

    public RepositoryGoinTransactionWrite(ZagejmiContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Either<Failure, Unit>> CreateAsync(
        Domain.Community.Goin.GoinTransaction goinTransaction,
        CancellationToken cancellationToken)
    {
        var goinTransactionModel = _mapper.Map<Domain.Community.Goin.GoinTransaction, ModelGoinTransaction>(goinTransaction);
        if (goinTransactionModel is null)
        {
            return new FailureMapper("Mapping from GoinTransaction to ModelGoinTransaction resulted in null.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await _context.Set<ModelGoinTransaction>().AddAsync(goinTransactionModel, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Unit.Default;
        }
        catch (OperationCanceledException e)
        {
            Log.Information(e, "Operation cancelled");
            return new FailureOperationCancelled(e.Message);
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to create GoinTransaction");
            return new FailureWallet(e.Message);
        }
    }
}
