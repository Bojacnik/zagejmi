using LanguageExt;
using SharedKernel.Failures;
using Zagejmi.Domain.Community.Goin;

namespace Zagejmi.Domain.Repository;

public interface IRepositoryGoinWalletRead
{
    public Task<Either<Failure, GoinWallet?>> GetByIdAsync(ulong id, CancellationToken cancellationToken);
}