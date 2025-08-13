namespace Zagejmi.Server.Application.CommandHandlers;

public interface IRequestHandler<in THandler, TResult>
{
    Task<TResult> Handle(THandler request, CancellationToken cancellationToken);

}