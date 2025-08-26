using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Zagejmi.Server.Domain.Repository;
using Zagejmi.Server.Infrastructure.Ctx;
using Zagejmi.Server.Infrastructure.Models;
using Zagejmi.SharedKernel.Failures;
using Zagejmi.SharedKernel.Util;

namespace Zagejmi.Server.Infrastructure.Repository.GoinWallet;

public class RepositoryGoinWalletWrite : IRepositoryGoinWalletWrite
{
    private readonly ZagejmiContext _context;
    private readonly IMapper _mapper;

    public RepositoryGoinWalletWrite(ZagejmiContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Either<Failure, Unit>> CreateAsync(Domain.Community.Goin.GoinWallet goinWallet,
        CancellationToken cancellationToken)
    {
        var goinWalletModel = _mapper.Map<Domain.Community.Goin.GoinWallet, ModelGoinWallet>(goinWallet);
        if (goinWalletModel is null)
        {
            return new FailureMapper("Mapping from GoinWallet to ModelGoinWallet resulted in null.");
        }
        
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await _context.Set<ModelGoinWallet>().AddAsync(goinWalletModel, cancellationToken);
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
            Log.Error(e, "Failed to create GoinWallet");
            return new FailureWallet(e.Message);
        }
    }

    public async Task<Either<Failure, Unit>> UpdateAsync(Domain.Community.Goin.GoinWallet goinWalletOld,
        Domain.Community.Goin.GoinWallet goinWalletNew, CancellationToken cancellationToken)
    {
        var goinWalletModelNew = _mapper.Map<Domain.Community.Goin.GoinWallet, ModelGoinWallet>(goinWalletNew);
        if (goinWalletModelNew is null)
        {
            return new FailureMapper("Mapping from GoinWallet to ModelGoinWallet resulted in null.");
        }
        
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var model = await _context.Set<ModelGoinWallet>().FindAsync([goinWalletOld.Id], cancellationToken);

            if (model == null)
            {
                Log.Error("GoinWallet not found for update");
                return new FailureDatabaseEntityNotFound("GoinWallet not found");
            }

            _context.Entry(model).CurrentValues.SetValues(goinWalletModelNew);
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
            Log.Error(e, "Failed to update GoinWallet");
            return new FailureWallet(e.Message);
        }
    }
}
