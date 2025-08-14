using LanguageExt;
using Zagejmi.SharedKernel.Failures;

namespace Zagejmi.Server.Domain.Repository;

public interface IRepositoryUserWrite
{
    Either<Failure, Guid> Add(string userName, string hashedPassword, string mailAddress);
    
    Either<Failure, Unit> Update();
}