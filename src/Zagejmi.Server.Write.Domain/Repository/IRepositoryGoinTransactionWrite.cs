using LanguageExt;
using Zagejmi.Server.Write.Domain.Community.Goin;
using Zagejmi.SharedKernel.Failures;

namespace Zagejmi.Server.Write.Domain.Repository;

public interface IRepositoryGoinTransactionWrite
{
    Task<Either<Failure, Unit>> CreateAsync(GoinTransaction goinTransaction, CancellationToken cancellationToken);
}