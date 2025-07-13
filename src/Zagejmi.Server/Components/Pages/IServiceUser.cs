using LanguageExt;
using SharedKernel.Failures;
using Zagejmi.Domain.Auth;

namespace Zagejmi.Components.Pages;

public interface IServiceUser
{
    public Either<FailureLogin, bool> Login(string username, string password);
    
}