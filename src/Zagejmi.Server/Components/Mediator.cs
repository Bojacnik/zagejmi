using MassTransit;
using MassTransit.Mediator;

namespace Zagejmi.Components;

public class Mediator : IMediator
{
    public ConnectHandle ConnectSendObserver(ISendObserver observer)
    {
        throw new NotImplementedException();
    }

    public Task Send<T>(T message, CancellationToken cancellationToken = new CancellationToken()) where T : class
    {
        throw new NotImplementedException();
    }

    public Task Send<T>(T message, IPipe<SendContext<T>> pipe, CancellationToken cancellationToken = new CancellationToken()) where T : class
    {
        throw new NotImplementedException();
    }

    public Task Send<T>(T message, IPipe<SendContext> pipe, CancellationToken cancellationToken = new CancellationToken()) where T : class
    {
        throw new NotImplementedException();
    }

    public Task Send(object message, CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task Send(object message, Type messageType, CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task Send(object message, IPipe<SendContext> pipe, CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task Send(object message, Type messageType, IPipe<SendContext> pipe, CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task Send<T>(object values, CancellationToken cancellationToken = new CancellationToken()) where T : class
    {
        throw new NotImplementedException();
    }

    public Task Send<T>(object values, IPipe<SendContext<T>> pipe, CancellationToken cancellationToken = new CancellationToken()) where T : class
    {
        throw new NotImplementedException();
    }

    public Task Send<T>(object values, IPipe<SendContext> pipe, CancellationToken cancellationToken = new CancellationToken()) where T : class
    {
        throw new NotImplementedException();
    }

    public ConnectHandle ConnectPublishObserver(IPublishObserver observer)
    {
        throw new NotImplementedException();
    }

    public Task<ISendEndpoint> GetPublishSendEndpoint<T>() where T : class
    {
        throw new NotImplementedException();
    }

    public Task Publish<T>(T message, CancellationToken cancellationToken = new CancellationToken()) where T : class
    {
        throw new NotImplementedException();
    }

    public Task Publish<T>(T message, IPipe<PublishContext<T>> publishPipe, CancellationToken cancellationToken = new CancellationToken()) where T : class
    {
        throw new NotImplementedException();
    }

    public Task Publish<T>(T message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = new CancellationToken()) where T : class
    {
        throw new NotImplementedException();
    }

    public Task Publish(object message, CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task Publish(object message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task Publish(object message, Type messageType, CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task Publish(object message, Type messageType, IPipe<PublishContext> publishPipe,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task Publish<T>(object values, CancellationToken cancellationToken = new CancellationToken()) where T : class
    {
        throw new NotImplementedException();
    }

    public Task Publish<T>(object values, IPipe<PublishContext<T>> publishPipe, CancellationToken cancellationToken = new CancellationToken()) where T : class
    {
        throw new NotImplementedException();
    }

    public Task Publish<T>(object values, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = new CancellationToken()) where T : class
    {
        throw new NotImplementedException();
    }

    public RequestHandle<T> CreateRequest<T>(T message, CancellationToken cancellationToken = new CancellationToken(),
        RequestTimeout timeout = new RequestTimeout()) where T : class
    {
        throw new NotImplementedException();
    }

    public RequestHandle<T> CreateRequest<T>(Uri destinationAddress, T message,
        CancellationToken cancellationToken = new CancellationToken(), RequestTimeout timeout = new RequestTimeout()) where T : class
    {
        throw new NotImplementedException();
    }

    public RequestHandle<T> CreateRequest<T>(ConsumeContext consumeContext, T message,
        CancellationToken cancellationToken = new CancellationToken(), RequestTimeout timeout = new RequestTimeout()) where T : class
    {
        throw new NotImplementedException();
    }

    public RequestHandle<T> CreateRequest<T>(ConsumeContext consumeContext, Uri destinationAddress, T message,
        CancellationToken cancellationToken = new CancellationToken(), RequestTimeout timeout = new RequestTimeout()) where T : class
    {
        throw new NotImplementedException();
    }

    public RequestHandle<T> CreateRequest<T>(object values, CancellationToken cancellationToken = new CancellationToken(),
        RequestTimeout timeout = new RequestTimeout()) where T : class
    {
        throw new NotImplementedException();
    }

    public RequestHandle<T> CreateRequest<T>(Uri destinationAddress, object values,
        CancellationToken cancellationToken = new CancellationToken(), RequestTimeout timeout = new RequestTimeout()) where T : class
    {
        throw new NotImplementedException();
    }

    public RequestHandle<T> CreateRequest<T>(ConsumeContext consumeContext, object values,
        CancellationToken cancellationToken = new CancellationToken(), RequestTimeout timeout = new RequestTimeout()) where T : class
    {
        throw new NotImplementedException();
    }

    public RequestHandle<T> CreateRequest<T>(ConsumeContext consumeContext, Uri destinationAddress, object values,
        CancellationToken cancellationToken = new CancellationToken(), RequestTimeout timeout = new RequestTimeout()) where T : class
    {
        throw new NotImplementedException();
    }

    public IRequestClient<T> CreateRequestClient<T>(RequestTimeout timeout = new RequestTimeout()) where T : class
    {
        throw new NotImplementedException();
    }

    public IRequestClient<T> CreateRequestClient<T>(ConsumeContext consumeContext, RequestTimeout timeout = new RequestTimeout()) where T : class
    {
        throw new NotImplementedException();
    }

    public IRequestClient<T> CreateRequestClient<T>(Uri destinationAddress, RequestTimeout timeout = new RequestTimeout()) where T : class
    {
        throw new NotImplementedException();
    }

    public IRequestClient<T> CreateRequestClient<T>(ConsumeContext consumeContext, Uri destinationAddress,
        RequestTimeout timeout = new RequestTimeout()) where T : class
    {
        throw new NotImplementedException();
    }

    public ClientFactoryContext Context { get; }
    public ConnectHandle ConnectConsumePipe<T>(IPipe<ConsumeContext<T>> pipe) where T : class
    {
        throw new NotImplementedException();
    }

    public ConnectHandle ConnectConsumePipe<T>(IPipe<ConsumeContext<T>> pipe, ConnectPipeOptions options) where T : class
    {
        throw new NotImplementedException();
    }

    public ConnectHandle ConnectRequestPipe<T>(Guid requestId, IPipe<ConsumeContext<T>> pipe) where T : class
    {
        throw new NotImplementedException();
    }

    public ConnectHandle ConnectConsumeObserver(IConsumeObserver observer)
    {
        throw new NotImplementedException();
    }

    public ConnectHandle ConnectConsumeMessageObserver<T>(IConsumeMessageObserver<T> observer) where T : class
    {
        throw new NotImplementedException();
    }
}