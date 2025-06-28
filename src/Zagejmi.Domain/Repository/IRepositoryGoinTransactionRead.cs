using LanguageExt;
using SharedKernel.Failures;
using Zagejmi.Domain.Community.Goin;
using Zagejmi.Domain.Community.User;

namespace Zagejmi.Domain.Repository;

public interface IRepositoryGoinTransactionRead
{
    public Task<Either<Failure, GoinTransaction?>> GetByIdAsync(
        ulong id,
        CancellationToken cancellationToken);

    public Task<Either<Failure, List<GoinTransaction>>> GetBySenderAsync(
        Person sender,
        CancellationToken cancellationToken);

    public Task<Either<Failure, List<GoinTransaction>>> GetByReceiver(Person receiver,
        CancellationToken cancellationToken);
}