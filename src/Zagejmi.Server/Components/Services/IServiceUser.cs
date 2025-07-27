using LanguageExt;
using SharedKernel.Failures;

namespace Zagejmi.Components.Services;

public interface IServiceUser
{
    public Task<Either<FailureLogin, bool>> Login(string username, string password);
    
    public Task<Either<FailureRegister, bool>> Register(string username, string password);
}