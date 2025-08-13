using LanguageExt;
using Zagejmi.Server.Write.Domain.Community.Goin;
using Zagejmi.SharedKernel.Failures;

namespace Zagejmi.Server.Domain.Repository;

public interface IRepositoryGoinWalletRead
{
    public Task<Either<Failure, GoinWallet?>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}