namespace Zagejmi.Read.Api.Services;

/// <summary>
///     RabbitMQ connection and queue settings for projection consumers.
/// </summary>
public sealed class RabbitMqProjectionOptions
{
    /// <summary>
    ///     Gets or sets broker host name.
    /// </summary>
    public string HostName { get; set; } = "localhost";

    /// <summary>
    ///     Gets or sets broker port.
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    ///     Gets or sets broker user name.
    /// </summary>
    public string UserName { get; set; } = "guest";

    /// <summary>
    ///     Gets or sets broker password.
    /// </summary>
    public string Password { get; set; } = "guest";

    /// <summary>
    ///     Gets or sets broker virtual host.
    /// </summary>
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    ///     Gets or sets queue name for profile events.
    /// </summary>
    public string QueueName { get; set; } = "zagejmi.profile.events";
}