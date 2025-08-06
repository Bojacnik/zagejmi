using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Zagejmi.Application;
using Zagejmi.SharedKernel.Failures;

namespace Zagejmi.Infrastructure.Persistence;

/// <summary>
/// An implementation of the Unit of Work pattern using Entity Framework Core.
/// It wraps the DbContext to save all tracked changes within a single database transaction.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly DbContext _dbContext;
    private static readonly ILogger Logger = Log.ForContext<UnitOfWork>();

    public UnitOfWork(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Either<Failure, Unit>> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Unit.Default;
        }
        catch (DbUpdateException ex)
        {
            Logger.Error(ex, "An error occurred while saving changes to the database.");
            // Inspect ex.InnerException for more details about constraint violations, etc.
            return new FailureDatabaseWrite("Database.SaveChanges.Error" + $"A database error occurred: {ex.InnerException?.Message ?? ex.Message}");
        }
    }
}