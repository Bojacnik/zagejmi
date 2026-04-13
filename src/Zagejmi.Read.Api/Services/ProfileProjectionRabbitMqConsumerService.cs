using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RabbitMQ.Client;

namespace Zagejmi.Read.Api.Services;

/// <summary>
///     Consumes profile-related stored-event messages from RabbitMQ and applies them to read projections.
/// </summary>
public sealed class ProfileProjectionRabbitMqConsumerService : BackgroundService
{
    private static readonly Action<ILogger, Exception?> ProjectionApplyFailedLogger =
        LoggerMessage.Define(
            LogLevel.Error,
            default,
            "Failed to apply RabbitMQ profile projection message.");

    private readonly ILogger<ProfileProjectionRabbitMqConsumerService> logger;
    private readonly RabbitMqProjectionOptions options;
    private readonly IServiceScopeFactory scopeFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ProfileProjectionRabbitMqConsumerService" /> class.
    /// </summary>
    /// <param name="scopeFactory">Scope factory used for resolving scoped projection services per message.</param>
    /// <param name="options">RabbitMQ connection and queue options.</param>
    /// <param name="logger">Logger instance.</param>
    public ProfileProjectionRabbitMqConsumerService(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqProjectionOptions> options,
        ILogger<ProfileProjectionRabbitMqConsumerService> logger)
    {
        this.scopeFactory = scopeFactory;
        this.options = options.Value;
        this.logger = logger;
    }

    /// <summary>
    ///     Runs the background projection subscription loop until cancellation.
    /// </summary>
    /// <param name="stoppingToken">Token signaled when the host is shutting down.</param>
    /// <returns>A task representing the lifetime of the consumer loop.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ConnectionFactory factory = new()
        {
            HostName = this.options.HostName,
            Port = this.options.Port,
            UserName = this.options.UserName,
            Password = this.options.Password,
            VirtualHost = this.options.VirtualHost
        };

        await using IConnection connection = await factory.CreateConnectionAsync(stoppingToken);
        await using IChannel channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            this.options.QueueName,
            true,
            false,
            false,
            cancellationToken: stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            BasicGetResult? result = await channel.BasicGetAsync(
                this.options.QueueName,
                false,
                stoppingToken);

            if (result == null)
            {
                await Task.Delay(250, stoppingToken);
                continue;
            }

            string payload = Encoding.UTF8.GetString(result.Body.ToArray());

            try
            {
                StoredEventMessage? message = JsonSerializer.Deserialize<StoredEventMessage>(payload);
                if (message != null)
                {
                    using IServiceScope scope = this.scopeFactory.CreateScope();
                    ProfileProjectionApplier applier =
                        scope.ServiceProvider.GetRequiredService<ProfileProjectionApplier>();

                    await applier.ApplyAsync(message, stoppingToken);
                    await applier.SetCheckpointAsync(message.TimestampUtc, stoppingToken);
                }

                await channel.BasicAckAsync(result.DeliveryTag, false, stoppingToken);
            }
            catch (Exception ex)
            {
                ProjectionApplyFailedLogger(this.logger, ex);
                await channel.BasicNackAsync(result.DeliveryTag, false, true, stoppingToken);
            }
        }
    }
}