namespace Zagejmi.Server.Application.CommandHandlers;

public interface IHandlerRequest<in THandler, TResult>
{
    Task<TResult> Handle(THandler request, CancellationToken cancellationToken);

}