using RabbitMQ.Client;
using Serilog;

namespace Zagejmi.Infrastructure.EventStore;

public class RabbitMqConnection
{
    public static async Task<IConnection> CreateConnection(
        string hostname = "localhost",
        int port = 5672,
        string username = "guest",
        string password = "guest",
        string virtualHost = "/")
    {
        var factory = new ConnectionFactory()
        {
            HostName = hostname,
            Port = AmqpTcpEndpoint.DefaultAmqpSslPort,
            UserName = username,
            Password = password,
            VirtualHost = virtualHost,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };

        try
        {
            Log.Information("Attempting to connect to RabbitMQ at {Hostname}:{Port} (vhost: {VirtualHost})",
                hostname,
                port,
                virtualHost
            );
            IConnection connection = await factory.CreateConnectionAsync();
            Log.Information("Connected to RabbitMQ successfully!");
            return connection;
        }
        catch (Exception ex)
        {
            Log.Information(
                "Could not connect to RabbitMQ: {ExMessage}",
                ex.Message
            );
            throw;
        }
    }
}