using AnyMapper;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Serilog;
using SharedKernel.Failures;
using Zagejmi.Domain.Repository;
using Zagejmi.Infrastructure.Models;

namespace Zagejmi.Infrastructure.Repository.GoinTransaction;

public class RepositoryGoinTransactionWrite : IRepositoryGoinTransactionWrite
{
    public RepositoryGoinTransactionWrite(DbContext context)
    {
        _context = context;
    }

    public async Task<Either<Failure, Unit>> CreateAsync(Domain.Community.Goin.GoinTransaction goinTransaction,
        CancellationToken cancellationToken)
    {
        var goinTransactionModel = Mapper.Map<ModelGoinTransaction>(goinTransaction);
        IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _context.Set<ModelGoinTransaction>().AddAsync(goinTransactionModel, cancellationToken);
        }
        catch (OperationCanceledException e)
        {
            try
            {
                var cts = new CancellationTokenSource();
                await transaction.RollbackAsync(cts.Token);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to rollback transaction");
                throw;
            }

            return new FailureOperationCancelled(e.Message);
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (OperationCanceledException e)
        {
            try
            {
                var cts = new CancellationTokenSource();
                await transaction.RollbackAsync(cts.Token);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to rollback transaction");
                throw;
            }

            return new FailureOperationCancelled(e.Message);
        }

        try
        {
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to commit transaction");
            throw;
        }


        return Unit.Default;
    }

    private readonly DbContext _context;
}