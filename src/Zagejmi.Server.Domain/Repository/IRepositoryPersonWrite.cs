using LanguageExt;
using System;
using System.Threading.Tasks;
using Zagejmi.Server.Domain.Community.People;
using Zagejmi.SharedKernel.Failures;

namespace Zagejmi.Server.Domain.Repository;

public interface IRepositoryPersonWrite
{
    Task<bool> ExistsByUserIdAsync(Guid userId);
    Task<Either<Failure, Guid>> CreatePersonWithProjectionAsync(Person person);
}
