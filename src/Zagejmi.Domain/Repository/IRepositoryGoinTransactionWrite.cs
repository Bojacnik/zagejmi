using LanguageExt;
using Zagejmi.Domain.Community.Goin;
using Zagejmi.SharedKernel.Failures;

namespace Zagejmi.Domain.Repository;

public interface IRepositoryGoinTransactionWrite
{
    Task<Either<Failure, Unit>> CreateAsync(GoinTransaction goinTransaction, CancellationToken cancellationToken);
}