using LanguageExt;
using SharedKernel;
using SharedKernel.Failures;
using Zagejmi.Domain.Community.Goin;

namespace Zagejmi.Domain.Repository;

public interface IRepositoryGoinTransactionWrite
{
    Task<Either<Failure, Unit>> CreateAsync(GoinTransaction goinTransaction, CancellationToken cancellationToken);
}