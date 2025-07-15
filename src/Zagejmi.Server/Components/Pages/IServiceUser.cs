using LanguageExt;
using SharedKernel.Failures;

namespace Zagejmi.Components.Pages;

public interface IServiceUser
{
    public Either<FailureLogin, bool> Login();
    
}