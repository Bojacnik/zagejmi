using LanguageExt;
using SharedKernel.Failures;
using Zagejmi.Domain.Events;
using Zagejmi.Domain.Community.User;

namespace Zagejmi.Domain.Repository;

public interface IRepositoryPersonRead
{
    Task<Either<Failure, Person>> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<Either<Failure, Person>> GetByUsernameAsync(string username, CancellationToken cancellationToken);
}