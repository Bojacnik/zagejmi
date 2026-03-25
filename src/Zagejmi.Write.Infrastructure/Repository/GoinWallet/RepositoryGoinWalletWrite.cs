using LanguageExt;

using Microsoft.EntityFrameworkCore.Storage;

using Serilog;

using Zagejmi.Write.Infrastructure.Ctx;
using Zagejmi.Write.Infrastructure.Models;

namespace Zagejmi.Write.Infrastructure.Repository.GoinWallet;

public class RepositoryGoinWalletWrite : IRepositoryGoinWalletWrite
{
    private readonly ZagejmiContext _context;
    private readonly IMapper _mapper;

    public RepositoryGoinWalletWrite(ZagejmiContext context, IMapper mapper)
    {
        this._context = context;
        this._mapper = mapper;
    }

    public async Task<Either<Failure, Unit>> CreateAsync(
        Domain.Goin.GoinWallet goinWallet,
        CancellationToken cancellationToken)
    {
        var goinWalletModel = this._mapper.Map<Domain.Goin.GoinWallet, ModelGoinWallet>(goinWallet);
        if (goinWalletModel is null)
        {
            return new FailureMapper("Mapping from GoinWallet to ModelGoinWallet resulted in null.");
        }

        await using IDbContextTransaction transaction =
            await this._context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await this._context.Set<ModelGoinWallet>().AddAsync(goinWalletModel, cancellationToken);
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
            Log.Error(e, "Failed to create GoinWallet");
            return new FailureWallet(e.Message);
        }
    }

    public async Task<Either<Failure, Unit>> UpdateAsync(
        Domain.Goin.GoinWallet goinWalletOld,
        Domain.Goin.GoinWallet goinWalletNew,
        CancellationToken cancellationToken)
    {
        var goinWalletModelNew = this._mapper.Map<Domain.Goin.GoinWallet, ModelGoinWallet>(goinWalletNew);
        if (goinWalletModelNew is null)
        {
            return new FailureMapper("Mapping from GoinWallet to ModelGoinWallet resulted in null.");
        }

        await using IDbContextTransaction transaction =
            await this._context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            ModelGoinWallet? model =
                await this._context.Set<ModelGoinWallet>().FindAsync([goinWalletOld.Id], cancellationToken);

            if (model == null)
            {
                Log.Error("GoinWallet not found for update");
                return new FailureDatabaseEntityNotFound("GoinWallet not found");
            }

            this._context.Entry(model).CurrentValues.SetValues(goinWalletModelNew);
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
            Log.Error(e, "Failed to update GoinWallet");
            return new FailureWallet(e.Message);
        }
    }
}