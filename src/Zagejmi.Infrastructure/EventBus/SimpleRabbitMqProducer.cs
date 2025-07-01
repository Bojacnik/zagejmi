using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;
using LanguageExt;
using RabbitMQ.Client;
using Serilog;
using SharedKernel;
using SharedKernel.Failures;

namespace Zagejmi.Infrastructure.EventBus;

public class SimpleRabbitMqProducer : IEventBusProducer
{
    private readonly IConnection _connection;
    private readonly ILogger _logger;

    // Inject a logger for better testability and consistency
    public SimpleRabbitMqProducer(IConnection connection, ILogger logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task<Either<Failure, Unit>> SendAsync(IDomainEvent @event, CancellationToken cancellationToken)
    {
        if (_connection is not { IsOpen: true })
        {
            const string errorMessage = "RabbitMQ connection is not open. Cannot send message.";
            _logger.Error(errorMessage);
            return new FailureRabbitMq(errorMessage);
        }

        try
        {
            await using IChannel channel = await _connection.CreateChannelAsync(
                new CreateChannelOptions(true, true,
                    new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions())),
                cancellationToken
            );

            await channel.QueueDeclareAsync(
                queue: @event.EventType,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            string message = JsonSerializer.Serialize(@event, @event.GetType());
            byte[] body = Encoding.UTF8.GetBytes(message);

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: @event.EventType,
                body: body,
                cancellationToken: cancellationToken);

            _logger.Information(
                " [x] Sent event {Timestamp} of type '{EventType}' to queue '{QueueName}'",
                @event.Timestamp,
                @event.EventType,
                @event.EventType
            );

            return Unit.Default;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to publish event {EventId} of type {EventType}", @event.Timestamp,
                @event.EventType);
            return new FailureRabbitMq($"Failed to publish event: {ex.Message}");
        }
    }
}