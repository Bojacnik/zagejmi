using System.Text;
using RabbitMQ.Client;
using Serilog;

namespace Zagejmi.Infrastructure.EventBus;

public static class RabbitMqProducer
{
    public static async Task SendMessage(string message, string queueName, IConnection? connection)
    {
        if (connection is not { IsOpen: true })
        {
            Log.Error("RabbitMQ connection is not open. Cannot send message.");
            return;
        }

        await using IChannel channel = await connection.CreateChannelAsync();

        QueueDeclareOk channelDeclareResult = await channel.QueueDeclareAsync(queue: queueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);


        byte[] body = Encoding.UTF8.GetBytes(message);

        // exchange: The exchange to publish the message to. Use "" for the default exchange.
        // routingKey: For the default exchange, this is the queue name. For other exchanges, it's used for routing.
        // basicProperties: Optional, message properties (e.g., persistent, priority, headers).
        // body: The message payload as a byte array.

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: queueName,
            body,
            cts.Token);

        Log.Information(
            " [x] Sent '{Message}' to queue '{QueueName}'",
            message,
            queueName
        );
    }
}