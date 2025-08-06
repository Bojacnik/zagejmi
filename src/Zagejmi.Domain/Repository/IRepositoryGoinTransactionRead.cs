using LanguageExt;
using Zagejmi.Domain.Community.Goin;
using Zagejmi.SharedKernel.Failures;

namespace Zagejmi.Domain.Repository;

public interface IRepositoryGoinTransactionRead
{
    public Task<Either<Failure, GoinTransaction?>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    public Task<Either<Failure, List<GoinTransaction>>> GetBySenderIdAsync(
        Guid senderId,
        CancellationToken cancellationToken);

    public Task<Either<Failure, List<GoinTransaction>>> GetByReceiver(
        Guid receiverId,
        CancellationToken cancellationToken);
}