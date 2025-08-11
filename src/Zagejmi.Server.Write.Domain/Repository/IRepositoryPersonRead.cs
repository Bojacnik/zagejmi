using LanguageExt;
using Zagejmi.Server.Write.Domain.Community.People;
using Zagejmi.SharedKernel.Failures;

namespace Zagejmi.Server.Write.Domain.Repository;

public interface IRepositoryPersonRead
{
    Task<Either<Failure, Person>> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<Either<Failure, Person>> GetByUsernameAsync(string username, CancellationToken cancellationToken);
}