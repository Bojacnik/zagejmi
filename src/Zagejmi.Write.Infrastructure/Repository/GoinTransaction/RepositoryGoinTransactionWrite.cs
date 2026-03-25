using System;
using System.Threading;
using System.Threading.Tasks;

using LanguageExt;

using Microsoft.EntityFrameworkCore.Storage;

using Serilog;

using Zagejmi.Write.Infrastructure.Ctx;
using Zagejmi.Write.Infrastructure.Models;

namespace Zagejmi.Write.Infrastructure.Repository.GoinTransaction;

public class RepositoryGoinTransactionWrite : IRepositoryGoinTransactionWrite
{
    private readonly ZagejmiContext _context;
    private readonly IMapper _mapper;

    public RepositoryGoinTransactionWrite(ZagejmiContext context, IMapper mapper)
    {
        this._context = context;
        this._mapper = mapper;
    }

    public async Task<Either<Failure, Unit>> CreateAsync(
        Domain.Goin.GoinTransaction goinTransaction,
        CancellationToken cancellationToken)
    {
        var goinTransactionModel = this._mapper.Map<Domain.Goin.GoinTransaction, ModelGoinTransaction>(goinTransaction);
        if (goinTransactionModel is null)
        {
            return new FailureMapper("Mapping from GoinTransaction to ModelGoinTransaction resulted in null.");
        }

        await using IDbContextTransaction transaction =
            await this._context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await this._context.Set<ModelGoinTransaction>().AddAsync(goinTransactionModel, cancellationToken);
            await this._context.SaveChangesAsync(cancellationToken);
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