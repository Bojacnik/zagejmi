using LanguageExt;
using SharedKernel.Failures;
using Zagejmi.Domain.Community.User;

namespace Zagejmi.Domain.Repository;

public interface IRepositoryPersonWrite
{
    public Task<Either<Failure, Unit>> CreatePerson(
        Person person,
        CancellationToken cancellationToken);

    public Task<Either<Failure, Unit>> UpdatePerson(
        Person personOld, Person personNew,
        CancellationToken cancellationToken);
}