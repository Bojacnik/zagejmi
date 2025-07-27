using LanguageExt;
using MassTransit.Mediator;
using SharedKernel.Failures;
using Zagejmi.Application.Commands.Person;

namespace Zagejmi.Components.Services;

public class ServiceUser(IMediator mediator) : IServiceUser
{
    public Task<Either<FailureLogin, bool>> Login(string username, string password)
    {
        throw new NotImplementedException();
    }

    public async Task<Either<FailureRegister, bool>> Register(string username, string password)
    {
        CommandPersonAnonCreate person = new();
        CancellationToken cts = CancellationToken.None;
        try
        {
            await mediator.Send(person, cts);
            return true;
        }
        catch (OperationCanceledException e)
        {
            return new FailureRegister("Failed to register because " + e.Message);
        }
    }
}