using LanguageExt;
using System;
using System.Threading.Tasks;
using Zagejmi.Server.Domain.Auth;
using Zagejmi.SharedKernel.Failures;

namespace Zagejmi.Server.Domain.Repository;

public interface IRepositoryUserWrite
{
    Task<Option<User>> GetByUsernameAsync(string username);
    Task<bool> ExistsByUsernameOrEmailAsync(string username, string email);
    Task<bool> ExistsByIdAsync(Guid id);
    Task<Either<Failure, Guid>> CreateUserAndPersonProjectionAsync(User user);
}
