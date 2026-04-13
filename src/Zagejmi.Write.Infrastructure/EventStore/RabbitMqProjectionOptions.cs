namespace Zagejmi.Write.Infrastructure.EventStore;

public sealed class RabbitMqProjectionOptions
{
    public string HostName { get; set; } = "localhost";

    public int Port { get; set; } = 5672;

    public string UserName { get; set; } = "guest";

    public string Password { get; set; } = "guest";

    public string VirtualHost { get; set; } = "/";

    public string QueueName { get; set; } = "zagejmi.profile.events";
}