using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using RabbitMQ.Client;

namespace Zagejmi.Write.Infrastructure.EventStore;

public sealed class RabbitMqStoredEventPublisher : IStoredEventPublisher
{
    private readonly RabbitMqProjectionOptions options;

    public RabbitMqStoredEventPublisher(IOptions<RabbitMqProjectionOptions> options)
    {
        this.options = options.Value;
    }

    public async Task PublishAsync(
        IReadOnlyCollection<Infrastructure.StoredEvent> events,
        CancellationToken cancellationToken = default)
    {
        if (events.Count == 0)
        {
            return;
        }

        ConnectionFactory factory = new()
        {
            HostName = this.options.HostName,
            Port = this.options.Port,
            UserName = this.options.UserName,
            Password = this.options.Password,
            VirtualHost = this.options.VirtualHost
        };

        await using IConnection connection = await factory.CreateConnectionAsync(cancellationToken);
        await using IChannel channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            this.options.QueueName,
            true,
            false,
            false,
            cancellationToken: cancellationToken);

        foreach (Infrastructure.StoredEvent storedEvent in events)
        {
            if (!storedEvent.EventType.EndsWith("UserProfileCreatedEvent", StringComparison.Ordinal) &&
                !storedEvent.EventType.EndsWith("UserProfileImageUpdatedEvent", StringComparison.Ordinal))
            {
                continue;
            }

            StoredEventMessage message = new()
            {
                EventId = storedEvent.Id,
                AggregateId = storedEvent.AggregateId,
                TimestampUtc = DateTime.SpecifyKind(storedEvent.Timestamp, DateTimeKind.Utc),
                EventType = storedEvent.EventType,
                Data = storedEvent.Data
            };

            string payload = JsonSerializer.Serialize(message);
            byte[] body = Encoding.UTF8.GetBytes(payload);

            BasicProperties properties = new()
            {
                Persistent = true,
                ContentType = "application/json"
            };

            await channel.BasicPublishAsync(
                string.Empty,
                this.options.QueueName,
                false,
                properties,
                body,
                cancellationToken);
        }
    }
}