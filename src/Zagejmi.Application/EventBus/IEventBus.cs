namespace Zagejmi.Application.EventBus;

public interface IEventBus
{
    public Task PublishAsync(string channel, string message, CancellationToken cancellationToken = default);

    public Task<string> ReceiveAsync(string channel,  CancellationToken cancellationToken = default);
}