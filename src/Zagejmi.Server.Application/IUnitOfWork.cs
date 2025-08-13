using LanguageExt;
using Zagejmi.SharedKernel.Failures;

namespace Zagejmi.Server.Application;

/// <summary>
/// Represents a unit of work that can be used to save all changes
/// made within a single business transaction.
/// </summary>
public interface IUnitOfWork
{
    public Task<Either<Failure, Unit>> SaveChangesAsync(CancellationToken cancellationToken = default);
}