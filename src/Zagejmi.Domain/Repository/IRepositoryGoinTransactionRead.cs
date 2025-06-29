using LanguageExt;
using SharedKernel.Failures;
using Zagejmi.Domain.Community.Goin;

namespace Zagejmi.Domain.Repository;

public interface IRepositoryGoinTransactionRead
{
    public Task<Either<Failure, GoinTransaction?>> GetByIdAsync(
        ulong id,
        CancellationToken cancellationToken);

    public Task<Either<Failure, List<GoinTransaction>>> GetBySenderAsync(
        GoinWallet sender,
        CancellationToken cancellationToken);

    public Task<Either<Failure, List<GoinTransaction>>> GetByReceiver(
        GoinWallet receiver,
        CancellationToken cancellationToken);
}