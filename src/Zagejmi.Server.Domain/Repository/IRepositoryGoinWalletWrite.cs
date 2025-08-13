using LanguageExt;
using Zagejmi.Server.Write.Domain.Community.Goin;
using Zagejmi.SharedKernel.Failures;

namespace Zagejmi.Server.Domain.Repository;

public interface IRepositoryGoinWalletWrite
{
    public Task<Either<Failure, Unit>> CreateAsync(
        GoinWallet goinWallet,
        CancellationToken cancellationToken);
    
    public Task<Either<Failure, Unit>> UpdateAsync(
        GoinWallet goinWalletOld,
        GoinWallet goinWalletNew,
        CancellationToken cancellationToken);
}