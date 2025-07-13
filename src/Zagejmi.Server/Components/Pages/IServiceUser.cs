using LanguageExt;

namespace Zagejmi.Components.Pages;

public interface IServiceUser
{
    public Either<LoginFailure, bool> Login();
    
}