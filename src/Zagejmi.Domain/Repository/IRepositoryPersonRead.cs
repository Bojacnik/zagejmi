using LanguageExt;
using Zagejmi.Domain.Community.People;
using Zagejmi.Domain.Community.People.Person;
using Zagejmi.SharedKernel.Failures;

namespace Zagejmi.Domain.Repository;

public interface IRepositoryPersonRead
{
    Task<Either<Failure, Person>> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<Either<Failure, Person>> GetByUsernameAsync(string username, CancellationToken cancellationToken);
}