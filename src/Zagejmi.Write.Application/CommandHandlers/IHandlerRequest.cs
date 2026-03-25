namespace Zagejmi.Write.Application.CommandHandlers;

public interface IHandlerRequest<in THandler, TResult>
{
    Task<TResult> Handle(THandler request, CancellationToken cancellationToken);

}