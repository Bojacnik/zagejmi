using AnyMapper;
using LanguageExt;
using Microsoft.EntityFrameworkCore.Storage;
using Serilog;
using Zagejmi.Domain.Repository;
using Zagejmi.Infrastructure.Ctx;
using Zagejmi.Infrastructure.Models;
using Zagejmi.SharedKernel.Failures;

namespace Zagejmi.Infrastructure.Repository.Person;

public class RepositoryPersonWrite : IRepositoryPersonWrite
{
    public RepositoryPersonWrite(ZagejmiContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Either<Failure, Unit>> CreatePerson(
        Domain.Community.People.Person.Person person,
        CancellationToken cancellationToken)
    {
        var personModel = Mapper.Map<ModelPerson>(person);

        Task<IDbContextTransaction> transaction;

        try
        {
            transaction = _dbContext.Database.BeginTransactionAsync(cancellationToken);
        }
        catch (OperationCanceledException e)
        {
            Log.Information("Operation cancelled");
            return new FailureOperationCancelled(e.Message);
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to begin transaction");
            throw;
        }

        try
        {
            await _dbContext.AddAsync(personModel, cancellationToken);
        }
        catch (OperationCanceledException e)
        {
            Log.Information(e, "Operation cancelled");
            CancellationTokenSource cts = new();
            try
            {
                await _dbContext.Database.RollbackTransactionAsync(cts.Token);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to rollback transaction");
                throw;
            }

            return new FailureOperationCancelled(e.Message);
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to add person");
            throw;
        }

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (OperationCanceledException e)
        {
            Log.Information(e, "Operation cancelled");
            return new FailureOperationCancelled("Operation cancelled");
        }

        try
        {
            await _dbContext.Database.CommitTransactionAsync(cancellationToken);
        }
        catch (OperationCanceledException e)
        {
            Log.Information(e, "Operation cancelled");
            CancellationTokenSource cts = new();
            try
            {
                await _dbContext.Database.RollbackTransactionAsync(cts.Token);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to rollback transaction");
                throw;
            }

            return new FailureOperationCancelled("Operation cancelled");
        }

        return Unit.Default;
    }

    public async Task<Either<Failure, Unit>> UpdatePerson(
        Domain.Community.People.Person.Person personOld,
        Domain.Community.People.Person.Person personNew,
        CancellationToken cancellationToken)
    {
        var opersonModelOld = Mapper.Map<ModelPerson>(personOld);
        try
        {
            _dbContext.Database.BeginTransactionAsync(cancellationToken);
        }
        catch (OperationCanceledException e)
        {
            Log.Information("Operation cancelled");
            return new FailureOperationCancelled(e.Message);
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to begin transaction");
            throw;
        }

        try
        {
            ModelPerson? result = await _dbContext.Set<ModelPerson>().FindAsync(
                [opersonModelOld, cancellationToken],
                cancellationToken);

            if (result == null)
            {
                Log.Error("Person not found");
                throw new ArgumentException("Person not found");
            }

            try
            {
                _dbContext.Entry(result).CurrentValues.SetValues(personNew);
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to update person");
            }

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (OperationCanceledException e)
            {
                Log.Information(e, "Operation cancelled");
                return new FailureOperationCancelled("Operation cancelled");
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to add person");
                throw;
            }

            try
            {
                await _dbContext.Database.CommitTransactionAsync(cancellationToken);
            }
            catch (OperationCanceledException ex)
            {
                Log.Information(ex, "Operation cancelled");
                try
                {
                    CancellationTokenSource cts = new();
                    await _dbContext.Database.RollbackTransactionAsync(cts.Token);
                }
                catch (OperationCanceledException e)
                {
                    Log.Error("Failed to rollback transaction");
                    throw;
                }
                catch (Exception exx)
                {
                    Log.Error(exx, "Failed to rollback transaction");
                    throw;
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to add person");
                throw;
            }

            return Unit.Default;
        }
        catch (OperationCanceledException e)
        {
            Log.Information(e, "Operation cancelled");
            return new FailureOperationCancelled("Operation cancelled");
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to update person");
            throw;
        }
    }

    private ZagejmiContext _dbContext;
}