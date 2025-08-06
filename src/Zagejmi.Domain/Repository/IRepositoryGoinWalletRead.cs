using LanguageExt;
using Zagejmi.Domain.Community.Goin;
using Zagejmi.SharedKernel.Failures;

namespace Zagejmi.Domain.Repository;

public interface IRepositoryGoinWalletRead
{
    public Task<Either<Failure, GoinWallet?>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}