using AnyMapper;
using LanguageExt;
using Microsoft.EntityFrameworkCore.Storage;
using Serilog;
using Zagejmi.Server.Write.Domain.Repository;
using Zagejmi.Server.Write.Infrastructure.Ctx;
using Zagejmi.Server.Write.Infrastructure.Models;
using Zagejmi.SharedKernel.Failures;

namespace Zagejmi.Server.Write.Infrastructure.Repository.GoinWallet;

public class RepositoryGoinWalletWrite : IRepositoryGoinWalletWrite
{
    public RepositoryGoinWalletWrite(ZagejmiContext context)
    {
        _context = context;
    }


    public async Task<Either<Failure, Unit>> CreateAsync(Domain.Community.Goin.GoinWallet goinWallet,
        CancellationToken cancellationToken)
    {
        var goinWalletModel = Mapper.Map<ModelGoinWallet>(goinWallet);
        try
        {
            await _context.Database.BeginTransactionAsync(cancellationToken);
        }
        catch (OperationCanceledException e)
        {
            Log.Information(e, "Operation cancelled");
            return new FailureOperationCancelled(e.Message);
        }

        try
        {
            _ = await _context.Set<ModelGoinWallet>().AddAsync(goinWalletModel, cancellationToken);
        }
        catch (OperationCanceledException e)
        {
            Log.Information(e, "Operation cancelled");
            return new FailureOperationCancelled(e.Message);
        }
        catch (Exception e)
        {
            Log.Error(e, "Operation failed");
            try
            {
                CancellationTokenSource cts = new();
                await _context.Database.RollbackTransactionAsync(cts.Token);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to rollback transaction");
                throw;
            }
        }

        try
        {
            CancellationTokenSource cts = new();
            await _context.SaveChangesAsync(cts.Token);
        }
        catch (OperationCanceledException e)
        {
            Log.Information(e, "Operation cancelled");
            try
            {
                CancellationTokenSource cts = new();
                await _context.Database.RollbackTransactionAsync(cts.Token);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to rollback transaction");
                throw;
            }
        }

        return Unit.Default;
    }

    public async Task<Either<Failure, Unit>> UpdateAsync(Domain.Community.Goin.GoinWallet goinWalletOld,
        Domain.Community.Goin.GoinWallet goinWalletNew, CancellationToken cancellationToken)
    {
        var goinWalletModelOld = Mapper.Map<ModelGoinWallet>(goinWalletOld);
        var goinWalletModelNew = Mapper.Map<ModelGoinWallet>(goinWalletNew);

        try
        {
            IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            var model = await _context.FindAsync<ModelGoinWallet>(
                [goinWalletModelOld.Id, cancellationToken],
                cancellationToken
            );

            if (model == null)
            {
                Log.Error("GoinWallet not found");
                return new FailureDatabaseEntityNotFound("GoinWallet not found");
            }

            _context.Entry(model).CurrentValues.SetValues(goinWalletModelNew);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (OperationCanceledException e)
            {
                Log.Information(e, "Operation cancelled");
                try
                {
                    CancellationTokenSource cts = new();
                    await transaction.RollbackAsync(cts.Token);
                    return new FailureOperationCancelled(e.Message);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to rollback transaction");
                    throw;
                }
            }

            try
            {
                await transaction.CommitAsync(cancellationToken);
            }
            catch (OperationCanceledException e)
            {
                Log.Information(e, "Operation cancelled");
                try
                {
                    CancellationTokenSource cts = new();
                    await transaction.RollbackAsync(cts.Token);
                    return new FailureOperationCancelled(e.Message);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to rollback transaction");
                    throw;
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to commit transaction");
                throw;
            }

            return Unit.Default;
        }
        catch (OperationCanceledException e)
        {
            Log.Information(e, "Transaction cancelled");
            return new FailureOperationCancelled(e.Message);
        }
    }

    private ZagejmiContext _context;
}